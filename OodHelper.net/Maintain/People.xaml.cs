﻿using System;
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

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    [Svn("$Id: People.xaml.cs 182 2010-09-21 21:12:06Z woakesdavid $")]
    public partial class People : Window, INotifyPropertyChanged
    {
        public People()
        {
            InitializeComponent();
            dSetGridSource = SetGridSource;
            SelectMode = false;
            DataContext = this;
        }

        public int? Id { get; private set; }

        private bool? _selectMode;
        public bool? SelectMode
        {
            get
            {
                return _selectMode;
            }
            private set
            {
                _selectMode = value;
                OnPropertyChanged("SelectMode");
            }
        }

        public People(int? id)
            : this()
        {
            Id = id;
        }

        public People(bool selectMode, int? id)
            : this(id)
        {
            SelectMode = selectMode;
        }

        private delegate void DSetGridSource(DataTable ppl);
        private DSetGridSource dSetGridSource;
        Working w;

        private void LoadGrid()
        {
            w = new Working();
            w.Show();
            PeopleData.ItemsSource = null;
            Task.Factory.StartNew(() =>
            {
                Db c = new Db("SELECT * " +
                    "FROM people " +
                    "ORDER BY surname, firstname");
                DataTable ppl = c.GetData(null);
                c.Dispose();
                Dispatcher.Invoke(dSetGridSource, ppl);
            });
        }

        private void SetGridSource(DataTable ppl)
        {
            PeopleData.ItemsSource = ppl.DefaultView;
            if (Peoplename.Text != string.Empty) FilterPeople();
            if (Id != null)
            {
                foreach (DataRowView vr in PeopleData.Items)
                {
                    DataRow r = vr.Row;
                    if (((int)r["id"]) == Id.Value)
                    {
                        PeopleData.SelectedItem = vr;
                        PeopleData.ScrollIntoView(vr);
                        break;
                    }
                }
            }
            w.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
                Id = (int)((DataRowView)PeopleData.SelectedItem).Row["id"];
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Person p = new Person(0);
            if (p.ShowDialog().Value)
            {
                LoadGrid();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                DataRowView i = (DataRowView) PeopleData.SelectedItem;
                if ((int)i.Row["id"] == (int)i.Row["main_id"])
                {
                    Person p = new Person((int)i.Row["id"]);
                    if (p.ShowDialog().Value)
                    {
                        LoadGrid();
                    }
                }
                else
                {
                    FamilyMember f = new FamilyMember((int)i.Row["id"], (int)i.Row["main_id"]);
                    if (f.ShowDialog().Value)
                    {
                        LoadGrid();
                    }
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                bool change = false;
                foreach (DataRowView i in PeopleData.SelectedItems)
                {
                    string name = i.Row["firstname"].ToString() + " " +
                        i.Row["surname"].ToString();
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?",
                        "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel) break;
                    if (result == MessageBoxResult.Yes)
                    {
                        Db del = new Db("DELETE FROM people " +
                            "WHERE id = @id");
                        Hashtable d = new Hashtable();
                        d["id"] = (int)i.Row["id"];
                        del.ExecuteNonQuery(d);
                        change = true;
                    }
                }
                if (change) LoadGrid();
            }
        }

        System.Timers.Timer t = null;

        void Peoplename_TextChanged(object sender, TextChangedEventArgs e)
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
                if (Peoplename.Text != string.Empty)
                {
                    ((DataView)PeopleData.ItemsSource).RowFilter =
                        "firstname LIKE '%" + Peoplename.Text + "%'" +
                        " or surname LIKE '%" + Peoplename.Text + "%'" +
                        " or address1 LIKE '%" + Peoplename.Text + "%'" +
                        " or address2 LIKE '%" + Peoplename.Text + "%'" +
                        " or address3 LIKE '%" + Peoplename.Text + "%'" +
                        " or address4 LIKE '%" + Peoplename.Text + "%'" +
                        " or postcode LIKE '%" + Peoplename.Text + "%'" +
                        " or hometel LIKE '%" + Peoplename.Text + "%'" +
                        " or worktel LIKE '%" + Peoplename.Text + "%'" +
                        " or mobile LIKE '%" + Peoplename.Text + "%'" +
                        " or email LIKE '%" + Peoplename.Text + "%'" +
                        " or club LIKE '%" + Peoplename.Text + "%'" +
                        " or member LIKE '%" + Peoplename.Text + "%'";
                }
                else
                {
                    ((DataView)PeopleData.ItemsSource).RowFilter = null;
                }
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                DataRowView i = (DataRowView)PeopleData.SelectedItem;
                if ((int)i.Row["id"] == (int)i.Row["main_id"])
                    AddFamilyMember.IsEnabled = true;
                else
                    AddFamilyMember.IsEnabled = false;
            }
        }

        private void AddFamilyMember_Click(object sender, RoutedEventArgs e)
        {
            DataRowView i = (DataRowView) PeopleData.SelectedItem;
            if ((int)i.Row["id"] == (int)i.Row["main_id"] && (string)i.Row["member"] == "Family")
            {
                FamilyMember f = new FamilyMember(0, (int)i.Row["main_id"]);
                if (f.ShowDialog().Value)
                    LoadGrid();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGrid();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Peoplename.Focus();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rv = PeopleData.SelectedItem as DataRowView;
            if (rv != null)
            {
                Id = rv.Row["id"] as int?;
                Close();
            }
            else
            {
                MessageBox.Show("You must select a person first", "Please select", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}