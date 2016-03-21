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
    }
}
