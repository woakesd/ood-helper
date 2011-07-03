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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OodHelper.Maintain;
using OodHelper.Rules;

namespace OodHelper.Results
{
    /// <summary>
    /// Interaction logic for SelectBoats.xaml
    /// </summary>
    public partial class SelectBoats : Window
    {
        private RowDefinition[] rdef;
        private SelectedBoats[] sbt;
        private Hashtable boatClasses;
        private ResultsEditor[] reds;
        private BoatSelectRule[] rules;

        private StringBuilder BoatsSql;

        public SelectBoats(ResultsEditor[] raceEdits)
        {
            InitializeComponent();

            reds = raceEdits;
            boatClasses = new Hashtable();

            rdef = new RowDefinition[reds.Length];
            sbt = new SelectedBoats[reds.Length];
            rules = new BoatSelectRule[reds.Length];

            for (int i = 0; i < reds.Length; i++)
            {
                TabItem ti = new TabItem();
                ti.Header = reds[i].RaceClass;

                sbt[i] = new SelectedBoats(reds[i].Rid);
                boatClasses[reds[i].RaceClass] = sbt[i];

                sbt[i].FleetName.Content = reds[i].RaceName;
                
                ti.Content = sbt[i];
                Fleets.Items.Add(ti);

                DataView d = (DataView) reds[i].Races.ItemsSource;
                DataTable bts = new DataTable();
                bts.TableName = "boats";
                bts.Columns.Add(new DataColumn("bid"));
                DataColumn[] pk = new DataColumn[1];
                pk[0] = bts.Columns["bid"];
                bts.PrimaryKey = pk;
                bts.Columns.Add(new DataColumn("boatname"));
                bts.Columns.Add(new DataColumn("boatclass"));
                bts.Columns.Add(new DataColumn("sailno"));
                bts.Columns.Add(new DataColumn("handicap_status"));
                bts.Columns.Add(new DataColumn("open_handicap"));
                bts.Columns.Add(new DataColumn("rolling_handicap"));
                foreach (DataRow r in d.Table.Rows)
                {
                    DataRow gr = bts.NewRow();
                    gr["bid"] = r["bid"];
                    gr["boatname"] = r["boatname"];
                    gr["boatclass"] = r["boatclass"];
                    gr["sailno"] = r["sailno"];
                    gr["handicap_status"] = r["handicap_status"];
                    gr["open_handicap"] = r["open_handicap"];
                    gr["rolling_handicap"] = r["rolling_handicap"];
                    bts.Rows.Add(gr);
                }
                bts.AcceptChanges();
                ItemCollection ic = sbt[i].Boats.Items;
                sbt[i].Boats.ItemsSource = bts.DefaultView;
                sbt[i].Boats.IsReadOnly = true;
                sbt[i].Boats.ContextMenu = new ContextMenu();

                rules[i] = new BoatSelectRule(reds[i].RaceClass);
            }

            for (int i = 0; i < sbt.Length; i++)
            {
                for (int j = 0; j < reds.Length; j++)
                {
                    if (j != i)
                    {
                        MenuItem m = new MenuItem();
                        m.Header = "Move to " + reds[j].RaceClass;
                        m.Command = new FleetChanger(sbt[i].Boats, sbt[j].Boats);
                        sbt[i].Boats.ContextMenu.Items.Add(m);
                    }
                }
            }

            BoatsSql = new StringBuilder(@"SELECT * FROM boats ");
            bool yachts = false;
            bool dinghies = false;

            for (int i = 0; i < reds.Length; i++)
            {
                if (reds[i].RaceClass.IndexOf("Yacht") >= 0)
                    yachts = true;

                if (reds[i].RaceClass.IndexOf("Division") >= 0)
                    yachts = true;

                if (reds[i].RaceClass.IndexOf("Dinghy") >= 0)
                    dinghies = true;

                if (reds[i].RaceClass.IndexOf("All") >= 0)
                {
                    yachts = true;
                    dinghies = true;
                }
            }

            if (!dinghies && yachts)
                BoatsSql.Append(@"WHERE dinghy = 0 ");
            else if (dinghies && !yachts)
                BoatsSql.Append(@"WHERE dinghy = 1 ");

            BoatsSql.Append(@"ORDER BY boatname");

            Db c = new Db(BoatsSql.ToString());
            DataTable dt = c.GetData(null);

            Boats.ItemsSource = dt.DefaultView;
            Boats.IsReadOnly = true;
        }

