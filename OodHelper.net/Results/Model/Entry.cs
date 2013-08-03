using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Windows;
using OodHelper.Converters;

namespace OodHelper.Results.Model
{
    public class Entry : IEntry
    {
        public static IList<IEntry> GetEntries(int RaceId, int? BoatId, DateTime? StartDate)
        {
            using (Db _conn = new Db(@"SELECT r.rid, r.bid, boatname, boatclass, sailno, r.start_date, " +
                    "r.finish_code, r.finish_date, r.interim_date, r.laps, r.override_points, r.elapsed, r.standard_corrected, r.corrected, r.place, " +
                    "r.points, r.open_handicap, r.rolling_handicap, r.achieved_handicap, " +
                    "r.new_rolling_handicap, r.handicap_status, r.c, r.a, r.performance_index " +
                    "FROM races r INNER JOIN boats ON boats.bid = r.bid " +
                    "WHERE r.rid = @rid " +
                    "AND boats.bid = ISNULL(@bid, boats.bid) " +
                    "ORDER BY place"))
            {
                Hashtable _para = new Hashtable();
                _para["rid"] = RaceId;
                _para["bid"] = BoatId;
                DataTable Data = _conn.GetData(_para);

                IList<IEntry> _entries = (from _ent in Data.AsEnumerable()
                                          select new Entry(_ent, StartDate) as IEntry).ToList();
                return _entries;
            }
        }

        private DataRow _row;
        public Entry(DataRow result, DateTime? StartDate)
        {
            _row = result;
            _startDate = StartDate;
        }

        public int rid { get { return (int)_row["rid"]; } set { } }
        public int bid { get { return (int)_row["bid"]; } set { } }
        public string boatname { get { return _row["boatname"] as string; } set { _row["boatname"] = value; } }
        public string boatclass { get { return _row["boatclass"] as string; } set { _row["boatclass"] = value; } }
        public string sailno { get { return _row["sailno"] as string; } set { _row["sailno"] = value; } }

        private DateTime? _startDate;

        public DateTime? start_date
        {
            get
            {
                return _row["start_date"] as DateTime?;
            }

            set
            {
                _row["start_date"] = value;
            }
        }

        public string finish_code
        {
            get
            {
                return _row["finish_code"] as string;
            }
            set
            {
                _row["finish_code"] = value;
            }
        }

        public DateTime? finish_date
        {
            get
            {
                return _row["finish_date"] as DateTime?;
            }

            set
            {
                _row["finish_date"] = value;
            }
        }

        public DateTime? interim_date
        {
            get
            {
                return _row["interim_date"] as DateTime?;
            }

            set
            {
                _row["interim_date"] = value;
            }
        }

        public double? override_points
        {
            get
            {
                return _row["override_points"] as double?;
            }
            set
            {
                _row["override_points"] = value;
            }
        }

        public int? laps
        {
            get
            {
                return _row["laps"] as int?;
            }
            set
            {
                _row["laps"] = value;
            }
        }

        public int? elapsed
        {
            get
            {
                return _row["elapsed"] as int?;
            }
            set
            {
                _row["elapsed"] = value;
            }
        }

        public double? corrected
        {
            get
            {
                return _row["corrected"] as double?;
            }
            set
            {
                _row["corrected"] = value;
            }
        }

        public double? standard_corrected
        {
            get
            {
                return _row["standard_corrected"] as double?;
            }
            set
            {
                _row["standard_corrected"] = value;
            }
        }

        public int? place
        {
            get
            {
                return _row["place"] as int?;
            }
            set
            {
                _row["place"] = value;
            }
        }

        public double? points
        {
            get
            {
                return _row["points"] as double?;
            }
            set
            {
                _row["points"] = value;
            }
        }

        public int? open_handicap
        {
            get
            {
                return _row["open_handicap"] as int?;
            }

            set
            {
                _row["open_handicap"] = value;
            }
        }

        public int? rolling_handicap
        {
            get
            {
                return _row["rolling_handicap"] as int?;
            }

            set
            {
                _row["rolling_handicap"] = value;
            }
        }

        public int? achieved_handicap
        {
            get
            {
                return _row["achieved_handicap"] as int?;
            }

            set
            {
                _row["achieved_handicap"] = value;
            }
        }

        public int? new_rolling_handicap
        {
            get
            {
                return _row["new_rolling_handicap"] as int?;
            }

            set
            {
                _row["new_rolling_handicap"] = value;
            }
        }

