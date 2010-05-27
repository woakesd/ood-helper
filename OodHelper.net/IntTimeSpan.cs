using System;
using System.Windows;
using System.Windows.Data;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class IntTimeSpan : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value)
            {
                int seconds = (Int32)value;
                if (seconds < 999999)
                {
                    TimeSpan s = new TimeSpan(0, 0, seconds);
                    return s.ToString();
                }
                return seconds.ToString();
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            TimeSpan resultDateTime;
            if (TimeSpan.TryParse(strValue, out resultDateTime))
            {
                return (int)resultDateTime.TotalSeconds;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}