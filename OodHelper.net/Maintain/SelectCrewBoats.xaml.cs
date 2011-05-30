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
using OodHelper.Maintain;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for SelectBoats.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class SelectCrewBoats : Window
    {
        private StringBuilder BoatsSql;
        private int Id { get; set; }

        public SelectCrewBoats(int id)
        {
            InitializeComponent();

            Id = id;

            BoatsSql = new StringBuilder(@"SELECT bid, boatname, boatclass, sailno, handicap_status, rolling_handicap FROM boats ORDER BY boatname");

            Db c = new Db(BoatsSql.ToString());

            Boats.ItemsSource = c.GetData(null).DefaultView;
            Boats.IsReadOnly = true;

            BoatsSql.Clear();
            BoatsSql.Append("SELECT boats.bid, boatname, boatclass, sailno, handicap_status, rolling_handicap " + 
                "FROM boats INNER JOIN boat_crew ON boats.bid = boat_crew.bid " +
                "WHERE boat_crew.id = @id " +
                "ORDER BY boatname");
            c = new Db(BoatsSql.ToString());

            Hashtable p = new Hashtable();
            p["id"] = Id;

            BoatsSelected.ItemsSource = c.GetData(p).DefaultView;
            BoatsSelected.IsReadOnly = true;
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
                        if (((DataView)BoatsSelected.ItemsSource).Table.Select("bid = " + rv["bid"].ToString()).Length > 0)
                        {
                            alreadySelected = true;
                            break;
                        }

                    if (!alreadySelected)
                    {
                        AddBoat(rv);
                    }
                }
            }
        }

        private void AddBoat(DataRowView rv)
        {
            DataRow dr = ((DataView)BoatsSelected.ItemsSource).Table.NewRow();
            dr["bid"] = rv["bid"];
            dr["boatname"] = rv["boatname"];
            dr["boatclass"] = rv["boatclass"];
            dr["sailno"] = rv["sailno"];
            dr["handicap_status"] = rv["handicap_status"];
            dr["rolling_handicap"] = rv["rolling_handicap"];
            ((DataView)BoatsSelected.ItemsSource).Table.Rows.Add(dr);
            ((DataView)BoatsSelected.ItemsSource).Table.AcceptChanges();
            ((DataView)BoatsSelected.ItemsSource).Sort = "Boatname";
            Notify.Text = string.Format("Added {0}", dr["boatname"]);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Hashtable a = new Hashtable();
            a["id"] = Id;

            Db delete = new Db("DELETE FROM boat_crew WHERE id = @id");
            delete.ExecuteNonQuery(a);

            Db add = new Db(@"INSERT INTO boat_crew
                    (id, bid)
                    VALUES (@id, @bid)");

            foreach (DataRow r in ((DataView)BoatsSelected.ItemsSource).Table.Rows)
            {
                a["bid"] = r["bid"];
                add.ExecuteNonQuery(a);
            }

            this.DialogResult = true;
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

        private void RemoveBoats()
        {
            DataTable sb = ((DataView)BoatsSelected.ItemsSource).Table;
            DataRowView[] x = new DataRowView[BoatsSelected.SelectedItems.Count];
            BoatsSelected.SelectedItems.CopyTo(x,0);
            foreach (DataRowView rv in x)
            {
                sb.Rows.Remove(rv.Row);
            }
            sb.AcceptChanges();
        }

        private void DeselectBoat_Click(object sender, RoutedEventArgs e)
        {
            RemoveBoats();
        }

        private void BoatsSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveBoats();
        }
    }
}
