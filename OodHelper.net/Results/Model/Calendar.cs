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
            using (Db _conn = new Db(@"SELECT [start_date]
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

        public enum RaceTypes { Undefined, FixedLength, AverageLap, Hybrid, TimeGate, SternChase, }

        public int Rid { get; set; }

        public string Event { get { return Data["event"] as string; } }

        public RaceTypes RaceType { get; set; }

        public string EventClass { get; set; }

        public string Course { get; set; }

        public string WindSpeed { get; set; }

        public string WindDirection { get; set; }

        public int? Laps { get; set; }

        public string Ood { get; set; }

        public DateTime StartDate { get; set; }

        public string Handicap { get; set; }

        public int Extension { get; set; }

        public DateTime TimeLimitFixed { get; set; }

        public int TimeLimitDelta { get; set; }
    }
}
