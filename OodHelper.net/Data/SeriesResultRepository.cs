using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OodHelper.Data
{
    // Bind the bare name to the EF entity, not the legacy OodHelper.SeriesResult domain class that is
    // visible here via the parent namespace and would otherwise win. This alias must live inside the
    // namespace scope to take precedence over the enclosing namespace (same as the context /
    // ResultsDownloadService / ResultsUploadService). Re-apply if the context is re-scaffolded.
    using SeriesResult = OodHelper.Data.Entities.SeriesResult;

    internal sealed class SeriesResultRepository : ISeriesResultRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public SeriesResultRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public SeriesResultHeader? GetSeriesHeader(int sid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var s = ctx.Series.AsNoTracking().FirstOrDefault(x => x.Sid == sid);
                return s == null ? null : new SeriesResultHeader(s.Sname, s.Discards);
            }
        }

        public IReadOnlyList<SeriesRaceToScore> GetRacesToScore(int sid)
        {
            //
            // Old SQL: calendar INNER JOIN calendar_series_join INNER JOIN races, WHERE sid + raced,
            // GROUP BY the calendar columns. The group key includes the calendar PK (rid), so the
            // grouping just de-duplicates the races-join fan-out — i.e. distinct raced calendar rows
            // in the series that have at least one race row. Expressed directly below.
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.CalendarSeriesJoins
                    .Where(j => j.Sid == sid)
                    .Join(ctx.Calendars.Where(c => c.Raced == true), j => j.Rid, c => c.Rid, (j, c) => c)
                    .Where(c => ctx.Races.Any(r => r.Rid == c.Rid))
                    .OrderBy(c => c.StartDate)
                    .Select(c => new SeriesRaceToScore(c.Rid, c.Racetype, c.Handicapping, c.Event, c.Class))
                    .ToList();
            }
        }

        public IReadOnlyList<SeriesEntryRow> GetEntryRows(int sid)
        {
            //
            // Old SQL: races LEFT JOIN calendar_series_join LEFT JOIN calendar, WHERE sid + raced +
            // (finish_code OR finish_date not null). The sid/raced predicates make the joins behave as
            // inner joins. start_date/class come from calendar; points/codes from races.
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Races
                    .Join(ctx.CalendarSeriesJoins.Where(j => j.Sid == sid), r => r.Rid, j => j.Rid,
                        (r, j) => r)
                    .Join(ctx.Calendars, r => r.Rid, c => c.Rid, (r, c) => new { r, c })
                    .Where(x => x.c.Raced == true && (x.r.FinishCode != null || x.r.FinishDate != null))
                    .OrderBy(x => x.c.Class)
                    .Select(x => new { x.c.Class, x.r.Rid, x.c.StartDate, x.r.Bid, x.r.Points, x.r.OverridePoints, x.r.FinishCode })
                    .AsEnumerable()
                    .Select(x => new SeriesEntryRow(x.Class, x.Rid, x.StartDate!.Value, x.Bid, x.Points, x.OverridePoints, x.FinishCode))
                    .ToList();
            }
        }

        public void SaveSeriesResults(int sid, string division, IReadOnlyList<SeriesResultRow> rows)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                ctx.SeriesResults
                    .Where(r => r.Sid == sid && r.Division == division)
                    .ExecuteDelete();

                foreach (var row in rows)
                {
                    ctx.SeriesResults.Add(new SeriesResult
                    {
                        Sid = sid,
                        Bid = row.Bid,
                        Division = division,
                        Entered = row.Entered,
                        Gross = row.Gross,
                        Nett = row.Nett,
                        Place = row.Place
                    });
                }

                ctx.SaveChanges();
            }
        }

        public IReadOnlyDictionary<int, BoatDisplayInfo> GetBoats(IReadOnlyCollection<int> bids)
        {
            var set = bids.ToHashSet();
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Boats.AsNoTracking()
                    .Where(b => set.Contains(b.Bid))
                    .ToDictionary(b => b.Bid, b => new BoatDisplayInfo(b.Boatname, b.Boatclass, b.Sailno));
            }
        }
    }
}
