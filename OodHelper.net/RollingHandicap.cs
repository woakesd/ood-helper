using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace OodHelper
{
    [Svn("$Id$")]
    class RollingHandicap : OpenHandicap
    {
        protected override void CorrectedTime()
        {
            //
            // Select all boats and work out elapsed, corrected and stdcorr
            //
            foreach (DataRow dr in (from r in racedata.AsEnumerable()
                                    where r.Field<string>("finish_code") != "DNF"
                                        && r.Field<string>("finish_code") != "DSQ"
                                        && r.Field<DateTime?>("start_date") != null
                                        && r.Field<DateTime?>("finish_date") != null
                                        && (!averageLap || r.Field<int?>("laps") != null)
                                    select r))
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
        }
    }
}
