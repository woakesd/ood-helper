using System;
using System.Data;
using System.Text.RegularExpressions;
using OodHelper.Converters;

namespace OodHelper.Results
{
    internal class ResultModel : NotifyPropertyChanged
    {
        private readonly DataRow _row;
        private DateTime _startDate;

        public ResultModel(DataRow result, DateTime startDate)
        {
            _row = result;
            _startDate = startDate;
        }

        public int Rid
        {
            get { return (int) _row["rid"]; }
        }

        public int Bid
        {
            get { return (int) _row["bid"]; }
        }

        public string BoatName
        {
            get { return _row["boatname"] as string; }
        }

        public string BoatClass
        {
            get { return _row["boatclass"] as string; }
        }

        public string SailNo
        {
            get { return _row["sailno"] as string; }
        }

        public DateTime? StartDate
        {
            get
            {
                if (_row["start_date"] != DBNull.Value)
                    return ((DateTime) _row["start_date"]).Date;
                return null;
            }

            set
            {
                if (_row["start_date"] != DBNull.Value)
                    _row["start_date"] = value + ((DateTime) _row["start_date"]).TimeOfDay;
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
                    return ((DateTime) _row["start_date"]).ToString("HH:mm:ss");
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
            get { return _row["finish_code"] as string; }
            set
            {
                _row["finish_code"] = value;
                OnPropertyChanged("FinishCode");
            }
        }

        public DateTime? FinishDate
        {
            get { return ReadDate(_row["finish_date"]); }

            set
            {
                _row["finish_date"] = SetFinishDate(value, _row["finish_date"]);
                OnPropertyChanged("FinishTime");
                OnPropertyChanged("FinishDate");
            }
        }

        public DateTime? InterimDate
        {
            get { return ReadDate(_row["interim_date"]); }

            set
            {
                _row["interim_date"] = SetFinishDate(value, _row["interim_date"]);
                OnPropertyChanged("InterimTime");
                OnPropertyChanged("InterimDate");
            }
        }

        public string FinishTime
        {
            get { return ReadTime(_row["finish_date"]); }

            set
            {
                SetFinishTime(value, "finish_date");
                OnPropertyChanged("FinishTime");
                OnPropertyChanged("FinishDate");
            }
        }

        public string InterimTime
        {
            get { return ReadTime(_row["interim_date"]); }

            set
            {
                SetFinishTime(value, "interim_date");
                OnPropertyChanged("InterimTime");
                OnPropertyChanged("InterimDate");
            }
        }

        public bool RestrictedSail
        {
            get { return _row["restricted_sail"] != DBNull.Value && (bool) _row["restricted_sail"]; }

            set
            {
                if (value)
                {
                    _row["restricted_sail"] = true;
                    if (_row["open_handicap"] != DBNull.Value)
                        _row["open_handicap"] = (int) Math.Round((int) _row["open_handicap"]*1.04);
                    if (_row["rolling_handicap"] != DBNull.Value)
                        _row["rolling_handicap"] = (int) Math.Round((int) _row["rolling_handicap"]*1.04);
                }
                else
                {
                    _row["restricted_sail"] = DBNull.Value;
                    if (_row["open_handicap"] != DBNull.Value)
                        _row["open_handicap"] = (int)Math.Round((int)_row["open_handicap"] / 1.04);
                    if (_row["rolling_handicap"] != DBNull.Value)
                        _row["rolling_handicap"] = (int)Math.Round((int)_row["rolling_handicap"] / 1.04);
                }
                OnPropertyChanged("OpenHandicap");
                OnPropertyChanged("RollingHandicap");
            }
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
            get { return (_row["laps"] != DBNull.Value) ? _row["laps"].ToString() : string.Empty; }
            set
            {
                var tmp = ValueParsers.ReadInt(value);
                if (tmp.HasValue)
                    _row["laps"] = tmp;
                else
                    _row["laps"] = DBNull.Value;
                OnPropertyChanged("Laps");
            }
        }

        public string Elapsed
        {
            get
            {
                if (_row["elapsed"] == DBNull.Value) return string.Empty;
                var s = new TimeSpan(0, 0, (int) _row["elapsed"]);
                return s.ToString(s.Days > 0 ? @"d\ hh\:mm\:ss" : @"hh\:mm\:ss");
            }
        }

        public string Corrected
        {
            get
            {
                if (_row["corrected"] == DBNull.Value) return string.Empty;
                var s = new TimeSpan((long) ((double) _row["corrected"]*10000000));
                return s.ToString(s.Days > 0 ? @"d\ hh\:mm\:ss\.ff" : @"hh\:mm\:ss\.ff");
            }
        }

        public string StandardCorrected
        {
            get
            {
                if (_row["standard_corrected"] == DBNull.Value) return string.Empty;
                var s = new TimeSpan((long) ((double) _row["standard_corrected"]*10000000));
                return s.ToString(s.Days > 0 ? @"d\ hh\:mm\:ss\.ff" : @"hh\:mm\:ss\.ff");
            }
        }

        public string Place
        {
            get { return _row["place"].ToString(); }
            set
            {
                var tmp = ValueParsers.ReadInt(value);
                if (tmp.HasValue)
                    _row["place"] = tmp;
                else
                    _row["place"] = DBNull.Value;
                OnPropertyChanged("Place");
            }
        }

        public string Points
        {
            get { return _row["points"].ToString(); }
            set
            {
                _row["points"] = ValueParsers.ReadDouble(value);
                OnPropertyChanged("Points");
            }
        }

        public string OpenHandicap
        {
            get { return _row["open_handicap"].ToString(); }
        }

        public string RollingHandicap
        {
            get { return _row["rolling_handicap"].ToString(); }
        }

        public string AchievedHandicap
        {
            get { return _row["achieved_handicap"].ToString(); }
        }

        public string NewRollingHandicap
        {
            get { return _row["new_rolling_handicap"].ToString(); }
        }

        public string HandicapStatus
        {
            get { return _row["handicap_status"] as string; }
        }

        public string C
        {
            get { return _row["c"] as string; }
        }

        public string A
        {
            get { return _row["a"] as string; }
        }

        public string PerformanceIndex
        {
            get { return _row["performance_index"].ToString(); }
        }

        private DateTime? ReadDate(object dateTimeValue)
        {
            if (dateTimeValue != DBNull.Value)
                return ((DateTime) dateTimeValue).Date;
            return null;
        }

        private static DateTime? SetFinishDate(DateTime? value, object dateTimeValue)
        {
            if (dateTimeValue != DBNull.Value)
                return value + ((DateTime) dateTimeValue).TimeOfDay;
            return value;
        }

        private void SetFinishTime(string value, string dateTimeValue)
        {
            TimeSpan resultTime;
            var finishCode = new Regex("[a-zA-Z]{3,4}");
            if (TimeSpan.TryParseExact(value, @"hh' 'mm' 'ss", null, out resultTime)
                || TimeSpan.TryParseExact(value, @"hhmmss", null, out resultTime)
                || TimeSpan.TryParseExact(value, @"hh':'mm':'ss", null, out resultTime))
            {
                if (_row[dateTimeValue] != DBNull.Value)
                    _row[dateTimeValue] = ((DateTime) _row[dateTimeValue]).Date + resultTime;
                else
                    _row[dateTimeValue] = _startDate.Date + resultTime;
            }
            else if (finishCode.IsMatch(value))
            {
                FinishCode = value;
            }
            else
                _row[dateTimeValue] = DBNull.Value;
        }

        private static string ReadTime(object dateTimeValue)
        {
            if (dateTimeValue != DBNull.Value)
            {
                var finish = (DateTime) dateTimeValue;
                return finish.ToString("HH:mm:ss");
            }
            return string.Empty;
        }
    }
}