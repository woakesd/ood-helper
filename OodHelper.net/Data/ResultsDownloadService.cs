using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    // Bind the bare name to the EF entity, not the legacy OodHelper.SeriesResult domain class that is
    // visible via the parent namespace and would otherwise win (mirrors the alias in OodHelperContext).
    using SeriesResult = OodHelper.Data.Entities.SeriesResult;

    internal sealed class ResultsDownloadService : IResultsDownloadService
    {
        private readonly IDbContextFactory<OodHelperContext> _factory;

        public ResultsDownloadService(IDbContextFactory<OodHelperContext> factory)
        {
            _factory = factory;
        }

        //
        // The download is a full replace: clear every local table, then re-load it from the website.
        // It runs inside one MySQL read transaction (snapshot) and one SQL Server write transaction, so
        // any failure or cancellation rolls back and leaves the local database untouched. Identity
        // values (rid/bid/sid) from the website are preserved via SqlBulkCopyOptions.KeepIdentity,
        // replacing the legacy SET IDENTITY_INSERT toggling; DatabaseAdmin.ReseedDatabase() then
        // realigns the identity seeds afterwards, exactly as before.
        //
        public async Task DownloadAsync(IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            var mysql = new MySqlConnectionStringBuilder(Settings.Mysql).ConnectionString;
            await using var mcon = new MySqlConnection(mysql);
            await mcon.OpenAsync(ct);
            await using var mtrn = await mcon.BeginTransactionAsync(ct);

            await using var ctx = await _factory.CreateDbContextAsync(ct);
            await using var tx = await ctx.Database.BeginTransactionAsync(ct);

            var keepIdentity = new BulkConfig { SqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity };

            var step = 0;
            const int totalSteps = 8;
            void Report(string message) => progress?.Report(new DownloadProgress(step++ * 100 / totalSteps, message));

            //
            // Clear the local tables first, children before parents, so a foreign key never blocks a
            // delete regardless of cascade configuration. (updates is append-only, matching the legacy
            // download, so it is not cleared.)
            //
            await ctx.Races.ExecuteDeleteAsync(ct);
            await ctx.SeriesResults.ExecuteDeleteAsync(ct);
            await ctx.CalendarSeriesJoins.ExecuteDeleteAsync(ct);
            await ctx.Calendars.ExecuteDeleteAsync(ct);
            await ctx.Boats.ExecuteDeleteAsync(ct);
            await ctx.Series.ExecuteDeleteAsync(ct);
            await ctx.SelectRules.ExecuteDeleteAsync(ct);
            await ctx.PortsmouthNumbers.ExecuteDeleteAsync(ct);

            //
            // Re-load each table from the website, parents before children so the foreign keys hold.
            //
            Report("Loading Boats");
            await ctx.BulkInsertAsync(await ReadBoatsAsync(mcon, mtrn, ct), keepIdentity, cancellationToken: ct);

            Report("Loading Calendar");
            await ctx.BulkInsertAsync(await ReadCalendarAsync(mcon, mtrn, ct), keepIdentity, cancellationToken: ct);

            ct.ThrowIfCancellationRequested();
            Report("Loading Races");
            await ctx.BulkInsertAsync(await ReadRacesAsync(mcon, mtrn, ct), cancellationToken: ct);

            Report("Loading Series");
            await ctx.BulkInsertAsync(await ReadSeriesAsync(mcon, mtrn, ct), keepIdentity, cancellationToken: ct);

            ct.ThrowIfCancellationRequested();
            Report("Loading Series links");
            await ctx.BulkInsertAsync(await ReadJoinAsync(mcon, mtrn, ct), cancellationToken: ct);

            Report("Loading Series Results");
            await ctx.BulkInsertAsync(await ReadSeriesResultsAsync(mcon, mtrn, ct), cancellationToken: ct);

            ct.ThrowIfCancellationRequested();
            Report("Loading Select Rules");
            await ctx.BulkInsertAsync(await ReadSelectRulesAsync(mcon, mtrn, ct), cancellationToken: ct);

            Report("Loading Portsmouth numbers");
            await ctx.BulkInsertAsync(await ReadPortsmouthNumbersAsync(mcon, mtrn, ct), cancellationToken: ct);

            //
            // Record the website's latest upload time locally (append-only, as the legacy code did).
            //
            var maxUpload = await ReadMaxUploadAsync(mcon, mtrn, ct);
            if (maxUpload.HasValue)
                await ctx.BulkInsertAsync(new List<Update> { new() { Upload = maxUpload, Dummy = 2 } },
                    cancellationToken: ct);

            await tx.CommitAsync(ct);
            await mtrn.CommitAsync(ct);
            await mcon.CloseAsync();

            DatabaseAdmin.ReseedDatabase();
            progress?.Report(new DownloadProgress(100, "All done"));
        }

        //
        // Per-table readers. Each opens an explicit-column SELECT against the website (back-ticked to
        // dodge reserved words such as `event`/`condition`) and maps every row onto its EF entity,
        // leaving local-only columns (e.g. boats.small_cat_handicap_rating) at their defaults.
        //
        private static async Task<List<Boat>> ReadBoatsAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            const string sql = "SELECT `bid`, `boatname`, `boatclass`, `sailno`, " +
                               "`dngy` dinghy, `h` hulltype, `distance`, " +
                               "`ohp` open_handicap, `ohstat` handicap_status, " +
                               "`rhp` rolling_handicap, `csf` crew_skill_factor, " +
                               "`eng` engine_propeller, `kl` keel, `deviations`, " +
                               "`subscriptn` subscription, `berth`, `boatmemo`, " +
                               "`hired`, `p`, `s`, `beaten`, `uid` FROM boats";
            var list = new List<Boat>();
            await using var cmd = new MySqlCommand(sql, mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new Boat
                {
                    Bid = ReqInt(r, "bid"),
                    Boatname = GetString(r, "boatname"),
                    Boatclass = GetString(r, "boatclass"),
                    Sailno = GetString(r, "sailno"),
                    Dinghy = GetBool(r, "dinghy"),
                    Hulltype = GetString(r, "hulltype"),
                    Distance = GetInt(r, "distance"),
                    OpenHandicap = GetInt(r, "open_handicap"),
                    HandicapStatus = GetString(r, "handicap_status"),
                    RollingHandicap = GetInt(r, "rolling_handicap"),
                    CrewSkillFactor = GetInt(r, "crew_skill_factor"),
                    EnginePropeller = GetString(r, "engine_propeller"),
                    Keel = GetString(r, "keel"),
                    Deviations = GetString(r, "deviations"),
                    Subscription = GetString(r, "subscription"),
                    Berth = GetString(r, "berth"),
                    Boatmemo = GetString(r, "boatmemo"),
                    Hired = GetBool(r, "hired"),
                    P = GetString(r, "p"),
                    S = GetBool(r, "s"),
                    Beaten = GetInt(r, "beaten"),
                    Uid = GetGuid(r, "uid")
                });
            }
            return list;
        }

        private static async Task<List<Calendar>> ReadCalendarAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            const string sql = "SELECT `rid`, `start_date`, `class`, `event`, `price_code`, `course`, `ood`, `venue`, " +
                               "`racetype`, `handicapping`, `visitors`, `flag`, `time_limit_type`, `time_limit_fixed`, " +
                               "`time_limit_delta`, `extension`, `memo`, `is_race`, `raced`, `approved`, `course_choice`, " +
                               "`laps_completed`, `wind_speed`, `wind_direction`, `standard_corrected_time`, " +
                               "`result_calculated` FROM calendar_new";
            var list = new List<Calendar>();
            await using var cmd = new MySqlCommand(sql, mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new Calendar
                {
                    Rid = ReqInt(r, "rid"),
                    StartDate = GetDateTime(r, "start_date"),
                    Class = GetString(r, "class"),
                    Event = GetString(r, "event"),
                    PriceCode = GetString(r, "price_code"),
                    Course = GetString(r, "course"),
                    Ood = GetString(r, "ood"),
                    Venue = GetString(r, "venue"),
                    Racetype = GetString(r, "racetype"),
                    Handicapping = GetString(r, "handicapping"),
                    Visitors = GetInt(r, "visitors"),
                    Flag = GetString(r, "flag"),
                    TimeLimitType = GetString(r, "time_limit_type"),
                    TimeLimitFixed = GetDateTime(r, "time_limit_fixed"),
                    TimeLimitDelta = GetInt(r, "time_limit_delta"),
                    Extension = GetInt(r, "extension"),
                    Memo = GetString(r, "memo"),
                    IsRace = GetBool(r, "is_race"),
                    Raced = GetBool(r, "raced"),
                    Approved = GetBool(r, "approved"),
                    CourseChoice = GetString(r, "course_choice"),
                    LapsCompleted = GetInt(r, "laps_completed"),
                    WindSpeed = GetString(r, "wind_speed"),
                    WindDirection = GetString(r, "wind_direction"),
                    StandardCorrectedTime = GetDouble(r, "standard_corrected_time"),
                    ResultCalculated = GetDateTime(r, "result_calculated")
                });
            }
            return list;
        }

        private static async Task<List<Race>> ReadRacesAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            const string sql = "SELECT `rid`, `bid`, `start_date`, `finish_date`, `interim_date`, `restricted_sail`, " +
                               "`last_edit`, `finish_code`, `laps`, `elapsed`, `corrected`, `standard_corrected`, " +
                               "`handicap_status`, `open_handicap`, `rolling_handicap`, `achieved_handicap`, " +
                               "`new_rolling_handicap`, `place`, `points`, `override_points`, `performance_index`, " +
                               "`a`, `c` FROM races_new";
            var list = new List<Race>();
            await using var cmd = new MySqlCommand(sql, mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new Race
                {
                    Rid = ReqInt(r, "rid"),
                    Bid = ReqInt(r, "bid"),
                    StartDate = GetDateTime(r, "start_date"),
                    FinishDate = GetDateTime(r, "finish_date"),
                    InterimDate = GetDateTime(r, "interim_date"),
                    RestrictedSail = GetBool(r, "restricted_sail"),
                    LastEdit = GetDateTime(r, "last_edit"),
                    FinishCode = GetString(r, "finish_code"),
                    Laps = GetInt(r, "laps"),
                    Elapsed = GetInt(r, "elapsed"),
                    Corrected = GetDouble(r, "corrected"),
                    StandardCorrected = GetDouble(r, "standard_corrected"),
                    HandicapStatus = GetString(r, "handicap_status"),
                    OpenHandicap = GetInt(r, "open_handicap"),
                    RollingHandicap = GetInt(r, "rolling_handicap"),
                    AchievedHandicap = GetInt(r, "achieved_handicap"),
                    NewRollingHandicap = GetInt(r, "new_rolling_handicap"),
                    Place = GetInt(r, "place"),
                    Points = GetDouble(r, "points"),
                    OverridePoints = GetDouble(r, "override_points"),
                    PerformanceIndex = GetInt(r, "performance_index"),
                    A = GetString(r, "a"),
                    C = GetString(r, "c")
                });
            }
            return list;
        }

        private static async Task<List<Series>> ReadSeriesAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            const string sql = "SELECT `sid`, `sname`, `discards` FROM series";
            var list = new List<Series>();
            await using var cmd = new MySqlCommand(sql, mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new Series
                {
                    Sid = ReqInt(r, "sid"),
                    Sname = GetString(r, "sname"),
                    Discards = GetString(r, "discards")
                });
            }
            return list;
        }

        private static async Task<List<CalendarSeriesJoin>> ReadJoinAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            var list = new List<CalendarSeriesJoin>();
            await using var cmd = new MySqlCommand("SELECT `sid`, `rid` FROM calendar_series_join", mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add(new CalendarSeriesJoin { Sid = ReqInt(r, "sid"), Rid = ReqInt(r, "rid") });
            return list;
        }

        private static async Task<List<SeriesResult>> ReadSeriesResultsAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            const string sql = "SELECT `sid`, `bid`, `division`, `entered`, `gross`, `nett`, `place` FROM series_results";
            var list = new List<SeriesResult>();
            await using var cmd = new MySqlCommand(sql, mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new SeriesResult
                {
                    Sid = ReqInt(r, "sid"),
                    Bid = ReqInt(r, "bid"),
                    Division = GetString(r, "division"),
                    Entered = GetInt(r, "entered"),
                    Gross = GetDouble(r, "gross"),
                    Nett = GetDouble(r, "nett"),
                    Place = GetInt(r, "place")
                });
            }
            return list;
        }

        private static async Task<List<SelectRule>> ReadSelectRulesAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            const string sql = "SELECT `id`, `name`, `parent`, `application`, `field`, `condition`, `string_value`, " +
                               "`number_bound1`, `number_bound2` FROM select_rules";
            var list = new List<SelectRule>();
            await using var cmd = new MySqlCommand(sql, mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new SelectRule
                {
                    Id = GetGuid(r, "id").GetValueOrDefault(),
                    Name = GetString(r, "name"),
                    Parent = GetGuid(r, "parent"),
                    Application = GetInt(r, "application"),
                    Field = GetString(r, "field"),
                    Condition = GetInt(r, "condition"),
                    StringValue = GetString(r, "string_value"),
                    NumberBound1 = GetDecimal(r, "number_bound1"),
                    NumberBound2 = GetDecimal(r, "number_bound2")
                });
            }
            return list;
        }

        private static async Task<List<PortsmouthNumber>> ReadPortsmouthNumbersAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            const string sql = "SELECT `id`, `class_name`, `no_of_crew`, `rig`, `spinnaker`, `engine`, `keel`, " +
                               "`number`, `status`, `notes` FROM portsmouth_numbers";
            var list = new List<PortsmouthNumber>();
            await using var cmd = new MySqlCommand(sql, mcon, mtrn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new PortsmouthNumber
                {
                    Id = GetGuid(r, "id").GetValueOrDefault(),
                    ClassName = GetString(r, "class_name"),
                    NoOfCrew = GetInt(r, "no_of_crew"),
                    Rig = GetString(r, "rig"),
                    Spinnaker = GetString(r, "spinnaker"),
                    Engine = GetString(r, "engine"),
                    Keel = GetString(r, "keel"),
                    Number = GetInt(r, "number"),
                    Status = GetString(r, "status"),
                    Notes = GetString(r, "notes")
                });
            }
            return list;
        }

        private static async Task<DateTime?> ReadMaxUploadAsync(MySqlConnection mcon, MySqlTransaction mtrn, CancellationToken ct)
        {
            await using var cmd = new MySqlCommand("SELECT MAX(`upload`) FROM updates", mcon, mtrn);
            var o = await cmd.ExecuteScalarAsync(ct);
            return o == null || o == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(o);
        }

        //
        // DBNull-aware column mappers. They read by name and use Convert.* so that whatever underlying
        // CLR type MySqlConnector hands back (e.g. sbyte for tinyint) lands on the right entity type.
        //
        private static int ReqInt(DbDataReader r, string col) => Convert.ToInt32(r.GetValue(r.GetOrdinal(col)));

        private static string GetString(DbDataReader r, string col)
        {
            var o = r.GetOrdinal(col);
            return r.IsDBNull(o) ? null : r.GetValue(o).ToString();
        }

        private static int? GetInt(DbDataReader r, string col)
        {
            var o = r.GetOrdinal(col);
            return r.IsDBNull(o) ? (int?)null : Convert.ToInt32(r.GetValue(o));
        }

        private static double? GetDouble(DbDataReader r, string col)
        {
            var o = r.GetOrdinal(col);
            return r.IsDBNull(o) ? (double?)null : Convert.ToDouble(r.GetValue(o));
        }

        private static decimal? GetDecimal(DbDataReader r, string col)
        {
            var o = r.GetOrdinal(col);
            return r.IsDBNull(o) ? (decimal?)null : Convert.ToDecimal(r.GetValue(o));
        }

        private static bool? GetBool(DbDataReader r, string col)
        {
            var o = r.GetOrdinal(col);
            return r.IsDBNull(o) ? (bool?)null : Convert.ToBoolean(r.GetValue(o));
        }

        private static DateTime? GetDateTime(DbDataReader r, string col)
        {
            var o = r.GetOrdinal(col);
            return r.IsDBNull(o) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(o));
        }

        private static Guid? GetGuid(DbDataReader r, string col)
        {
            var o = r.GetOrdinal(col);
            if (r.IsDBNull(o))
                return null;
            var v = r.GetValue(o);
            if (v is Guid g)
                return g;
            if (v is byte[] b)
                return new Guid(b);
            return Guid.Parse(v.ToString());
        }
    }
}
