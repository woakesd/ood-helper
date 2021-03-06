﻿using System;
using System.Collections.Generic;
using System.Collections;
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
    /// Interaction logic for Handicaps.xaml
    /// </summary>
    public partial class Handicaps : Window
    {
        public Handicaps()
        {
            InitializeComponent();
            dSetGridSource = SetGridSource;
        }

        private delegate void DSetGridSource(DataTable rcs);
        private DSetGridSource dSetGridSource;
        Working w;
        Guid redit = Guid.Empty;

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
                DataTable rcs = c.GetData(null);
                c.Dispose();
                Dispatcher.Invoke(dSetGridSource, rcs);
            });
        }

        private void SetGridSource(DataTable rcs)
        {
            ClassData.ItemsSource = rcs.DefaultView;
            if (ClassName.Text != string.Empty) FilterClasses();
            if (redit != Guid.Empty)
            {
                foreach (DataRowView vr in ClassData.Items)
                {
                    DataRow r = vr.Row;
                    if ((Guid)r["id"] == redit)
                    {
                        ClassData.ScrollIntoView(vr);
                        ClassData.SelectedItem = vr;
                        break;
                    }
                }
            }
            w.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGrid();
        }

        System.Timers.Timer t = null;

        void ClassName_TextChanged(object sender, TextChangedEventArgs e)
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
                Dispatcher.Invoke(new dFilterRaces(FilterClasses), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilterRaces();

        public void FilterClasses()
        {
            try
            {
                ((DataView)ClassData.ItemsSource).RowFilter =
                    "class_name LIKE '%" + ClassName.Text + "%'";
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Handicap h = new Handicap(Guid.Empty);
            if (h.ShowDialog().Value)
            {
                redit = h.Id;
                LoadGrid();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ClassData.SelectedItem != null)
            {
                DataRowView i = (DataRowView)ClassData.SelectedItem;
                Handicap h = new Handicap((Guid)i.Row["id"]);
                if (h.ShowDialog().Value)
                {
                    redit = h.Id;
                    LoadGrid();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ClassData.SelectedItem != null)
            {
                bool change = false;
                foreach (DataRowView i in ClassData.SelectedItems)
                {
                    string name = i.Row["class_name"].ToString();
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?",
                        "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel) break;
                    if (result == MessageBoxResult.Yes)
                    {
                        Db del = new Db("DELETE FROM portsmouth_numbers " +
                            "WHERE id = @id");
                        Hashtable d = new Hashtable();
                        d["id"] = i.Row["id"];
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
                ClassName.Focus();
        }
    }
}
