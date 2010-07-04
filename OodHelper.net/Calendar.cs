using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    [Svn("$Id$")]
    [Table(Name = "calendar")]
    public class Calendar : INotifyPropertyChanged
    {
        private DateTime? start_date_date;
        private string start_date_time;

        [Column(IsPrimaryKey=true)]
        public int rid;
        [Column]
        public DateTime? start_date
        {
            set
            {
                if (value.HasValue)
                {
                    start_date_date = value.Value.Date;
                    start_date_time = value.Value.ToString("HH:mm");
                }
                else
                {
                    start_date_date = null;
                    start_date_time = null;
                }
            }

            get
            {
                if (start_date_date != null)
                {
                    return start_date_date + ((start_date_time == null || start_date_time == string.Empty) ? 
                        new TimeSpan(0) : TimeSpan.ParseExact(start_date_time, "hh\\:mm", null));
                }
                return null;
            }
        }
        [Column]
        public string time_limit_type;
        public string TimeLimitType
        {
            get { return time_limit_type; }
            set { time_limit_type = value; OnPropertyChanged("TimeLimitType"); }
        }
        [Column]
        public DateTime? time_limit_fixed
        {
            set
            {
                if (value.HasValue)
                {
                    time_limit_fixed_date = value.Value.Date;
                    time_limit_fixed_time = value.Value.ToString("HH:mm");
                }
                else
                {
                    time_limit_fixed_date = null;
                    time_limit_fixed_time = null;
                }
            }

            get
            {
                if (time_limit_fixed_date != null)
                {
                    return time_limit_fixed_date + ((time_limit_fixed_time == null || time_limit_fixed_time == string.Empty) ? 
                        new TimeSpan(0) : TimeSpan.ParseExact(time_limit_fixed_time, "hh\\:mm", null));
                }
                return null;
            }
        }
        [Column]
        public int? time_limit_delta;
        public string TimeLimitDelta
        {
            get
            {
                if (time_limit_delta.HasValue)
                {
                    TimeSpan d = new TimeSpan(0, 0, time_limit_delta.Value);
                    if (d < new TimeSpan(1,0,0,0))
                        return d.ToString("h\\:mm");
                    else
                        return d.ToString("d\\ hh\\:mm");
                }
                else
                    return string.Empty;
            }

            set
            {
                if (value != string.Empty)
                {
                    try
                    {
                        time_limit_delta = (int)TimeSpan.ParseExact(value, "d\\ hh\\:mm", null).TotalSeconds;
                        OnPropertyChanged("TimeLimitDelta");
                    }
                    catch (Exception)
                    {
                        try
                        {
                            time_limit_delta = (int)TimeSpan.ParseExact(value, "h\\:mm", null).TotalSeconds;
                            OnPropertyChanged("TimeLimitDelta");
                        }
                        catch (Exception)
                        {
                            new ArgumentException("Time limit delta must be in format '1 02:50' or '2:30'");
                        }
                    }
                }
                else
                {
                    time_limit_delta = null;
                    OnPropertyChanged("TimeLimitDelta");
                }
            }
        }
        [Column]
        public int? extension;
        public string Extension
        {
            get
            {
                if (extension.HasValue)
                {
                    TimeSpan d = new TimeSpan(0, 0, extension.Value);
                    if (d < new TimeSpan(1, 0, 0, 0))
                        return d.ToString("h\\:mm");
                    else
                        return d.ToString("d\\ hh\\:mm");
                }
                else
                    return string.Empty;
            }

            set
            {
                if (value != string.Empty)
                {
                    try
                    {
                        extension = (int)TimeSpan.ParseExact(value, "d\\ hh\\:mm", null).TotalSeconds;
                        OnPropertyChanged("Extension");
                    }
                    catch (Exception)
                    {
                        try
                        {
                            extension = (int)TimeSpan.ParseExact(value, "h\\:mm", null).TotalSeconds;
                            OnPropertyChanged("Extension");
                        }
                        catch (Exception)
                        {
                            new ArgumentException("Time limit delta must be in format '1 02:50' or '2:30'");
                        }
                    }
                }
                else
                {
                    extension = null;
                    OnPropertyChanged("Extension");
                }
            }
        }
        [Column(Name = "class")]
        public string calendar_class;
        public string CalendarClass
        {
            get { return calendar_class; }
            set { calendar_class = value; OnPropertyChanged("CalendarClass"); }
        }
        [Column(Name = "event")]
        public string calendar_event;
        public string CalendarEvent
        {
            get { return calendar_event; }
            set { calendar_event = value; OnPropertyChanged("CalendarEvent"); }
        }
        [Column]
        public string price_code;
        public string PriceCode
        {
            get
            {
                return price_code;
            }
            set
            {
                price_code = value;
                OnPropertyChanged("PriceCode");
            }
        }
        [Column]
        public string course;
        public string Course
        {
            get { return course; }
            set { course = value; OnPropertyChanged("Course"); }
        }
        [Column]
        public string ood;
        public string Ood
        {
            get { return ood; }
            set { ood = value; OnPropertyChanged("Ood"); }
        }
        [Column]
        public string venue;
        public string Venue
        {
            get { return venue; }
            set { venue = value; OnPropertyChanged("Venue"); }
        }
        [Column]
        public bool? average_lap;
        public bool? AverageLap
        {
            get { return average_lap; }
            set { average_lap = value; OnPropertyChanged("AverageLap"); }
        }
        [Column]
        public bool? timegate;
        public bool? TimeGate
        {
            get { return timegate; }
            set { timegate = value; OnPropertyChanged("TimeGate"); }
        }
        [Column]
        public string handicapping;
        public string Handicapping
        {
            get
            {
                return handicapping;
            }
            set
            {
                handicapping = value;
                OnPropertyChanged("Handicapping");
            }
        }
        [Column]
        public int? visitors;
        public int? Visitors
        {
            get { return visitors; }
            set { visitors = value; OnPropertyChanged("Visitors"); }
        }
        [Column]
        public string flag;
        public string Flag
        {
            get { return flag; }
            set { flag = value; OnPropertyChanged("Flag"); }
        }
        [Column]
        public string memo;
        public string Memo
        {
            get { return memo; }
            set { memo = value; OnPropertyChanged("Memo"); }
        }
        [Column]
        public bool? is_race;
        [Column]
        public bool? raced;
        public bool? Raced
        {
            get { return raced; }
            set { raced = value; OnPropertyChanged("Raced"); }
        }
        [Column]
        public bool? approved;
        public bool? Approved
        {
            get { return approved; }
            set { approved = value; OnPropertyChanged("Approved"); }
        }
        [Column]
        public double? standard_corrected_time;
        public string SCT
        {
            get
            {
                TimeSpan t = new TimeSpan((long)(standard_corrected_time * 10000000));
                return t.ToString("hh\\:mm\\:ss\\.ff");
            }
        }
        [Column]
        public DateTime? result_calculated;

        public DateTime? StartDateDate
        {
            get
            {
                return start_date_date;
            }

            set
            {
                start_date_date = value;
                OnPropertyChanged("StartDateDate");
            }
        }

        public DateTime? TimeLimitFixedDate
        {
            get
            {
                return time_limit_fixed_date;
            }

            set
            {
                time_limit_fixed_date = value;
                OnPropertyChanged("TimeLimitFixedDate");
            }
        }

        private DateTime? time_limit_fixed_date;
        private string time_limit_fixed_time;
        public string TimeLimitFixedTime
        {
            get
            {
                return time_limit_fixed_time;
            }

            set
            {
                if (time_limit_fixed_date.HasValue)
                    try
                    {
                        time_limit_fixed_date.Value.Date.Add(TimeSpan.ParseExact(value, "h\\:mm", null));
                        OnPropertyChanged("TimeLimitFixedTime");
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Time limit time format must be like 12:50");
                    }
                else
                    throw new ArgumentException("Time limit date must be selected first");
            }
        }

        public string StartDateTime
        {
            get
            {
                return start_date_time;
            }

            set
            {
                if (start_date_date.HasValue)
                    try
                    {
                        start_date_date.Value.Date.Add(TimeSpan.ParseExact(value, "h\\:mm", null));
                        OnPropertyChanged("StartDateTime");
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Start time format must be like 12:50");
                    }
                else
                    throw new ArgumentException("Start date must be selected first");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
