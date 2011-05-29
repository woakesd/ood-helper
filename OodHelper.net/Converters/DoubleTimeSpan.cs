using System;
using System.Windows;
using System.Windows.Data;

namespace OodHelper.Converters
{
    [Svn("$Id$")]
    class DoubleTimeSpan : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value)
            {
                double seconds = (double)value;
                TimeSpan s = new TimeSpan(0, 0, 0, (int)Math.Truncate(seconds),
                    (int)Math.Round((seconds - Math.Truncate(seconds)) * 1000));
                if (s.Days > 0)
                    return s.ToString("d\\ hh\\:mm\\:ss\\.ff");
                return s.ToString("hh\\:mm\\:ss\\.ff");
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            TimeSpan resultDateTime;
            if (TimeSpan.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime.TotalSeconds;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}