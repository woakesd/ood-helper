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

        private int _baseyear;
        public int BaseYear
        {
            get
            {
                return _baseyear;
            }
            set
            {
                if (_baseyear != value && value > 0)
                {
                    DateTime _base = new DateTime(_baseyear == 0 ? 1 : _baseyear, 1, 1);
                    DateTime _newBase = new DateTime(value, 1, 1);
                    if (Data != null)
                        foreach (DataRow r in Data.Rows)
                        {
                            DateTime tmp = (DateTime)r["date"];
                            //TimeSpan ts = ;
                            r["date"] = _newBase.Add(tmp - _base);
                        }
                    _baseyear = value;
                    OnPropertyChanged("Data");
                }
                OnPropertyChanged("Year");
            }
        }

        public ReadFormat11()
        {
            BaseYear = DateTime.Today.Year + 1;
        }

        public void Load(string FileName)
        {
            _data = new DataTable();
            _data.Columns.Add("date", typeof(DateTime));
            _data.Columns.Add("height", typeof(double));
            _data.Columns.Add("current", typeof(double));
            _data.Columns.Add("flow", typeof(string));
            _data.Columns.Add("tide", typeof(string));

            using (StreamReader sr = File.OpenText(FileName))
            {
                DateTime based = new DateTime(BaseYear - 1, 12, 31);
                DateTime d = based;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Substring(4, 1) == ":")
                    {
                        int day;
                        day = Int32.Parse(line.Substring(0, 3));
                        //d = new DateTime(BaseYear - 1, 12, 31);
                        d = based.AddDays(day);
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
