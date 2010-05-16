﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for RaceEdit.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class RaceEdit : UserControl
    {
        private Db rddb;
        private DataTable rd;
        private Hashtable caldata;

        RaceScore scorer;

        private string raceclass = "";
        public string RaceClass
        {
            get { return raceclass; }
        }

        private string racename = "";
        public string RaceName
        {
            get { return racename; }
        }

        private DateTime mRaceDate;
        public DateTime RaceDate
        {
            get { return mRaceDate; }
        }

        private string mOod;
        public string Ood
        {
            get { return mOod; }
        }

        public string RaceStart
        {
            get { return start.Text; }
        }

        private int rid;
        public int Rid { get { return rid; } }

        private string eventname;

        public RaceEdit(int r)
        {
            InitializeComponent();

            rid = r;

            LoadGrid();

            Races.Loaded += new RoutedEventHandler(Races_Loaded);
        }

        private string mHandicap;
        public string Handicap
        {
            get
            {
                return mHandicap;
            }
        }

        private bool PreviousCompetitors(int rid)
        {
            return false;
            //Db cnt = new Db("SELECT COUNT(1) 
        }

        public void LoadGrid()
        {
            Db c = new Db("SELECT start, timelimit, extension, day, date, event, class, spec, hc, ood " +
                    "FROM calendar " +
                    "WHERE rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            caldata = c.GetHashtable(p);

            start.Text = caldata["start"].ToString();
            timeLimit.Text = caldata["timelimit"].ToString();
            extension.Text = caldata["extension"].ToString();
            raceName.Content = caldata["day"].ToString() + " " +
                ((DateTime)caldata["date"]).ToString("dd MMM yyyy") +
                " (" + ((caldata["hc"].ToString() == "r") ? "Rolling " : "Open ") + "handicap)";
            eventname = caldata["event"].ToString().Trim();
            racename = eventname + " - " + caldata["class"].ToString().Trim();
            raceclass = caldata["class"].ToString().Trim();
            mRaceDate = (DateTime)caldata["date"];
            mOod = caldata["ood"].ToString();
            mHandicap = (string)caldata["hc"];

            switch (Handicap)
            {
                case "r":
                    scorer = new RollingHandicap();
                    break;
                case "o":
                    scorer = new OpenHandicap();
                    break;
            }

            rddb = new Db("SELECT r.rid, r.bid, boatname, boatclass, sailno, r.start, " +
                    "r.fincode, r.fintime, r.laps, r.override_points, r.elapsed, r.standard_corrected, r.corrected, r.place, " +
                    "r.points, r.open_handicap, r.rolling_handicap, r.achieved_handicap, " +
                    "r.new_rolling_handicap, r.handicap_status, r.c, r.a, r.performance_index " +
                    "FROM races r INNER JOIN boats ON boats.bid = r.bid " +
                    "WHERE r.rid = @rid " +
                    "ORDER BY place");
            rd = rddb.GetData(p);

            //
            // Set the columns which are to be editable as not being read only 
            // in the dataset.
            //
            foreach (DataColumn col in rd.Columns)
            {
                col.ReadOnly = true;
            }
            if (caldata["spec"].ToString() == "t")
                rd.Columns["bstart"].ReadOnly = false;
            rd.Columns["fincode"].ReadOnly = false;
            rd.Columns["fintime"].ReadOnly = false;
            rd.Columns["laps"].ReadOnly = false;
            rd.Columns["override_points"].ReadOnly = false;

            Races.ItemsSource = rd.DefaultView;

            start.GotFocus += new RoutedEventHandler(start_GotFocus);
            start.LostFocus += new RoutedEventHandler(start_LostFocus);

            Races.AutoGeneratedColumns += new EventHandler(Races_AutoGeneratedColumns);
        }

        void Races_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGridTextColumn col = (DataGridTextColumn)Races.Columns[rd.Columns["elapsed"].Ordinal];
            Binding b = (Binding)col.Binding;
            b.Converter = new intTimeSpan();
            col.Binding = b;

            col = (DataGridTextColumn)Races.Columns[rd.Columns["standard_corrected"].Ordinal];
            b = (Binding)col.Binding;
            b.Converter = new dblTimeSpan();
            col.Binding = b;

            col = (DataGridTextColumn)Races.Columns[rd.Columns["corrected"].Ordinal];
            b = (Binding)col.Binding;
            b.Converter = new dblTimeSpan();
            col.Binding = b;

            Color x = new Color();
            x.A = 255;
            x.R = 240;
            x.B = 240;
            x.G = 240;
            SolidColorBrush vlg = new SolidColorBrush(x);
            foreach (DataGridColumn c in Races.Columns)
            {
                if (rd.Columns[(string) c.Header].ReadOnly)
                {
                    c.CellStyle = new System.Windows.Style();
                    c.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, vlg));
                    c.CellStyle.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Black));
                }
            }

            Races.PreparingCellForEdit += new EventHandler<DataGridPreparingCellForEditEventArgs>(Races_PreparingCellForEdit);
            Races.CellEditEnding += new EventHandler<DataGridCellEditEndingEventArgs>(Races_CellEditEnding);

            //Races.Columns[rd.Columns["rid"].Ordinal].Visibility = Visibility.Hidden;
        }

        private DataTable autoPopulateData = null;

        public int CountAutoPopulateData()
        {
            Db c = new Db("SELECT DISTINCT r.bid " +
                    "FROM calendar AS c1 " +
                    "INNER JOIN calendar_series_join AS cs1 ON c1.rid = cs1.rid " +
                    "INNER JOIN calendar_series_join AS cs2 ON cs2.sid = cs1.sid " +
                    "INNER JOIN calendar AS c2 ON c2.rid = cs2.rid AND c1.rid <> c2.rid AND c1.class = c2.class " +
                    "INNER JOIN races AS r ON r.rid = c2.rid " +
                    "WHERE c1.rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            autoPopulateData = c.GetData(p);
            return autoPopulateData.Rows.Count;
        }

        public void DoAutoPopulate()
        {
            Db add = new Db(@"INSERT INTO races
                    (rid, date, bid, rolling_handicap, handicap_status, start, open_handicap)
                    SELECT c.rid, c.date, b.bid, b.rolling_handicap, b.handicap_status, c.start + ':00', b.open_handicap
                    FROM boats b, calendar c
                    WHERE b.bid = @bid
                    AND c.rid = @rid");
            Hashtable a = new Hashtable();
            a["rid"] = Rid;
            foreach (DataRow r in autoPopulateData.Rows)
            {
                a["bid"] = r["bid"];
                add.ExecuteNonQuery(a);
            }
            LoadGrid();
        }

        private string preEdit;

        void Races_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            preEdit = ((TextBox)e.EditingElement).Text;
        }

        void Races_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Regex rxc = new Regex("^[a-z]{3}$", RegexOptions.IgnoreCase);
            Regex rxt = new Regex("^[0-9]{2}([: ][0-9]{2}){2}$");
            Regex rxl = new Regex("^[0-9]+$");
            switch (e.Column.Header.ToString())
            {
                case "fintime":
                    TextBox fintime = (TextBox)e.EditingElement;
                    if (rxt.IsMatch(fintime.Text))
                    {
                        Db u = new Db(@"UPDATE races
                                SET fintime = @fintime
                                WHERE rid = @rid
                                AND bid = @bid");
                        Hashtable p = new Hashtable();
                        p["fintime"] = fintime.Text.Replace(" ", ":");
                        fintime.Text = (string)p["fintime"];
                        p["bid"] = ((DataRowView)e.Row.Item).Row["bid"];
                        p["rid"] = Rid;
                        u.ExecuteNonQuery(p);
                    }
                    else if (rxc.IsMatch(fintime.Text))
                    {
                        Db u = new Db(@"UPDATE races
                                SET fintime = @fintime
                                , fincode = @fintime
                                WHERE rid = @rid
                                AND bid = @bid");
                        Hashtable p = new Hashtable();
                        p["fintime"] = fintime.Text.ToUpper();
                        fintime.Text = (string)p["fintime"];
                        p["bid"] = ((DataRowView)e.Row.Item).Row["bid"];
                        p["rid"] = Rid;
                        u.ExecuteNonQuery(p);
                    }
                    else
                    {
                        fintime.Text = preEdit;
                    }
                    break;
                case "fincode":
                    TextBox fincode = (TextBox)e.EditingElement;
                    if (rxc.IsMatch(fincode.Text))
                    {
                        Db u = new Db(@"UPDATE races
                                SET fincode = @fincode
                                WHERE rid = @rid
                                AND bid = @bid");
                        Hashtable p = new Hashtable();
                        p["fincode"] = fincode.Text.ToUpper();
                        fincode.Text = (string)p["fincode"];
                        p["bid"] = ((DataRowView)e.Row.Item).Row["bid"];
                        p["rid"] = Rid;
                        u.ExecuteNonQuery(p);
                    }
                    else
                    {
                        fincode.Text = preEdit;
                    }
                    break;
                case "laps":
                    TextBox laps = (TextBox)e.EditingElement;
                    if (rxl.IsMatch(laps.Text) && Int32.Parse(laps.Text) > 0)
                    {
                        Db u = new Db(@"UPDATE races
                                SET laps = @laps
                                WHERE rid = @rid
                                AND bid = @bid");
                        Hashtable p = new Hashtable();
                        p["laps"] = laps.Text;
                        p["bid"] = ((DataRowView)e.Row.Item).Row["bid"];
                        p["rid"] = Rid;
                        u.ExecuteNonQuery(p);
                    }
                    else
                    {
                        laps.Text = preEdit;
                    }
                    break;
            }
        }

        void start_LostFocus(object sender, RoutedEventArgs e)
        {
            Regex rx = new Regex("^[0-9][0-9][: ][0-9][0-9]$");
            if (rx.IsMatch(start.Text))
            {
                Db u = new Db(@"UPDATE races
                        SET start = @start
                        WHERE rid = @rid");
                Hashtable p = new Hashtable();
                start.Text = start.Text.Replace(' ', ':');
                p["start"] = start.Text + ":00";
                p["rid"] = Rid;
                u.ExecuteNonQuery(p);

                u = new Db(@"UPDATE calendar
                        SET start = @start
                        WHERE rid = @rid");
                p.Clear();
                p["start"] = start.Text;
                p["rid"] = Rid;
                u.ExecuteNonQuery(p);

                LoadGrid();
            }
            else
                start.Text = pcStart;
        }

        private string pcStart;

        void start_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        void start_GotFocus(object sender, RoutedEventArgs e)
        {
            pcStart = start.Text;
        }

        void Races_Loaded(object sender, RoutedEventArgs e)
        {
            //Races.cells;
        }

        private void buttonCalculate_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        public void Calculate()
        {
            if (scorer != null) scorer.Calculate(Rid);
            LoadGrid();
        }

        private class intTimeSpan : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value != DBNull.Value)
                {
                    int seconds = (Int32)value;
                    if (seconds < 999999)
                    {
                        TimeSpan s = new TimeSpan(0, 0, seconds);
                        return s.ToString();
                    }
                    return seconds.ToString();
                }
                else
                {
                    return "";
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string strValue = value as string;
                TimeSpan resultDateTime;
                if (TimeSpan.TryParse(strValue, out resultDateTime))
                {
                    return (int)resultDateTime.TotalSeconds;
                }
                return DependencyProperty.UnsetValue;
            }
        }

        private class dblTimeSpan : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value != DBNull.Value)
                {
                    double seconds = (double)value;
                    if (seconds < 999999)
                    {
                        TimeSpan s = new TimeSpan(0, 0, 0, (int)Math.Truncate(seconds),
                            (int)Math.Round((seconds - Math.Truncate(seconds)) * 1000));
                        return s.ToString().Replace("00000", "");
                    }
                    else
                        return seconds.ToString();
                }
                else
                {
                    return "";
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string strValue = value as string;
                TimeSpan resultDateTime;
                if (TimeSpan.TryParse(strValue, out resultDateTime))
                {
                    return (int)resultDateTime.TotalSeconds;
                }
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
