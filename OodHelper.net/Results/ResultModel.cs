using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;

namespace OodHelper.Results
{
    class ResultModel : NotifyPropertyChanged
    {
        private DataRow _row;
        public ResultModel(DataRow result, DateTime StartDate)
        {
            _row = result;
            _date = StartDate;
        }

        public int Rid { get { return (int)_row["rid"]; } }
        public int Bid { get { return (int)_row["bid"]; } }
        public string BoatName { get { return _row["boatname"] as string; } }
        public string BoatClass { get { return _row["boatclass"] as string; } }
        public string SailNo { get { return _row["sailno"] as string; } }

        private DateTime _date;
        public string StartDate {
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
                    TimeSpan.TryParseExact(value, "hh\\ mm\\ ss", null, out resultTime))
                    _row["start_date"] = _date + resultTime;
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
            }
        }

        public string FinishDate
        {
            get
            {
                if (_row["finish_date"] != DBNull.Value)
                {
                    DateTime finish = (DateTime)_row["finish_date"];
                    return finish.ToString("HH:mm:ss");
                }
                else
                    return string.Empty;
            }

            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) || 
                    TimeSpan.TryParseExact(value, "hh\\ mm\\ ss", null, out resultTime))
                    _row["finish_date"] = _date + resultTime;
                OnPropertyChanged("FinishDate");
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
                int tmp;
                if (Int32.TryParse(value, out tmp))
                    _row["laps"] = tmp;
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
                        return s.ToString("d\\ hh\\:mm\\:ss");
                    return s.ToString("hh\\:mm\\:ss");
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
                        return s.ToString("d\\ hh\\:mm\\:ss\\.ff");
                    return s.ToString("hh\\:mm\\:ss\\.ff");
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
                        return s.ToString("d\\ hh\\:mm\\:ss\\.ff");
                    return s.ToString("hh\\:mm\\:ss\\.ff");
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
                _row["place"] = value;
                OnPropertyChanged("Place");
            }
        }

        public string Points
        {
            get
            {
                return _row["place"].ToString();
            }
            set
            {
                Double tmp;
                if (Double.TryParse(value, out tmp))
                    _row["points"] = tmp;
                else if (value.Trim() == string.Empty)
                    _row["points"] = null;
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

        private string _a;
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
