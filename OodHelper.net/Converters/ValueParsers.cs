using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.Converters
{
    public class ValueParsers
    {
        public static int? ReadInt(string Value)
        {
            int _tmp;
            if (Int32.TryParse(Value, out _tmp))
                return _tmp;
            else
                return null;
        }

        public static double? ReadDouble(string Value)
        {
            Double _tmp;
            if (Double.TryParse(Value, out _tmp))
                return _tmp;
            else
                return null;
        }

        public static TimeSpan? ReadTimeSpan(string Value)
        {
            TimeSpan _tmp;

            if (!(TimeSpan.TryParseExact(Value, new [] { "hh':'mm", "h':'mm", "hh' 'mm", "h' 'mm", "hhmm", "hmm" }, null, out _tmp)))
                return null;
            
            return (Nullable<TimeSpan>)_tmp;
        }
    }
}
