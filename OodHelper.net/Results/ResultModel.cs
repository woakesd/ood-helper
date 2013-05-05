using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Windows;
using OodHelper.Converters;

namespace OodHelper.Results
{
    class ResultModel : NotifyPropertyChanged
    {
        private DataRow _row;
        public ResultModel(DataRow result, DateTime StartDate, DateTime LimitDate)
        {
            _row = result;
            _startDate = StartDate;
            _limitDate = LimitDate;
        }

        public int Rid { get { return (int)_row["rid"]; } }
        public int Bid { get { return (int)_row["bid"]; } }
        public string BoatName { get { return _row["boatname"] as string; } }
        public string BoatClass { get { return _row["boatclass"] as string; } }
        public string SailNo { get { return _row["sailno"] as string; } }

        private DateTime _startDate;
        private DateTime _limitDate;

        public DateTime? StartDate
        {
            get
            {
                if (_row["start_date"] != DBNull.Value)
                    return ((DateTime)_row["start_date"]).Date;
                else
                    return null;
            }

            set
            {
                if (_row["start_date"] != DBNull.Value)
                    _row["start_date"] = value + ((DateTime)_row["start_date"]).TimeOfDay;
                else
                    _row["start_date"] = value;
                OnPropertyChanged("StartTime");
                OnPropertyChanged("StartDate");
            }
        }

