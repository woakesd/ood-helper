using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
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

        public Calendar GetCalendar(int rid)
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
            // been edited since the last calculation. Ported verbatim from the original query;
            // the boolean is evaluated in SQL.
            //
            const string sql = @"
SELECT TOP 1 CASE WHEN result_calculated IS NULL
                  OR (mle IS NOT NULL AND result_calculated <= mle) THEN 1 ELSE 0 END
FROM (
    SELECT result_calculated, MAX(last_edit) AS mle
    FROM calendar c LEFT JOIN races r ON c.rid = r.rid
    WHERE c.rid = @rid
    GROUP BY result_calculated, standard_corrected_time
) x";
            using (var ctx = _contextFactory.CreateDbContext())
                return ExecuteScalarInt(ctx, sql, new SqlParameter("@rid", rid)) == 1;
        }

        public bool GetRefreshHandicapsEnabled(int rid)
        {
            const string sql = @"
SELECT COUNT(*) FROM (
    SELECT COUNT(1) AS cnt
    FROM races r1
    INNER JOIN races r2 ON r2.bid = r1.bid AND r2.rid <> r1.rid AND r2.start_date < r1.start_date AND r2.last_edit > r1.last_edit
    INNER JOIN races r3 ON r3.bid = r1.bid AND r3.rid <> r1.rid
    WHERE r1.rid = @rid
    GROUP BY r1.bid, r3.start_date, r3.new_rolling_handicap
    HAVING r3.start_date = MAX(r2.start_date)
) x";
            using (var ctx = _contextFactory.CreateDbContext())
                return ExecuteScalarInt(ctx, sql, new SqlParameter("@rid", rid)) > 0;
        }

        public int CountAutoPopulate(int rid)
        {
            string sql = @"SELECT COUNT(*) FROM (" + AutoPopulateBids + @") x";
            using (var ctx = _contextFactory.CreateDbContext())
                return ExecuteScalarInt(ctx, sql, new SqlParameter("@rid", rid));
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
        // Bulk operations (complex self-joins kept as raw SQL, off Db.cs)
        // -------------------------------------------------------------------------------------

        public void DoAutoPopulate(int rid)
        {
            string sql = @"
INSERT INTO races
    (rid, start_date, bid, rolling_handicap, handicap_status, open_handicap, last_edit)
SELECT c.rid, c.start_date, b.bid, b.rolling_handicap, b.handicap_status, b.open_handicap, GETDATE()
FROM boats b, calendar c
WHERE c.rid = @rid
AND b.bid IN (" + AutoPopulateBids + @")";
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Database.ExecuteSqlRaw(sql, new SqlParameter("@rid", rid));
        }

        public void RefreshRollingHandicaps(int rid)
        {
            //
            // Set each boat's rolling handicap to the new_rolling_handicap from its latest
            // previous race entry (set-based equivalent of the old read-then-loop-update).
            //
            const string sql = @"
UPDATE races
SET last_edit = GETDATE(), rolling_handicap = src.nrh
FROM races
INNER JOIN (
    SELECT r1.bid AS bid, r3.new_rolling_handicap AS nrh
    FROM races r1
    INNER JOIN races r2 ON r2.bid = r1.bid AND r2.rid <> r1.rid AND r2.start_date < r1.start_date
    INNER JOIN races r3 ON r3.bid = r1.bid AND r3.rid <> r1.rid
    WHERE r1.rid = @rid
    GROUP BY r1.bid, r3.start_date, r3.new_rolling_handicap
    HAVING r3.start_date = MAX(r2.start_date)
) src ON src.bid = races.bid
WHERE races.rid = @rid";
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Database.ExecuteSqlRaw(sql, new SqlParameter("@rid", rid));
        }

        public void MoveToFleet(int fromRid, int toRid, int bid)
        {
            const string sql = @"
UPDATE races
SET rid = @toRid, start_date = (SELECT start_date FROM calendar WHERE rid = @toRid)
WHERE rid = @fromRid AND bid = @bid";
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Database.ExecuteSqlRaw(sql,
                    new SqlParameter("@fromRid", fromRid),
                    new SqlParameter("@toRid", toRid),
                    new SqlParameter("@bid", bid));
        }

        public void ApplyEditedBoatHandicaps(int rid, int bid)
        {
            const string sql = @"
UPDATE r
SET r.rolling_handicap = b.rolling_handicap,
    r.handicap_status = b.handicap_status,
    r.open_handicap = b.open_handicap,
    r.last_edit = GETDATE()
FROM races r INNER JOIN boats b ON b.bid = r.bid
WHERE r.rid = @rid AND r.bid = @bid";
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Database.ExecuteSqlRaw(sql,
                    new SqlParameter("@rid", rid),
                    new SqlParameter("@bid", bid));
        }

        // -------------------------------------------------------------------------------------
        // Entry editing (SelectBoats)
        // -------------------------------------------------------------------------------------

        public void AddRaceEntry(int rid, int bid, DateTime startDate, string handicapStatus,
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

        private static int ExecuteScalarInt(OodHelperContext ctx, string sql, params SqlParameter[] parameters)
        {
            var conn = ctx.Database.GetDbConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                var mustOpen = conn.State != ConnectionState.Open;
                if (mustOpen) conn.Open();
                try
                {
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return 0;
                    return Convert.ToInt32(result);
                }
                finally
                {
                    if (mustOpen) conn.Close();
                }
            }
        }

        //
        // Distinct boats that have raced in another race of the same series and class. Used by
        // both CountAutoPopulate and DoAutoPopulate; bound parameter @rid is the current rid.
        //
        private const string AutoPopulateBids = @"
    SELECT DISTINCT r.bid
    FROM calendar AS c1
    INNER JOIN calendar_series_join AS cs1 ON c1.rid = cs1.rid
    INNER JOIN calendar_series_join AS cs2 ON cs2.sid = cs1.sid
    INNER JOIN calendar AS c2 ON c2.rid = cs2.rid AND c1.rid <> c2.rid AND c1.class = c2.class
    INNER JOIN races AS r ON r.rid = c2.rid
    INNER JOIN series AS s ON s.sid = cs1.sid AND c1.event LIKE s.sname + '%'
    WHERE c1.rid = @rid";
    }
}
