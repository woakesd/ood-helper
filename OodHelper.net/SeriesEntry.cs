using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    [Svn("$Id$")]
    public class SeriesEntry
    {
        public int rid;
        public int bid;
        public double? points = null;
        public double? override_points = null;
        public bool discard;
        public string code;
        public DateTime date;

        public string CodeDisplay
        {
            get
            {
                if (code != null && code != string.Empty)
                    return "(" + code + ")";
                return string.Empty;
            }
        }

        public double Points
        {
            get
            {
                if (override_points != null && override_points != 0.0)
                    return override_points.Value;
                else
                    return points.Value;
            }
        }
    }

}