        class FleetChanger : ICommand
        {
            private DataGrid toGrid;
            private DataGrid fromGrid;

            public FleetChanger(DataGrid from, DataGrid to)
            {
                fromGrid = from;
                toGrid = to;
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
                for (int r = 0; r < fromGrid.SelectedItems.Count; r++)
                {
                    DataRowView drv = fromGrid.SelectedItems[r] as DataRowView;
                    DataTable toTable = ((DataView)toGrid.ItemsSource).Table;
                    DataRow n = toTable.NewRow();
                    for (int i = 0; i < drv.Row.ItemArray.Length; i++)
                        n[i] = drv.Row[i];
                    toTable.Rows.Add(n);
                    toTable.AcceptChanges();
                }
                while (fromGrid.SelectedItems.Count > 0)
                {
                    DataRowView drv = fromGrid.SelectedItems[0] as DataRowView;
                    drv.DataView.Table.Rows.Remove(drv.Row);
                    drv.DataView.Table.AcceptChanges();
                }
            }

            #endregion
        }

        void m_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        System.Timers.Timer t = null;

        void Boatname_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (t == null)
                t = new System.Timers.Timer(500);
            else
                t.Stop();
            t.AutoReset = false;
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new dFilterBoats(FilterBoats), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilterBoats();

