using System;
using System.Collections.Generic;
using System.Data;
using OodHelper.Data;

namespace OodHelper.Results
{
    /// <summary>
    /// Builds the <see cref="DataTable"/> consumed by the printable results pages from the
    /// repository's projected rows. The column names, order and types reproduce exactly what the
    /// old inline SQL produced, so the pages' auto-generated-column logic and row-filter paging
    /// continue to work unchanged.
    /// </summary>
    internal static class ResultsPrintData
    {
        public static DataTable BuildTable(IReadOnlyList<RacePrintRow> rows)
        {
            var rd = new DataTable();
            rd.Columns.Add("order", typeof(int));
            rd.Columns.Add("Boat", typeof(string));
            rd.Columns.Add("Class", typeof(string));
            rd.Columns.Add("Sail No", typeof(string));
            rd.Columns.Add("Hcap", typeof(int));
            rd.Columns.Add("finish_code", typeof(string));
            rd.Columns.Add("finish_date", typeof(DateTime));
            rd.Columns.Add("Finish", typeof(string));
            rd.Columns.Add("Elapsed", typeof(int));
            rd.Columns.Add("Laps", typeof(int));
            rd.Columns.Add("Corrected", typeof(double));
            rd.Columns.Add("Pos", typeof(int));
            rd.Columns.Add("Pts", typeof(double));
            rd.Columns.Add("Achp", typeof(int));
            rd.Columns.Add("nhcp", typeof(int));
            rd.Columns.Add("%", typeof(double));
            rd.Columns.Add("C", typeof(string));
            rd.Columns.Add("A", typeof(string));
            rd.Columns.Add("PY", typeof(string));

            foreach (var row in rows)
            {
                var dr = rd.NewRow();
                dr["order"] = 0;
                dr["Boat"] = (object)row.Boat ?? DBNull.Value;
                dr["Class"] = (object)row.Class ?? DBNull.Value;
                dr["Sail No"] = (object)row.SailNo ?? DBNull.Value;
                dr["Hcap"] = (object)row.Hcap ?? DBNull.Value;
                dr["finish_code"] = (object)row.FinishCode ?? DBNull.Value;
                dr["finish_date"] = (object)row.FinishDate ?? DBNull.Value;
                dr["Finish"] = string.Empty;
                dr["Elapsed"] = (object)row.Elapsed ?? DBNull.Value;
                dr["Laps"] = (object)row.Laps ?? DBNull.Value;
                dr["Corrected"] = (object)row.Corrected ?? DBNull.Value;
                dr["Pos"] = (object)row.Place ?? DBNull.Value;
                dr["Pts"] = (object)row.Points ?? DBNull.Value;
                dr["Achp"] = (object)row.AchievedHandicap ?? DBNull.Value;
                dr["nhcp"] = (object)row.NewRollingHandicap ?? DBNull.Value;
                dr["%"] = (object)row.Percent ?? DBNull.Value;
                dr["C"] = (object)row.C ?? DBNull.Value;
                dr["A"] = (object)row.A ?? DBNull.Value;
                dr["PY"] = (object)row.Py ?? DBNull.Value;
                rd.Rows.Add(dr);
            }
            return rd;
        }
    }
}
