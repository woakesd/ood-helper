﻿using System;
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
    /// Interaction logic for Window2.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class RaceChooser : Window
    {
        private DataTable cal;
        Working w;

        public RaceChooser()
        {
            InitializeComponent();
            /*System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = new string(' ', 4);
            settings.NewLineOnAttributes = true;
            StringBuilder strbuild = new StringBuilder();
            System.Xml.XmlWriter xmlwrite = System.Xml.XmlWriter.Create(strbuild, settings);
            System.Windows.Markup.XamlWriter.Save(DateSel.CalendarItemStyle, xmlwrite);*/
        }

        void CalGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGridTextColumn c = (DataGridTextColumn)CalGrid.Columns[cal.Columns["start_date"].Ordinal];
            c.Binding.StringFormat = "dd MMM yyyy HH:mm";
        }

        private delegate void DSetGridSource();
        private DSetGridSource dSetGridSource;

        void RaceChooser_Loaded(object sender, RoutedEventArgs e)
        {
            w = new Working(this);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Db d = new Db("SELECT rid, start_date, event, class, raced " +
                        "FROM calendar " +
                        "WHERE is_race = 1 " +
                        "ORDER BY start_date");
                    cal = d.GetData(null);
                    Dispatcher.Invoke(dSetGridSource = SetGridSource, null);
                });
        }

        private void SetGridSource()
        {
            CalGrid.ItemsSource = cal.DefaultView;

            Db v = new Db("SELECT MAX(start_date) " +
                "FROM calendar " +
                "WHERE is_race = 1 " +
                "AND start_date <= @today");
            Hashtable p = new Hashtable();
            p["today"] = DateTime.Today.AddDays(1.0);
            Object o;
            DateTime lr = (o = v.GetScalar(p)) == DBNull.Value ? DateTime.Today : ((DateTime)o).Date;

            foreach (DataRowView vr in CalGrid.Items)
            {
                DataRow r = vr.Row;
                if (((DateTime)r["start_date"]).Date == lr)
                {
                    CalGrid.ScrollIntoView(vr);
                    CalGrid.SelectedItem = vr;
                    break;
                }
            }
            w.Close();
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
                ((DataView)CalGrid.ItemsSource).RowFilter =
                    "event LIKE '%" + Eventname.Text + "%'";
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Eventname.Focus();
        }

        private void buttonResults_Click(object sender, RoutedEventArgs e)
        {
            setChosenRaces();
        }

        public int[] Rids
        {
            get
            {
                return rids;
            }
        }
        private int[] rids;

        private void setChosenRaces()
        {
            DialogResult = true;
            int rowIndex = CalGrid.SelectedIndex;
            int r = (int) ((DataRowView)CalGrid.SelectedItem).Row["rid"];
            DateTime rd = (DateTime) ((DataRowView) CalGrid.SelectedItem).Row["start_date"];
            TimeSpan st = rd.TimeOfDay;
            ArrayList res = new ArrayList();
            res.Add(r);

            if (st.ToString("hhmm") != "0000")
            {
                for (int i = rowIndex - 1; i >= 0; i--)
                {
                    DateTime d = (DateTime)((DataRowView)CalGrid.Items[i]).Row["start_date"];
                    if ((rd - d).TotalMinutes > 15)
                        break;
                    res.Add(((DataRowView)CalGrid.Items[i]).Row["rid"]);
                }
                for (int i = rowIndex + 1; i < CalGrid.Items.Count; i++)
                {
                    DateTime d = (DateTime)((DataRowView)CalGrid.Items[i]).Row["start_date"];
                    if ((d - rd).TotalMinutes > 15)
                        break;
                    res.Add(((DataRowView)CalGrid.Items[i]).Row["rid"]);
                }
            }
            res.Sort();
            rids = (int[]) res.ToArray(Type.GetType("System.Int32"));
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
