using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OodHelper.ViewModel;
using OodHelper.Results.Model;

namespace OodHelper.Results.ViewModel
{
    public class ResultsEditorViewModel : ViewModelBase, IPrintSelectItem
    {
        readonly OodHelper.Results.Model.Race _result;

        public IList<IEntry> Entries { get { return _result.EventEntries; } }
        public override string DisplayName { get { return string.Format("{0} - {1}", _result.Event.eventName, _result.Event.eventClass); } }
        public CalendarEvent.RaceTypes RaceType { get { return _result.Event.racetype; } set { _result.Event.racetype = value; } }
        public CalendarEvent.Handicappings Handicapping { get { return _result.Event.handicapping; } set { _result.Event.handicapping = value; } }

        public DateTime StartDate
        {
            get
            {
                if (_result.Event.start_date.HasValue)
                    return _result.Event.start_date.Value.Date;
                return DateTime.Today;
            }

            set
            {
                TimeSpan _tmp;
                if (_result.Event.start_date.HasValue)
                    _tmp = _result.Event.start_date.Value.TimeOfDay;
                else
                    _tmp = TimeSpan.Zero;
                _result.Event.start_date = value + _tmp;
            }
        }

        public string StartTime
        {
            get
            {
                if (_result.Event.start_date.HasValue)
                    return _result.Event.start_date.Value.ToString("HH:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                if ((_tmp = Converters.ValueParser.ReadTimeSpan(value)).HasValue)
                {
                    if (_tmp.Value <= Converters.ValueParser.TwentyFourHours)
                    {
                        _result.Event.start_date = _result.Event.start_date.Value.Date + _tmp.Value;
                        base.OnPropertyChanged("StartTime");
                        base.OnPropertyChanged("StartDate");
                    }
                }
            }
        }

        public string TimeLimit
        {
            get
            {
                if (_result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.F && _result.Event.time_limit_fixed.HasValue)
                    return _result.Event.time_limit_fixed.Value.ToString("HH:mm");
                else if (_result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.D && _result.Event.time_limit_delta.HasValue)
                    return new TimeSpan(0, 0, _result.Event.time_limit_delta.Value).ToString("hh\\:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                if ((_tmp = Converters.ValueParser.ReadTimeSpan(value)) != null)
                {
                    if (_result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.F)
                    {
                        if (_tmp < Converters.ValueParser.TwentyFourHours)
                        {
                            if (_result.Event.time_limit_fixed.HasValue)
                                _result.Event.time_limit_fixed = _result.Event.time_limit_fixed.Value.Date + _tmp;
                            else if (_result.Event.start_date.HasValue)
                                _result.Event.time_limit_fixed = _result.Event.start_date.Value.Date + _tmp;
                        }
                    }
                    else if (_result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.D)
                        _result.Event.time_limit_delta = (int)_tmp.Value.TotalSeconds;
                    base.OnPropertyChanged("TimeLimit");
                }
            }
        }

        public string Extension
        {
            get
            {
                if (_result.Event.extension.HasValue)
                    return new TimeSpan(0, 0, _result.Event.extension.Value).ToString("hh\\:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                if ((_tmp = Converters.ValueParser.ReadTimeSpan(value)) != null)
                {
                    _result.Event.extension = (int)_tmp.Value.TotalSeconds;
                    base.OnPropertyChanged("Extension");
                }
            }
        }
        
        public ResultsEditorViewModel(OodHelper.Results.Model.Race Result)
        {
            if (Result == null) throw new ArgumentNullException("Result");

            _result = Result;
        }

        public string StandardCorrectedTime
        {
            get
            {
                if (_result.Event.standard_corrected_time.HasValue)
                {
                    long _sct = (long)Math.Round(_result.Event.standard_corrected_time.Value * 1000 * 1000 * 10);
                    TimeSpan _tmp = new TimeSpan(_sct);
                    return _tmp.ToString("hh':'mm':'ss");
                }
                return null;
            }
        }

        public string Course
        {
            get { return _result.Event.course; }
            set { _result.Event.course = value; OnPropertyChanged("Course"); }
        }

        public string WindSpeed
        {
            get { return _result.Event.wind_speed; }
            set { _result.Event.wind_speed = value; OnPropertyChanged("WindSpeed"); }
        }

        public string WindDirection
        {
            get { return _result.Event.wind_direction; }
            set { _result.Event.wind_direction = value; OnPropertyChanged("WindDirection"); }
        }

        public string Laps
        {
            get {
                return _result.Event.laps.ToString();
            }

            set
            {
                int? _tmp;
                if ((_tmp = Converters.ValueParser.ReadInt(value)) != null)
                {
                    _result.Event.laps = _tmp.Value;
                    OnPropertyChanged("Laps");
                }
            }
        }

        #region IPrintSelectItem
        public new void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);
        }

        public bool PrintIncludeAllVisible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PrintIncludeAll
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PrintInclude
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int PrintIncludeCopies
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string PrintIncludeDescription
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int PrintIncludeGroup
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion //IPrintSelectItem
    }
}