        public string handicap_status
        {
            get { return _row["handicap_status"] as string; }
            set { _row["handicap_status"] = value; }
        }

        public int? performance_index
        {
            get { return _row["performance_index"] as int?; }
            set { _row["performance_index"] = value; }
        }

        public string c
        {
            get { return _row["c"] as string; }
            set { _row["c"] = value; }
        }

        public string a
        {
            get { return _row["a"] as string; }
            set { _row["a"] = value; }
        }

        public void SaveChanges()
        {
            using (Db _conn = new Db())
            {
                _conn.Sql = @"MERGE races AS tgt
USING (SELECT @achieved_handicap achieved_handicap
    , @a a
    , @c c
    , @corrected corrected
    , @elapsed elapsed
    , @finish_code finish_code
    , @finish_date finish_date
    , @handicap_status handicap_status
    , @interim_date interim_date
    , @laps laps
	, @last_edit last_edit
    , @new_rolling_handicap new_rolling_handicap
    , @open_handicap open_handicap
    , @override_points override_points
	, @performance_index performance_index
    , @place place
    , @points points
    , @rolling_handicap rolling_handicap
    , @standard_corrected standard_corrected
    , @start_date start_date
    , @bid bid
    , @rid rid) AS src
ON src.bid = tgt.bid and src.rid = tgt.rid
WHEN MATCHED THEN
UPDATE SET start_date = src.start_date
      ,finish_code = src.finish_code
      ,finish_date = src.finish_date
      ,interim_date = src.interim_date
      ,last_edit = src.last_edit
      ,laps = src.laps
      ,place = src.place
      ,points = src.points
      ,override_points = src.override_points
      ,elapsed = src.elapsed
      ,corrected = src.corrected
      ,standard_corrected = src.standard_corrected
      ,handicap_status = src.handicap_status
      ,open_handicap = src.open_handicap
      ,rolling_handicap = src.rolling_handicap
      ,achieved_handicap = src.achieved_handicap
      ,new_rolling_handicap = src.new_rolling_handicap
      ,performance_index = src.performance_index
      ,a = src.a
      ,c = src.c
WHEN NOT MATCHED THEN
INSERT ([rid]
    ,[bid]
    ,[start_date]
    ,[finish_code]
    ,[finish_date]
    ,[interim_date]
    ,[last_edit]
    ,[laps]
    ,[place]
    ,[points]
    ,[override_points]
    ,[elapsed]
    ,[corrected]
    ,[standard_corrected]
    ,[handicap_status]
    ,[open_handicap]
    ,[rolling_handicap]
    ,[achieved_handicap]
    ,[new_rolling_handicap]
    ,[performance_index]
    ,[a]
    ,[c])
VALUES (src.rid
    ,src.bid
    ,src.start_date
    ,src.finish_code
    ,src.finish_date
    ,src.interim_date
    ,src.last_edit
    ,src.laps
    ,src.place
    ,src.points
    ,src.override_points
    ,src.elapsed
    ,src.corrected
    ,src.standard_corrected
    ,src.handicap_status
    ,src.open_handicap
    ,src.rolling_handicap
    ,src.achieved_handicap
    ,src.new_rolling_handicap
    ,src.performance_index
    ,src.a
    ,src.c);";
                Hashtable _para = new Hashtable();
                _para["rid"] = rid;
                _para["bid"] = bid;
                _para["start_date"] = start_date;
                _para["finish_code"] = finish_code;
                _para["finish_date"] = finish_date;
                _para["interim_date"] = interim_date;
                _para["last_edit"] = DateTime.Now;
                _para["laps"] = laps;
                _para["place"] = place;
                _para["points"] = points;
                _para["override_points"] = override_points;
                _para["elapsed"] = elapsed;
                _para["corrected"] = corrected;
                _para["standard_corrected"] = standard_corrected;
                _para["handicap_status"] = handicap_status;
                _para["open_handicap"] = open_handicap;
                _para["rolling_handicap"] = rolling_handicap;
                _para["achieved_handicap"] = achieved_handicap;
                _para["new_rolling_handicap"] = new_rolling_handicap;
                _para["performance_index"] = performance_index;
                _para["a"] = a;
                _para["c"] = c;

                _conn.ExecuteNonQuery(_para);
            }
        }
    }
}
