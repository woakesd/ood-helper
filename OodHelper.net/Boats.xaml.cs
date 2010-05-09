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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    [Svn("$Id: OodHelperWindow.xaml.cs 17588 2010-05-04 19:37:30Z david $")]
    public partial class Boats : Window
    {
        public Boats()
        {
            InitializeComponent();
            LoadGrid();

            Boatname.TextChanged += new TextChangedEventHandler(Boatname_TextChanged);
        }

        private void LoadGrid()
        {
            Db c = new Db("SELECT * FROM boats");
            DataTable bts = c.GetData(null);
            BoatData.ItemsSource = bts.DefaultView;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddBoat_Click(object sender, RoutedEventArgs e)
        {
            Boat b = new Boat(0);
            if (b.ShowDialog().Value)
            {
                LoadGrid();
            }
        }

        private void EditBoat_Click(object sender, RoutedEventArgs e)
        {
            if (BoatData.SelectedItem != null)
            {
                DataRowView i = (DataRowView) BoatData.SelectedItem;
                Boat b = new Boat((int)i.Row["bid"]);
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
    }
}
