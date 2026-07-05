using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    internal sealed class RaceScoreRepository : IRaceScoreRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public RaceScoreRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // -------------------------------------------------------------------------------------
        // Reads
        // -------------------------------------------------------------------------------------

        public RaceScoreHeader GetHeader(int rid)
        {
            //
            // Header read ported from OpenHandicap: the time limit is resolved from the F/D type,
            // and last_edit is the max across the race's rows.
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var c = ctx.Calendars.AsNoTracking()
                    .Where(x => x.Rid == rid)
                    .Select(x => new
                    {
                        x.Racetype,
                        x.StandardCorrectedTime,
                        x.ResultCalculated,
                        x.TimeLimitType,
                        x.TimeLimitFixed,
                        x.TimeLimitDelta,
                        x.StartDate,
                        x.Extension
                    })
                    .FirstOrDefault();
                if (c == null)
                    return null;

                var maxLastEdit = ctx.Races.Where(r => r.Rid == rid).Max(r => (DateTime?)r.LastEdit);

                DateTime? timeLimit;
                if (c.TimeLimitType == "F")
                    timeLimit = c.TimeLimitFixed;
                else if (c.TimeLimitType == "D" && c.StartDate.HasValue && c.TimeLimitDelta.HasValue)
                    timeLimit = c.StartDate.Value.AddSeconds(c.TimeLimitDelta.Value);
                else
                    timeLimit = null;

                return new RaceScoreHeader(
                    RaceType: c.Racetype,
                    StandardCorrectedTime: c.StandardCorrectedTime,
                    ResultCalculated: c.ResultCalculated,
                    MaxLastEdit: maxLastEdit,
                    TimeLimit: timeLimit,
                    Extension: c.Extension);
            }
        }

        public IList<Race> GetScoringRows(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Races
                    .AsNoTracking()
                    .Where(r => r.Rid == rid)
                    .OrderBy(r => r.Bid)
                    .ToList();
            }
        }

        public IReadOnlyDictionary<int, int> GetPreviousNewRollingHandicaps(int rid)
        {
            //
            // For each boat in the race, the new rolling handicap of its most recent prior race
            // (in-memory equivalent of the old grouped self-join). Boats whose prior race has a null
            // new_rolling_handicap are omitted, so the caller falls back to the open handicap.
            //
            var result = new Dictionary<int, int>();
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var ridRows = ctx.Races.AsNoTracking()
                    .Where(r => r.Rid == rid)
                    .Select(r => new { r.Bid, r.StartDate })
                    .ToList();
                if (ridRows.Count == 0)
                    return result;

                var bids = ridRows.Select(r => r.Bid).Distinct().ToList();
                var boatRaces = ctx.Races.AsNoTracking()
                    .Where(r => bids.Contains(r.Bid))
                    .Select(r => new { r.Bid, r.StartDate, r.NewRollingHandicap })
                    .ToList()
                    .ToLookup(r => r.Bid);

                foreach (var r1 in ridRows)
                {
                    var latestPrior = boatRaces[r1.Bid]
                        .Where(r => r.StartDate < r1.StartDate)
                        .OrderByDescending(r => r.StartDate)
                        .FirstOrDefault();
                    if (latestPrior != null && latestPrior.NewRollingHandicap.HasValue)
                        result[r1.Bid] = latestPrior.NewRollingHandicap.Value;
                }
            }
            return result;
        }

        public double? GetPriorPerformancePercent(int rid, int bid, DateTime beforeStart)
        {
            //
            // The boat's most recent scored race (other than this one, on or before the given start)
            // and its achieved-vs-open performance percentage. Null filters mirror the original SQL's
            // three-valued logic (place/standard_corrected_time NULLs are excluded).
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var top = ctx.Races.AsNoTracking()
                    .Where(r => r.Bid == bid
                                && r.Rid != rid
                                && r.Place.HasValue && r.Place.Value != 999
                                && r.StartDate <= beforeStart)
                    .Join(ctx.Calendars, r => r.Rid, c => c.Rid, (r, c) => new { r, c })
                    .Where(x => x.c.StandardCorrectedTime.HasValue && x.c.StandardCorrectedTime.Value != 0)
                    .OrderByDescending(x => x.r.StartDate)
                    .Select(x => new { x.r.AchievedHandicap, x.r.OpenHandicap })
                    .FirstOrDefault();

                if (top == null || !top.AchievedHandicap.HasValue
                    || !top.OpenHandicap.HasValue || top.OpenHandicap.Value == 0)
                    return null;

                return (top.AchievedHandicap.Value - top.OpenHandicap.Value) * 100.0 / top.OpenHandicap.Value;
            }
        }

        // -------------------------------------------------------------------------------------
        // Writes
        // -------------------------------------------------------------------------------------

        public void DeleteDidNotCompete(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Races.Where(r => r.Rid == rid && (r.FinishCode == "DNC" || r.FinishCode == "BAD"))
                    .ExecuteDelete();
        }

        public void CommitRows(IEnumerable<Race> rows)
        {
            //
            // Persist every scored row via detached clones (same pattern as
            // RaceResultsRepository.UpdateRaceRow) so navigation properties are never cascaded.
            // last_edit is preserved (not bumped) so the result_calculated stamp written afterward
            // keeps the "already calculated" gate satisfied.
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                foreach (var row in rows)
                {
                    var clone = new Race
                    {
                        Rid = row.Rid,
                        Bid = row.Bid,
                        StartDate = row.StartDate,
                        FinishCode = row.FinishCode,
                        FinishDate = row.FinishDate,
                        InterimDate = row.InterimDate,
                        RestrictedSail = row.RestrictedSail,
                        LastEdit = row.LastEdit,
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
                    ctx.Races.Attach(clone);
                    ctx.Entry(clone).State = EntityState.Modified;
                }
                ctx.SaveChanges();
            }
        }

        public void UpdateCalendarSct(int rid, double sct)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s
                        .SetProperty(c => c.StandardCorrectedTime, (double?)sct)
                        .SetProperty(c => c.Raced, (bool?)true));
        }

        public void MarkResultCalculated(int rid)
        {
            var now = DateTime.Now;
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s
                        .SetProperty(c => c.ResultCalculated, (DateTime?)now)
                        .SetProperty(c => c.Raced, (bool?)true));
        }

        public void UpdateBoatRollingHandicaps(int rid, IEnumerable<(int bid, int newRollingHandicap)> handicaps)
        {
            //
            // Set the boat's rolling handicap, but only when this race is the boat's latest race —
            // i.e. it has no later race than its entry in this race (the old NOT EXISTS guard).
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                foreach (var (bid, nrh) in handicaps)
                {
                    var thisStart = ctx.Races.Where(r => r.Rid == rid && r.Bid == bid)
                        .Select(r => r.StartDate).FirstOrDefault();
                    bool hasLater = ctx.Races.Any(r => r.Bid == bid && r.Rid != rid && r.StartDate > thisStart);
                    if (!hasLater)
                        ctx.Boats.Where(b => b.Bid == bid)
                            .ExecuteUpdate(s => s.SetProperty(b => b.RollingHandicap, (int?)nrh));
                }
            }
        }
    }
}
