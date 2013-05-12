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
using OodHelper.Maintain.Models;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    public partial class Boats : Window
    {
        public Boats()
        {
            Owner = App.Current.MainWindow;
            InitializeComponent();
            Width = System.Windows.SystemParameters.VirtualScreenWidth * 0.8;
            Height = System.Windows.SystemParameters.VirtualScreenHeight * 0.8;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void LoadGrid()
        {
            DataTable bts;
            if (Boatname.Text.Trim() != string.Empty)
            {
                BoatData.ItemsSource = null;
                using (Db c = new Db(@"SELECT *
FROM boats
WHERE boatname LIKE @filter
or sailno LIKE @filter
or boatclass LIKE @filter
ORDER BY boatname"))
                {
                    Hashtable _para = new Hashtable();
                    _para["filter"] = string.Format("%{0}%", Boatname.Text);
                    bts = c.GetData(_para);
                };
                BoatData.ItemsSource = bts.DefaultView;
            }
            else
                BoatData.ItemsSource = null;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            BoatView b = new BoatView(0);
            if (b.ShowDialog().Value)
            {
                Boatname.Text = ((BoatModel)b.DataContext).BoatName;
                LoadGrid();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (BoatData.SelectedItem != null)
            {
                DataRowView i = (DataRowView) BoatData.SelectedItem;
                BoatView b = new BoatView((int)i.Row["bid"]);
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
            LoadGrid();
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
