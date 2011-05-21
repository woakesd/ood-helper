using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Series.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class Series : Window
    {
        public Series()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void LoadGrid()
        {
            Db c = new Db(@"SELECT *
                FROM series
                ORDER BY sname");
            DataTable rcs = c.GetData(null);
            c.Dispose();
            SeriesData.ItemsSource = rcs.DefaultView;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                FilterText.Focus();
        }

        System.Timers.Timer t = null;

        void FilterText_TextChanged(object sender, TextChangedEventArgs e)
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
                Dispatcher.Invoke(new dFilter(Filter), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilter();

        public void Filter()
        {
            try
            {
                ((DataView)SeriesData.ItemsSource).RowFilter =
                    "sname LIKE '%" + FilterText.Text + "%'";
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            SeriesEdit s = new SeriesEdit(0);
            if (s.ShowDialog() == true)
                LoadGrid();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rv = SeriesData.SelectedItem as DataRowView;
            if (rv != null)
            {
                int sid = (int)rv.Row["sid"];
                SeriesEdit s = new SeriesEdit(sid);
                if (s.ShowDialog() == true)
                    LoadGrid();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SeriesData.SelectedItem != null)
            {
                try
                {

                    bool change = false;
                    foreach (DataRowView i in SeriesData.SelectedItems)
                    {
                        string name = i.Row["sname"].ToString();
                        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?",
                            "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Cancel) break;
                        if (result == MessageBoxResult.Yes)
                        {
                            Db del = new Db("DELETE FROM series " +
                                "WHERE sid = @id");
                            Hashtable d = new Hashtable();
                            d["id"] = (int)i.Row["sid"];
                            del.ExecuteNonQuery(d);
                            del = new Db("DELETE FROM calendar_series_join " +
                                "WHERE sid = @id");
                            del.ExecuteNonQuery(d);
                            change = true;
                        }
                    }
                    if (change) LoadGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