        public string StartTime
        {
            get
            {
                if (_row["start_date"] != DBNull.Value)
                    return ((DateTime)_row["start_date"]).ToString("HH:mm:ss");
                else
                    return string.Empty;
            }

            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) || 
                    TimeSpan.TryParseExact(value, @"hh\ mm\ ss", null, out resultTime))
                    _row["start_date"] = _startDate.Date + resultTime;
                OnPropertyChanged("StartTime");
                OnPropertyChanged("StartDate");
            }
        }
        
        public string FinishCode
        {
            get
            {
                return _row["finish_code"] as string;
            }
            set
            {
                _row["finish_code"] = value;
                OnPropertyChanged("FinishCode");
            }
        }

        public DateTime? FinishDate
        {
            get
            {
                return ReadDate(_row["finish_date"]);
            }

            set
            {
                SetFinishDate(value, _row["finish_date"]);
                OnPropertyChanged("FinishTime");
                OnPropertyChanged("FinishDate");
            }
        }

        public DateTime? InterimDate
        {
            get
            {
                return ReadDate(_row["interim_date"]);
            }

            set
            {
                SetFinishDate(value, _row["interim_date"]);
                OnPropertyChanged("InterimTime");
                OnPropertyChanged("InterimDate");
            }
        }

        private DateTime? ReadDate(object DateTimeValue)
        {
            if (DateTimeValue != DBNull.Value)
                return ((DateTime)DateTimeValue).Date;
            else
                return null;
        }

        private void SetFinishDate(DateTime? value, object DateTimeValue)
        {
            if (DateTimeValue != DBNull.Value)
                DateTimeValue = value + ((DateTime)DateTimeValue).TimeOfDay;
            else
                DateTimeValue = value;
        }

        public string FinishTime
        {
            get
            {
                return ReadTime(_row["finish_date"]);
            }

            set
            {
                SetFinishTime(value, "finish_date");
                OnPropertyChanged("FinishTime");
                OnPropertyChanged("FinishDate");
            }
        }

        public string InterimTime
        {
            get
            {
                return ReadTime(_row["interim_date"]);
            }

            set
            {
                SetFinishTime(value, "interim_date");
                OnPropertyChanged("InterimTime");
                OnPropertyChanged("InterimDate");
            }
        }

        private void SetFinishTime(string value, string DateTimeValue)
        {
            TimeSpan resultTime;
            System.Text.RegularExpressions.Regex _finishCode = new System.Text.RegularExpressions.Regex("[a-zA-Z]{3,4}");
            if (TimeSpan.TryParseExact(value, @"hh\ mm\ ss", null, out resultTime)
                || TimeSpan.TryParseExact(value, @"hhmmss", null, out resultTime))
            {
                if (_row[DateTimeValue] != DBNull.Value)
                    _row[DateTimeValue] = ((DateTime)_row[DateTimeValue]).Date + resultTime;
                else
                    _row[DateTimeValue] = _startDate.Date + resultTime;
            }
            else if (_finishCode.IsMatch(value))
            {
                FinishCode = value;
            }
            else
                _row[DateTimeValue] = DBNull.Value;
        }

        private string ReadTime(object DateTimeValue)
        {
            if (DateTimeValue != DBNull.Value)
            {
                DateTime finish = (DateTime)DateTimeValue;
                return finish.ToString("HH:mm:ss");
            }
            else
                return string.Empty;
        }

        public string OverridePoints
        {
            get
            {
                return (_row["override_points"] != DBNull.Value) ? _row["override_points"].ToString() : string.Empty;
            }
            set
            {
                double tmp;
                if (Double.TryParse(value, out tmp))
                    _row["override_points"] = tmp;
                else
                    _row["override_points"] = DBNull.Value;

                OnPropertyChanged("OverridePoints");
            }
        }

        public string Laps
        {
            get
            {
                return (_row["laps"] != DBNull.Value) ? _row["laps"].ToString() : string.Empty;
            }
            set
            {
                int? _tmp = ValueParsers.ReadInt(value);
                if (_tmp.HasValue)
                    _row["laps"] = _tmp;
                else
                    _row["laps"] = DBNull.Value;
                OnPropertyChanged("Laps");
            }
        }

        public string Elapsed
        {
            get
            {
                if (_row["elapsed"] != DBNull.Value)
                {
                    TimeSpan s = new TimeSpan(0,0,(int)_row["elapsed"]);
                    if (s.Days > 0)
                        return s.ToString(@"d\ hh\:mm\:ss");
                    return s.ToString(@"hh\:mm\:ss");
                }
                else
                    return string.Empty;
            }
            set
            {
            }
        }

        public string Corrected
        {
            get
            {
                if (_row["corrected"] != DBNull.Value)
                {
                    TimeSpan s = new TimeSpan((long)((double)_row["corrected"] * 10000000));
                    if (s.Days > 0)
                        return s.ToString(@"d\ hh\:mm\:ss\.ff");
                    return s.ToString(@"hh\:mm\:ss\.ff");
                }
                else
                    return string.Empty;
            }
            set
            {
            }
        }

        public string StandardCorrected
        {
            get
            {
                if (_row["standard_corrected"] != DBNull.Value)
                {
                    TimeSpan s = new TimeSpan((long)((double)_row["standard_corrected"] * 10000000));
                    if (s.Days > 0)
                        return s.ToString(@"d\ hh\:mm\:ss\.ff");
                    return s.ToString(@"hh\:mm\:ss\.ff");
                }
                else
                    return string.Empty;
            }
            set
            {
            }
        }

        public string Place
        {
            get
            {
                return _row["place"].ToString();
            }
            set
            {
                _row["place"] = ValueParsers.ReadInt(value);
                OnPropertyChanged("Place");
            }
        }

        public string Points
        {
            get
            {
                return _row["points"].ToString();
            }
            set
            {
                _row["points"] = ValueParsers.ReadDouble(value);
                OnPropertyChanged("Points");
            }
        }

        public string OpenHandicap
        {
            get
            {
                return _row["open_handicap"].ToString();
            }

            set
            {
            }
        }

        public string RollingHandicap
        {
            get
            {
                return _row["rolling_handicap"].ToString();
            }

            set
            {
            }
        }

        public string AchievedHandicap
        {
            get
            {
                return _row["achieved_handicap"].ToString();
            }

            set
            {
            }
        }

        public string NewRollingHandicap
        {
            get
            {
                return _row["new_rolling_handicap"].ToString();
            }

            set
            {
            }
        }

        public string HandicapStatus
        {
            get { return _row["handicap_status"] as string; }
            set { }
        }

        public string C
        {
            get { return _row["c"] as string; }
            set { }
        }

        public string A
        {
            get { return _row["a"] as string; }
            set { }
        }

        public string PerformanceIndex
        {
            get { return _row["performance_index"].ToString(); }
            set { }
        }
    }
}
