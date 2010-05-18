using System;
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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for SelectBoats.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class SelectBoats : Window
    {
        private RowDefinition[] rdef;
        private SelectedBoats[] sbt;
        private Hashtable boatClasses;
        private RaceEdit[] reds;

        public SelectBoats(RaceEdit[] raceEdits)
        {
            InitializeComponent();

            reds = raceEdits;
            boatClasses = new Hashtable();

            rdef = new RowDefinition[reds.Length];
            sbt = new SelectedBoats[reds.Length];

            for (int i = 0; i < reds.Length; i++)
            {
                rdef[i] = new RowDefinition();
                rdef[i].Height = new GridLength(1, GridUnitType.Star);
                Fleets.RowDefinitions.Add(rdef[i]);

                sbt[i] = new SelectedBoats(reds[i].Rid);
                boatClasses[reds[i].RaceClass] = sbt[i];
                Grid.SetRow(sbt[i], i);
                sbt[i].VerticalContentAlignment = VerticalAlignment.Stretch;
                sbt[i].FleetName.Content = reds[i].RaceName;
                sbt[i].VerticalAlignment = VerticalAlignment.Stretch;
                Fleets.Children.Add(sbt[i]);

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
                foreach (DataRow r in d.Table.Rows)
                {
                    DataRow gr = bts.NewRow();
                    gr["bid"] = r["bid"];
                    gr["boatname"] = r["boatname"];
                    gr["boatclass"] = r["boatclass"];
                    gr["sailno"] = r["sailno"];
                    bts.Rows.Add(gr);
                }
                bts.AcceptChanges();
                sbt[i].Boats.ItemsSource = bts.DefaultView;
                sbt[i].Boats.IsReadOnly = true;
            }

            string sql = @"SELECT * FROM boats ";
            bool yachts = false;
            bool dinghies = false;

            for (int i = 0; i < reds.Length; i++)
            {
                if (reds[i].RaceClass.IndexOf("Yacht") >= 0)
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
                sql += @"WHERE dinghy = 0 ";
            else if (dinghies && !yachts)
                sql += @"WHERE dinghy = 1 ";

            sql += @"ORDER BY boatname";

            Db c = new Db(sql);
            DataTable dt = c.GetData(null);

            Boats.ItemsSource = dt.DefaultView;
            Boats.IsReadOnly = true;

            Boatname.TextChanged += new TextChangedEventHandler(Boatname_TextChanged);
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
            //dFilterBoats df = ;
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
                //((DataView)Boats.ItemsSource).Sort = "boatname";
                ((DataView)Boats.ItemsSource).RowFilter =
                    "boatname LIKE '" + Boatname.Text + "%'" +
                    "or sailno LIKE '" + Boatname.Text + "%'" +
                    "or boatclass LIKE '" + Boatname.Text + "%'"
                    ;
                //Boats
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

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddBoats();
        }

        private void AddBoats()
        {
            IList x = Boats.SelectedItems;
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
                    bool dngy = (bool)rv["dinghy"];
                    int ohp = (int)rv["open_handicap"];
                    string h = "";
                    if (rv["hulltype"] != DBNull.Value) h = (string)rv["hulltype"];
                    if (dngy)
                    {
                        if (boatClasses.ContainsKey("C Dinghy") && h == "C")
                        {
                            AddBoat((SelectedBoats)boatClasses["C Dinghy"], rv);
                        }
                        else if (boatClasses.ContainsKey("Dinghy"))
                        {
                            AddBoat((SelectedBoats)boatClasses["Dinghy"], rv);
                        }
                        else if (boatClasses.ContainsKey("All"))
                        {
                            AddBoat((SelectedBoats)boatClasses["All"], rv);
                        }
                    }
                    else
                    {
                        if (boatClasses.ContainsKey("F Yacht") && ohp <= 974)
                        {
                            AddBoat((SelectedBoats)boatClasses["F Yacht"], rv);
                        }
                        else if (boatClasses.ContainsKey("S Yacht") && ohp > 974)
                        {
                            AddBoat((SelectedBoats)boatClasses["S Yacht"], rv);
                        }
                        else if (boatClasses.ContainsKey("Yacht"))
                        {
                            AddBoat((SelectedBoats)boatClasses["Yacht"], rv);
                        }
                        else if (boatClasses.ContainsKey("All"))
                        {
                            AddBoat((SelectedBoats)boatClasses["All"], rv);
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
            ((DataView)sbt.Boats.ItemsSource).Table.Rows.Add(dr);
            ((DataView)sbt.Boats.ItemsSource).Table.AcceptChanges();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Db delete = new Db("DELETE FROM races WHERE rid = @rid AND bid = @bid");
            Db add = new Db(@"INSERT INTO races
                    (rid, date, bid, open_handicap, handicap_status, start, rolling_handicap)
                    SELECT c.rid, c.date, b.bid, b.open_handicap, b.handicap_status, c.start + ':00', b.rolling_handicap
                    FROM boats b, calendar c
                    WHERE b.bid = @bid
                    AND c.rid = @rid");
            Hashtable a = new Hashtable();

            this.DialogResult = true;
            for (int i = 0; i < sbt.Length; i++)
            {
                DataTable rd = ((DataView)reds[i].Races.ItemsSource).Table;
                a["rid"] = sbt[i].RaceId;
                DataTable sb = ((DataView)sbt[i].Boats.ItemsSource).Table;
                Hashtable selectedBids = new Hashtable();
                foreach (DataRow r in sb.Rows)
                {
                    a["bid"] = r["bid"];
                    if (rd.Select("bid = " + a["bid"] + " AND rid = " + a["rid"]).Length == 0)
                    {
                        add.ExecuteNonQuery(a);
                    }
                    selectedBids[r["bid"].ToString()] = true;
                }
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
            Boat b = new Boat(0);
            b.ShowDialog();
        }
    }
}
