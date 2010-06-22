﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class RaceSeriesResult
    {
        public RaceSeriesResult(int SeriesId)
        {
            Db c = new Db("SELECT c.handicapping, c.rid " +
                "FROM calendar c " +
                "INNER JOIN calendar_series_join j ON c.rid = j.rid " +
                "WHERE j.sid = @sid " +
                "AND c.raced = 1 " + 
                "ORDER BY c.start_date");

            Hashtable p = new Hashtable();
            p["sid"] = SeriesId;
            DataTable races = c.GetData(p);

            foreach (DataRow race in races.Rows)
            {
                IRaceScore scorer;
                switch (race["handicapping"].ToString().ToUpper())
                {
                    case "R":
                        scorer = new RollingHandicap();
                        break;
                    default:
                        scorer = new OpenHandicap();
                        break;
                }
                scorer.Calculate((int)race["rid"]);
            }

            c = new Db("SELECT c.class, r.rid, r.start_date, bid, points, override_points, finish_code " +
                "FROM races r " +
                "LEFT JOIN calendar_series_join j ON r.rid = j.rid " +
                "LEFT JOIN calendar c ON c.rid = j.rid " +
                "WHERE j.sid = @sid " +
                "ORDER BY c.class");

            DataTable rd = c.GetData(p);

            //
            // This is a list of boat entries in a series' events.
            //
            SeriesEvent Event;

            //
            // And this is the list of all the series' events.
            //
            Dictionary<string, Dictionary<int, SeriesEvent>> SeriesData = new Dictionary<string,Dictionary<int,SeriesEvent>>();

            foreach (DataRow re in rd.Rows)
            {
                if (!SeriesData.ContainsKey(re["class"].ToString()))
                {
                    SeriesData.Add(re["class"].ToString(), new Dictionary<int, SeriesEvent>());
                }
                Dictionary<int, SeriesEvent> sData = SeriesData[re["class"].ToString()];
                if (!sData.ContainsKey((int)re["rid"]))
                    sData[(int)re["rid"]] = new SeriesEvent((int)re["rid"]);
                Event = sData[(int)re["rid"]];
                SeriesEntry se = new SeriesEntry();
                se.bid = (int)re["bid"];
                se.rid = Event.Rid;
                se.code = re["finish_code"].ToString().ToUpper();
                se.date = (DateTime)re["start_date"];
                if (re["points"] != DBNull.Value)
                    se.points = (double)re["points"];
                if (re["override_points"] != DBNull.Value)
                    se.override_points = (double)re["override_points"];
                Event.AddEntry(se);
            }

            Dictionary<string, SeriesResult> SeriesResults = new Dictionary<string,SeriesResult>();
            foreach (string className in SeriesData.Keys)
            {
                SeriesResult sr = new SeriesResult(SeriesData[className]);
                sr.Score();
                SeriesResults.Add(className, sr);
            }
        }
    }
}
