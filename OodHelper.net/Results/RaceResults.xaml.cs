using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Printing;
using System.Windows.Xps;
using OodHelper.Maintain;
using OodHelper.Results.Model;
using OodHelper.Results.ViewModel;

namespace OodHelper.Results
{
    /// <summary>
    /// Interaction logic for RaceResults.xaml
    /// </summary>
    public partial class RaceResults : UserControl
    {
        private ResultEditorViewModel[] reds;

        public RaceResults(int[] rids)
        {
            InitializeComponent();

            reds = new ViewModel.ResultEditorViewModel[rids.Length];

            //bool askAutoPopulate = true, doAutoPopulate = false;
            for (int i = 0; i < rids.Length; i++)
            {
                ICalendarEvent _event = new CalendarEvent(rids[i]);
                IList<IEntry> _entries = Entry.GetEntries(rids[i], _event.start_date);
                IRace _race = new Race(_event, _entries);
                ResultEditorViewModel _resultEditor = new ResultEditorViewModel(_race);

                reds[i] = _resultEditor;
                TabItem t = new TabItem();
                t.Header = _resultEditor.DisplayName;
                t.Content = _resultEditor;
                raceTabControl.Items.Add(t);
                _resultEditor.ContextMenuItems = new List<CommandListItem>();
                _resultEditor.ContextMenuItems.Add(new CommandListItem() { Text = "Edit Boat", Command = 
                    new RelayCommand(param => this.EditBoat(_resultEditor))
                });
            }

            for (int i = 0; i < rids.Length; i++)
            {
                ResultEditorViewModel from = ((TabItem)raceTabControl.Items[i]).Content as ResultEditorViewModel;
                IList<CommandListItem> _commandList = from.ContextMenuItems;

                if (rids.Length > 1)
                {
                    for (int j = 0; j < rids.Length; j++)
                    {
                        ResultEditorViewModel to = ((TabItem)raceTabControl.Items[j]).Content as ResultEditorViewModel;
                        if (i != j)
                        {
                            CommandListItem _moveBoat = new CommandListItem();
                            _moveBoat.Text = string.Format("Move to {0}", to.DisplayName);
                            //mi.Command = new RelayCommand(param => this.ChangeFleet(from, to));
                            _commandList.Add(_moveBoat);
                        }
                    }
                }
            }
        }

        public void EditBoat(object parameter)
        {
            bool reload = false;
            ResultEditorViewModel rr = parameter as ResultEditorViewModel;

            if (rr != null && rr.SelectedEntry != null)
            {
                ResultEntryViewModel _entry = rr.SelectedEntry as ResultEntryViewModel;
                if (_entry != null)
                {
                    int bid = _entry.Bid;
                    BoatView edit = new BoatView(bid);
                    if (edit.ShowDialog().Value)
                    {
                        Db c = new Db(@"SELECT bid, rolling_handicap, handicap_status, open_handicap
                                FROM boats WHERE bid = @bid");
                        Hashtable p = new Hashtable();
                        p["bid"] = bid;
                        Hashtable d = c.GetHashtable(p);
                        foreach (object o in d.Keys)
                            p[o] = d[o];
                        p["rid"] = rr.Result.Event.rid;
                        c = new Db(@"UPDATE races
                                SET rolling_handicap = @rolling_handicap,
                                handicap_status = @handicap_status,
                                open_handicap = @open_handicap,
                                last_edit = GETDATE()
                                WHERE rid = @rid
                                AND bid = @bid");
                        c.ExecuteNonQuery(p);
                        reload = true;
                    }
                }
            }
        }

        class FleetChanger : ICommand
        {
            private int toRid;
            private int fromRid;
            private RaceResults rr;
            public FleetChanger(RaceResults r, int from, int to)
            {
                fromRid = from;
                toRid = to;
                rr = r;
            }

            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            // disable unused event warning
#pragma warning disable 67
            public event EventHandler CanExecuteChanged;
#pragma warning restore 67

            public void Execute(object parameter)
            {
                DataGrid races = (DataGrid)parameter;
                if (races.SelectedCells.Count > 0)
                {
                    Db s = new Db(@"SELECT start_date
                            FROM calendar
                            WHERE rid = @torid");
                    Hashtable p = new Hashtable();
                    p["torid"] = toRid;
                    DateTime rstart = (DateTime)s.GetScalar(p);
                    Db c = new Db(@"UPDATE races
                            SET rid = @torid
                            , start_date = @start_date
                            WHERE rid = @fromrid
                            AND bid = @bid");
                    p["fromrid"] = fromRid;
                    p["start_date"] = rstart;
                    foreach (DataGridCellInfo inf in races.SelectedCells)
                    {
                        Entry drv = inf.Item as Entry;
                        p["bid"] = drv.bid;
                        c.ExecuteNonQuery(p);
                    }

                    //for (int i = 0; i < rr.reds.Length; i++)
                    //    rr.reds[i].LoadGrid();
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
                    VisualsToXpsDocument collator = write.CreateVisualsCollator() as VisualsToXpsDocument;
                    Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                    collator.BeginBatchWrite();
                    System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            w.SetRange(0, reds.Length);
                            for (int i = 0; i < reds.Length; i++)
                            {
                                ViewModel.ResultEditorViewModel red = reds[i];
                                if (red.PrintInclude)
                                {
                                    string msg = null;
                                    Dispatcher.Invoke(new Action(delegate()
                                    {
                                        msg = string.Format("Printing {0}", red.DisplayName);
                                    }));
                                    w.SetProgress(msg, i + 1);
                                    System.Threading.Thread.Sleep(50);
                                    Dispatcher.Invoke(new Action(delegate()
                                    {
                                        Page p = null;
                                        IResultsPage rp = null;

                                        switch (red.Handicapping)
                                        {
                                            case Model.CalendarEvent.Handicappings.o:
                                                //p = (Page)new OpenHandicapResultsPage(red);
                                                break;
                                            case Model.CalendarEvent.Handicappings.r:
                                                //p = (Page)new RollingHandicapResultsPage(red);
                                                break;
                                        }
                                        rp = p as IResultsPage;
                                        p.Width = pd.PrintableAreaWidth;
                                        p.Measure(ps);
                                        p.Arrange(new Rect(new Point(0, 0), ps));
                                        p.UpdateLayout();

                                        int pno = 1;
                                        while (rp.PrintPage(collator, pno)) pno++;
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
            DockPanel d = Parent as DockPanel;
            if (d != null)
                d.Children.Remove(this);
            else
            {
                TabItem t = Parent as TabItem;
                TabControl tc = t.Parent as TabControl;
                if (tc != null)
                    tc.Items.Remove(t);
                t.Content = null;
            }
        }

        private void ChooseBoats_Click(object sender, RoutedEventArgs e)
        {
            //SelectBoats dlg = new SelectBoats(reds);
            //if (dlg.ShowDialog() == true)
            //{
            //    for (int i = 0; i < reds.Length; i++)
            //        reds[i].LoadGrid();
            //}
        }
    }
}
