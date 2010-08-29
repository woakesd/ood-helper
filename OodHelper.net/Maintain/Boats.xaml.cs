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

namespace OodHelper.net.Maintain
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    [Svn("$Id: Boats.xaml.cs 149 2010-08-24 22:09:28Z woakesdavid $")]
    public partial class Boats : Window
    {
        public Boats()
        {
            InitializeComponent();
            dSetGridSource = SetGridSource;
        }

        void Boats_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGrid();
        }

        private delegate void DSetGridSource(DataTable ppl);
        private DSetGridSource dSetGridSource;
        Working w;

        private void LoadGrid()
        {
            w = new Working(this);
            BoatData.ItemsSource = null;
            Task.Factory.StartNew(() =>
            {
                Db c = new Db("SELECT * " +
                    "FROM boats " +
                    "ORDER BY boatname");
                DataTable bts = c.GetData(null);
                c.Dispose();
                Dispatcher.Invoke(dSetGridSource, bts);
            });
        }

        private void SetGridSource(DataTable bts)
        {
            BoatData.ItemsSource = bts.DefaultView;
            if (Boatname.Text != string.Empty) FilterBoats();
            w.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            BoatEdit b = new BoatEdit(0);
            if (b.ShowDialog().Value)
            {
                LoadGrid();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (BoatData.SelectedItem != null)
            {
                DataRowView i = (DataRowView) BoatData.SelectedItem;
                BoatEdit b = new BoatEdit((int)i.Row["bid"]);
                if (b.ShowDialog().Value)
                {
                    LoadGrid();
                }
            }
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
                ((DataView)BoatData.ItemsSource).RowFilter =
                    "boatname LIKE '%" + Boatname.Text + "%'" +
                    "or sailno LIKE '%" + Boatname.Text + "%'" +
                    "or boatclass LIKE '%" + Boatname.Text + "%'";
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BoatData.SelectedItem != null)
            {
                bool change = false;
                foreach (DataRowView i in BoatData.SelectedItems)
                {
                    string name = i.Row["boatname"].ToString();
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?",
                        "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel) break;
                    if (result == MessageBoxResult.Yes)
                    {
                        Db del = new Db("DELETE FROM boats " +
                            "WHERE bid = @bid");
                        Hashtable d = new Hashtable();
                        d["bid"] = (int)i.Row["bid"];
                        del.ExecuteNonQuery(d);
                        change = true;
                    }
                }
                if (change) LoadGrid();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Boatname.Focus();
        }
    }
}
