using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Windows;
using OodHelper.Converters;

namespace OodHelper.Results.Model
{
    public class Entry : IEntry
    {
        private DataRow _row;
        public Entry(DataRow result, DateTime StartDate, DateTime LimitDate)
        {
            _row = result;
            _startDate = StartDate;
            _limitDate = LimitDate;
        }

        public int rid { get { return (int)_row["rid"]; } set { } }
        public int bid { get { return (int)_row["bid"]; } set { } }
        public string boatname { get { return _row["boatname"] as string; } set { _row["boatname"] = value; } }
        public string boatclass { get { return _row["boatclass"] as string; } set { _row["boatclass"] = value; } }
        public string sailno { get { return _row["sailno"] as string; } set { _row["sailno"] = value; } }

        private DateTime _startDate;
        private DateTime _limitDate;

        public DateTime? start_date
        {
            get
            {
                return _row["start_date"] as DateTime?;
            }

            set
            {
                _row["start_date"] = value;
            }
        }

        public string finish_code
        {
            get
            {
                return _row["finish_code"] as string;
            }
            set
            {
                _row["finish_code"] = value;
            }
        }

        public DateTime? finish_date
        {
            get
            {
                return _row["finish_date"] as DateTime?;
            }

            set
            {
                _row["finish_date"] = value;
            }
        }

        public DateTime? interim_date
        {
            get
            {
                return _row["interim_date"] as DateTime?;
            }

            set
            {
                _row["interim_date"] = value;
            }
        }

        public double? override_points
        {
            get
            {
                return _row["override_points"] as double?;
            }
            set
            {
                _row["override_points"] = value;
            }
        }

        public int? laps
        {
            get
            {
                return _row["laps"] as int?;
            }
            set
            {
                _row["laps"] = value;
            }
        }

        public int? elapsed
        {
            get
            {
                return _row["elapsed"] as int?;
            }
            set
            {
                _row["elapsed"] = value;
            }
        }

        public double? corrected
        {
            get
            {
                return _row["corrected"] as double?;
            }
            set
            {
                _row["corrected"] = value;
            }
        }

        public double? standard_corrected
        {
            get
            {
                return _row["standard_corrected"] as double?;
            }
            set
            {
                _row["standard_corrected"] = value;
            }
        }

        public int? place
        {
            get
            {
                return _row["place"] as int?;
            }
            set
            {
                _row["place"] = value;
            }
        }

        public double? points
        {
            get
            {
                return _row["points"] as double?;
            }
            set
            {
                _row["points"] = value;
            }
        }

        public int? open_handicap
        {
            get
            {
                return _row["open_handicap"] as int?;
            }

            set
            {
                _row["open_handicap"] = value;
            }
        }

        public int? rolling_handicap
        {
            get
            {
                return _row["rolling_handicap"] as int?;
            }

            set
            {
                _row["rolling_handicap"] = value;
            }
        }

        public int? achieved_handicap
        {
            get
            {
                return _row["achieved_handicap"] as int?;
            }

            set
            {
                _row["achieved_handicap"] = value;
            }
        }

        public int? new_rolling_handicap
        {
            get
            {
                return _row["new_rolling_handicap"] as int?;
            }

            set
            {
                _row["new_rolling_handicap"] = value;
            }
        }

        public string handicap_status
        {
            get { return _row["handicap_status"] as string; }
            set { _row["handicap_status"] = value; }
        }

        public string c
        {
            get { return _row["c"] as string; }
            set { _row["c"] = value; }
        }

        public string a
        {
            get { return _row["a"] as string; }
            set { _row["a"] = value; }
        }
    }
}
