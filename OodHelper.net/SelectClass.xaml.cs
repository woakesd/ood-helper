using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    public partial class SelectClass : Window
    {
        public SelectClass()
        {
            InitializeComponent();
            dSetGridSource = SetGridSource;
        }

        public Guid? Id { get; private set; }

        private delegate void DSetGridSource(DataTable ppl);
        private DSetGridSource dSetGridSource;
        Working w;

        private void LoadGrid()
        {
            w = new Working(this);
            w.Show();
            ClassData.ItemsSource = null;
            Task.Factory.StartNew(() =>
            {
                Db c = new Db("SELECT * " +
                    "FROM portsmouth_numbers " +
                    "ORDER BY class_name");
                DataTable cls = c.GetData(null);
                c.Dispose();
                Dispatcher.Invoke(dSetGridSource, cls);
            });
        }

        private void SetGridSource(DataTable cls)
        {
            ClassData.ItemsSource = cls.DefaultView;
            if (Classname.Text != string.Empty) FilterPeople();
            if (Id != null)
            {
                foreach (DataRowView vr in ClassData.Items)
                {
                    DataRow r = vr.Row;
                    if (((Guid)r["id"]) == Id.Value)
                    {
                        ClassData.SelectedItem = vr;
                        ClassData.ScrollIntoView(vr);
                        break;
                    }
                }
            }
            w.Close();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (ClassData.SelectedItem != null)
            {
                Id = ((DataRowView)ClassData.SelectedItem).Row["id"] as Guid?;
                this.DialogResult = true;
            }
            else
                this.DialogResult = false;

            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Id = null;
            this.DialogResult = false;
            Close();
        }

        System.Timers.Timer t = null;

        void Classname_TextChanged(object sender, TextChangedEventArgs e)
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
                Dispatcher.Invoke(new dFilterPeople(FilterPeople), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilterPeople();

        public void FilterPeople()
        {
            try
            {
                if (Classname.Text != string.Empty)
                {
                    ((DataView)ClassData.ItemsSource).RowFilter =
                        "class_name LIKE '%" + Classname.Text + "%'";
                }
                else
                {
                    ((DataView)ClassData.ItemsSource).RowFilter = null;
                }
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGrid();
        }
    }
}
