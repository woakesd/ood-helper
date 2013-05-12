using System;
using System.Collections;
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
        public IEntry Entry { get; set; }
        private ICalendarEvent _event { get; set; }

        public ResultEntryViewModel(IEntry Entry, ICalendarEvent CalEvent)
        {
            this.Entry = Entry;
            _event = CalEvent;
        }

        public void SaveChanges()
        {
            Entry.SaveChanges();
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
                }
            }
        }

        public DateTime? RaceStart { get; set; }

        public int Rid { get { return Entry.rid; } }
        public int Bid { get { return Entry.bid; } }
        public string BoatName { get { return Entry.boatname; } set { Entry.boatname = value; OnPropertyChanged("BoatName"); } }
        public string BoatClass { get { return Entry.boatclass; } set { Entry.boatclass = value; OnPropertyChanged("BoatClass"); } }
        public string SailNo { get { return Entry.sailno; } set { Entry.sailno = value; OnPropertyChanged("SailNo"); } }

        public DateTime? StartDate
        {
            get
            {
                if (Entry.start_date.HasValue)
                    return (DateTime)Entry.start_date.Value.Date;
                else
                    return null;
            }

            set
            {
                Entry.start_date = value;
                OnPropertyChanged("StartDate");
                OnPropertyChanged("StartTime");
            }
        }

        public string StartTime
        {
            get
            {
                if (Entry.start_date.HasValue)
                    return Entry.start_date.Value.ToString("HH:mm:ss");
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
                        if (Entry.start_date.HasValue)
                            Entry.start_date = Entry.start_date.Value.Date + _tmp;
                        else if (_event != null && _event.start_date != null)
                            Entry.start_date = _event.start_date.Value.Date + _tmp;
                        else
                            Entry.start_date = DateTime.Today + _tmp;

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
                return Entry.finish_code;
            }

            set
            {
                if (value == string.Empty || IsFinishCode(value))
                {
                    Entry.finish_code = value.ToUpper();
                    OnPropertyChanged("FinishCode");
                }
            }
        }

        public DateTime? FinishDate
        {
            get
            {
                return Entry.finish_date.HasValue ? Entry.finish_date.Value.Date : (DateTime?)null;
            }

            set
            {
                if (value.HasValue)
                {
                    if (Entry.finish_date.HasValue)
                        Entry.finish_date = value.Value.Date + Entry.finish_date.Value.TimeOfDay;
                    else
                        Entry.finish_date = value.Value.Date;
                }
                else
                    Entry.finish_date = value;

                OnPropertyChanged("FinishDate");
                OnPropertyChanged("FinishTime");
            }
        }

        public DateTime? InterimDate
        {
            get
            {
                return Entry.interim_date.HasValue ? Entry.interim_date.Value.Date : (DateTime?)null;
            }

            set
            {
                if (value.HasValue)
                {
                    if (Entry.interim_date.HasValue)
                        Entry.interim_date = value.Value.Date + Entry.interim_date.Value.TimeOfDay;
                    else
                        Entry.interim_date = value.Value.Date;
                }
                else
                    Entry.interim_date = value;

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
                if (Entry.finish_date.HasValue)
                    return Entry.finish_date.Value.ToString("HH:mm:ss");
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
                    Entry.finish_date = ReadFinishTime(value, Entry.finish_date);
                }
                OnPropertyChanged("FinishTime");
            }
        }

        private DateTime? ReadFinishTime(string value, DateTime? FinishDate)
        {
            TimeSpan? _tmp;
            _tmp = Converters.ValueParser.ReadTimeSpan(value);
            if (_tmp.HasValue)
            {
                if (_tmp.Value < Converters.ValueParser.TwentyFourHours)
                {
                    if (FinishDate.HasValue)
                        return FinishDate.Value.Date + _tmp;
                    else if (_event != null && _event.start_date != null)
                        return _event.start_date.Value.Date + _tmp;
                    else
                        return DateTime.Today + _tmp;
                }
            }
            return null;
        }

        public string InterimTime
        {
            get
            {
                if (Entry.interim_date.HasValue)
                    return Entry.interim_date.Value.ToString("HH:mm:ss");
                else
                    return string.Empty;
            }

            set
            {
                Entry.interim_date = ReadFinishTime(value, Entry.interim_date);
                OnPropertyChanged("InterimTime");
            }
        }

        public string OverridePoints
        {
            get
            {
                return Entry.override_points.ToString();
            }
            set
            {
                double tmp;
                if (Double.TryParse(value, out tmp))
                    Entry.override_points = tmp;
                else
                    Entry.override_points = null;

                OnPropertyChanged("OverridePoints");
            }
        }

        public string Laps
        {
            get
            {
                return Entry.laps.ToString();
            }
            set
            {
                int? _tmp = Converters.ValueParser.ReadInt(value);
                Entry.laps = _tmp;
                OnPropertyChanged("Laps");
            }
        }

        public string Elapsed
        {
            get
            {
                if (Entry.elapsed.HasValue)
                {
                    TimeSpan s = new TimeSpan(0, 0, (int)Entry.elapsed);
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
                return OutputCorrected(Entry.corrected);
            }
            set
            {
            }
        }

        public string StandardCorrected
        {
            get
            {
                return OutputCorrected(Entry.standard_corrected);
            }
            set
            {
            }
        }

        private string OutputCorrected(double? Corrected)
        {
            if (Corrected.HasValue)
            {
                TimeSpan s = new TimeSpan((long)(Corrected * 10000000));
                if (s.Days > 0)
                    return s.ToString(@"d\ hh\:mm\:ss\.ff");
                return s.ToString(@"hh\:mm\:ss\.ff");
            }
            else
                return string.Empty;
        }

        public string Place
        {
            get
            {
                return Entry.place.ToString();
            }
            set
            {
                if (_event != null && _event.racetype == CalendarEvent.RaceTypes.SternChase)
                    Entry.place = Converters.ValueParser.ReadInt(value);
                OnPropertyChanged("Place");
            }
        }

        public string Points
        {
            get
            {
                return Entry.points.ToString();
            }
            set
            {
            }
        }

        public int? OpenHandicap
        {
            get
            {
                return Entry.open_handicap;
            }

            set
            {
                Entry.open_handicap = value;
                OnPropertyChanged("OpenHandicap");
            }
        }

        public int? RollingHandicap
        {
            get
            {
                return Entry.rolling_handicap;
            }

            set
            {
                Entry.open_handicap = value;
                OnPropertyChanged("RollingHandicap");
            }
        }

        public string AchievedHandicap
        {
            get
            {
                return Entry.achieved_handicap.ToString();
            }

            set
            {
            }
        }

        public string NewRollingHandicap
        {
            get
            {
                return Entry.new_rolling_handicap.ToString();
            }

            set
            {
            }
        }

        public string HandicapStatus
        {
            get { return Entry.handicap_status; }
            set { Entry.handicap_status = value; OnPropertyChanged("HandicapStatus"); }
        }

        public string C
        {
            get { return Entry.c; }
            set { }
        }

        public string A
        {
            get { return Entry.a; }
            set { }
        }
    }
}
