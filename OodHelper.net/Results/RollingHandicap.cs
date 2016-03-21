using System;
using System.Data;
using System.Linq;

namespace OodHelper.Results
{
    internal class RollingHandicap : OpenHandicap
    {
        protected override void CorrectedTime()
        {
            EnumerableRowCollection<DataRow> query = (from r in Racedata.AsEnumerable()
                where r.Field<string>("finish_code") != "DNF"
                      && r.Field<string>("finish_code") != "DSQ"
                      && r.Field<DateTime?>("start_date") != null
                      && r.Field<DateTime?>("finish_date") != null
                      &&
                      (RaceType != CalendarModel.RaceTypes.HybridOld ||
                       r.Field<DateTime?>("interim_date") != null && r.Field<int?>("laps") != null)
                      &&
                      (RaceType != CalendarModel.RaceTypes.Hybrid ||
                       r.Field<DateTime?>("interim_date") != null && r.Field<int?>("laps") != null)
                      && (RaceType != CalendarModel.RaceTypes.AverageLap || r.Field<int?>("laps") != null)
                select r);

            //
            // Average laps
            //
            double avgLaps = Math.Round(query.Average(r => (r.Field<int?>("laps")) ?? 0), 1);

            //
            // Select all boats and work out elapsed, corrected and stdcorr
            //
            foreach (DataRow dr in query)
            {
                var start = dr["start_date"] as DateTime?;
                var finish = dr["finish_date"] as DateTime?;
                var interim = dr["interim_date"] as DateTime?;
                TimeSpan? fixedPart;
                TimeSpan? averageLapPart;

                var e = finish - start;
                dr["elapsed"] = e.Value.TotalSeconds;

                var laps = dr["laps"] as int?;

                var hcap = (int) dr["rolling_handicap"];
                var ohp = (int) dr["open_handicap"];

                //
                // if spec is 'a' then this is average lap so corrected times are per lap,
                // otherwise assume everyone did same number of laps.
                //
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.AverageLap:
                        dr["corrected"] = Math.Round(e.Value.TotalSeconds*1000/hcap)/laps.Value;
                        dr["standard_corrected"] = Math.Round(e.Value.TotalSeconds*1000/ohp)/laps.Value;
                        break;
                    case CalendarModel.RaceTypes.HybridOld:
                        fixedPart = interim - start;
                        averageLapPart = finish - interim;
                        dr["corrected"] = Math.Round(fixedPart.Value.TotalSeconds*1000/hcap) +
                                          Math.Round(averageLapPart.Value.TotalSeconds*1000/hcap)/laps.Value;
                        dr["standard_corrected"] = Math.Round(fixedPart.Value.TotalSeconds*1000/ohp) +
                                                   Math.Round(averageLapPart.Value.TotalSeconds*1000/ohp)/laps.Value;
                        break;
                    case CalendarModel.RaceTypes.Hybrid:
                        fixedPart = interim - start;
                        averageLapPart = finish - interim;
                        dr["corrected"] = Math.Round(fixedPart.Value.TotalSeconds*1000/hcap) +
                                          Math.Round(averageLapPart.Value.TotalSeconds*1000/hcap*avgLaps)/laps.Value;
                        dr["standard_corrected"] = Math.Round(fixedPart.Value.TotalSeconds*1000/ohp) +
                                                   Math.Round(averageLapPart.Value.TotalSeconds*1000/ohp*avgLaps)/
                                                   laps.Value;
                        break;
                    case CalendarModel.RaceTypes.FixedLength:
                    case CalendarModel.RaceTypes.TimeGate:
                        dr["corrected"] = Math.Round(e.Value.TotalSeconds*1000/hcap);
                        dr["standard_corrected"] = Math.Round(e.Value.TotalSeconds*1000/ohp);
                        break;
                }
                dr["place"] = 0;
            }
        }
    }
}