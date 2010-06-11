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
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Races.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class Races : Window
    {
        int redit = 0;

        public Races()
        {
            InitializeComponent();
            dSetGridSource = SetGridSource;
        }

        void Races_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGrid();
        }

        private delegate void DSetGridSource(DataTable ppl);
        private DSetGridSource dSetGridSource;
        Working w;

        private void LoadGrid()
        {
            w = new Working(this);
            RaceData.ItemsSource = null;
            Task.Factory.StartNew(() =>
            {
                Db c = new Db("SELECT * " +
                    "FROM calendar " +
                    "ORDER BY start_date");
                DataTable rcs = c.GetData(null);
                c.Dispose();
                Dispatcher.Invoke(dSetGridSource, rcs);
            });
        }

        private void SetGridSource(DataTable rcs)
        {
            RaceData.ItemsSource = rcs.DefaultView;
            if (Eventname.Text != "") FilterRaces();
            if (redit != 0)
            {
                foreach (DataRowView vr in RaceData.Items)
                {
                    DataRow r = vr.Row;
                    if ((int)r["rid"] == redit)
                    {
                        RaceData.ScrollIntoView(vr);
                        RaceData.SelectedItem = vr;
                        break;
                    }
                }
            }
            w.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Race b = new Race(0);
            if (b.ShowDialog().Value)
            {
                redit = b.Rid;
                LoadGrid();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (RaceData.SelectedItem != null)
            {
                DataRowView i = (DataRowView) RaceData.SelectedItem;
                Race b = new Race((int)i.Row["rid"]);
                if (b.ShowDialog().Value)
                {
                    redit = b.Rid;
                    LoadGrid();
                }
            }
        }

        System.Timers.Timer t = null;

        void Eventname_TextChanged(object sender, TextChangedEventArgs e)
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
                Dispatcher.Invoke(new dFilterRaces(FilterRaces), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilterRaces();

        public void FilterRaces()
        {
            try
            {
                ((DataView)RaceData.ItemsSource).RowFilter =
                    "event LIKE '%" + Eventname.Text + "%'";
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (RaceData.SelectedItem != null)
            {
                bool change = false;
                foreach (DataRowView i in RaceData.SelectedItems)
                {
                    string name = i.Row["event"].ToString();
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?",
                        "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel) break;
                    if (result == MessageBoxResult.Yes)
                    {
                        Db del = new Db("DELETE FROM calendar " +
                            "WHERE rid = @rid");
                        Hashtable d = new Hashtable();
                        d["rid"] = (int)i.Row["rid"];
                        del.ExecuteNonQuery(d);
                        change = true;
                    }
                }
                if (change) LoadGrid();
            }
        }
    }
}
