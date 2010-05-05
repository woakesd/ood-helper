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
    [Svn("$Id: RaceChooser.xaml.cs 17583 2010-05-02 17:23:57Z david $")]
    public partial class RaceChooser : Window
    {
        private DataTable cal;

        public RaceChooser()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(RaceChooser_Loaded);

            Db d = new Db("SELECT rid, date, start, event, class, timelimit, extension " +
                "FROM calendar WHERE computer IN (1,2)");
            cal = d.GetData(null);

            CalGrid.ItemsSource = cal.DefaultView;
            CalGrid.MouseDoubleClick += new MouseButtonEventHandler(cal_MouseDoubleClick);
            CalGrid.AutoGeneratedColumns += new EventHandler(CalGrid_AutoGeneratedColumns);
        }

        void CalGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGridTextColumn c = (DataGridTextColumn)CalGrid.Columns[cal.Columns["date"].Ordinal];
            c.Binding.StringFormat = "dd MMM yyyy";
        }

        void RaceChooser_Loaded(object sender, RoutedEventArgs e)
        {
            Db v = new Db("SELECT MAX(date) " +
                "FROM calendar " +
                "WHERE computer IN (1,2) " +
                "AND date <= @today");
            Hashtable p = new Hashtable();
            p["today"] = DateTime.Today;
            Object o;
            DateTime lr = (o = v.GetScalar(p)) == DBNull.Value ? DateTime.Today: (DateTime) o;

            foreach (DataRowView vr in CalGrid.Items)
            {
                DataRow r = vr.Row;
                Object d = r["date"];
                if ((DateTime) r["date"] == lr)
                {
                    CalGrid.ScrollIntoView(vr);
                    CalGrid.SelectedItem = vr;
                    break;
                }
            }
        }

        void cal_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
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
            int rowIndex = CalGrid.SelectedIndex;
            int r = (int) ((DataRowView)CalGrid.SelectedItem).Row["rid"];
            TimeSpan? st = Common.tspan(((DataRowView)CalGrid.SelectedItem).Row["start"].ToString());
            DateTime rd = (DateTime) ((DataRowView) CalGrid.SelectedItem).Row["date"];
            ArrayList res = new ArrayList();
            res.Add(r);

            for (int i = rowIndex-1; i >= 0; i--)
            {
                TimeSpan? t = Common.tspan(((DataRowView)CalGrid.Items[i]).Row["start"].ToString());
                DateTime d = (DateTime) ((DataRowView)CalGrid.Items[i]).Row["date"];
                if (d != rd || ((TimeSpan)(st - t)).TotalMinutes > 15)
                    break;
                res.Add(((DataRowView)CalGrid.Items[i]).Row["rid"]);
            }
            for (int i = rowIndex+1; i < CalGrid.Items.Count; i++)
            {
                TimeSpan? t = Common.tspan(((DataRowView)CalGrid.Items[i]).Row["start"].ToString());
                DateTime d = (DateTime) ((DataRowView)CalGrid.Items[i]).Row["date"];
                if (d != rd || ((TimeSpan)(t - st)).TotalMinutes > 15)
                    break;
                res.Add(((DataRowView)CalGrid.Items[i]).Row["rid"]);
            }
            res.Sort();
            rids = (int[]) res.ToArray(Type.GetType("System.Int32"));
            this.Close();
        }
    }
}
