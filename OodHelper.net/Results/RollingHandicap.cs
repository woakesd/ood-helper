using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using OodHelper.Results;

namespace OodHelper
{
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
                                        && (RaceType != CalendarModel.RaceTypes.Hybrid || r.Field<DateTime?>("finish_date_2") != null && r.Field<int?>("laps") != null)
                                        && (RaceType != CalendarModel.RaceTypes.AverageLap || r.Field<int?>("laps") != null)
                                    select r))
            {
                DateTime? _start = dr["start_date"] as DateTime?;
                DateTime? _finish = dr["finish_date"] as DateTime?;
                DateTime? _interim = dr["finish_date_2"] as DateTime?;
                TimeSpan? _fixedPart = null;
                TimeSpan? _averageLapPart = null;

                if (RaceType == CalendarModel.RaceTypes.Hybrid)
                {
                    _fixedPart = _interim - _start;
                    _averageLapPart = _finish - _interim;
                }

                TimeSpan? e = _finish - _start;
                dr["elapsed"] = e.Value.TotalSeconds;

                int? _laps = dr["laps"] as int?;

                int hcap = (int)dr["rolling_handicap"];
                int ohp = (int)dr["open_handicap"];

                //
                // if spec is 'a' then this is average lap so corrected times are per lap,
                // otherwise assume everyone did same number of laps.
                //
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.AverageLap:
                        dr["corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / hcap) / _laps.Value;
                        dr["standard_corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / ohp) / _laps.Value;
                        break;
                    case CalendarModel.RaceTypes.Hybrid:
                        dr["corrected"] = Math.Round(_fixedPart.Value.TotalSeconds * 1000 / hcap) +
                            Math.Round(_averageLapPart.Value.TotalSeconds * 1000 / hcap / _laps.Value);
                        dr["standard_corrected"] = Math.Round(_fixedPart.Value.TotalSeconds * 1000 / ohp) +
                            Math.Round(_averageLapPart.Value.TotalSeconds * 1000 / ohp / _laps.Value);
                        break;
                    case CalendarModel.RaceTypes.FixedLength:
                    case CalendarModel.RaceTypes.TimeGate:
                        dr["corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / hcap);
                        dr["standard_corrected"] = Math.Round(e.Value.TotalSeconds * 1000 / ohp);
                        break;
                }
                dr["place"] = 0;
            }
        }
    }
}
