using System;

namespace OodHelper
{
    public class SeriesEntry
    {
        public int bid;
        public string code;
        public DateTime date;
        public bool discard = false;
        public double? override_points = null;
        public double? points = null;
        public int rid;

        public bool IsAverageScore
        {
            get
            {
                switch (code)
                {
                    case "OOD":
                    case "AVG":
                    case "RSC":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool Discard => discard;

        public string CodeDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(code))
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
                if (points.HasValue)
                    return points.Value;
                return double.NaN;
            }
        }
    }
}