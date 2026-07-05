using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    internal sealed class RaceResultsRepository : IRaceResultsRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public RaceResultsRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // -------------------------------------------------------------------------------------
        // Reads
        // -------------------------------------------------------------------------------------

        public Calendar? GetCalendar(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Calendars.AsNoTracking().FirstOrDefault(c => c.Rid == rid);
            }
        }

        public IReadOnlyList<Race> GetRaceRows(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Races
                    .AsNoTracking()
                    .Include(r => r.BidNavigation)
                    .Where(r => r.Rid == rid)
                    .OrderBy(r => r.Place)
                    .ToList();
            }
        }

        public bool GetCalculateEnabled(int rid)
        {
            //
            // Calculation is allowed when results have never been calculated, or a race row has
            // been edited since the last calculation. (Equivalent to the original grouped TOP-1
            // query: result_calculated IS NULL, or the max last_edit is at/after result_calculated.)
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var cal = ctx.Calendars.AsNoTracking().FirstOrDefault(c => c.Rid == rid);
                if (cal == null)
                    return false;
                if (cal.ResultCalculated == null)
                    return true;
                var maxLastEdit = ctx.Races.Where(r => r.Rid == rid).Max(r => (DateTime?)r.LastEdit);
                return maxLastEdit != null && cal.ResultCalculated <= maxLastEdit;
            }
        }

        public bool GetRefreshHandicapsEnabled(int rid)
        {
            //
            // A refresh is offered when some boat in this race has an earlier race (for that boat)
            // that has been edited more recently than this race row — i.e. its rolling handicap may
            // now be stale. (Set-based original re-expressed in memory; the row counts per race are
            // small.)
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var ridRows = ctx.Races.AsNoTracking()
                    .Where(r => r.Rid == rid)
                    .Select(r => new { r.Bid, r.StartDate, r.LastEdit })
                    .ToList();
                if (ridRows.Count == 0)
                    return false;

                var bids = ridRows.Select(r => r.Bid).Distinct().ToList();
                var boatRaces = ctx.Races.AsNoTracking()
                    .Where(r => bids.Contains(r.Bid) && r.Rid != rid)
                    .Select(r => new { r.Bid, r.StartDate, r.LastEdit })
                    .ToList()
                    .ToLookup(r => r.Bid);

                return ridRows.Any(r1 => boatRaces[r1.Bid]
                    .Any(r2 => r2.StartDate < r1.StartDate && r2.LastEdit > r1.LastEdit));
            }
        }

        public int CountAutoPopulate(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                return GetAutoPopulateBids(ctx, rid).Count;
        }

        // -------------------------------------------------------------------------------------
        // Writes (immediate, per the agreed persistence model)
        // -------------------------------------------------------------------------------------

        public void UpdateRaceRow(Race row)
        {
            //
            // Persist a single race row. We write all scalar columns (equivalent to the old
            // dynamic UPDATE) via a detached clone so the navigation properties on the caller's
            // entity are never touched/cascaded.
            //
            var clone = new Race
            {
                Rid = row.Rid,
                Bid = row.Bid,
                StartDate = row.StartDate,
                FinishCode = row.FinishCode,
                FinishDate = row.FinishDate,
                InterimDate = row.InterimDate,
                RestrictedSail = row.RestrictedSail,
                LastEdit = DateTime.Now,
                Laps = row.Laps,
                Place = row.Place,
                Points = row.Points,
                OverridePoints = row.OverridePoints,
                Elapsed = row.Elapsed,
                Corrected = row.Corrected,
                StandardCorrected = row.StandardCorrected,
                HandicapStatus = row.HandicapStatus,
                OpenHandicap = row.OpenHandicap,
                RollingHandicap = row.RollingHandicap,
                AchievedHandicap = row.AchievedHandicap,
                NewRollingHandicap = row.NewRollingHandicap,
                PerformanceIndex = row.PerformanceIndex,
                A = row.A,
                C = row.C
            };
            using (var ctx = _contextFactory.CreateDbContext())
            {
                ctx.Races.Attach(clone);
                ctx.Entry(clone).State = EntityState.Modified;
                ctx.SaveChanges();
            }
        }

        public void UpdateCourse(int rid, string course)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.CourseChoice, course));
        }

        public void UpdateWindSpeed(int rid, string windSpeed)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.WindSpeed, windSpeed));
        }

        public void UpdateWindDirection(int rid, string windDirection)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.WindDirection, windDirection));
        }

        public void UpdateLaps(int rid, int? laps)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.LapsCompleted, laps));
        }

        public void UpdateRaceType(int rid, string raceType)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.Racetype, raceType));
        }

        public void UpdateStartDate(int rid, DateTime startDate)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                ctx.Races.Where(r => r.Rid == rid)
                    .ExecuteUpdate(s => s
                        .SetProperty(r => r.StartDate, startDate)
                        .SetProperty(r => r.LastEdit, DateTime.Now));
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.StartDate, startDate));
            }
        }

        public void UpdateTimeLimitFixed(int rid, DateTime timeLimitFixed)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.TimeLimitFixed, timeLimitFixed));
        }

        public void UpdateTimeLimitDelta(int rid, int timeLimitDeltaSeconds)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.TimeLimitDelta, timeLimitDeltaSeconds));
        }

        public void UpdateExtension(int rid, int extensionSeconds)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.Extension, extensionSeconds));
        }

        // -------------------------------------------------------------------------------------
        // Bulk operations
        // -------------------------------------------------------------------------------------

        public void DoAutoPopulate(int rid)
        {
            //
            // Add a race entry for every boat that has raced in another race of the same series and
            // class, seeded with that boat's current handicap fields (the old INSERT … SELECT).
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var startDate = ctx.Calendars.Where(c => c.Rid == rid)
                    .Select(c => c.StartDate).FirstOrDefault();
                var bids = GetAutoPopulateBids(ctx, rid);
                if (bids.Count == 0)
                    return;

                var boats = ctx.Boats.AsNoTracking()
                    .Where(b => bids.Contains(b.Bid))
                    .ToList();
                var now = DateTime.Now;
                ctx.Races.AddRange(boats.Select(b => new Race
                {
                    Rid = rid,
                    Bid = b.Bid,
                    StartDate = startDate,
                    RollingHandicap = b.RollingHandicap,
                    HandicapStatus = b.HandicapStatus,
                    OpenHandicap = b.OpenHandicap,
                    LastEdit = now
                }));
                ctx.SaveChanges();
            }
        }

        public void RefreshRollingHandicaps(int rid)
        {
            //
            // Set each boat's rolling handicap in this race to the new_rolling_handicap from its
            // latest previous race entry (in-memory equivalent of the old UPDATE … FROM self-join).
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var ridRows = ctx.Races.Where(r => r.Rid == rid).ToList();
                if (ridRows.Count == 0)
                    return;

                var bids = ridRows.Select(r => r.Bid).Distinct().ToList();
                var boatRaces = ctx.Races.AsNoTracking()
                    .Where(r => bids.Contains(r.Bid) && r.Rid != rid)
                    .Select(r => new { r.Bid, r.StartDate, r.NewRollingHandicap })
                    .ToList()
                    .ToLookup(r => r.Bid);

                var now = DateTime.Now;
                foreach (var r1 in ridRows)
                {
                    var latestPrior = boatRaces[r1.Bid]
                        .Where(r => r.StartDate < r1.StartDate)
                        .OrderByDescending(r => r.StartDate)
                        .FirstOrDefault();
                    if (latestPrior == null)
                        continue;
                    r1.RollingHandicap = latestPrior.NewRollingHandicap;
                    r1.LastEdit = now;
                }
                ctx.SaveChanges();
            }
        }

        public void MoveToFleet(int fromRid, int toRid, int bid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var toStart = ctx.Calendars.Where(c => c.Rid == toRid)
                    .Select(c => c.StartDate).FirstOrDefault();
                ctx.Races.Where(r => r.Rid == fromRid && r.Bid == bid)
                    .ExecuteUpdate(s => s
                        .SetProperty(r => r.Rid, toRid)
                        .SetProperty(r => r.StartDate, toStart));
            }
        }

        public void ApplyEditedBoatHandicaps(int rid, int bid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var boat = ctx.Boats.Where(b => b.Bid == bid)
                    .Select(b => new { b.RollingHandicap, b.HandicapStatus, b.OpenHandicap })
                    .FirstOrDefault();
                if (boat == null)
                    return;

                var now = DateTime.Now;
                ctx.Races.Where(r => r.Rid == rid && r.Bid == bid)
                    .ExecuteUpdate(s => s
                        .SetProperty(r => r.RollingHandicap, boat.RollingHandicap)
                        .SetProperty(r => r.HandicapStatus, boat.HandicapStatus)
                        .SetProperty(r => r.OpenHandicap, boat.OpenHandicap)
                        .SetProperty(r => r.LastEdit, now));
            }
        }

        // -------------------------------------------------------------------------------------
        // Entry editing (SelectBoats)
        // -------------------------------------------------------------------------------------

        public void AddRaceEntry(int rid, int bid, DateTime startDate, string? handicapStatus,
            int? openHandicap, int? rollingHandicap)
        {
            var row = new Race
            {
                Rid = rid,
                Bid = bid,
                StartDate = startDate,
                HandicapStatus = handicapStatus,
                OpenHandicap = openHandicap,
                RollingHandicap = rollingHandicap,
                LastEdit = DateTime.Now
            };
            using (var ctx = _contextFactory.CreateDbContext())
            {
                ctx.Races.Add(row);
                ctx.SaveChanges();
            }
        }

        public void DeleteRaceEntry(int rid, int bid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Races.Where(r => r.Rid == rid && r.Bid == bid).ExecuteDelete();
        }

        // -------------------------------------------------------------------------------------
        // Print pages
        // -------------------------------------------------------------------------------------

        public IReadOnlyList<RacePrintRow> GetPrintRows(int rid, bool rolling)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var rows = ctx.Races.AsNoTracking()
                    .Where(r => r.Rid == rid && (r.FinishDate != null || r.FinishCode != null))
                    .Join(ctx.Boats, r => r.Bid, b => b.Bid, (r, b) => new { r, b })
                    .OrderBy(x => x.r.Place)
                    .ToList();

                return rows.Select(x =>
                {
                    var r = x.r;
                    var b = x.b;
                    int? hcap = rolling ? r.RollingHandicap : r.OpenHandicap;
                    double? points = r.OverridePoints ?? r.Points;
                    double? percent = (r.AchievedHandicap.HasValue && r.OpenHandicap.HasValue && r.OpenHandicap.Value != 0)
                        ? Math.Round((r.AchievedHandicap.Value - r.OpenHandicap.Value) * 100.0 / r.OpenHandicap.Value, 1)
                        : (double?)null;
                    string boat = b.Boatname + (r.RestrictedSail == true ? " (RS)" : string.Empty);
                    return new RacePrintRow(boat, b.Boatclass, b.Sailno, hcap, r.FinishCode, r.FinishDate,
                        r.Elapsed, r.Laps, r.Corrected, r.Place, points, r.AchievedHandicap,
                        r.NewRollingHandicap, percent, r.C, r.A, r.HandicapStatus);
                }).ToList();
            }
        }

        // -------------------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------------------

        //
        // Distinct boats that have raced in another race of the same series and class as the given
        // race. Used by both CountAutoPopulate and DoAutoPopulate. Re-expresses the old self-join over
        // calendar_series_join + series (event LIKE sname%) as LINQ.
        //
        private static List<int> GetAutoPopulateBids(OodHelperContext ctx, int rid)
        {
            var current = ctx.Calendars.AsNoTracking()
                .Where(c => c.Rid == rid)
                .Select(c => new { c.Event, c.Class })
                .FirstOrDefault();
            if (current == null)
                return new List<int>();

            string? namePrefixSource = current.Event;
            string? raceClass = current.Class;

            var query =
                from cs1 in ctx.CalendarSeriesJoins
                where cs1.Rid == rid
                join s in ctx.Series on cs1.Sid equals s.Sid
                where namePrefixSource != null && EF.Functions.Like(namePrefixSource, s.Sname + "%")
                join cs2 in ctx.CalendarSeriesJoins on cs1.Sid equals cs2.Sid
                join c2 in ctx.Calendars on cs2.Rid equals c2.Rid
                where c2.Rid != rid && c2.Class == raceClass
                join r in ctx.Races on c2.Rid equals r.Rid
                select r.Bid;

            return query.Distinct().ToList();
        }
    }
}
