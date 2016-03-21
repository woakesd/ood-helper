using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace OodHelper.Rules
{
    class ConditionNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ct = value as ConditionType?;
            if (ct == null) return "";
 
            return CamelCaseToWordsRegex.Replace((Enum.GetName(typeof(ConditionType), ct) ?? ""), "$1 $2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            if (strValue == null) return null;

            ConditionType ct;
            if (Enum.TryParse(strValue.Replace(" ", ""), out ct))
                return ct;

            return null;
        }

        public static readonly Regex CamelCaseToWordsRegex = new Regex("(.)([A-Z])", RegexOptions.Compiled);
    }
}
