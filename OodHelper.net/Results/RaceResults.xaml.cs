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
                ResultEditorViewModel r = new ResultEditorViewModel(_race);

                reds[i] = r;
                TabItem t = new TabItem();
                t.Header = r.DisplayName;
                t.Content = r;
                raceTabControl.Items.Add(t);
                //r.ContextMenu = new ContextMenu();
            }

            //for (int i = 0; i < rids.Length; i++)
            //{
            //    ResultsEditor from = (ResultsEditor)((TabItem)raceTabControl.Items[i]).Content;
            //    ContextMenu m = from.ContextMenu;
            //    MenuItem editBoat = new MenuItem();
            //    editBoat.Header = "Edit Boat";
            //    editBoat.Command = new EditBoatCmd();
            //    editBoat.CommandParameter = from;
            //    m.Items.Add(editBoat);
            //    if (rids.Length > 1)
            //    {
            //        for (int j = 0; j < rids.Length; j++)
            //        {
            //            ResultsEditor to = (ResultsEditor)((TabItem)raceTabControl.Items[j]).Content;
            //            if (i != j)
            //            {
            //                MenuItem mi = new MenuItem();
            //                mi.Header = "Move to " + to.DisplayName;
            //                mi.Command = new FleetChanger(this, from.Rid, to.Rid);
            //                mi.CommandParameter = from.Races;
            //                m.Items.Add(mi);
            //            }
            //        }
            //    }
            //}
        }

        class EditBoatCmd : ICommand
        {
            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            protected virtual void OnCanExecuteChanged()
            {
                EventHandler handler = this.CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, null);
                }
            }


            public void Execute(object parameter)
            {
                bool reload = false;
                ResultsEditor rr = (ResultsEditor)parameter;
                IList<DataGridCellInfo> cc = rr.Races.SelectedCells;

                foreach (DataGridCellInfo inf in rr.Races.SelectedCells)
                {
                    Entry rv = inf.Item as Entry;
                    int bid = rv.bid;
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
