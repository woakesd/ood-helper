using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace OodHelper.Rules
{
    class ConditionNameListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var ct = value as IEnumerable<ConditionType>;
            if (ct != null)
            {
                return from x in ct
                       select ConditionNameConverter.CamelCaseToWordsRegex.Replace(Enum.GetName(typeof(ConditionType), x) ?? "", "$1 $2");
            }
            return new[] { string.Empty };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var strValue = value as string;
            if (strValue == null) return null;
            ConditionType ct;
            if (Enum.TryParse(strValue.Replace(" ", ""), out ct))
                return ct;
            return null;
        }
    }
}
