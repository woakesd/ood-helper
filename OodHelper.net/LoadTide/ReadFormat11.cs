using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace OodHelper.LoadTide
{
    class ReadFormat11: NotifyPropertyChanged
    {
        private DataTable _data;
        public DataTable Data { get { return _data; } set { _data = value; OnPropertyChanged("Data"); } }

        public ReadFormat11()
        {
        }

        public void Load(string FileName)
        {
            _data = new DataTable();
            _data.Columns.Add("date", typeof(DateTime));
            _data.Columns.Add("height", typeof(double));
            _data.Columns.Add("current", typeof(double));

            using (StreamReader sr = File.OpenText(FileName))
            {
                DateTime d = DateTime.Today;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Substring(4, 1) == ":")
                    {
                        int day;
                        day = Int32.Parse(line.Substring(0, 3));
                        d = new DateTime(2010, 12, 31);
                        d = d.AddDays(day);
                    }
                    line = line.Substring(6).Trim();
                    int centimeters;
                    string[] heights = line.Split(new char[] { ' ' });
                    for (int i = 0; i < heights.Length; i++)
                    {
                        centimeters = Int32.Parse(heights[i]);
                        DataRow r = _data.NewRow();
                        r["date"] = d;
                        r["height"] = (double) (centimeters / 100.0);
                        _data.Rows.Add(r);
                        d = d.AddMinutes(10);
                    }
                }
                sr.Close();
            }

            for (int i = 1; i < _data.Rows.Count; i++)
            {
                try
                {
                    _data.Rows[i]["current"] = Math.Round(((double)_data.Rows[i]["height"] - (double)_data.Rows[i-1]["height"]) * 8.32, 1);
                }
                catch (Exception)
                {
                }
            }

            _data.AcceptChanges();

            Data = _data;
        }
    }
}
