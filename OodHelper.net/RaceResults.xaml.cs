﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Printing;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for RaceResults.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class RaceResults : UserControl
    {
        private RaceEdit[] reds;

        public RaceResults(int[] rids)
        {
            InitializeComponent();

            reds = new RaceEdit[rids.Length];

            bool askAutoPopulate = true, doAutoPopulate = false;
            for (int i = 0; i < rids.Length; i++)
            {
                RaceEdit r = new RaceEdit(rids[i]);
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
                RaceEdit from = (RaceEdit)((TabItem)raceTabControl.Items[i]).Content;
                ContextMenu m = from.ContextMenu;
                MenuItem editBoat = new MenuItem();
                editBoat.Header = "Edit Boat";
                editBoat.Command = new EditBoat();
                editBoat.CommandParameter = from;
                m.Items.Add(editBoat);
                if (rids.Length > 1)
                {
                    for (int j = 0; j < rids.Length; j++)
                    {
                        RaceEdit to = (RaceEdit)((TabItem)raceTabControl.Items[j]).Content;
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

        class EditBoat : ICommand
        {
            public EditBoat()
            {
            }

            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                bool reload = false;
                RaceEdit rr = (RaceEdit)parameter;
                foreach (DataRowView drv in rr.Races.SelectedItems)
                {
                    int bid = (int)drv.Row["bid"];
                    Boat edit = new Boat(bid);
                    if (edit.ShowDialog().Value)
                    {
                        Db c = new Db("SELECT bid, rolling_handicap, handicap_status, open_handicap " +
                            "FROM boats WHERE bid = @bid");
                        Hashtable p = new Hashtable();
                        p["bid"] = bid;
                        Hashtable d = c.GetHashtable(p);
                        foreach (object o in d.Keys)
                            p[o] = d[o];
                        p["rid"] = rr.Rid;
                        c = new Db("UPDATE races " +
                                "SET rolling_handicap = @rolling_handicap, " +
                                "handicap_status = @handicap_status, " +
                                "open_handicap = @open_handicap " +
                                "WHERE rid = @rid " +
                                "AND bid = @bid");
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

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                DataGrid races = (DataGrid)parameter;
                if (races.SelectedItems.Count > 0)
                {
                    Db s = new Db(@"SELECT start_date
                            FROM calendar
                            WHERE rid = @torid");
                    Hashtable p = new Hashtable();
                    p["torid"] = toRid;
                    DateTime rstart = (DateTime)s.GetScalar(p);
                    Db c = new Db(@"UPDATE races
                            SET rid = @torid
                            , start = @start
                            WHERE rid = @fromrid
                            AND bid = @bid");
                    p["fromrid"] = fromRid;
                    p["start"] = rstart.TimeOfDay.ToString("hh\\:mm\\:ss");
                    foreach (DataRowView drv in races.SelectedItems)
                    {
                        p["bid"] = drv.Row["bid"];
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
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                foreach (RaceEdit red in reds)
                {
                    Page p = null;

                    switch (red.Handicap)
                    {
                        case "o":
                            p = (Page)new OpenHandicapResultsPage(red);
                            p.Width = pd.PrintableAreaWidth;
                            break;
                        case "r":
                            p = (Page)new RollingHandicapResultsPage(red);
                            p.Width = pd.PrintableAreaWidth;
                            break;
                    }
                    p.Measure(ps);
                    p.Arrange(new Rect(new Point(0, 0), ps));
                    p.UpdateLayout();
                    pd.PrintVisual(p, "Race results");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ((DockPanel)Parent).Children.Remove(this);
        }

        private void ChooseBoats_Click(object sender, RoutedEventArgs e)
        {
            SelectBoats dlg = new SelectBoats(reds);
            bool? ret = dlg.ShowDialog();
            if (ret.Value)
            {
                for (int i = 0; i < reds.Length; i++)
                    reds[i].LoadGrid();
            }
        }
    }
}
