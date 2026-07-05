using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace OodHelper.Data
{
    internal sealed class SunTideUploadService : ISunTideUploadService
    {
        private static readonly string[] SunColumns = { "date", "sunrise", "sunset" };
        private static readonly string[] TideColumns = { "date", "height", "current", "flow", "tide" };

        //
        // Replace one year of sun data: delete the covered date range, then insert every computed row.
        // (The legacy upload toggled `ALTER TABLE sun DISABLE/ENABLE KEYS`, which is dropped here: as an
        // implicit-commit DDL it actually committed the delete mid-transaction, defeating the rollback.)
        //
        public async Task UploadSunAsync(DataTable sun, IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            var rows = sun.Rows.Cast<DataRow>().ToList();
            if (rows.Count == 0)
                return;

            await using var con = await OpenAsync(ct);
            await using var trn = await con.BeginTransactionAsync(ct);

            progress?.Report(new DownloadProgress(0, "Uploading Sun Data"));
            await DeleteRangeAsync(con, trn, "sun", rows, ct);
            await MySqlBulkWriter.InsertRowsAsync(con, trn, "sun", SunColumns, rows,
                r => new[] { r["date"], r["sunrise"], r["sunset"] }, ct);

            await trn.CommitAsync(ct);
            progress?.Report(new DownloadProgress(100, "All done"));
        }

        //
        // Replace the loaded tide range. Tide files cover a whole year at fine resolution, so the insert
        // is chunked and progress is reported per chunk, preserving the moving progress bar the legacy
        // upload gave.
        //
        public async Task UploadTideAsync(DataTable tide, IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            var rows = tide.Rows.Cast<DataRow>().ToList();
            if (rows.Count == 0)
                return;

            await using var con = await OpenAsync(ct);
            await using var trn = await con.BeginTransactionAsync(ct);

            progress?.Report(new DownloadProgress(0, "Uploading Tide Data"));
            await DeleteRangeAsync(con, trn, "tidedata", rows, ct);

            const int chunk = 1000;
            for (var i = 0; i < rows.Count; i += chunk)
            {
                ct.ThrowIfCancellationRequested();
                var slice = rows.GetRange(i, Math.Min(chunk, rows.Count - i));
                await MySqlBulkWriter.InsertRowsAsync(con, trn, "tidedata", TideColumns, slice,
                    r => new[] { r["date"], r["height"], r["current"], r["flow"], r["tide"] }, ct);
                progress?.Report(new DownloadProgress((i + slice.Count) * 100 / rows.Count, "Uploading Tide Data"));
            }

            await trn.CommitAsync(ct);
            progress?.Report(new DownloadProgress(100, "All done"));
        }

        private static async Task<MySqlConnection> OpenAsync(CancellationToken ct)
        {
            var mysql = new MySqlConnectionStringBuilder(Settings.Mysql).ConnectionString;
            var con = new MySqlConnection(mysql);
            await con.OpenAsync(ct);
            return con;
        }

        //
        // Both tables are keyed by `date`; the rows are date-ordered, so the first and last rows bound
        // the range to clear before re-inserting.
        //
        private static async Task DeleteRangeAsync(MySqlConnection con, MySqlTransaction trn, string table,
            IReadOnlyList<DataRow> rows, CancellationToken ct)
        {
            await using var del = new MySqlCommand(
                $"DELETE FROM `{table}` WHERE `date` >= @start AND `date` <= @end", con, trn);
            del.Parameters.AddWithValue("start", rows[0]["date"]);
            del.Parameters.AddWithValue("end", rows[rows.Count - 1]["date"]);
            await del.ExecuteNonQueryAsync(ct);
        }
    }
}
