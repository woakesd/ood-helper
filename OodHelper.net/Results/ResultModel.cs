using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using OodHelper;

namespace OodHelper.Results
{
    class ResultModel : NotifyPropertyChanged
    {
        public ResultModel()
        {
        }

        public int Rid { get; set; }
        public int Bid { get; set; }
        public string BoatName { get; set; }
        public string BoatClass { get; set; }
        public string SailNo { get; set; }

        private DateTime _date = DateTime.Today;
        private DateTime _startDate;
        public string StartDate {
            get
            {
                return _startDate.ToString("HH:mm:ss");
            }
            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) || 
                    TimeSpan.TryParseExact(value, "hh\\ mm\\ ss", null, out resultTime))
                {
                    _startDate = _date + resultTime;
                }
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

        private int? _overridePoints;
        public string OverridePoints
        {
            get
            {
                return _overridePoints.ToString();
            }
            set
            {
                int tmp;
                if (Int32.TryParse(value, out tmp))
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
    }
}
