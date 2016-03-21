using System;
using System.Windows.Data;

namespace OodHelper.Converters
{
    class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && value != null)
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            if (strValue != string.Empty && strValue != null)
            {
                return strValue;
            }
            return DBNull.Value;
        }
    }
}
