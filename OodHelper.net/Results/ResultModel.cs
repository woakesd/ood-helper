using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
//using OodHelper;

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
        
        private string _finishCode;
        public string FinishCode
        {
            get
            {
                return _finishCode;
            }
            set
            {
                _finishCode = value;
            }
        }

        private DateTime _finishDate;
        public string FinishDate
        {
            get
            {
                return _finishDate.ToString("HH:mm:ss");
            }

            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) ||
                    TimeSpan.TryParseExact(value, "hh\\ mm\\ ss", null, out resultTime))
                {
                    _finishDate = _date + resultTime;
                }
                OnPropertyChanged("FinishDate");
            }
        }

        private double? _overridePoints;
        public string OverridePoints
        {
            get
            {
                return _overridePoints.ToString();
            }
            set
            {
                double tmp;
                if (Double.TryParse(value, out tmp))
                    _overridePoints = tmp;
                OnPropertyChanged("OverridePoints");
            }
        }

        private int _laps;
        public string Laps
        {
            get
            {
                return _laps.ToString();
            }
            set
            {
                int tmp;
                if (Int32.TryParse(value, out tmp))
                    _laps = tmp;
                OnPropertyChanged("Laps");
            }
        }

        private int? _elapsed;
        public string Elapsed
        {
            get
            {
                if (_elapsed.HasValue)
                    return new TimeSpan(_elapsed.Value*10000000).ToString();
                else
                    return string.Empty;
            }
            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) ||
                    TimeSpan.TryParseExact(value, "hh\\ mm\\ ss", null, out resultTime))
                {
                    _elapsed = (int)Math.Round(resultTime.TotalSeconds);
                }
                OnPropertyChanged("Elapsed");
            }
        }

        private double? _corrected;
        public string Corrected
        {
            get
            {
                if (_corrected.HasValue)
                    return new TimeSpan((long)Math.Round(_corrected.Value*10000000)).ToString();
                else
                    return string.Empty;
            }
            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) ||
                    TimeSpan.TryParseExact(value, "hh\\ mm\\ ss\\ ff", null, out resultTime))
                {
                    _corrected = resultTime.TotalSeconds;
                }
                OnPropertyChanged("Corrected");
            }
        }

        private double? _standardCorrected;
        public string StandardCorrected
        {
            get
            {
                if (_standardCorrected.HasValue)
                    return new TimeSpan((long)Math.Round(_standardCorrected.Value * 10000000)).ToString();
                else
                    return string.Empty;
            }
            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) ||
                    TimeSpan.TryParseExact(value, "hh\\ mm\\ ss\\ ff", null, out resultTime))
                {
                    _standardCorrected = resultTime.TotalSeconds;
                }
                OnPropertyChanged("StandardCorrected");
            }
        }

        private int? _place;
        public int? Place
        {
            get
            {
                return _place;
            }
            set
            {
                _place = value;
                OnPropertyChanged("Place");
            }
        }

        private double? _points;
        public string Points
        {
            get
            {
                return _place.ToString();
            }
            set
            {
                Double tmp;
                if (Double.TryParse(value, out tmp))
                    _points = tmp;
                else if (value.Trim() == string.Empty)
                    _points = null;
                OnPropertyChanged("Points");
            }
        }

        private int? _openHandicap;
        public string OpenHandicap
        {
            get
            {
                return _openHandicap.ToString();
            }

            set
            {
            }
        }

        private int? _rollingHandicap;
        public string RollingHandicap
        {
            get
            {
                return _rollingHandicap.ToString();
            }

            set
            {
            }
        }

        private int? _achievedHandicap;
        public string AchievedHandicap
        {
            get
            {
                return _achievedHandicap.ToString();
            }

            set
            {
            }
        }

        private int? _newRollingHandicap;
        public string NewRollingHandicap
        {
            get
            {
                return _newRollingHandicap.ToString();
            }

            set
            {
            }
        }

        private string _handicapStatus;
        public string HandicapStatus
        {
            get { return _handicapStatus; }
            set { _handicapStatus = value; }
        }

        private string _c;
        public string C
        {
            get { return _c; }
            set { _c= value; }
        }

        private string _a;
        public string A
        {
            get { return _a; }
            set { _a = value; }
        }

        private int _performanceIndex;
        public string PerformanceIndex
        {
            get { return _performanceIndex.ToString(); }
            set { }
        }
    }
}
