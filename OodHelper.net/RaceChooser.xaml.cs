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

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class RaceChooser : Window
    {
        private DataTable cal;

        public RaceChooser()
        {
            InitializeComponent();
            FilterRaces();
        }

        void CalGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGridTextColumn c = (DataGridTextColumn)CalGrid.Columns[cal.Columns["start_date"].Ordinal];
            c.Binding.StringFormat = "dd MMM yyyy HH:mm";
        }

        void RaceChooser_Loaded(object sender, RoutedEventArgs e)
        {
            Db v = new Db("SELECT MAX(start_date) " +
                "FROM calendar " +
                "WHERE is_race = 1 " +
                "AND start_date <= @today");
            Hashtable p = new Hashtable();
            p["today"] = DateTime.Today.AddDays(1.0);
            Object o;
            DateTime lr = (o = v.GetScalar(p)) == DBNull.Value ? DateTime.Today : ((DateTime)o).Date;
            DateSel.SelectedDate = lr;
        }

        private void SetGridSource()
        {
            CalGrid.ItemsSource = cal.DefaultView;
            if (cal.Rows.Count > 0)
                CalGrid.SelectedIndex = 0;
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
                Hashtable p = new Hashtable();
                StringBuilder sql = new StringBuilder(@"SELECT DISTINCT DATEPART(year, start_date), 
DATEPART(month, start_date), DATEPART(day, start_date)
FROM calendar
WHERE is_race = 1 ");
                if (Eventname.Text != "")
                {
                    p["event"] = String.Format("%{0}%", Eventname.Text);
                    sql.Append("AND event LIKE @event ");
                }
                sql.Append("ORDER BY DATEPART(year, start_date), DATEPART(month, start_date), DATEPART(day, start_date)");
                Db c = new Db(sql.ToString());
                DataTable d = c.GetData(p);
                CalendarDateRange dr = new CalendarDateRange();
                DateSel.BlackoutDates.Clear();
                if (d.Rows.Count > 0)
                {
                    int year, month, day;
                    year = (int)d.Rows[0][0];
                    month = (int)d.Rows[0][1];
                    day = (int)d.Rows[0][2];
                    DateSel.DisplayDateStart = new DateTime(year, month, day);

                    year = (int)d.Rows[d.Rows.Count - 1][0];
                    month = (int)d.Rows[d.Rows.Count - 1][1];
                    day = (int)d.Rows[d.Rows.Count - 1][2];
                    DateSel.DisplayDateEnd = new DateTime(year, month, day);

                    DateTime? lr = null;
                    for (int i = 0; i < d.Rows.Count - 1; i++)
                    {
                        CalendarDateRange r = new CalendarDateRange(
                            new DateTime((int)d.Rows[i][0], (int)d.Rows[i][1], (int)d.Rows[i][2]),
                            new DateTime((int)d.Rows[i + 1][0], (int)d.Rows[i + 1][1], (int)d.Rows[i + 1][2]));
                        if (lr == null && r.Start >= DateTime.Today) lr = r.Start;
                        if (r.End - r.Start > new TimeSpan(1, 0, 0, 0))
                        {
                            r.End = r.End.AddDays(-1);
                            r.Start = r.Start.AddDays(1);
                            DateSel.BlackoutDates.Add(r);
                        }
                    }
                    if (lr == null) lr = new DateTime((int)d.Rows[d.Rows.Count - 1][0],
                        (int)d.Rows[d.Rows.Count - 1][1],
                        (int)d.Rows[d.Rows.Count - 1][2]);
                    DateSel.SelectedDate = lr.Value;
                    if (DateSel.SelectedDate.HasValue)
                        DateSel.DisplayDate = DateSel.SelectedDate.Value;
                }
                
                ((DataView)CalGrid.ItemsSource).RowFilter =
                    "event LIKE '%" + Eventname.Text + "%'";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Eventname.Focus();
            if (e.Key == Key.W && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                setChosenRaces();
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
            if (CalGrid.SelectedItem == null) return;
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

        private void DateSel_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateSel.SelectedDate.HasValue)
            {
                Hashtable p = new Hashtable();
                Db d = new Db("SELECT rid, start_date, event, class, raced " +
                    "FROM calendar " +
                    "WHERE is_race = 1 " +
                    "AND start_date >= @startdate AND start_date < @enddate " +
                    "ORDER BY start_date");
                p["startdate"] = DateSel.SelectedDate.Value;
                p["enddate"] = DateSel.SelectedDate.Value.AddDays(1);
                cal = d.GetData(p);
                SetGridSource();
            }
        }

        private void buttonToday_Click(object sender, RoutedEventArgs e)
        {
            if (DateSel.DisplayDateStart >= DateTime.Today)
                DateSel.SelectedDate = DateSel.DisplayDateStart;
            else if (DateSel.DisplayDateEnd <= DateTime.Today)
                DateSel.SelectedDate = DateSel.DisplayDateEnd;
            else
            {
                DateTime sel = DateTime.Today;
                bool isSet = false;
                while (!isSet)
                {
                    try
                    {
                        DateSel.SelectedDate = sel;
                        isSet = true;
                    }
                    catch 
                    {
                        sel = sel.AddDays(-1);
                    }
                }
            }
            if (DateSel.SelectedDate.HasValue)
                DateSel.DisplayDate = DateSel.SelectedDate.Value;
        }
    }
}
