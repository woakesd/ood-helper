using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class RollingHandicap : OpenHandicap
    {
        public override void CorrectedTime()
        {
            //
            // update all boats setting elapsed, corrected, stdcorr, place and pts.
            //
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            string sql = @"UPDATE races
                SET elapsed = NULL
                ,corrected = NULL
                ,standard_corrected = NULL
                ,place = 999
                ,points = NULL
                ,performance_index = NULL
                ,c = NULL
                ,a = NULL
                WHERE rid = @rid";
            Db c = new Db(sql);
            c.ExecuteNonQuery(p);
    
            //
            // Next select all boats and work out elapsed, corrected and stdcorr
            //
            sql = @"SELECT bid, rid, start_date, 
                CASE finish_code 
                    WHEN 'DNF' THEN NULL 
                    WHEN 'DSQ' THEN NULL 
                    ELSE finish_date END finish_date,
                rolling_handicap, open_handicap, laps, 
                elapsed, corrected, standard_corrected, place, performance_index
                FROM races
                WHERE rid = @rid";
            c = new Db(sql);
            DataTable dt = c.GetData(p);

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["start_date"] != DBNull.Value && dr["finish_date"] != DBNull.Value && (!averageLap || dr["laps"] != DBNull.Value))
                {
                    DateTime? s = dr["start_date"] as DateTime?;
                    DateTime? f = dr["finish_date"] as DateTime?;

                    TimeSpan? e = f - s;
                    dr["elapsed"] = e.Value.TotalSeconds;

                    int? l = dr["laps"] as int?;

                    int hcap = (int)dr["rolling_handicap"];
                    int ohp = (int)dr["open_handicap"];

                    //
                    // if spec is 'a' then this is average lap so corrected times are per lap,
                    // otherwise assume everyone did same number of laps.
                    //
                    if (averageLap)
                    {
                        dr["corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / hcap) / l.Value;
                        dr["standard_corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / ohp) / l.Value;
                    }
                    else
                    {
                        dr["corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / hcap);
                        dr["standard_corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / ohp);
                    }
                    dr["place"] = 0;
                }
            }
            //
            // Update the database.
            //
            c.Commit(dt);
            dt.Dispose();
        }
    }
}
