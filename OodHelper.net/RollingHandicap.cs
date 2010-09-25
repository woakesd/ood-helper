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
        protected override void CorrectedTime()
        {
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            //
            // Select all boats and work out elapsed, corrected and stdcorr
            //
            String sql = @"SELECT bid, rid, start_date, 
                CASE finish_code 
                    WHEN 'DNF' THEN NULL 
                    WHEN 'DSQ' THEN NULL 
                    ELSE finish_date END finish_date,
                rolling_handicap, open_handicap, laps, 
                elapsed, corrected, standard_corrected, place, performance_index
                FROM races
                WHERE rid = @rid";
            Db c = new Db(sql);
            DataTable dt = c.GetData(p);

            c = new Db(@"UPDATE races 
                SET elapsed = @elapsed
                , corrected = @corrected
                , standard_corrected = @standard_corrected
                , place = @place
                WHERE rid = @rid
                AND bid = @bid");

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["start_date"] != DBNull.Value && dr["finish_date"] != DBNull.Value && (!averageLap || dr["laps"] != DBNull.Value))
                {
                    p["bid"] = dr["bid"];
                    DateTime? s = dr["start_date"] as DateTime?;
                    DateTime? f = dr["finish_date"] as DateTime?;

                    TimeSpan? e = f - s;
                    p["elapsed"] = e.Value.TotalSeconds;

                    int? l = dr["laps"] as int?;

                    int hcap = (int)dr["rolling_handicap"];
                    int ohp = (int)dr["open_handicap"];

                    //
                    // if spec is 'a' then this is average lap so corrected times are per lap,
                    // otherwise assume everyone did same number of laps.
                    //
                    if (averageLap)
                    {
                        p["corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / hcap) / l.Value;
                        p["standard_corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / ohp) / l.Value;
                    }
                    else
                    {
                        p["corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / hcap);
                        p["standard_corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / ohp);
                    }
                    p["place"] = 0;

                    c.ExecuteNonQuery(p);
                }
            }
            //
            // Update the database.
            //
            //c.Commit(dt);
            dt.Dispose();
        }
    }
}
