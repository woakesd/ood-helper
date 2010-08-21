using System;
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
    public partial class RaceEdit : UserControl, IPrintSelectItem
    {
        private Db rddb;
        private DataTable rd;
        private Hashtable caldata;

        public IRaceScore scorer;

        private string mCourse;
        public string Course
        {
            get
            {
                return mCourse;
            }

            set
            {
                mCourse = value;
                Db c = new Db(@"UPDATE calendar SET course_choice = @course WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["rid"] = Rid;
                p["course"] = mCourse;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        private string mWindSpeed;
        public string WindSpeed
        {
            get
            {
                return mWindSpeed;
            }

            set
            {
                mWindSpeed = value;
                Db c = new Db(@"UPDATE calendar SET wind_speed = @wind_speed WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["rid"] = Rid;
                p["wind_speed"] = mWindSpeed;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        private string mWindDirection;
        public string WindDirection
        {
            get
            {
                return mWindDirection;
            }

            set
            {
                mWindDirection = value;
                Db c = new Db(@"UPDATE calendar SET wind_direction = @wind_direction WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["rid"] = Rid;
                p["wind_direction"] = mWindDirection;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        private int? mLaps;
        public int? Laps
        {
            get
            {
                return mLaps;
            }

            set
            {
                //if (value != DBNull.Value)
                mLaps = value;
                Db c = new Db(@"UPDATE calendar SET laps_completed = @laps WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["rid"] = Rid;
                p["laps"] = mLaps;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        public string RaceClass { get; set; }

        public string RaceName { get; set; }

        public DateTime? StartDate { get; set; }

        public string Ood { get; set; }

        public bool PrintIncludeAllVisible { get; set; }
        public bool PrintIncludeAll { get; set; }
        public bool PrintInclude { get; set; }
        public int PrintIncludeCopies { get; set; }
        public string PrintIncludeDescription
        {
            get
            {
                return RaceName;
            }

            set
            {
            }
        }
        public int PrintIncludeGroup { get; set; }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }

        public string RaceStart
        {
            get { return StartTime.ToString("hh\\:mm"); }
        }

        private int rid;
        public int Rid { get { return rid; } }

        private string eventname;

        public RaceEdit(int r)
        {
            InitializeComponent();

            PrintIncludeCopies = 1;
            PrintIncludeGroup = 0;
            rid = r;
            LoadGrid();
            SetColumnAttributes();
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
        }

        public TimeSpan StartTime
        {
            get
            {
                return StartDate.Value.TimeOfDay;
            }
            set
            {
                if (StartDate.Value.TimeOfDay != value)
                {
                    StartDate = StartDate.Value.Date + value;
                    Db u = new Db(@"UPDATE races
                        SET start_date = @start_date
                        , last_edit = GETDATE()
                        WHERE rid = @rid");
                    Hashtable p = new Hashtable();
                    p["start_date"] = StartDate;
                    p["rid"] = Rid;
                    u.ExecuteNonQuery(p);

                    u = new Db(@"UPDATE calendar
                        SET start_date = @start_date
                        WHERE rid = @rid");
                    u.ExecuteNonQuery(p);

                    LoadGrid();
                }
            }
        }

        private DateTime? time_limit_fixed;
        private int? time_limit_delta;
        public TimeSpan? TimeLimit
        {
            get
            {
                if (time_limit_fixed.HasValue)
                    return time_limit_fixed.Value.TimeOfDay;
                else if (time_limit_delta.HasValue)
                    return new TimeSpan(0, 0, time_limit_delta.Value);
                else
                    return null;
            }

            set
            {
                if (time_limit_fixed.HasValue)
                {
                    time_limit_fixed = time_limit_fixed.Value.Date + value;
                    Db u = new Db(@"UPDATE calendar
                        SET time_limit_fixed = @time_limit_fixed
                        WHERE rid = @rid");
                    Hashtable p = new Hashtable();
                    p["time_limit_fixed"] = time_limit_fixed;
                    p["rid"] = Rid;
                    u.ExecuteNonQuery(p);
                }
                else if (time_limit_delta.HasValue)
                {
                    time_limit_delta = (int)value.Value.TotalSeconds;
                    Db u = new Db(@"UPDATE calendar
                        SET time_limit_delta = @time_limit_delta
                        WHERE rid = @rid");
                    Hashtable p = new Hashtable();
                    p["time_limit_delta"] = time_limit_delta;
                    p["rid"] = Rid;
                    u.ExecuteNonQuery(p);
                }
            }
        }

        private int extension;

        public TimeSpan Extension
        {
            get { return new TimeSpan(0, 0, extension); }
            set {
                extension = (int)value.TotalSeconds;
                Db u = new Db(@"UPDATE calendar
                        SET extension = @extension
                        WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["extension"] = extension;
                p["rid"] = Rid;
                u.ExecuteNonQuery(p);
            }
        }

        public void LoadGrid()
        {
            Db c = new Db(@"SELECT start_date, time_limit_fixed, time_limit_delta, extension, 
                    event, class, timegate, average_lap, handicapping, laps_completed,
                    standard_corrected_time, ood, course_choice, wind_speed, wind_direction
                    FROM calendar
                    WHERE rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            caldata = c.GetHashtable(p);

            StartDate = (caldata["start_date"] as DateTime?).Value;
            
            time_limit_fixed = caldata["time_limit_fixed"] as DateTime?;
            time_limit_delta = caldata["time_limit_delta"] as int?;

            if (caldata["extension"] != DBNull.Value)
                extension = (int)caldata["extension"];
            raceName.Content = StartDate.Value.ToString("ddd dd MMM yyyy") +
                " (" + ((caldata["handicapping"].ToString() == "r") ? "Rolling " : "Open ") + "handicap)";
            eventname = caldata["event"].ToString().Trim();
            RaceName = eventname + " - " + caldata["class"].ToString().Trim();
            RaceClass = caldata["class"].ToString().Trim();
            Ood = caldata["ood"].ToString();
            if (caldata["handicapping"] != DBNull.Value)
                mHandicap = (string)caldata["handicapping"];
            sct.Text = Common.HMS((double)caldata["standard_corrected_time"]);

            mCourse = caldata["course_choice"] as string;
            mWindSpeed = caldata["wind_speed"] as string;
            mWindDirection = caldata["wind_direction"] as string;
            mLaps = caldata["laps_completed"] as int?;

            if (scorer == null)
            {
                switch (Handicap)
                {
                    case "r":
                        scorer = new RollingHandicap();
                        break;
                    case "o":
                        scorer = new OpenHandicap();
                        break;
                }
            }

            rddb = new Db("SELECT r.rid, r.bid, boatname, boatclass, sailno, r.start_date, " +
                    "r.finish_code, r.finish_date, r.laps, r.override_points, r.elapsed, r.standard_corrected, r.corrected, r.place, " +
                    "r.points, r.open_handicap, r.rolling_handicap, r.achieved_handicap, " +
                    "r.new_rolling_handicap, r.handicap_status, r.c, r.a, r.performance_index " +
                    "FROM races r INNER JOIN boats ON boats.bid = r.bid " +
                    "WHERE r.rid = @rid " +
                    "ORDER BY place");
            rd = rddb.GetData(p);
            rd.RowChanged += new DataRowChangeEventHandler(rd_RowChanged);

            //
            // Set the columns which are to be editable as not being read only 
            // in the dataset.
            //
            foreach (DataColumn col in rd.Columns)
            {
                col.ReadOnly = true;
            }
            if ((bool)caldata["timegate"])
                rd.Columns["start_date"].ReadOnly = false;
            rd.Columns["finish_code"].ReadOnly = false;
            rd.Columns["finish_date"].ReadOnly = false;
            if ((bool)caldata["average_lap"])
                rd.Columns["laps"].ReadOnly = false;
            rd.Columns["override_points"].ReadOnly = false;

            Races.ItemsSource = rd.DefaultView;
            this.DataContext = this;
        }

        void rd_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            Hashtable p = new Hashtable();
            p["rid"] = e.Row["rid"];
            p["bid"] = e.Row["bid"];
            StringBuilder sql = new StringBuilder("UPDATE races SET last_edit = GETDATE()");
            foreach (DataColumn c in e.Row.Table.Columns)
            {
                if (!c.ReadOnly)
                {
                    p[c.ColumnName] = e.Row[c.ColumnName];
                        sql.AppendFormat(",{0} = @{0}", c.ColumnName);
                }
            }
            sql.Append(" WHERE rid = @rid AND bid = @bid");
            Db d = new Db(sql.ToString());
            d.ExecuteNonQuery(p);
        }

        void SetColumnAttributes()
        {
            Binding b;
            DataGridTextColumn col;

            if (time_limit_fixed.HasValue && StartDate.Value.Date < time_limit_fixed.Value.Date
                || time_limit_delta.HasValue && StartDate.Value.Date < (StartDate.Value.AddSeconds((double)time_limit_delta) + Extension).Date)
            {
                DateTime defaultFinish;
                if (time_limit_fixed.HasValue)
                    defaultFinish = time_limit_fixed.Value;
                else
                    defaultFinish = StartDate.Value.AddSeconds((double)time_limit_delta) + Extension;

                col = (DataGridTextColumn)Races.Columns[rd.Columns["start_date"].Ordinal];
                b = (Binding)col.Binding;
                b.Converter = new MyDateTimeConverter(StartDate.Value.Date);
                col.Binding = b;

                col = (DataGridTextColumn)Races.Columns[rd.Columns["finish_date"].Ordinal];
                b = (Binding)col.Binding;
                b.Converter = new MyDateTimeConverter(defaultFinish.Date);
                col.Binding = b;
            }
            else
            {
                col = (DataGridTextColumn)Races.Columns[rd.Columns["start_date"].Ordinal];
                b = (Binding)col.Binding;
                b.Converter = new DateTimeTimeConverter(StartDate.Value.Date);
                col.Binding = b;

                col = (DataGridTextColumn)Races.Columns[rd.Columns["finish_date"].Ordinal];
                b = (Binding)col.Binding;
                b.Converter = new DateTimeTimeConverter(StartDate.Value.Date);
                col.Binding = b;
            }

            foreach (DataGridColumn gc in Races.Columns)
                gc.IsReadOnly = rd.Columns[gc.SortMemberPath].ReadOnly;

            Color x = new Color();
            x.A = 255;
            x.R = 224;
            x.B = 224;
            x.G = 224;
            SolidColorBrush vlg = new SolidColorBrush(x);
            foreach (DataGridColumn c in Races.Columns)
            {
                if (c.IsReadOnly)
                {
                    c.CellStyle = new System.Windows.Style();
                    c.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, vlg));
                    c.CellStyle.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Black));
                }
            }
        }

        private void DataGridCell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridCell cell = sender as DataGridCell;
                if (cell != null)
                {
                    TextBox c = cell.Content as TextBox;
                    if (c == null || ((e.Key != Key.Right || c.SelectionStart != 0 || c.Text.Length == 0)
                        && (e.Key != Key.Left && e.Key != Key.Right || c.SelectionStart == 0 || c.SelectionStart + c.SelectionLength >= c.Text.Length)
                        && (e.Key != Key.Left || c.SelectionStart + c.SelectionLength < c.Text.Length || c.Text.Length == 0)))
                    {
                        Races.CommitEdit();
                        cell.IsEditing = false;
                    }
                }
                e.Handled = false;
            }
        }

        void DataGridCell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridCell cell = sender as DataGridCell;
                if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
                {
                    cell.Focus();
                    Races.BeginEdit();
                }
            }
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                    cell.IsSelected = true;
                    Races.BeginEdit();
                }
                /*DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }*/
            }
        }

        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
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
                    (rid, start_date, bid, rolling_handicap, handicap_status, open_handicap, last_edit)
                    SELECT c.rid, c.start_date, b.bid, b.rolling_handicap, b.handicap_status, b.open_handicap, GETDATE()
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

        private void buttonCalculate_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        public void Calculate()
        {
            if (scorer != null) scorer.Calculate(Rid);
            LoadGrid();
        }

        private void Notes_Click(object sender, RoutedEventArgs e)
        {
            RaceNotes rn = new RaceNotes(Rid);
            rn.ShowDialog();
        }
    }
}
