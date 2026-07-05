using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Printing;
using System.Windows.Xps;
using OodHelper.Maintain;

namespace OodHelper.Results
{
    /// <summary>
    /// Interaction logic for RaceResults.xaml.
    /// </summary>
    /// <remarks>
    /// The host keeps the WPF-specific plumbing (tabs, per-tab context menus, the XPS print
    /// pipeline) but is now driven by a <see cref="RaceResultsViewModel"/>; all database work
    /// goes through <see cref="OodHelper.Data.IRaceResultsRepository"/> via that view-model.
    /// </remarks>
    public partial class RaceResults : UserControl
    {
        private readonly RaceResultsViewModel _vm;
        private ResultsEditor[] reds;

        public RaceResults(RaceResultsViewModel viewModel)
        {
            InitializeComponent();

            _vm = viewModel;
            reds = _vm.Editors.Select(ev => new ResultsEditor(ev)).ToArray();

            bool askAutoPopulate = true, doAutoPopulate = false;
            for (int i = 0; i < reds.Length; i++)
            {
                ResultsEditor r = reds[i];
                if ((r.Rows == null || r.Rows.Count == 0) && r.CountAutoPopulateData() > 0)
                {
                    if (askAutoPopulate)
                    {
                        askAutoPopulate = false;
                        if (MessageBox.Show("Would you like to copy competitors from previous race?",
                            "Auto populate", MessageBoxButton.YesNo,
                            MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        {
                            doAutoPopulate = true;
                        }
                    }
                    if (doAutoPopulate)
                    {
                        r.DoAutoPopulate();
                    }
                }
                TabItem t = new TabItem();
                t.Header = r.RaceName;
                t.Content = r;
                raceTabControl.Items.Add(t);
                r.ContextMenu = new ContextMenu();
            }

            for (int i = 0; i < reds.Length; i++)
            {
                ResultsEditor from = reds[i];
                ContextMenu m = from.ContextMenu;
                MenuItem editBoat = new MenuItem();
                editBoat.Header = "Edit Boat";
                editBoat.Command = new EditBoatCmd(this);
                editBoat.CommandParameter = from;
                m.Items.Add(editBoat);
                if (reds.Length > 1)
                {
                    for (int j = 0; j < reds.Length; j++)
                    {
                        ResultsEditor to = reds[j];
                        if (i != j)
                        {
                            MenuItem mi = new MenuItem();
                            mi.Header = "Move to " + to.RaceName;
                            mi.Command = new FleetChanger(this, from.Rid, to.Rid);
                            mi.CommandParameter = from.Races;
                            m.Items.Add(mi);
                        }
                    }
                }
            }
        }

        class EditBoatCmd : ICommand
        {
            private readonly RaceResults _owner;

            public EditBoatCmd(RaceResults owner)
            {
                _owner = owner;
            }

            #region ICommand Members

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            // disable unused event warning
#pragma warning disable 67
            public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

            public void Execute(object? parameter)
            {
                bool reload = false;
                ResultsEditor rr = (ResultsEditor) parameter!;

                foreach (DataGridCellInfo inf in rr.Races.SelectedCells)
                {
                    ResultRowViewModel? rv = inf.Item as ResultRowViewModel;
                    if (rv == null) continue;
                    int bid = rv.Bid;
                    BoatView edit = new BoatView(bid);
                    if (edit.ShowDialog() == true)
                    {
                        _owner._vm.ApplyEditedBoatHandicaps(rr.Rid, bid);
                        reload = true;
                    }
                }
                if (reload) rr.LoadGrid();
            }

            #endregion
        }

        class FleetChanger : ICommand
        {
            private readonly RaceResults _owner;
            private readonly int _toRid;
            private readonly int _fromRid;

            public FleetChanger(RaceResults owner, int from, int to)
            {
                _owner = owner;
                _fromRid = from;
                _toRid = to;
            }

            #region ICommand Members

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            // disable unused event warning
#pragma warning disable 67
            public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

            public void Execute(object? parameter)
            {
                DataGrid races = (DataGrid) parameter!;
                if (races.SelectedCells.Count > 0)
                {
                    foreach (DataGridCellInfo inf in races.SelectedCells)
                    {
                        ResultRowViewModel? rv = inf.Item as ResultRowViewModel;
                        if (rv == null) continue;
                        _owner._vm.MoveToFleet(_fromRid, _toRid, rv.Bid);
                    }

                    for (int i = 0; i < _owner.reds.Length; i++)
                        _owner.reds[i].LoadGrid();
                }
            }

            #endregion
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            ResultsPrintSelector rps = new ResultsPrintSelector(reds);
            if (rps.ShowDialog() == true)
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    Working w = new Working(App.Current.MainWindow);
                    w.Show();
                    XpsDocumentWriter write = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
                    VisualsToXpsDocument? collator = write.CreateVisualsCollator() as VisualsToXpsDocument;
                    Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                    collator!.BeginBatchWrite();
                    System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            w.SetRange(0, reds.Length);
                            for (int i = 0; i < reds.Length; i++)
                            {
                                ResultsEditor red = reds[i];
                                if (red.PrintInclude)
                                {
                                    string msg = "";
                                    Dispatcher.Invoke(new Action(delegate()
                                    {
                                        msg = string.Format("Printing {0}", red.RaceName);
                                    }));
                                    w.SetProgress(msg, i + 1);
                                    System.Threading.Thread.Sleep(50);
                                    Dispatcher.Invoke(new Action(delegate()
                                    {
                                        Page? p = null;
                                        IResultsPage? rp = null;

                                        switch (red.Handicap)
                                        {
                                            case "o":
                                                p = (Page)new OpenHandicapResultsPage(red);
                                                break;
                                            case "r":
                                                p = (Page)new RollingHandicapResultsPage(red);
                                                break;
                                        }
                                        rp = p as IResultsPage;
                                        p!.Width = pd.PrintableAreaWidth;
                                        p.Measure(ps);
                                        p.Arrange(new Rect(new Point(0, 0), ps));
                                        p.UpdateLayout();

                                        int pno = 1;
                                        while (rp!.PrintPage(collator, pno)) pno++;
                                    }));
                                }
                            }
                            Dispatcher.Invoke(new Action(delegate()
                            {
                                collator.EndBatchWrite();
                            }));
                            w.CloseWindow();
                        });
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DockPanel? d = Parent as DockPanel;
            if (d != null)
                d.Children.Remove(this);
            else
            {
                TabItem? t = Parent as TabItem;
                TabControl? tc = t!.Parent as TabControl;
                if (tc != null)
                    tc.Items.Remove(t);
                t.Content = null;
            }
        }

        private void ChooseBoats_Click(object sender, RoutedEventArgs e)
        {
            SelectBoats dlg = new SelectBoats(reds);
            if (dlg.ShowDialog() == true)
            {
                for (int i = 0; i < reds.Length; i++)
                    reds[i].LoadGrid();
            }
        }
    }
}