        public void FilterBoats()
        {
            try
            {
                ((DataView)Boats.ItemsSource).RowFilter =
                    "boatname LIKE '%" + Boatname.Text.Replace("'","''") + "%'" +
                    "or sailno LIKE '%" + Boatname.Text.Replace("'", "''") + "%'" +
                    "or boatclass LIKE '%" + Boatname.Text.Replace("'", "''") + "%'"
                    ;
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        void Boats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddBoats();
        }

        private void AddBoats()
        {
            if (Boats.Items.Count == 1)
                Boats.SelectAll();
            IList x = Boats.SelectedItems;
            if (x.Count == 0)
            {
                MessageBox.Show("No boats are highlighted (hint: click on the boat name)", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                foreach (DataRowView rv in x)
                {
                    bool alreadySelected = false;
                    for (int i = 0; i < sbt.Length; i++)
                    {
                        if (((DataView)sbt[i].Boats.ItemsSource).Table.Select("bid = " + rv["bid"].ToString()).Length > 0)
                        {
                            alreadySelected = true;
                        }
                    }
                    
                    if (!alreadySelected)
                    {
                        List<BoatSelectRule> choices = new List<BoatSelectRule>();
                        for (int i = 0; i < rules.Length; i++)
                        {
                            if (rules[i].AppliesToBoat(rv))
                            {
                                choices.Add(rules[i]);
                            }
                        }
                        if (choices.Count == 1)
                            AddBoat((SelectedBoats)boatClasses[choices[0].Name], rv);
                        else if (choices.Count > 1)
                        {
                            RuleSelector rs = new RuleSelector(choices);
                            if (rs.ShowDialog().Value)
                            {
                                BoatSelectRule rc = rs.RuleChoice.SelectedItem as BoatSelectRule;
                                if (rc != null) AddBoat((SelectedBoats)boatClasses[rc.Name], rv);
                            }
                        }
                        else
                        {
                            RuleSelector rs = new RuleSelector(new List<BoatSelectRule>(rules));
                            if (rs.ShowDialog().Value)
                            {
                                BoatSelectRule rc = rs.RuleChoice.SelectedItem as BoatSelectRule;
                                if (rc != null) AddBoat((SelectedBoats)boatClasses[rc.Name], rv);
                            }
                        }
                    }
                }
            }
        }

        private void AddBoat(SelectedBoats sbt, DataRowView rv)
        {
            DataRow dr = ((DataView)sbt.Boats.ItemsSource).Table.NewRow();
            dr["bid"] = rv["bid"];
            dr["boatname"] = rv["boatname"];
            dr["boatclass"] = rv["boatclass"];
            dr["sailno"] = rv["sailno"];
            dr["handicap_status"] = rv["handicap_status"];
            dr["open_handicap"] = rv["open_handicap"];
            dr["rolling_handicap"] = rv["rolling_handicap"];
            ((DataView)sbt.Boats.ItemsSource).Table.Rows.Add(dr);
            ((DataView)sbt.Boats.ItemsSource).Table.AcceptChanges();
            ((DataView)sbt.Boats.ItemsSource).Sort = "Boatname";
            Notify.Text = string.Format("Added {0} to {1}", new object[] { dr["boatname"], sbt.FleetName.Content });
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Db delete = new Db("DELETE FROM races WHERE rid = @rid AND bid = @bid");
            Db add = new Db(@"INSERT INTO races
                    (rid, bid, start_date, handicap_status, open_handicap, rolling_handicap, last_edit)
                    VALUES (@rid, @bid, @start_date, @handicap_status, @open_handicap, @rolling_handicap, GETDATE())");
            Hashtable a = new Hashtable();

            this.DialogResult = true;
            for (int i = 0; i < sbt.Length; i++)
            {
                DataTable rd = ((DataView)reds[i].Races.ItemsSource).Table;
                a["rid"] = sbt[i].RaceId;
                DataTable sb = ((DataView)sbt[i].Boats.ItemsSource).Table;
                Hashtable selectedBids = new Hashtable();
                //
                // For each boat in selected boats check to see if it is in the race edit control,
                // if not then add it.
                //
                foreach (DataRow r in sb.Rows)
                {
                    if (rd.Select("bid = " + r["bid"] + " AND rid = " + a["rid"]).Length == 0)
                    {
                        a["bid"] = r["bid"];
                        a["start_date"] = reds[i].StartDate;
                        a["handicap_status"] = r["handicap_status"];
                        a["open_handicap"] = r["open_handicap"];
                        a["rolling_handicap"] = r["rolling_handicap"];
                        add.ExecuteNonQuery(a);
                    }
                    selectedBids[r["bid"].ToString()] = true;
                }
                //
                // For each boat in the race edit control check to see if it is in selected boats,
                // if not then delete it.
                //
                foreach (DataRow r in rd.Rows)
                {
                    a["bid"] = r["bid"];
                    if (!selectedBids.ContainsKey(r["bid"].ToString()))
                    {
                        delete.ExecuteNonQuery(a);
                    }
                }
            }
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void NewBoat_Click(object sender, RoutedEventArgs e)
        {
            BoatView b = new BoatView(0);
            if (b.ShowDialog() == true)
            {
                Db c = new Db(BoatsSql.ToString());
                DataTable dt = c.GetData(null);

                if (b.Bid > 0)
                {
                    c = new Db("SELECT boatname FROM boats WHERE bid = @bid");
                    Hashtable p = new Hashtable();
                    p["bid"] = b.Bid;
                    Boatname.Text = c.GetScalar(p) as string;
                }

                Boats.ItemsSource = dt.DefaultView;
                if (Boatname.Text != string.Empty)
                    FilterBoats();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Boatname.Focus();
            else if (e.Key == Key.F4)
            {
                AddBoats();
            }
        }

        private void SelectBoat_Click(object sender, RoutedEventArgs e)
        {
            AddBoats();
        }

        private void DeselectBoat_Click(object sender, RoutedEventArgs e)
        {
            TabItem fleet = Fleets.SelectedItem as TabItem;
            if (fleet != null)
            {
                SelectedBoats sb = fleet.Content as SelectedBoats;
                if (sb != null)
                {
                    sb.RemoveBoats();
                }
            }
        }
    }
}