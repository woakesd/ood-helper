using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace OodHelper.Rules
{
    [Svn("$Id$")]
    class ConditionNameListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<ConditionType> ct = value as IEnumerable<ConditionType>;
            if (ct != null)
            {
                return from x in ct
                     select System.Enum.GetName(typeof(ConditionType), x).Replace("_"," ");
                //return list;
            }
            else
            {
                return new string[] { string.Empty };
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            if (strValue != null)
            {
                ConditionType ct;
                if (System.Enum.TryParse<ConditionType>(strValue.Replace(" ", "_"), out ct))
                    return ct;
            }
            return null;
        }
    }
}
