using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
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
            // Grouped header read ported verbatim from OpenHandicap: the time limit is resolved from
            // the F/D type, and last_edit is the max across the race's rows.
            //
            const string sql = @"
SELECT c.racetype, c.standard_corrected_time, c.result_calculated, MAX(r.last_edit) AS last_edit,
CASE c.time_limit_type
    WHEN 'F' THEN c.time_limit_fixed
    WHEN 'D' THEN DATEADD(SECOND, c.time_limit_delta, c.start_date)
END AS time_limit, c.extension
FROM calendar c LEFT JOIN races r ON c.rid = r.rid
WHERE c.rid = @rid
GROUP BY c.racetype, c.start_date, c.result_calculated, c.standard_corrected_time, CASE c.time_limit_type
    WHEN 'F' THEN c.time_limit_fixed
    WHEN 'D' THEN DATEADD(SECOND, c.time_limit_delta, c.start_date)
END, c.extension";

            using (var ctx = _contextFactory.CreateDbContext())
            {
                var conn = ctx.Database.GetDbConnection();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@rid", rid));

                    var mustOpen = conn.State != ConnectionState.Open;
                    if (mustOpen) conn.Open();
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                return null;

                            return new RaceScoreHeader(
                                RaceType: reader.IsDBNull(0) ? null : reader.GetString(0),
                                StandardCorrectedTime: reader.IsDBNull(1) ? (double?)null : Convert.ToDouble(reader.GetValue(1)),
                                ResultCalculated: reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                MaxLastEdit: reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                TimeLimit: reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                Extension: reader.IsDBNull(5) ? (int?)null : Convert.ToInt32(reader.GetValue(5)));
                        }
                    }
                    finally
                    {
                        if (mustOpen) conn.Close();
                    }
                }
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
            // Batched form of the per-boat InitialiseFields lookup: each boat in the race paired
            // with the new rolling handicap of its most recent prior race.
            //
            const string sql = @"
SELECT r1.bid, r3.new_rolling_handicap
FROM races r1
INNER JOIN races r2 ON r2.bid = r1.bid AND r2.start_date < r1.start_date
INNER JOIN races r3 ON r3.bid = r1.bid
WHERE r1.rid = @rid
GROUP BY r1.bid, r3.start_date, r3.new_rolling_handicap
HAVING r3.start_date = MAX(r2.start_date)";

            var result = new Dictionary<int, int>();
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var conn = ctx.Database.GetDbConnection();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@rid", rid));

                    var mustOpen = conn.State != ConnectionState.Open;
                    if (mustOpen) conn.Open();
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.IsDBNull(1)) continue; // null prior nrh -> caller falls back to open
                                result[reader.GetInt32(0)] = Convert.ToInt32(reader.GetValue(1));
                            }
                        }
                    }
                    finally
                    {
                        if (mustOpen) conn.Close();
                    }
                }
            }
            return result;
        }

        public double? GetPriorPerformancePercent(int rid, int bid, DateTime beforeStart)
        {
            const string sql = @"
SELECT TOP(1) CONVERT(FLOAT,(achieved_handicap - open_handicap))/open_handicap * 100
FROM races INNER JOIN calendar ON races.rid = calendar.rid
WHERE bid = @bid
AND races.rid != @rid
AND place != 999
AND standard_corrected_time <> 0
AND races.start_date <= @bstart
ORDER BY races.start_date DESC";

            using (var ctx = _contextFactory.CreateDbContext())
            {
                var conn = ctx.Database.GetDbConnection();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@bid", bid));
                    cmd.Parameters.Add(new SqlParameter("@rid", rid));
                    cmd.Parameters.Add(new SqlParameter("@bstart", beforeStart));

                    var mustOpen = conn.State != ConnectionState.Open;
                    if (mustOpen) conn.Open();
                    try
                    {
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        return Convert.ToDouble(result);
                    }
                    finally
                    {
                        if (mustOpen) conn.Close();
                    }
                }
            }
        }

        // -------------------------------------------------------------------------------------
        // Writes
        // -------------------------------------------------------------------------------------

        public void DeleteDidNotCompete(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Database.ExecuteSqlRaw(
                    "DELETE FROM races WHERE rid = @rid AND finish_code IN ('DNC', 'BAD')",
                    new SqlParameter("@rid", rid));
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
                ctx.Database.ExecuteSqlRaw(
                    "UPDATE calendar SET standard_corrected_time = @sct, raced = 1 WHERE rid = @rid",
                    new SqlParameter("@sct", sct),
                    new SqlParameter("@rid", rid));
        }

        public void MarkResultCalculated(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Database.ExecuteSqlRaw(
                    "UPDATE calendar SET result_calculated = GETDATE(), raced = 1 WHERE rid = @rid",
                    new SqlParameter("@rid", rid));
        }

        public void UpdateBoatRollingHandicaps(int rid, IEnumerable<(int bid, int newRollingHandicap)> handicaps)
        {
            const string sql = @"
UPDATE boats
SET rolling_handicap = @new_rolling_handicap
WHERE bid = @bid
AND NOT EXISTS (SELECT 1
    FROM races r1, races r2
    WHERE r1.rid = @rid
    AND r2.rid <> r1.rid
    AND r2.bid = r1.bid
    AND r1.bid = @bid
    AND r2.start_date > r1.start_date)";

            using (var ctx = _contextFactory.CreateDbContext())
            {
                foreach (var (bid, nrh) in handicaps)
                {
                    ctx.Database.ExecuteSqlRaw(sql,
                        new SqlParameter("@new_rolling_handicap", nrh),
                        new SqlParameter("@bid", bid),
                        new SqlParameter("@rid", rid));
                }
            }
        }
    }
}
