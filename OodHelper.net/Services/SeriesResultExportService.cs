using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data;

namespace OodHelper.Services
{
    // Disambiguate from the legacy OodHelper.SeriesResult domain class, which is visible here via the
    // parent namespace and would otherwise win over the entity type. This alias must live inside the
    // namespace scope to take precedence over the enclosing namespace.
    using SeriesResult = OodHelper.Data.Entities.SeriesResult;

    public interface ISeriesResultExportService
    {
        /// <summary>
        /// Writes every row of the series_results table, joined to its series name and boat details, to
        /// an .xlsx workbook at <paramref name="filePath"/>, replacing any existing file. Returns the
        /// row count.
        /// </summary>
        Task<int> ExportSeriesResultsAsync(string filePath, CancellationToken ct = default);
    }

    internal sealed class SeriesResultExportService : ISeriesResultExportService
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public SeriesResultExportService(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        //
        // A series_results row flattened with the series name and its boat's details. Those fields are
        // nullable because the joins below are outer joins: series_results has no foreign keys in the
        // model, so a row whose sid or bid has no parent still belongs in the extract.
        //
        private sealed record ExportRow(
            SeriesResult Result,
            string? Sname,
            string? Boatname,
            string? Boatclass,
            string? Sailno);

        //
        // One entry per exported column, in sheet order: the key columns, the descriptive series/boat
        // columns joined in for readability, then the series_results table's own columns in table order.
        // Headers are the raw database column names (see the mapping in OodHelperContext).
        //
        private static readonly (string Header, Func<ExportRow, object?> Value)[] Columns =
        {
            ("sid", x => x.Result.Sid),
            ("bid", x => x.Result.Bid),
            ("sname", x => x.Sname),
            ("boatname", x => x.Boatname),
            ("boatclass", x => x.Boatclass),
            ("sailno", x => x.Sailno),
            ("division", x => x.Result.Division),
            ("entered", x => x.Result.Entered),
            ("gross", x => x.Result.Gross),
            ("nett", x => x.Result.Nett),
            ("place", x => x.Result.Place),
        };

        public async Task<int> ExportSeriesResultsAsync(string filePath, CancellationToken ct = default)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync(ct);

            var rows = await (
                from sr in ctx.SeriesResults.AsNoTracking()
                join s in ctx.Series on sr.Sid equals s.Sid into series
                from s in series.DefaultIfEmpty()
                join b in ctx.Boats on sr.Bid equals b.Bid into boats
                from b in boats.DefaultIfEmpty()
                orderby sr.Sid, sr.Division, sr.Place, sr.Bid
                select new ExportRow(sr, s.Sname, b.Boatname, b.Boatclass, b.Sailno))
                .ToListAsync(ct);

            ExcelWorkbookWriter.Write(filePath, "Series Results", Columns, rows);
            return rows.Count;
        }
    }
}
