using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data;
using OodHelper.Data.Entities;

namespace OodHelper.Services
{
    public interface IRaceExportService
    {
        /// <summary>
        /// Writes every row of the races table, joined to its calendar and boat details, to an .xlsx
        /// workbook at <paramref name="filePath"/>, replacing any existing file. Returns the row count.
        /// </summary>
        Task<int> ExportRacesAsync(string filePath, CancellationToken ct = default);
    }

    internal sealed class RaceExportService : IRaceExportService
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public RaceExportService(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        //
        // A races row flattened with the descriptive columns from its calendar and boat. The calendar
        // and boat fields are nullable because the joins below are outer joins: a race whose rid or bid
        // has no matching parent still belongs in the extract, just without those details.
        //
        private sealed record ExportRow(
            Race Race,
            string? Event,
            string? Class,
            string? Boatname,
            string? Boatclass,
            string? Sailno);

        //
        // One entry per exported column, in sheet order: the race key, the descriptive calendar/boat
        // columns joined in for readability, then the races table's own columns in table order. Headers
        // are the raw database column names (see the mappings in OodHelperContext). Adding a column
        // means adding one line here.
        //
        private static readonly (string Header, Func<ExportRow, object?> Value)[] Columns =
        {
            ("rid", x => x.Race.Rid),
            ("bid", x => x.Race.Bid),
            ("event", x => x.Event),
            ("class", x => x.Class),
            ("boatname", x => x.Boatname),
            ("boatclass", x => x.Boatclass),
            ("sailno", x => x.Sailno),
            ("start_date", x => x.Race.StartDate),
            ("finish_code", x => x.Race.FinishCode),
            ("finish_date", x => x.Race.FinishDate),
            ("interim_date", x => x.Race.InterimDate),
            ("restricted_sail", x => x.Race.RestrictedSail),
            ("last_edit", x => x.Race.LastEdit),
            ("laps", x => x.Race.Laps),
            ("place", x => x.Race.Place),
            ("points", x => x.Race.Points),
            ("override_points", x => x.Race.OverridePoints),
            ("elapsed", x => x.Race.Elapsed),
            ("corrected", x => x.Race.Corrected),
            ("standard_corrected", x => x.Race.StandardCorrected),
            ("handicap_status", x => x.Race.HandicapStatus),
            ("open_handicap", x => x.Race.OpenHandicap),
            ("rolling_handicap", x => x.Race.RollingHandicap),
            ("achieved_handicap", x => x.Race.AchievedHandicap),
            ("new_rolling_handicap", x => x.Race.NewRollingHandicap),
            ("performance_index", x => x.Race.PerformanceIndex),
            ("a", x => x.Race.A),
            ("c", x => x.Race.C),
        };

        public async Task<int> ExportRacesAsync(string filePath, CancellationToken ct = default)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync(ct);

            //
            // Outer joins rather than Include: both navigations are required in the model, so Include
            // would emit inner joins and silently drop a race whose calendar or boat row is missing.
            // The extract must account for every races row.
            //
            var rows = await (
                from r in ctx.Races.AsNoTracking()
                join c in ctx.Calendars on r.Rid equals c.Rid into calendars
                from c in calendars.DefaultIfEmpty()
                join b in ctx.Boats on r.Bid equals b.Bid into boats
                from b in boats.DefaultIfEmpty()
                orderby r.Rid, r.Bid
                select new ExportRow(r, c.Event, c.Class, b.Boatname, b.Boatclass, b.Sailno))
                .ToListAsync(ct);

            ExcelWorkbookWriter.Write(filePath, "Races", Columns, rows);
            return rows.Count;
        }
    }
}
