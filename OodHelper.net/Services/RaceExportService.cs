using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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

        // StyleIndex into the stylesheet's CellFormats for a datetime cell (see BuildStylesheet).
        private const uint DateStyleIndex = 1U;

        // Excel's own date system starts here; see ValueCell for how earlier dates are handled.
        private static readonly DateTime ExcelEpoch = new DateTime(1900, 1, 1);

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

            WriteWorkbook(filePath, rows);
            return rows.Count;
        }

        //
        // Builds the workbook in a temporary file alongside the destination and moves it into place only
        // once it is complete, so a mid-write failure cannot leave a truncated .xlsx sitting where the
        // user expects a good one (or destroy the previous export it would have overwritten).
        //
        private static void WriteWorkbook(string filePath, IReadOnlyList<ExportRow> rows)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            var tempPath = Path.Combine(directory ?? ".", Path.GetRandomFileName() + ".xlsx");
            try
            {
                BuildWorkbook(tempPath, rows);
                File.Move(tempPath, filePath, overwrite: true);
            }
            catch
            {
                try { File.Delete(tempPath); } catch (IOException) { /* best effort */ }
                throw;
            }
        }

        private static void BuildWorkbook(string filePath, IReadOnlyList<ExportRow> rows)
        {
            using var doc = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);

            var workbookPart = doc.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = BuildStylesheet();
            stylesPart.Stylesheet.Save();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var header = new Row();
            foreach (var col in Columns)
                header.Append(TextCell(col.Header));
            sheetData.Append(header);

            foreach (var row in rows)
            {
                var sheetRow = new Row();
                //
                // A cell is emitted for every column even when the value is null: cells carry no explicit
                // reference, so position is taken from order and a skipped cell would shift the rest of
                // the row left.
                //
                foreach (var col in Columns)
                    sheetRow.Append(ValueCell(col.Value(row)));
                sheetData.Append(sheetRow);
            }

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1U,
                Name = "Races"
            });

            workbookPart.Workbook.Save();
        }

        private static Cell ValueCell(object? value)
        {
            switch (value)
            {
                case null:
                    return new Cell();
                case DateTime dt:
                    //
                    // ToOADate throws below year 0100 and Excel cannot render pre-1900 dates anyway, so a
                    // stray sentinel date degrades to an ISO-8601 text cell instead of failing the export.
                    //
                    return dt >= ExcelEpoch
                        ? new Cell
                        {
                            StyleIndex = DateStyleIndex,
                            DataType = CellValues.Number,
                            CellValue = new CellValue(dt.ToOADate().ToString(CultureInfo.InvariantCulture))
                        }
                        : TextCell(dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                case bool b:
                    return new Cell
                    {
                        DataType = CellValues.Boolean,
                        CellValue = new CellValue(b ? "1" : "0")
                    };
                case int i:
                    return NumberCell(i);
                case double d:
                    return NumberCell(d);
                case decimal m:
                    return NumberCell((double)m);
                case string s:
                    return TextCell(s);
                default:
                    return TextCell(value.ToString() ?? string.Empty);
            }
        }

        private static Cell NumberCell(double value) =>
            new Cell
            {
                DataType = CellValues.Number,
                CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture))
            };

        // Inline strings keep the workbook self-contained without a shared-string table.
        private static Cell TextCell(string? text) =>
            new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(text ?? string.Empty))
            };

        //
        // Minimal stylesheet: the default cell format at index 0 plus one datetime format at index 1,
        // referencing a custom numbering format so datetime cells render as a readable timestamp rather
        // than a raw OADate serial. The single font/fill/border are required for a valid stylesheet.
        //
        private static Stylesheet BuildStylesheet() =>
            new Stylesheet(
                new NumberingFormats(
                    new NumberingFormat
                    {
                        NumberFormatId = 164U,
                        FormatCode = "yyyy-mm-dd hh:mm:ss"
                    })
                { Count = 1U },
                new Fonts(new Font()) { Count = 1U },
                new Fills(new Fill(new PatternFill { PatternType = PatternValues.None })) { Count = 1U },
                new Borders(new Border()) { Count = 1U },
                new CellStyleFormats(new CellFormat()) { Count = 1U },
                new CellFormats(
                    new CellFormat(),
                    new CellFormat
                    {
                        NumberFormatId = 164U,
                        FormatId = 0U,
                        ApplyNumberFormat = true
                    })
                { Count = 2U });
    }
}
