using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace OodHelper.LoadTide
{
    class ReadAutoTideData
    {
        public DataTable Data;

        public ReadAutoTideData(string FileName)
        {
            Data = new DataTable();
            Data.Columns.Add("date", typeof(DateTime));
            Data.Columns.Add("height", typeof(double));
            Data.Columns.Add("current", typeof(double));
            Data.Columns.Add("icurrent", typeof(double));

            DateTime currdate = DateTime.Today, date;
            TimeSpan time;
            double height;

            string[] fd = File.ReadAllLines(FileName);
            foreach (string t in fd)
            {
                if (DateTime.TryParseExact(t, "d MMM yyyy dddd", null, System.Globalization.DateTimeStyles.None, out date))
                {
                    currdate = date;
                }
                else if (t.Length >= 12 && TimeSpan.TryParseExact(t.Substring(0, 5), "hh\\:mm", null, out time) && Double.TryParse(t.Substring(5, 7), out height))
                {
                    DataRow tr = Data.NewRow();
                    tr["date"] = currdate + time;
                    tr["height"] = height;
                    Data.Rows.Add(tr);
                }
            }

            for (int i = 1; i < Data.Rows.Count; i++)
            {
                Data.Rows[i]["icurrent"] = Math.Round(((double)Data.Rows[i]["height"] - (double)Data.Rows[i - 1]["height"]) * 8.32, 1);
            }

            for (int i = 3; i < Data.Rows.Count - 2; i++)
            {
                Data.Rows[i]["current"] = Math.Round((
                    (double)Data.Rows[i - 2]["icurrent"] +
                    (double)Data.Rows[i - 1]["icurrent"] +
                    (double)Data.Rows[i]["icurrent"] +
                    (double)Data.Rows[i + 1]["icurrent"] +
                    (double)Data.Rows[i + 2]["icurrent"]) / 5, 1);
            }
            Data.Columns.Remove("icurrent");
        }
    }
}
