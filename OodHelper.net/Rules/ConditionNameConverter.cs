using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace OodHelper.Rules
{
    [Svn("$Id: IntConverter.cs 113 2010-08-15 12:37:12Z woakesdavid $")]
    class ConditionNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ConditionType? ct = value as ConditionType?;
            if (ct != null)
            {
                return System.Enum.GetName(typeof(ConditionType), ct).Replace("_", " ");
            }
            else
            {
                return string.Empty;
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
