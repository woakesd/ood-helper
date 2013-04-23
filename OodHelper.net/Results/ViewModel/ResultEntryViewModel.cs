using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OodHelper.ViewModel;
using OodHelper.Results.Model;

namespace OodHelper.Results.ViewModel
{
    public class ResultEntryViewModel : ViewModelBase
    {
        private IEntry _entry { get; set; }
        private ICalendarEvent _event { get; set; }

        public ResultEntryViewModel(IEntry Entry, ICalendarEvent CalEvent)
        {
            _entry = Entry;
            _event = CalEvent;
        }

        public void CalendarEvent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_event != null)
            {
                switch (e.PropertyName)
                {
                    case "StartDate":
                        if (_event.start_date.HasValue && _event.racetype != CalendarEvent.RaceTypes.TimeGate && _event.racetype != CalendarEvent.RaceTypes.SternChase)
                            StartDate = _event.start_date.Value;
                        break;
                    case "StartTime":
                        if (_event.start_date.HasValue && _event.racetype != CalendarEvent.RaceTypes.TimeGate && _event.racetype != CalendarEvent.RaceTypes.SternChase)
                            StartDate = _event.start_date.Value;
                        break;
                    case "TimeLimit":
                        switch (_event.time_limit_type)
                        {
                            case CalendarEvent.TimeLimitTypes.F:
                                if (_event.time_limit_delta.HasValue)
                                    RaceTimeLimit = RaceStart.Value.AddSeconds(_event.time_limit_delta.Value);
                                else
                                    RaceTimeLimit = null;
                                break;
                            case CalendarEvent.TimeLimitTypes.D:
                                RaceTimeLimit = _event.time_limit_fixed;
                                break;
                        }
                        break;
                }
            }
        }

        public DateTime? RaceStart { get; set; }
        public DateTime? RaceTimeLimit { get; set; }

        public int Rid { get { return _entry.rid; } }
        public int Bid { get { return _entry.bid; } }
        public string BoatName { get { return _entry.boatname as string; } }
        public string BoatClass { get { return _entry.boatclass as string; } }
        public string SailNo { get { return _entry.sailno as string; } }

        public DateTime? StartDate
        {
            get
            {
                if (_entry.start_date.HasValue)
                    return (DateTime)_entry.start_date.Value.Date;
                else
                    return null;
            }

            set
            {
                _entry.start_date = value;
                OnPropertyChanged("StartDate");
                OnPropertyChanged("StartTime");
            }
        }

        public string StartTime
        {
            get
            {
                if (_entry.start_date.HasValue)
                    return _entry.start_date.Value.ToString("HH:mm:ss");
                else
                    return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                _tmp = Converters.ValueParser.ReadTimeSpan(value);
                if (_tmp.HasValue)
                {
                    if (_tmp.Value < Converters.ValueParser.TwentyFourHours)
                    {
                        if (_entry.start_date.HasValue)
                            _entry.start_date = _entry.start_date.Value.Date + _tmp;
                        else if (_event != null && _event.start_date != null)
                            _entry.start_date = _event.start_date.Value.Date + _tmp;
                        else
                            _entry.start_date = DateTime.Today + _tmp;

                        OnPropertyChanged("StartDate");
                        OnPropertyChanged("StartTime");
                    }
                }
            }
        }

        public string FinishCode
        {
            get
            {
                return _entry.finish_code;
            }

            set
            {
                if (value == string.Empty || IsFinishCode(value))
                {
                    _entry.finish_code = value.ToUpper();
                    OnPropertyChanged("FinishCode");
                }
            }
        }

        public DateTime? FinishDate
        {
            get
            {
                return _entry.finish_date.HasValue ? _entry.finish_date.Value.Date : (DateTime?)null;
            }

            set
            {
                if (value.HasValue)
                {
                    if (_entry.finish_date.HasValue)
                        _entry.finish_date = value.Value.Date + _entry.finish_date.Value.TimeOfDay;
                    else
                        _entry.finish_date = value.Value.Date;
                }
                else
                    _entry.finish_date = value;

                OnPropertyChanged("FinishDate");
                OnPropertyChanged("FinishTime");
            }
        }

        public DateTime? InterimDate
        {
            get
            {
                return _entry.interim_date.HasValue ? _entry.interim_date.Value.Date : (DateTime?)null;
            }

            set
            {
                if (value.HasValue)
                {
                    if (_entry.interim_date.HasValue)
                        _entry.interim_date = value.Value.Date + _entry.interim_date.Value.TimeOfDay;
                    else
                        _entry.interim_date = value.Value.Date;
                }
                else
                    _entry.interim_date = value;

                OnPropertyChanged("InterimDate");
                OnPropertyChanged("InterimTime");
            }
        }

        private bool IsFinishCode(string value)
        {
            Regex _entryCode = new Regex("[a-z]{3}", RegexOptions.IgnoreCase);
            if (_entryCode.IsMatch(value))
                return true;
            return false;
        }

        public string FinishTime
        {
            get
            {
                if (_entry.finish_date.HasValue)
                    return _entry.finish_date.Value.ToString("HH:mm:ss");
                else
                    return string.Empty;
            }

            set
            {
                if (IsFinishCode(value))
                {
                    FinishCode = value;
                }
                else
                {
                    TimeSpan? _tmp;
                    _tmp = Converters.ValueParser.ReadTimeSpan(value);
                    if (_tmp.HasValue)
                    {
                        if (_tmp.Value < Converters.ValueParser.TwentyFourHours)
                        {
                            if (_entry.finish_date.HasValue)
                                _entry.finish_date = _entry.finish_date.Value.Date + _tmp;
                            else if (_event != null && _event.start_date != null)
                                _entry.finish_date = _event.start_date.Value.Date + _tmp;
                            else
                                _entry.finish_date = DateTime.Today + _tmp;

                            OnPropertyChanged("FinishTime");
                        }
                    }
                }
            }
        }

        public string InterimTime
        {
            get
            {
                if (_entry.interim_date.HasValue)
                    return _entry.interim_date.Value.ToString("HH:mm:ss");
                else
                    return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                _tmp = Converters.ValueParser.ReadTimeSpan(value);
                if (_tmp.HasValue)
                {
                    if (_tmp.Value < Converters.ValueParser.TwentyFourHours)
                    {
                        if (_entry.interim_date.HasValue)
                            _entry.interim_date = _entry.interim_date.Value.Date + _tmp;
                        else if (_event != null && _event.start_date != null)
                            _entry.interim_date = _event.start_date.Value.Date + _tmp;
                        else
                            _entry.interim_date = DateTime.Today + _tmp;

                        OnPropertyChanged("InterimTime");
                    }
                }
            }
        }

        public string OverridePoints
        {
            get
            {
                return _entry.override_points.ToString();
            }
            set
            {
                double tmp;
                if (Double.TryParse(value, out tmp))
                    _entry.override_points = tmp;
                else
                    _entry.override_points = null;

                OnPropertyChanged("OverridePoints");
            }
        }

        public string Laps
        {
            get
            {
                return _entry.laps.ToString();
            }
            set
            {
                int? _tmp = Converters.ValueParser.ReadInt(value);
                _entry.laps = _tmp;
                OnPropertyChanged("Laps");
            }
        }

        public string Elapsed
        {
            get
            {
                if (_entry.elapsed.HasValue)
                {
                    TimeSpan s = new TimeSpan(0, 0, (int)_entry.elapsed);
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
                if (_entry.corrected.HasValue)
                {
                    TimeSpan s = new TimeSpan((long)(_entry.corrected * 10000000));
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
                if (_entry.standard_corrected.HasValue)
                {
                    TimeSpan s = new TimeSpan((long)(_entry.standard_corrected * 10000000));
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
                return _entry.place.ToString();
            }
            set
            {
                if (_event != null || _event.racetype == CalendarEvent.RaceTypes.SternChase)
                    _entry.place = Converters.ValueParser.ReadInt(value);
                OnPropertyChanged("Place");
            }
        }

        public string Points
        {
            get
            {
                return _entry.points.ToString();
            }
            set
            {
                _entry.points = Converters.ValueParser.ReadDouble(value);
                OnPropertyChanged("Points");
            }
        }

        public string OpenHandicap
        {
            get
            {
                return _entry.open_handicap.ToString();
            }

            set
            {
            }
        }

        public string RollingHandicap
        {
            get
            {
                return _entry.rolling_handicap.ToString();
            }

            set
            {
            }
        }

        public string AchievedHandicap
        {
            get
            {
                return _entry.achieved_handicap.ToString();
            }

            set
            {
            }
        }

        public string NewRollingHandicap
        {
            get
            {
                return _entry.new_rolling_handicap.ToString();
            }

            set
            {
            }
        }

        public string HandicapStatus
        {
            get { return _entry.handicap_status; }
            set { }
        }

        public string C
        {
            get { return _entry.c; }
            set { }
        }

        public string A
        {
            get { return _entry.a; }
            set { }
        }
    }
}
