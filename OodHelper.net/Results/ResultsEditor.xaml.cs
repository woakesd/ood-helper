using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
using OodHelper.Converters;

namespace OodHelper.Results
{
    /// <summary>
    /// Interaction logic for RaceEdit.xaml
    /// </summary>
    public partial class ResultsEditor : UserControl, IPrintSelectItem
    {
        private Db rddb;
        private DataTable RaceDataTable;
        private Hashtable caldata;

        public IRaceScore Scorer;

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
        public string Laps
        {
            get
            {
                return mLaps.ToString();
            }

            set
            {
                mLaps = ValueParsers.ReadInt(value);

                Db c = new Db(@"UPDATE calendar SET laps_completed = @laps WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["rid"] = Rid;
                p["laps"] = mLaps;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        private CalendarModel.RaceTypes _raceType;
        public CalendarModel.RaceTypes RaceType
        {
            get
            {
                return _raceType;
            }
            set
            {
                _raceType = value;

                Db c = new Db(@"UPDATE calendar SET racetype = @racetype WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["rid"] = Rid;
                p["racetype"] = _raceType.ToString();
                c.ExecuteNonQuery(p);
                c.Dispose();
                OnPropertyChanged("RaceType");
                OnPropertyChanged("StartReadOnly");
                OnPropertyChanged("StartTimeVisible");
                OnPropertyChanged("StartDateVisible");
                OnPropertyChanged("InterimReadOnly");
                OnPropertyChanged("InterimTimeVisible");
                OnPropertyChanged("InterimDateVisible");
                OnPropertyChanged("FinishReadOnly");
                OnPropertyChanged("FinishTimeVisible");
                OnPropertyChanged("FinishDateVisible");
                OnPropertyChanged("LapsEnabled");
                OnPropertyChanged("LapsReadOnly");
                OnPropertyChanged("LapsVisible");

                CalculateEnabled = true;
                OnPropertyChanged("CalculateEnabled");

                SetEditableColumns();
                CreateScorer();
                SetColumnAttributes();
            }
        }

        public bool LapsEnabled { get { return RaceType != CalendarModel.RaceTypes.AverageLap; } }

        public string RaceClass { get; set; }

        public string RaceName { get; set; }

        public DateTime? StartDate { get; set; }

        public string RaceStart
        {
            get { return StartTime.ToString("hh\\:mm"); }
        }

        public TimeSpan StartTime
        {
            get
            {
                return StartDate.Value.TimeOfDay;
            }

            set
            {
                if (value > TimeSpan.FromDays(1.0) || StartDate.Value.Date + value >= StartDate.Value.AddDays(1))
                    MessageBox.Show("You cannot set the start time to this value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
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
        }

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

        public bool CalculateEnabled { get; set; }
        public bool RefreshHandicapsEnabled { get; set; }

        private int rid;
        public int Rid { get { return rid; } }

        private string eventname;

        public ResultsEditor(int r)
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
                    if (value >= TimeSpan.FromDays(1.0) || time_limit_fixed.Value.Date + value >= time_limit_fixed.Value.Date.AddDays(1))
                        MessageBox.Show("You cannot set the start time to this value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
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
                }
                else if (time_limit_delta.HasValue)
                {
                    if (value >= TimeSpan.FromDays(1.0))
                        MessageBox.Show("You cannot set the start time to this value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
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

        DateTime LimitDate = DateTime.Now;

        public void LoadGrid()
        {
            Db c = new Db(@"SELECT start_date, time_limit_fixed, time_limit_delta, extension, 
                    event, class, racetype, handicapping, laps_completed,
                    standard_corrected_time, ood, course_choice, wind_speed, wind_direction
                    FROM calendar
                    WHERE rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            caldata = c.GetHashtable(p);

            StartDate = (DateTime) caldata["start_date"];
            
            time_limit_fixed = caldata["time_limit_fixed"] as DateTime?;
            time_limit_delta = caldata["time_limit_delta"] as int?;

            if (time_limit_fixed.HasValue)
                LimitDate = time_limit_fixed.Value;
            else if (time_limit_delta.HasValue)
                LimitDate = StartDate.Value.AddSeconds(time_limit_delta.Value);

            if (caldata["extension"] != DBNull.Value)
                extension = (int)caldata["extension"];
            
            raceName.Content = StartDate.Value.ToString("ddd dd MMM yyyy") + " (";
            switch (caldata["handicapping"].ToString())
            {
                case "r":
                    raceName.Content += "Rolling Handicap";
                    break;
                case "o":
                    raceName.Content += "Open Handicap";
                    break;
                default:
                    raceName.Content += "No handicapping method";
                    break;
            }
            raceName.Content += ")";

            eventname = caldata["event"].ToString().Trim();
            RaceName = eventname + " - " + caldata["class"].ToString().Trim();
            RaceClass = caldata["class"].ToString().Trim();
            Ood = caldata["ood"].ToString();
            if (caldata["handicapping"] != DBNull.Value)
                mHandicap = (string)caldata["handicapping"];
            double? dsct = caldata["standard_corrected_time"] as double?;
            if (dsct.HasValue)
                sct.Text = Common.HMS(dsct.Value);
            
            if (caldata["racetype"] != DBNull.Value)
            {
                if (!Enum.TryParse<CalendarModel.RaceTypes>(caldata["racetype"].ToString(), out _raceType))
                    _raceType = CalendarModel.RaceTypes.Undefined;
            }

            CalculateEnabled = false;
            c = new Db(@"SELECT result_calculated, MAX(last_edit) last_edit
                FROM calendar c LEFT JOIN races r ON c.rid = r.rid
                WHERE c.rid = @rid
                GROUP BY result_calculated, standard_corrected_time");
            Hashtable calc = c.GetHashtable(p);
            if (calc["result_calculated"] == DBNull.Value || calc["last_edit"] != DBNull.Value && (DateTime)calc["result_calculated"] <= (DateTime)calc["last_edit"])
                CalculateEnabled = true;
            OnPropertyChanged("CalculateEnabled");

            RefreshHandicapsEnabled = false;
            c = new Db(@"SELECT COUNT(1)
                FROM races r1 
                INNER JOIN races r2 ON r2.bid = r1.bid AND r2.rid <> r1.rid AND r2.start_date < r1.start_date AND r2.last_edit > r1.last_edit
                INNER JOIN races r3 ON r3.bid = r1.bid AND r3.rid <> r1.rid
                WHERE r1.rid = @rid
                GROUP BY r1.bid, r3.start_date, r3.new_rolling_handicap
                HAVING r3.start_date = MAX(r2.start_date)");
            int? updateable = c.GetScalar(p) as int?;
            if (updateable > 0)
                RefreshHandicapsEnabled = true;
            OnPropertyChanged("RefreshHandicapsEnabled");

            mCourse = caldata["course_choice"] as string;
            mWindSpeed = caldata["wind_speed"] as string;
            mWindDirection = caldata["wind_direction"] as string;
            mLaps = caldata["laps_completed"] as int?;

            CreateScorer();

            rddb = new Db("SELECT r.rid, r.bid, boatname, boatclass, sailno, r.start_date, " +
                    "r.finish_code, r.finish_date, r.interim_date, r.laps, r.override_points, r.elapsed, r.standard_corrected, r.corrected, r.place, " +
                    "r.points, r.open_handicap, r.rolling_handicap, r.achieved_handicap, " +
                    "r.new_rolling_handicap, r.handicap_status, r.c, r.a, r.performance_index " +
                    "FROM races r INNER JOIN boats ON boats.bid = r.bid " +
                    "WHERE r.rid = @rid " +
                    "ORDER BY place");
            RaceDataTable = rddb.GetData(p);
            RaceDataTable.RowChanged += new DataRowChangeEventHandler(rd_RowChanged);

            SetEditableColumns();

            Races.ItemsSource = (from DataRow r in RaceDataTable.Rows
                                          select new ResultModel(r, StartDate.Value, LimitDate)).ToList<ResultModel>();
            this.DataContext = this;
        }

        private void CreateScorer()
        {
            Scorer = null;
            switch (RaceType)
            {
                case CalendarModel.RaceTypes.AverageLap:
                case CalendarModel.RaceTypes.FixedLength:
                case CalendarModel.RaceTypes.TimeGate:
                case CalendarModel.RaceTypes.HybridOld:
                case CalendarModel.RaceTypes.Hybrid:
                    switch (Handicap)
                    {
                        case "r":
                            Scorer = new RollingHandicap();
                            break;
                        case "o":
                            Scorer = new OpenHandicap();
                            break;
                    }
                    break;
                case CalendarModel.RaceTypes.SternChase:
                    Scorer = new SternChaseScorer();
                    break;
            }
        }

        private void SetEditableColumns()
        {
            //
            // Set the columns which are to be editable as not being read only 
            // in the dataset.
            //
            foreach (DataColumn col in RaceDataTable.Columns)
            {
                col.ReadOnly = true;
            }

            //
            // Adjust updatable columns according to race type
            //
            switch (RaceType)
            {
                case CalendarModel.RaceTypes.AverageLap:
                    RaceDataTable.Columns["finish_date"].ReadOnly = false;
                    RaceDataTable.Columns["override_points"].ReadOnly = false;
                    RaceDataTable.Columns["finish_code"].ReadOnly = false;
                    RaceDataTable.Columns["laps"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.FixedLength:
                    RaceDataTable.Columns["finish_date"].ReadOnly = false;
                    RaceDataTable.Columns["override_points"].ReadOnly = false;
                    RaceDataTable.Columns["finish_code"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.HybridOld:
                case CalendarModel.RaceTypes.Hybrid:
                    RaceDataTable.Columns["finish_date"].ReadOnly = false;
                    RaceDataTable.Columns["override_points"].ReadOnly = false;
                    RaceDataTable.Columns["finish_code"].ReadOnly = false;
                    RaceDataTable.Columns["interim_date"].ReadOnly = false;
                    RaceDataTable.Columns["laps"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.TimeGate:
                    RaceDataTable.Columns["start_date"].ReadOnly = false;
                    RaceDataTable.Columns["finish_date"].ReadOnly = false;
                    RaceDataTable.Columns["override_points"].ReadOnly = false;
                    RaceDataTable.Columns["finish_code"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.SternChase:
                    RaceDataTable.Columns["place"].ReadOnly = false;
                    RaceDataTable.Columns["override_points"].ReadOnly = false;
                    RaceDataTable.Columns["finish_code"].ReadOnly = false;
                    break;
            }
        }

        public bool PlaceReadOnly
        {
            get {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.SternChase:
                        return false;
                }
                return true;
            }
        }

        public bool DisplayDate
        {
            //
            // If time limit date is not the same day as start date then we show
            // start and finish dates as well as time.
            //
            get
            {
                return StartDate.Value.Date != LimitDate.Date;
            }
        }

        //
        // Start only enterable for Stern Chase and Time Gate races.
        //
        public bool StartReadOnly
        {
            get
            {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.SternChase:
                    case CalendarModel.RaceTypes.TimeGate:
                        return false;
                }
                return true;
            }
        }

        public Visibility StartTimeVisible
        {
            get
            {
                if (!StartReadOnly)
                        return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility StartDateVisible
        {
            get
            {
                if (DisplayDate && !StartReadOnly)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool FinishReadOnly
        {
            get
            {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.SternChase:
                        return true;
                }
                return false;
            }
        }

        public Visibility FinishTimeVisible
        {
            get
            {
                if (!FinishReadOnly)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility FinishDateVisible
        {
            get
            {
                if (DisplayDate && !FinishReadOnly)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        //
        // Interim only writeable if race type is hybrid race
        //
        public bool InterimReadOnly
        {
            get
            {
                return RaceType != CalendarModel.RaceTypes.HybridOld && RaceType != CalendarModel.RaceTypes.Hybrid;
            }
        }

        public Visibility InterimTimeVisible
        {
            get
            {
                if (!InterimReadOnly)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility InterimDateVisible
        {
            get
            {
                if (DisplayDate && !InterimReadOnly)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        //
        // Laps entry for boats only visible for average lap and hybrid races
        //
        public bool LapsReadOnly
        {
            get
            {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.AverageLap:
                    case CalendarModel.RaceTypes.HybridOld:
                    case CalendarModel.RaceTypes.Hybrid:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public Visibility LapsVisible
        {
            get
            {
                if (!LapsReadOnly)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        public Visibility StdCorrectedVisible
        {
            get
            {
                if (RaceType != CalendarModel.RaceTypes.SternChase && Handicap == "o")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility CorrectedVisible
        {
            get
            {
                if (RaceType != CalendarModel.RaceTypes.SternChase && Handicap == "r")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility OpenHandicapVisible
        {
            get
            {
                if (Handicap == "o")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility RollingHandicapVisible
        {
            get
            {
                if (Handicap == "r")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
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
            CalculateEnabled = true;
            OnPropertyChanged("CalculateEnabled");
        }

        void SetColumnAttributes()
        {
            foreach (DataGridColumn c in Races.Columns)
            {
                Color _veryLightGray = new Color();
                _veryLightGray.A = 255;
                _veryLightGray.R = 224;
                _veryLightGray.G = 224;
                _veryLightGray.B = 224;
                SolidColorBrush _veryLightGrayBrush = new SolidColorBrush(_veryLightGray);
                //if (false && c.IsReadOnly)
                //{
                //    if (!c.IsSealed)
                //        c.CellStyle = new System.Windows.Style();
                //        c.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, _veryLightGrayBrush));
                //}
            }
        }

        //
        // Will hold the list found below so that if the user says yes to auto population we don'_task need to select
        // again.
        //
        private DataTable autoPopulateData = null;

        //
        // Bit of a misnomer, this actually gets a list of boats that have done at least one other race in
        // a series.
        //
        public int CountAutoPopulateData()
        {
            Db c = new Db("SELECT DISTINCT r.bid " +
                    "FROM calendar AS c1 " +
                    "INNER JOIN calendar_series_join AS cs1 ON c1.rid = cs1.rid " +
                    "INNER JOIN calendar_series_join AS cs2 ON cs2.sid = cs1.sid " +
                    "INNER JOIN calendar AS c2 ON c2.rid = cs2.rid AND c1.rid <> c2.rid AND c1.class = c2.class " +
                    "INNER JOIN races AS r ON r.rid = c2.rid " +
                    "WHERE c1.rid = @rid " +
                    "AND c1.event = c2.event");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            autoPopulateData = c.GetData(p);
            return autoPopulateData.Rows.Count;
        }

        //
        // Put the auto populate data into the races table for this race.
        //
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
            if (Scorer != null)
            {
                BackgroundWorker calc = new BackgroundWorker();
                calc.DoWork += new DoWorkEventHandler(Scorer.Calculate);
                Working w = new Working(App.Current.MainWindow, calc);
                calc.RunWorkerCompleted += new RunWorkerCompletedEventHandler(calc_RunWorkerCompleted);
                calc.RunWorkerAsync(rid);
                w.ShowDialog();
            }
        }

        void calc_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadGrid();
        }

        private void Notes_Click(object sender, RoutedEventArgs e)
        {
            RaceNotes rn = new RaceNotes(Rid);
            rn.ShowDialog();
        }

        /*
         * Refresh rolling handicaps for each boat entered in race from lastest previous race entry
         */
        private void buttonRefreshRolling_Click(object sender, RoutedEventArgs e)
        {
            Db c = new Db(@"SELECT r1.bid, r3.new_rolling_handicap
                FROM races r1 
                INNER JOIN races r2 ON r2.bid = r1.bid AND r2.rid <> r1.rid AND r2.start_date < r1.start_date
                INNER JOIN races r3 ON r3.bid = r1.bid AND r3.rid <> r1.rid
                WHERE r1.rid = @rid
                GROUP BY r1.bid, r3.start_date, r3.new_rolling_handicap
                HAVING r3.start_date = MAX(r2.start_date)");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            DataTable d = c.GetData(p);
            c = new Db(@"UPDATE races
                SET last_edit = GETDATE()
                , rolling_handicap = @rhp
                WHERE rid = @rid
                AND bid = @bid");
            foreach (DataRow r in d.Rows)
            {
                p["bid"] = r["bid"];
                p["rhp"] = r["new_rolling_handicap"];
                c.ExecuteNonQuery(p);
            }
            LoadGrid();
        }
    }
}
