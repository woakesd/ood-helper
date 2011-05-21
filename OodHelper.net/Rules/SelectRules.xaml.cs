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

namespace OodHelper.Rules
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class SelectRules : Window
    {
        public SelectRules()
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
            w = new Working();
            w.Show();
            BoatData.ItemsSource = null;
            Task.Factory.StartNew(() =>
            {
                Db c = new Db(@"SELECT id, name
                    FROM select_rules
                    WHERE parent IS NULL
                    ORDER BY name");
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
            SelectRuleEdit b = new SelectRuleEdit(null);
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
                SelectRuleEdit b = new SelectRuleEdit(i.Row["id"] as Guid?);
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
                    "name LIKE '%" + Boatname.Text + "%'";
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
                    string name = i.Row["name"].ToString();
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?",
                        "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel) break;
                    if (result == MessageBoxResult.Yes)
                    {
                        BoatSelectRule b = new BoatSelectRule(i.Row["id"] as Guid?);
                        b.Delete();
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
