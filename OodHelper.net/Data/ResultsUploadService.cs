using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using OodHelper.Data.Entities;
using OodHelper.Website;

namespace OodHelper.Data
{
    // Bind the bare name to the EF entity, not the legacy OodHelper.SeriesResult domain class that is
    // visible via the parent namespace and would otherwise win (mirrors the alias in OodHelperContext).
    using SeriesResult = OodHelper.Data.Entities.SeriesResult;

    internal sealed class ResultsUploadService : IResultsUploadService
    {
        private readonly IDbContextFactory<OodHelperContext> _factory;

        public ResultsUploadService(IDbContextFactory<OodHelperContext> factory)
        {
            _factory = factory;
        }

        //
        // The upload is a full replace: read the local tables through EF, then clear and re-load every
        // website table from them. All website writes run inside one MySQL transaction, so any failure
        // or cancellation rolls back and leaves the website untouched. The website's upload time is then
        // mirrored back into the local updates table so CheckForUpdates does not subsequently see the
        // website as newer, exactly as the legacy upload did.
        //
        public async Task UploadAsync(IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            await using var ctx = await _factory.CreateDbContextAsync(ct);

            //
            // Read the whole local database first (read-only). races is filtered to raced calendar
            // entries that actually have a result, exactly as the legacy upload SELECT was.
            //
            var boats = await ctx.Boats.AsNoTracking().ToListAsync(ct);
            var calendars = await ctx.Calendars.AsNoTracking().ToListAsync(ct);
            var races = await ctx.Races.AsNoTracking()
                .Where(r => ctx.Calendars.Any(c => c.Rid == r.Rid && c.Raced == true)
                            && (r.Place != null || r.FinishDate != null || r.FinishCode != null))
                .ToListAsync(ct);
            var series = await ctx.Series.AsNoTracking().ToListAsync(ct);
            var joins = await ctx.CalendarSeriesJoins.AsNoTracking().ToListAsync(ct);
            var seriesResults = await ctx.SeriesResults.AsNoTracking().ToListAsync(ct);
            var selectRules = await ctx.SelectRules.AsNoTracking().ToListAsync(ct);
            var portsmouth = await ctx.PortsmouthNumbers.AsNoTracking().ToListAsync(ct);

            var mysql = new MySqlConnectionStringBuilder(Settings.Mysql).ConnectionString;
            await using var con = new MySqlConnection(mysql);
            await con.OpenAsync(ct);
            await using var trn = await con.BeginTransactionAsync(ct);

            var step = 0;
            const int totalSteps = 8;
            void Report(string message) => progress?.Report(new DownloadProgress(step++ * 100 / totalSteps, message));

            //
            // boats is the one table whose website column names differ from the local/EF names
            // (dngy, h, ohp, ohstat, rhp, csf, eng, kl, subscriptn); the column list below is the
            // website spelling, the value selector reads the matching EF property. The website
            // boats table also has uid and a local-only `delete` column; uid is sent, delete is left
            // to its default. Every other table shares its column names with the local schema.
            //
            Report("Uploading boats");
            await ReplaceTableAsync(con, trn, "boats",
                [ "boatname", "boatclass", "sailno", "dngy", "h", "bid", "distance", "ohp", "ohstat",
                    "rhp", "csf", "eng", "kl", "deviations", "subscriptn", "p", "s", "boatmemo", "beaten",
                    "berth", "hired", "uid" ],
                boats,
                b => [ b.Boatname, b.Boatclass, b.Sailno, b.Dinghy, b.Hulltype, b.Bid, b.Distance,
                    b.OpenHandicap, b.HandicapStatus, b.RollingHandicap, b.CrewSkillFactor, b.EnginePropeller,
                    b.Keel, b.Deviations, b.Subscription, b.P, b.S, b.Boatmemo, b.Beaten, b.Berth, b.Hired, b.Uid ],
                ct);

            Report("Uploading calendar");
            await ReplaceTableAsync(con, trn, "calendar_new",
                [ "rid", "start_date", "time_limit_type", "time_limit_fixed", "time_limit_delta",
                    "extension", "class", "event", "price_code", "course", "ood", "venue", "racetype",
                    "handicapping", "visitors", "flag", "memo", "is_race", "raced", "approved", "course_choice",
                    "laps_completed", "wind_speed", "wind_direction", "standard_corrected_time", "result_calculated" ],
                calendars,
                c => [ c.Rid, c.StartDate, c.TimeLimitType, c.TimeLimitFixed, c.TimeLimitDelta,
                    c.Extension, c.Class, c.Event, c.PriceCode, c.Course, c.Ood, c.Venue, c.Racetype,
                    c.Handicapping, c.Visitors, c.Flag, c.Memo, c.IsRace, c.Raced, c.Approved, c.CourseChoice,
                    c.LapsCompleted, c.WindSpeed, c.WindDirection, c.StandardCorrectedTime, c.ResultCalculated ],
                ct);

            ct.ThrowIfCancellationRequested();
            Report("Uploading races");
            await ReplaceTableAsync(con, trn, "races_new",
                [ "rid", "bid", "start_date", "finish_code", "finish_date", "interim_date",
                    "restricted_sail", "last_edit", "laps", "place", "points", "override_points", "elapsed",
                    "corrected", "standard_corrected", "handicap_status", "open_handicap", "rolling_handicap",
                    "achieved_handicap", "new_rolling_handicap", "performance_index", "a", "c" ],
                races,
                r => [ r.Rid, r.Bid, r.StartDate, r.FinishCode, r.FinishDate, r.InterimDate,
                    r.RestrictedSail, r.LastEdit, r.Laps, r.Place, r.Points, r.OverridePoints, r.Elapsed,
                    r.Corrected, r.StandardCorrected, r.HandicapStatus, r.OpenHandicap, r.RollingHandicap,
                    r.AchievedHandicap, r.NewRollingHandicap, r.PerformanceIndex, r.A, r.C ],
                ct);

            Report("Uploading series");
            await ReplaceTableAsync(con, trn, "series",
                ["sid", "sname", "discards"],
                series,
                s => [s.Sid, s.Sname, s.Discards],
                ct);

            ct.ThrowIfCancellationRequested();
            Report("Uploading series links");
            await ReplaceTableAsync(con, trn, "calendar_series_join",
                ["sid", "rid"],
                joins,
                j => [j.Sid, j.Rid],
                ct);

            Report("Uploading series results");
            await ReplaceTableAsync(con, trn, "series_results",
                ["sid", "bid", "division", "entered", "gross", "nett", "place"],
                seriesResults,
                sr => [sr.Sid, sr.Bid, sr.Division, sr.Entered, sr.Gross, sr.Nett, sr.Place],
                ct);

            ct.ThrowIfCancellationRequested();
            Report("Uploading select rules");
            await ReplaceTableAsync(con, trn, "select_rules",
                [ "id", "name", "parent", "application", "field", "condition", "string_value",
                    "number_bound1", "number_bound2" ],
                selectRules,
                r => [ r.Id, r.Name, r.Parent, r.Application, r.Field, r.Condition, r.StringValue,
                    r.NumberBound1, r.NumberBound2 ],
                ct);

            Report("Uploading portsmouth numbers");
            await ReplaceTableAsync(con, trn, "portsmouth_numbers",
                [ "id", "class_name", "no_of_crew", "rig", "spinnaker", "engine", "keel", "number",
                    "status", "notes" ],
                portsmouth,
                p => [ p.Id, p.ClassName, p.NoOfCrew, p.Rig, p.Spinnaker, p.Engine, p.Keel,
                    p.Number, p.Status, p.Notes ],
                ct);

            //
            // Record the upload time on the website (truncated to whole seconds, as the legacy upload
            // did), then commit. updates is append-only, so it is not cleared.
            //
            var now = DateTime.Now;
            var uploadTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            await using (var upd = new MySqlCommand("INSERT INTO `updates` (`upload`, `dummy`) VALUES (@dt, 2)", con, trn))
            {
                upd.Parameters.AddWithValue("dt", uploadTime);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await trn.CommitAsync(ct);
            await con.CloseAsync();

            //
            // Mirror the website's upload time into the local updates table so CheckForUpdates does not
            // then see the website as newer. updates is keyless, so it goes in via bulk insert, exactly
            // as the download writes it.
            //
            await ctx.BulkInsertAsync(new List<Update> { new Update { Upload = uploadTime, Dummy = 2 } },
                cancellationToken: ct);

            PushResultNotification.ResultPublished();
            progress?.Report(new DownloadProgress(100, "All done"));
        }

        //
        // Clear a website table and re-load it from the supplied rows. The batched, parameterised insert
        // is shared with the other upload paths via MySqlBulkWriter.
        //
        private static async Task ReplaceTableAsync<T>(MySqlConnection con, MySqlTransaction trn,
            string table, string[] columns, IReadOnlyList<T> rows, Func<T, object[]> values, CancellationToken ct)
        {
            await using (var del = new MySqlCommand($"DELETE FROM `{table}`", con, trn))
                await del.ExecuteNonQueryAsync(ct);

            await MySqlBulkWriter.InsertRowsAsync(con, trn, table, columns, rows, values, ct);
        }
    }
}
