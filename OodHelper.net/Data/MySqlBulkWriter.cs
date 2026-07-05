using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace OodHelper.Data
{
    //
    // Shared helpers for writing rows to the club website's MySQL database. Used by every upload path
    // (results, sun, tide), all of which run over raw MySqlConnector inside a transaction. InsertRows
    // is a parameterised, batched multi-row INSERT (replacing the legacy hand-built literal SQL, which
    // formatted values with the current culture and was open to quoting bugs); Normalize maps a CLR
    // value onto what the website expects.
    //
    internal static class MySqlBulkWriter
    {
        //
        // Append the supplied rows to an (already cleared, or range-deleted) website table. Column names
        // are back-ticked to dodge reserved words such as `event` / `condition`.
        //
        public static async Task InsertRowsAsync<T>(MySqlConnection con, MySqlTransaction trn,
            string table, string[] columns, IReadOnlyList<T> rows, Func<T, object?[]> values, CancellationToken ct)
        {
            if (rows.Count == 0)
                return;

            var colList = string.Join(",", columns.Select(c => $"`{c}`"));
            const int batchSize = 500;
            for (var start = 0; start < rows.Count; start += batchSize)
            {
                var count = Math.Min(batchSize, rows.Count - start);
                var sb = new StringBuilder("INSERT INTO `").Append(table).Append("` (").Append(colList).Append(") VALUES ");
                await using var cmd = new MySqlCommand { Connection = con, Transaction = trn };
                for (var i = 0; i < count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append('(');
                    var vals = values(rows[start + i]);
                    for (var j = 0; j < columns.Length; j++)
                    {
                        if (j > 0) sb.Append(',');
                        var name = "@p" + i + "_" + j;
                        sb.Append(name);
                        cmd.Parameters.AddWithValue(name, Normalize(vals[j]));
                    }
                    sb.Append(')');
                }
                cmd.CommandText = sb.ToString();
                await cmd.ExecuteNonQueryAsync(ct);
            }
        }

        //
        // NULLs (CLR null or DBNull) and NaN doubles become NULL, bools become 1/0, and GUIDs are written
        // in {brace} form (matching the legacy upload and the format the download's GUID reader parses
        // back). Everything else passes straight through.
        //
        public static object Normalize(object? v)
        {
            switch (v)
            {
                case null: return DBNull.Value;
                case DBNull: return DBNull.Value;
                case bool b: return b ? 1 : 0;
                case double d when double.IsNaN(d): return DBNull.Value;
                case Guid g: return g.ToString("B");
                default: return v;
            }
        }
    }
}
