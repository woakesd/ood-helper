using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using OodHelper.Converters;

namespace OodHelper.Results
{
    /// <summary>
    ///     Interaction logic for RaceEdit.xaml
    /// </summary>
    public partial class ResultsEditor : IPrintSelectItem
    {
        private readonly int _rid;
        private DateTime _limitDate = DateTime.Now;
        private DataTable _raceDataTable;

        public IRaceScore Scorer;
        private CalendarModel.RaceTypes _raceType;
        private DataTable _autoPopulateData;
        private Hashtable _caldata;
        private string _eventname;
        private int _extension;

        private string _course;
        private string _handicap;
        private int? _laps;
        private string _windDirection;

        private string _windSpeed;
        private Db _rddb;
        private int? _timeLimitDelta;
        private DateTime? _timeLimitFixed;

        public ResultsEditor(int r)
        {
            InitializeComponent();

            PrintIncludeCopies = 1;
            PrintIncludeGroup = 0;
            _rid = r;
            LoadGrid();
            SetColumnAttributes();
        }

        public string Course
        {
            get { return _course; }

            set
            {
                _course = value;
                var c = new Db(@"UPDATE calendar SET course_choice = @course WHERE rid = @rid");
                var p = new Hashtable();
                p["rid"] = Rid;
                p["course"] = _course;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        public string WindSpeed
        {
            get { return _windSpeed; }

            set
            {
                _windSpeed = value;
                var c = new Db(@"UPDATE calendar SET wind_speed = @wind_speed WHERE rid = @rid");
                var p = new Hashtable();
                p["rid"] = Rid;
                p["wind_speed"] = _windSpeed;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        public string WindDirection
        {
            get { return _windDirection; }

            set
            {
                _windDirection = value;
                var c = new Db(@"UPDATE calendar SET wind_direction = @wind_direction WHERE rid = @rid");
                var p = new Hashtable();
                p["rid"] = Rid;
                p["wind_direction"] = _windDirection;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        public string Laps
        {
            get { return _laps.ToString(); }

            set
            {
                _laps = ValueParsers.ReadInt(value);

                var c = new Db(@"UPDATE calendar SET laps_completed = @laps WHERE rid = @rid");
                var p = new Hashtable();
                p["rid"] = Rid;
                p["laps"] = _laps;
                c.ExecuteNonQuery(p);
                c.Dispose();
            }
        }

        public CalendarModel.RaceTypes RaceType
        {
            get { return _raceType; }
            set
            {
                _raceType = value;

                var c = new Db(@"UPDATE calendar SET racetype = @racetype WHERE rid = @rid");
                var p = new Hashtable();
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

        public bool LapsEnabled
        {
            get { return RaceType != CalendarModel.RaceTypes.AverageLap; }
        }

        public string RaceClass { get; set; }

        public string RaceName { get; set; }

        public DateTime StartDate { get; set; }

        public string RaceStart
        {
            get { return StartTime.ToString("hh\\:mm"); }
        }

        public TimeSpan StartTime
        {
            get { return StartDate.TimeOfDay; }

            set
            {
                if (value > TimeSpan.FromDays(1.0) || StartDate.Date + value >= StartDate.AddDays(1))
                    MessageBox.Show("You cannot set the start time to this value", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                else
                {
                    if (StartDate.TimeOfDay != value)
                    {
                        StartDate = StartDate.Date + value;
                        var u = new Db(@"UPDATE races
                        SET start_date = @start_date
                        , last_edit = GETDATE()
                        WHERE rid = @rid");
                        var p = new Hashtable();
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

        public bool CalculateEnabled { get; set; }
        public bool RefreshHandicapsEnabled { get; set; }

        public int Rid
        {
            get { return _rid; }
        }

        public string Handicap
        {
            get { return _handicap; }
        }

        public TimeSpan? TimeLimit
        {
            get
            {
                if (_timeLimitFixed.HasValue)
                    return _timeLimitFixed.Value.TimeOfDay;
                if (_timeLimitDelta.HasValue)
                    return new TimeSpan(0, 0, _timeLimitDelta.Value);
                return null;
            }

            set
            {
                if (_timeLimitFixed.HasValue)
                {
                    if (value >= TimeSpan.FromDays(1.0) ||
                        _timeLimitFixed.Value.Date + value >= _timeLimitFixed.Value.Date.AddDays(1))
                        MessageBox.Show("You cannot set the start time to this value", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    else
                    {
                        _timeLimitFixed = _timeLimitFixed.Value.Date + value;
                        var u = new Db(@"UPDATE calendar
                        SET time_limit_fixed = @_timeLimitFixed
                        WHERE rid = @rid");
                        var p = new Hashtable();
                        p["_timeLimitFixed"] = _timeLimitFixed;
                        p["rid"] = Rid;
                        u.ExecuteNonQuery(p);
                    }
                }
                else if (_timeLimitDelta.HasValue)
                {
                    if (value >= TimeSpan.FromDays(1.0))
                        MessageBox.Show("You cannot set the start time to this value", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    else
                    {
                        _timeLimitDelta = (int) value.Value.TotalSeconds;
                        var u = new Db(@"UPDATE calendar
                        SET time_limit_delta = @_timeLimitDelta
                        WHERE rid = @rid");
                        var p = new Hashtable();
                        p["_timeLimitDelta"] = _timeLimitDelta;
                        p["rid"] = Rid;
                        u.ExecuteNonQuery(p);
                    }
                }
            }
        }

        public TimeSpan Extension
        {
            get { return new TimeSpan(0, 0, _extension); }
            set
            {
                _extension = (int) value.TotalSeconds;
                var u = new Db(@"UPDATE calendar
                        SET extension = @extension
                        WHERE rid = @rid");
                var p = new Hashtable();
                p["extension"] = _extension;
                p["rid"] = Rid;
                u.ExecuteNonQuery(p);
            }
        }

        public bool PlaceReadOnly
        {
            get
            {
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
            get { return StartDate.Date != _limitDate.Date; }
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
            get { return RaceType != CalendarModel.RaceTypes.HybridOld && RaceType != CalendarModel.RaceTypes.Hybrid; }
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
                return Visibility.Collapsed;
            }
        }

        public Visibility CorrectedVisible
        {
            get
            {
                if (RaceType != CalendarModel.RaceTypes.SternChase && Handicap == "r")
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility OpenHandicapVisible
        {
            get
            {
                if (Handicap == "o")
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility RollingHandicapVisible
        {
            get
            {
                if (Handicap == "r")
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool PrintIncludeAllVisible { get; set; }
        public bool PrintIncludeAll { get; set; }
        public bool PrintInclude { get; set; }
        public int PrintIncludeCopies { get; set; }

        public string PrintIncludeDescription
        {
            get { return RaceName; }

            set { }
        }

        public int PrintIncludeGroup { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void LoadGrid()
        {
            var c = new Db(@"SELECT start_date, time_limit_fixed, time_limit_delta, extension, 
                    event, class, racetype, handicapping, laps_completed,
                    standard_corrected_time, ood, course_choice, wind_speed, wind_direction
                    FROM calendar
                    WHERE rid = @rid");
            var p = new Hashtable();
            p["rid"] = Rid;
            _caldata = c.GetHashtable(p);

            StartDate = (DateTime) _caldata["start_date"];

            _timeLimitFixed = _caldata["time_limit_fixed"] as DateTime?;
            _timeLimitDelta = _caldata["time_limit_delta"] as int?;

            if (_timeLimitFixed.HasValue)
                _limitDate = _timeLimitFixed.Value;
            else if (_timeLimitDelta.HasValue)
                _limitDate = StartDate.AddSeconds(_timeLimitDelta.Value);

            if (_caldata["extension"] != DBNull.Value)
                _extension = (int) _caldata["extension"];

            raceName.Content = StartDate.ToString("ddd dd MMM yyyy") + " (";
            switch (_caldata["handicapping"].ToString())
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

            _eventname = _caldata["event"].ToString().Trim();
            RaceName = _eventname + " - " + _caldata["class"].ToString().Trim();
            RaceClass = _caldata["class"].ToString().Trim();
            Ood = _caldata["ood"].ToString();
            if (_caldata["handicapping"] != DBNull.Value)
                _handicap = (string) _caldata["handicapping"];
            var dsct = _caldata["standard_corrected_time"] as double?;
            if (dsct.HasValue)
                sct.Text = Common.HMS(dsct.Value);

            if (_caldata["racetype"] != DBNull.Value)
            {
                if (!Enum.TryParse(_caldata["racetype"].ToString(), out _raceType))
                    _raceType = CalendarModel.RaceTypes.Undefined;
            }

            CalculateEnabled = false;
            c = new Db(@"SELECT result_calculated, MAX(last_edit) last_edit
                FROM calendar c LEFT JOIN races r ON c.rid = r.rid
                WHERE c.rid = @rid
                GROUP BY result_calculated, standard_corrected_time");
            Hashtable calc = c.GetHashtable(p);
            if (calc["result_calculated"] == DBNull.Value ||
                calc["last_edit"] != DBNull.Value &&
                (DateTime) calc["result_calculated"] <= (DateTime) calc["last_edit"])
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
            var updateable = c.GetScalar(p) as int?;
            if (updateable > 0)
                RefreshHandicapsEnabled = true;
            OnPropertyChanged("RefreshHandicapsEnabled");

            _course = _caldata["course_choice"] as string;
            _windSpeed = _caldata["wind_speed"] as string;
            _windDirection = _caldata["wind_direction"] as string;
            _laps = _caldata["laps_completed"] as int?;

            CreateScorer();

            _rddb = new Db("SELECT r.rid, r.bid, boatname, boatclass, sailno, r.start_date, " +
                          "r.finish_code, r.finish_date, r.interim_date, r.restricted_sail, r.laps, r.override_points, r.elapsed, r.standard_corrected, r.corrected, r.place, " +
                          "r.points, r.open_handicap, r.rolling_handicap, r.achieved_handicap, " +
                          "r.new_rolling_handicap, r.handicap_status, r.c, r.a, r.performance_index " +
                          "FROM races r INNER JOIN boats ON boats.bid = r.bid " +
                          "WHERE r.rid = @rid " +
                          "ORDER BY place");
            _raceDataTable = _rddb.GetData(p);
            _raceDataTable.RowChanged += rd_RowChanged;

            SetEditableColumns();

            Races.ItemsSource = (from DataRow r in _raceDataTable.Rows
                select new ResultModel(r, StartDate)).ToList();
            DataContext = this;
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
            foreach (DataColumn col in _raceDataTable.Columns)
            {
                col.ReadOnly = true;
            }

            //
            // Adjust updatable columns according to race type
            //
            switch (RaceType)
            {
                case CalendarModel.RaceTypes.AverageLap:
                    _raceDataTable.Columns["finish_date"].ReadOnly = false;
                    _raceDataTable.Columns["override_points"].ReadOnly = false;
                    _raceDataTable.Columns["finish_code"].ReadOnly = false;
                    _raceDataTable.Columns["laps"].ReadOnly = false;
                    _raceDataTable.Columns["restricted_sail"].ReadOnly = false;
                    _raceDataTable.Columns["open_handicap"].ReadOnly = false;
                    _raceDataTable.Columns["rolling_handicap"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.FixedLength:
                    _raceDataTable.Columns["finish_date"].ReadOnly = false;
                    _raceDataTable.Columns["override_points"].ReadOnly = false;
                    _raceDataTable.Columns["finish_code"].ReadOnly = false;
                    _raceDataTable.Columns["restricted_sail"].ReadOnly = false;
                    _raceDataTable.Columns["open_handicap"].ReadOnly = false;
                    _raceDataTable.Columns["rolling_handicap"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.HybridOld:
                case CalendarModel.RaceTypes.Hybrid:
                    _raceDataTable.Columns["finish_date"].ReadOnly = false;
                    _raceDataTable.Columns["override_points"].ReadOnly = false;
                    _raceDataTable.Columns["finish_code"].ReadOnly = false;
                    _raceDataTable.Columns["interim_date"].ReadOnly = false;
                    _raceDataTable.Columns["laps"].ReadOnly = false;
                    _raceDataTable.Columns["restricted_sail"].ReadOnly = false;
                    _raceDataTable.Columns["open_handicap"].ReadOnly = false;
                    _raceDataTable.Columns["rolling_handicap"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.TimeGate:
                    _raceDataTable.Columns["start_date"].ReadOnly = false;
                    _raceDataTable.Columns["finish_date"].ReadOnly = false;
                    _raceDataTable.Columns["override_points"].ReadOnly = false;
                    _raceDataTable.Columns["finish_code"].ReadOnly = false;
                    _raceDataTable.Columns["restricted_sail"].ReadOnly = false;
                    _raceDataTable.Columns["open_handicap"].ReadOnly = false;
                    _raceDataTable.Columns["rolling_handicap"].ReadOnly = false;
                    break;
                case CalendarModel.RaceTypes.SternChase:
                    _raceDataTable.Columns["place"].ReadOnly = false;
                    _raceDataTable.Columns["override_points"].ReadOnly = false;
                    _raceDataTable.Columns["finish_code"].ReadOnly = false;
                    _raceDataTable.Columns["restricted_sail"].ReadOnly = false;
                    _raceDataTable.Columns["open_handicap"].ReadOnly = false;
                    _raceDataTable.Columns["rolling_handicap"].ReadOnly = false;
                    break;
            }
        }

        private void rd_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            var p = new Hashtable();
            p["rid"] = e.Row["rid"];
            p["bid"] = e.Row["bid"];
            var sql = new StringBuilder("UPDATE races SET last_edit = GETDATE()");
            foreach (DataColumn c in from DataColumn c in e.Row.Table.Columns where !c.ReadOnly select c)
            {
                p[c.ColumnName] = e.Row[c.ColumnName];
                sql.AppendFormat(",{0} = @{0}", c.ColumnName);
            }
            sql.Append(" WHERE rid = @rid AND bid = @bid");
            var d = new Db(sql.ToString());
            d.ExecuteNonQuery(p);
            CalculateEnabled = true;
            OnPropertyChanged("CalculateEnabled");
        }

        private static void SetColumnAttributes()
        {
            //foreach (DataGridColumn c in Races.Columns)
            //{
            //    var _veryLightGray = new Color();
            //    _veryLightGray.A = 255;
            //    _veryLightGray.R = 224;
            //    _veryLightGray.G = 224;
            //    _veryLightGray.B = 224;
            //    var _veryLightGrayBrush = new SolidColorBrush(_veryLightGray);
            //    if (false && c.IsReadOnly)
            //    {
            //        if (!c.IsSealed)
            //            c.CellStyle = new System.Windows.Style();
            //        c.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, _veryLightGrayBrush));
            //    }
            //}
        }

        //
        // Will hold the list found below so that if the user says yes to auto population we don't need to select
        // again.
        //

        //
        // Bit of a misnomer, this actually gets a list of boats that have done at least one other race in
        // a series.
        //
        public int CountAutoPopulateData()
        {
            var c = new Db("SELECT DISTINCT r.bid " +
                           "FROM calendar AS c1 " +
                           "INNER JOIN calendar_series_join AS cs1 ON c1.rid = cs1.rid " +
                           "INNER JOIN calendar_series_join AS cs2 ON cs2.sid = cs1.sid " +
                           "INNER JOIN calendar AS c2 ON c2.rid = cs2.rid AND c1.rid <> c2.rid AND c1.class = c2.class " +
                           "INNER JOIN races AS r ON r.rid = c2.rid " +
                           "INNER JOIN series AS s ON s.sid = cs1.sid AND c1.event LIKE s.sname + '%' " +
                           "WHERE c1.rid = @rid ");
            var p = new Hashtable();
            p["rid"] = Rid;
            _autoPopulateData = c.GetData(p);
            return _autoPopulateData.Rows.Count;
        }

        //
        // Put the auto populate data into the races table for this race.
        //
        public void DoAutoPopulate()
        {
            var add = new Db(@"INSERT INTO races
                    (rid, start_date, bid, rolling_handicap, handicap_status, open_handicap, last_edit)
                    SELECT c.rid, c.start_date, b.bid, b.rolling_handicap, b.handicap_status, b.open_handicap, GETDATE()
                    FROM boats b, calendar c
                    WHERE b.bid = @bid
                    AND c.rid = @rid");
            var a = new Hashtable();
            a["rid"] = Rid;
            foreach (DataRow r in _autoPopulateData.Rows)
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
                var calc = new BackgroundWorker();
                calc.DoWork += Scorer.Calculate;
                var w = new Working(Application.Current.MainWindow, calc);
                calc.RunWorkerCompleted += calc_RunWorkerCompleted;
                calc.RunWorkerAsync(_rid);
                w.ShowDialog();
            }
        }

        private void calc_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadGrid();
        }

        private void Notes_Click(object sender, RoutedEventArgs e)
        {
            var rn = new RaceNotes(Rid);
            rn.ShowDialog();
        }

        /*
         * Refresh rolling handicaps for each boat entered in race from lastest previous race entry
         */

        private void buttonRefreshRolling_Click(object sender, RoutedEventArgs e)
        {
            var c = new Db(@"SELECT r1.bid, r3.new_rolling_handicap
                FROM races r1 
                INNER JOIN races r2 ON r2.bid = r1.bid AND r2.rid <> r1.rid AND r2.start_date < r1.start_date
                INNER JOIN races r3 ON r3.bid = r1.bid AND r3.rid <> r1.rid
                WHERE r1.rid = @rid
                GROUP BY r1.bid, r3.start_date, r3.new_rolling_handicap
                HAVING r3.start_date = MAX(r2.start_date)");
            var p = new Hashtable();
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