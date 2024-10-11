using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Printing;
using System.Windows.Xps;
using OodHelper.Maintain;

namespace OodHelper.Results
{
    /// <summary>
    /// Interaction logic for RaceResults.xaml
    /// </summary>
    public partial class RaceResults : UserControl
    {
        private ResultsEditor[] reds;

        public RaceResults(int[] rids)
        {
            InitializeComponent();

            reds = new ResultsEditor[rids.Length];

            bool askAutoPopulate = true, doAutoPopulate = false;
            for (int i = 0; i < rids.Length; i++)
            {
                ResultsEditor r = new ResultsEditor(rids[i]);
                if (!r.Races.HasItems && r.CountAutoPopulateData() > 0)
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
                reds[i] = r;
                TabItem t = new TabItem();
                t.Header = r.RaceName;
                t.Content = r;
                raceTabControl.Items.Add(t);
                r.ContextMenu = new ContextMenu();
            }

            for (int i = 0; i < rids.Length; i++)
            {
                ResultsEditor from = (ResultsEditor)((TabItem)raceTabControl.Items[i]).Content;
                ContextMenu m = from.ContextMenu;
                MenuItem editBoat = new MenuItem();
                editBoat.Header = "Edit Boat";
                editBoat.Command = new EditBoatCmd();
                editBoat.CommandParameter = from;
                m.Items.Add(editBoat);
                if (rids.Length > 1)
                {
                    for (int j = 0; j < rids.Length; j++)
                    {
                        ResultsEditor to = (ResultsEditor)((TabItem)raceTabControl.Items[j]).Content;
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
            public EditBoatCmd()
            {
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
                ResultsEditor rr = (ResultsEditor)parameter!;
                IList<DataGridCellInfo> cc = rr.Races.SelectedCells;

                foreach (DataGridCellInfo inf in rr.Races.SelectedCells)
                {
                    ResultModel? rv = inf.Item as ResultModel;
                    int bid = rv!.Bid;
                    BoatView edit = new BoatView(bid);
                    if (edit.ShowDialog()!.Value)
                    {
                        Db c = new Db(@"SELECT bid, rolling_handicap, handicap_status, open_handicap
                                FROM boats WHERE bid = @bid");
                        var p = new Hashtable
                        {
                            ["bid"] = bid
                        };
                        var d = c.GetHashtable(p);
                        foreach (object o in d.Keys)
                            p[o] = d[o];
                        p["rid"] = rr.Rid;
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
                if (reload) rr.LoadGrid();
            }

            #endregion
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
                DataGrid races = (DataGrid)parameter!;
                if (races.SelectedCells.Count > 0)
                {
                    Db s = new Db(@"SELECT start_date
                            FROM calendar
                            WHERE rid = @torid");
                    Hashtable p = new Hashtable
                    {
                        ["torid"] = toRid
                    };
                    var rstart = (DateTime)s.GetScalar(p);
                    var c = new Db(@"UPDATE races
                            SET rid = @torid
                            , start_date = @start_date
                            WHERE rid = @fromrid
                            AND bid = @bid");
                    p["fromrid"] = fromRid;
                    p["start_date"] = rstart;
                    foreach (DataGridCellInfo inf in races.SelectedCells)
                    {
                        var drv = inf.Item as ResultModel;
                        p["bid"] = drv!.Bid;
                        c.ExecuteNonQuery(p);
                    }

                    for (int i = 0; i < rr.reds.Length; i++)
                        rr.reds[i].LoadGrid();
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
                    var collator = write.CreateVisualsCollator() as VisualsToXpsDocument;
                    Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                    collator!.BeginBatchWrite();
                    Task t = Task.Factory.StartNew(() =>
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
                                        Page p;
                                        IResultsPage? rp;

                                        switch (red.Handicap)
                                        {
                                            case "o":
                                                p = new OpenHandicapResultsPage(red);
                                                break;
                                            case "r":
                                                p = new RollingHandicapResultsPage(red);
                                                break;
                                            default:
                                                return;
                                        }
                                        rp = p as IResultsPage;
                                        p.Width = pd.PrintableAreaWidth;
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
            var d = Parent as DockPanel;
            if (d != null)
                d.Children.Remove(this);
            else
            {
                var t = Parent as TabItem;
                if (t!.Parent is TabControl tc)
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
