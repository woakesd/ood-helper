using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.Converters
{
    public class ValueParser
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

        public static readonly TimeSpan TwentyFourHours = new TimeSpan(1, 0, 0, 0);

        public static TimeSpan? ReadTimeSpan(string Value)
        {
            TimeSpan _tmp;

            //
            // The parse string hmm doesn't work, so pad left with a zero and
            // pick up something like 932 with hhmm.
            //
            if (Value.Length == 3) Value = Value.PadLeft(4, '0');

            if (!(TimeSpan.TryParseExact(Value, new [] { "hh':'mm", "h':'mm", "hh' 'mm", "h' 'mm", "hhmm" }, null, out _tmp)))
                return null;
            
            return (Nullable<TimeSpan>)_tmp;
        }
    }
}
