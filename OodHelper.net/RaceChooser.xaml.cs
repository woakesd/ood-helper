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
                    Db d = new Db("SELECT rid, start_date, event, class " +
                        "FROM calendar " +
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
                "WHERE start_date <= @today");
            Hashtable p = new Hashtable();
            p["today"] = DateTime.Today;
            Object o;
            DateTime lr = (o = v.GetScalar(p)) == DBNull.Value ? DateTime.Today : (DateTime)o;

            foreach (DataRowView vr in CalGrid.Items)
            {
                DataRow r = vr.Row;
                if ((DateTime)r["start_date"] == lr)
                {
                    CalGrid.ScrollIntoView(vr);
                    CalGrid.SelectedItem = vr;
                    break;
                }
            }
            w.Close();
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
            DateTime rd = (DateTime) ((DataRowView) CalGrid.SelectedItem).Row["start_date"];
            TimeSpan st = rd.TimeOfDay;
            ArrayList res = new ArrayList();
            res.Add(r);

            if (st.ToString("hhmm") != "0000")
            {
                for (int i = rowIndex - 1; i >= 0; i--)
                {
                    //TimeSpan? t = Common.tspan(((DataRowView)CalGrid.Items[i]).Row["start"].ToString());
                    DateTime d = (DateTime)((DataRowView)CalGrid.Items[i]).Row["start_date"];
                    if (d != rd || (rd - d).TotalMinutes > 15)
                        break;
                    res.Add(((DataRowView)CalGrid.Items[i]).Row["rid"]);
                }
                for (int i = rowIndex + 1; i < CalGrid.Items.Count; i++)
                {
                    //TimeSpan? t = Common.tspan(((DataRowView)CalGrid.Items[i]).Row["start"].ToString());
                    DateTime d = (DateTime)((DataRowView)CalGrid.Items[i]).Row["start_date"];
                    if (d != rd || (d - rd).TotalMinutes > 15)
                        break;
                    res.Add(((DataRowView)CalGrid.Items[i]).Row["rid"]);
                }
            }
            res.Sort();
            rids = (int[]) res.ToArray(Type.GetType("System.Int32"));
            this.Close();
        }
    }
}
