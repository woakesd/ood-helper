using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.Results.Model
{
    public class Calendar
    {
        private Hashtable Data;

        public Calendar(int RaceId)
        {
            this.Rid = RaceId;
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

        public enum RaceTypes { Undefined, FixedLength, AverageLap, Hybrid, TimeGate, SternChase, }

        public int Rid { get; set; }

        public string Event { get { return Data["event"] as string; } }

        public RaceTypes RaceType
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
                Data["racetype"] = value;
            }
        }

        public string EventClass { get { return Data["class"] as string; } }

        public string Course { get { return Data["course"] as string; } set { Data["course"] = value; } }

        public string WindSpeed { get { return Data["wind_speed"] as string; } set { Data["wind_speed"] = value; } }

        public string WindDirection { get { return Data["wind_direction"] as string; } set { Data["wind_direction"] = value; } }

        public int? Laps { get { return Data["laps"] as int?; } set { Data["laps"] = value; } }

        public string Ood { get { return Data["ood"] as string; } set { Data["ood"] = value; } }

        public DateTime? StartDate { get { return Data["start_date"] as DateTime?; } set { Data["start_date"] = value; } }

        public string Handicap { get; set; }

        public int? Extension { get; set; }

        public enum TimeLimitTypes { Undefined, Fixed, Delta, }

        public TimeLimitTypes TimeLimitType
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
            set { Data["time_limit_type"] = value; }
        }

        public DateTime? TimeLimitFixed { get { return Data["time_limit_fixed"] as DateTime?; } set { Data["time_limit_fixed"] = value; } }

        public int? TimeLimitDelta { get; set; }
    }
}
