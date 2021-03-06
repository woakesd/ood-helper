﻿using System;
using System.Windows.Data;

namespace OodHelper.Converters
{
    class TimeSpanHMConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && value != null)
            {
                TimeSpan x = (TimeSpan)value;
                return x.ToString("hh\\:mm");
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            TimeSpan resultTime;
            if (TimeSpan.TryParse(strValue, out resultTime) || TimeSpan.TryParseExact(strValue, "hh\\ mm", null, out resultTime))
            {
                return resultTime;
            }
            return DBNull.Value; // DependencyProperty.UnsetValue;
        }
    }
}
