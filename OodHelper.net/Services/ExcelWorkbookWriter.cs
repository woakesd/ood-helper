using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OodHelper.Services
{
    /// <summary>
    /// Writes a single-sheet .xlsx workbook from a column definition and a list of rows. Shared by the
    /// export services so each one only has to describe its query and its columns.
    /// </summary>
    internal static class ExcelWorkbookWriter
    {
        // StyleIndex into the stylesheet's CellFormats for a datetime cell (see BuildStylesheet).
        private const uint DateStyleIndex = 1U;

        // Excel's own date system starts here; see ValueCell for how earlier dates are handled.
        private static readonly DateTime ExcelEpoch = new DateTime(1900, 1, 1);

        /// <summary>
        /// Builds the workbook in a temporary file alongside the destination and moves it into place only
        /// once it is complete, so a mid-write failure cannot leave a truncated .xlsx sitting where the
        /// user expects a good one (or destroy the previous export it would have overwritten).
        /// </summary>
        public static void Write<T>(string filePath, string sheetName,
            (string Header, Func<T, object?> Value)[] columns, IReadOnlyList<T> rows)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            var tempPath = Path.Combine(directory ?? ".", Path.GetRandomFileName() + ".xlsx");
            try
            {
                BuildWorkbook(tempPath, sheetName, columns, rows);
                File.Move(tempPath, filePath, overwrite: true);
            }
            catch
            {
                // Best effort: whatever stops the temp file being deleted (a lock, an ACL, a path the
                // OS rejects) must not replace the export failure the caller needs to see.
                try { File.Delete(tempPath); } catch { }
                throw;
            }
        }

        private static void BuildWorkbook<T>(string filePath, string sheetName,
            (string Header, Func<T, object?> Value)[] columns, IReadOnlyList<T> rows)
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
            foreach (var col in columns)
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
                foreach (var col in columns)
                    sheetRow.Append(ValueCell(col.Value(row)));
                sheetData.Append(sheetRow);
            }

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1U,
                Name = sheetName
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
