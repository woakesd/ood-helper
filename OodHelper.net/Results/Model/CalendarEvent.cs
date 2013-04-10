using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.Results.Model
{
    public class CalendarEvent : OodHelper.Results.Model.ICalendarEvent
    {
        private Hashtable Data;

        public CalendarEvent(int RaceId)
        {
            this.rid = RaceId;
            using (Db _conn = new Db(@"SELECT [rid]
      ,[start_date]
      ,[time_limit_type]
      ,[time_limit_fixed]
      ,[time_limit_delta]
      ,[extension]
      ,[class]
      ,[event]
      ,[price_code]
      ,[course]
      ,[ood]
      ,[venue]
      ,[racetype]
      ,[handicapping]
      ,[visitors]
      ,[flag]
      ,[memo]
      ,[is_race]
      ,[raced]
      ,[approved]
      ,[course_choice]
      ,[laps_completed]
      ,[wind_speed]
      ,[wind_direction]
      ,[standard_corrected_time]
      ,[result_calculated]
  FROM [calendar]
  WHERE [rid] = @rid"))
            {
                Hashtable _para = new Hashtable();
                _para["rid"] = RaceId;
                Data = _conn.GetHashtable(_para);
            }
        }

        public void Update()
        {
            if (rid == 0)
            {
            }
            else
            {
                using (Db _conn = new Db(@"UPDATE [calendar]
   SET [start_date] = @start_date
      ,[time_limit_type] = @time_limit_type
      ,[time_limit_fixed] = @time_limit_fixed
      ,[time_limit_delta] = @time_limit_delta
      ,[extension] = @extension
      ,[class] = @class
      ,[event] = @event
      ,[price_code] = @price_code
      ,[course] = @course
      ,[ood] = @ood
      ,[venue] = @venue
      ,[racetype] = @racetype
      ,[handicapping] = @handicapping
      ,[visitors] = @visitors
      ,[flag] = @flag
      ,[memo] = @memo
      ,[is_race] = @is_race
      ,[raced] = @raced
      ,[approved] = @approved
      ,[course_choice] = @course_choice
      ,[laps_completed] = @laps_completed
      ,[wind_speed] = @wind_speed
      ,[wind_direction] = @wind_direction
      ,[standard_corrected_time] = @standard_corrected_time
      ,[result_calculated] = @result_calculated
 WHERE [rid] = @rid"))
                {
                    _conn.ExecuteNonQuery(Data);
                }
            }
        }

        public int rid { get; set; }

        public string eventName { get { return Data["event"] as string; } }

        public enum RaceTypes { Undefined, FixedLength, AverageLap, Hybrid, TimeGate, SternChase, }

        public RaceTypes racetype
        {
            get
            {
                RaceTypes _tmp = RaceTypes.Undefined;
                if (Data["racetype"] != DBNull.Value)
                {
                    if (!Enum.TryParse<RaceTypes>(Data["racetype"].ToString(), out _tmp))
                        ;
                }
                return _tmp;
            }
            set
            {
                if (value != RaceTypes.Undefined)
                    Data["racetype"] = value.ToString();
                else
                    Data["handicapping"] = null;
            }
        }

        public string eventClass { get { return Data["class"] as string; } }

        public string course { get { return Data["course"] as string; } set { Data["course"] = value; } }

        public string wind_speed { get { return Data["wind_speed"] as string; } set { Data["wind_speed"] = value; } }

        public string wind_direction { get { return Data["wind_direction"] as string; } set { Data["wind_direction"] = value; } }

        public int? laps { get { return Data["laps"] as int?; } set { Data["laps"] = value; } }

        public string ood { get { return Data["ood"] as string; } set { Data["ood"] = value; } }

        public DateTime? start_date { get { return Data["start_date"] as DateTime?; } set { Data["start_date"] = value; } }

        public enum Handicappings { Undefined, o, r, }

        public Handicappings handicapping
        {
            get
            {
                Handicappings _tmp = Handicappings.Undefined;
                if (!Enum.TryParse<Handicappings>(Data["handicapping"].ToString(), out _tmp))
                    ;
                return _tmp;
            }
            set
            {
                if (value != Handicappings.Undefined)
                    Data["handicapping"] = value.ToString();
                else
                    Data["handicapping"] = null;
            }
        }

        public int? extension { get { return Data["extension"] as int?; } set { Data["extension"] = value; } }

        public enum TimeLimitTypes { Undefined, F, D, }

        public TimeLimitTypes time_limit_type
        {
            get
            {
                TimeLimitTypes _tmp = TimeLimitTypes.Undefined;
                if (Data["time_limit_type"] != DBNull.Value)
                {
                    if (!Enum.TryParse<TimeLimitTypes>(Data["time_limit_type"].ToString(), out _tmp))
                        ;
                }
                return _tmp;
            }
            set
            {
                if (value != TimeLimitTypes.Undefined)
                    Data["time_limit_type"] = value.ToString();
                else
                    Data["time_limit_type"] = null;
            }
        }

        public DateTime? time_limit_fixed { get { return Data["time_limit_fixed"] as DateTime?; } set { Data["time_limit_fixed"] = value; } }

        public int? time_limit_delta { get { return Data["time_limit_delta"] as int?; } set { Data["time_limit_delta"] = value; } }

        public double? standard_corrected_time { get { return Data["standard_corrected_time"] as double?; } set { Data["standard_corrected_time"] = value; } }
    }
}
