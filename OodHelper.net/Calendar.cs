using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace OodHelper
{
    [Svn("$Id$")]
    [Table(Name = "calendar")]
    public class Calendar : NotifyPropertyChanged
    {
        [Column(IsPrimaryKey = true)]
        public int rid
        {
            get { return mRid; }
            set { mRid = value; OnPropertyChanged("rid"); }
        }
        private int mRid;

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
                OnPropertyChanged("start_date_date");
                OnPropertyChanged("start_date_time");
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

        private DateTime? mStart_date_date;
        public DateTime? start_date_date 
        {
            get { return mStart_date_date; }
            set 
            {
                mStart_date_date = value;
                if (start_date_time == string.Empty || start_date_time == null)
                {
                    start_date_time = "00:00";
                }
                OnPropertyChanged("start_date_date");
            }
        }

        private string mStartDateTime;
        public string start_date_time
        {
            get
            {
                return mStartDateTime;
            }

            set
            {
                if (!start_date_date.HasValue)
                {
                    start_date_date = DateTime.Today;
                    OnPropertyChanged("start_date_date");
                }
                try
                {
                    mStartDateTime = TimeSpan.ParseExact(value, "h\\:mm", null).ToString("hh\\:mm");
                    OnPropertyChanged("start_date_time");
                }
                catch (Exception)
                {
                    throw new ArgumentException("Start time format must be like 12:50");
                }
            }
        }

        [Column]
        public string time_limit_type { get; set; }

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

        public DateTime? time_limit_fixed_date { get; set; }
        private string mTimeLimitFixedTime;
        public string time_limit_fixed_time
        {
            get
            {
                return mTimeLimitFixedTime;
            }

            set
            {
                if (value != null)
                {
                    if (!time_limit_fixed_date.HasValue)
                    {
                        time_limit_fixed_date = DateTime.Today;
                        OnPropertyChanged("time_limit_fixed_date");
                    }
                    try
                    {
                        mTimeLimitFixedTime = TimeSpan.ParseExact(value, "h\\:mm", null).ToString("hh\\:mm");
                        OnPropertyChanged("time_limit_fixed_time");
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Time limit time format must be like 12:50");
                    }
                }
            }
        }

        [Column]
        public int? time_limit_delta
        {
            get
            {
                return mTimeLimitDelta;
            }
            set
            {
                mTimeLimitDelta = value;
                OnPropertyChanged("time_limit_delta");
                OnPropertyChanged("TimeLimitDelta");
            }
        }
        private int? mTimeLimitDelta;
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
                    return null;
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
                            throw new ArgumentException("Time limit delta must be in format '1 02:50' or '2:30'");
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
                            throw new ArgumentException("Time limit delta must be in format '1 02:50' or '2:30'");
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
        public string calendar_class { get { return mcalendar_class; } set { mcalendar_class = value; OnPropertyChanged("calendar_class"); } }
        private string mcalendar_class;
        [Column(Name = "event")]
        public string calendar_event { get { return mcalendar_event; } set { mcalendar_event = value; OnPropertyChanged("calendar_event"); } }
        private string mcalendar_event;
        [Column]
        public string price_code { get; set; }
        [Column]
        public string course { get; set; }
        [Column]
        public string ood { get; set; }
        [Column]
        public string venue { get; set; }
        [Column]
        public bool? average_lap { get; set; }
        [Column]
        public bool? timegate { get; set; }
        [Column]
        public string handicapping { get; set; }
        [Column]
        public int? visitors { get; set; }
        [Column]
        public string flag { get; set; }
        [Column]
        public string memo { get; set; }
        [Column]
        public bool? is_race { get; set; }
        [Column]
        public bool? raced { get; set; }
        [Column]
        public bool? approved { get; set; }
        [Column]
        public double? standard_corrected_time;
        public string SCT
        {
            get
            {
                if (standard_corrected_time.HasValue)
                {
                    TimeSpan t = new TimeSpan((long)(standard_corrected_time * 10000000));
                    return t.ToString("hh\\:mm\\:ss\\.ff");
                }
                return string.Empty;
            }
        }
        [Column]
        public DateTime? result_calculated { get; set; }
    }
}
