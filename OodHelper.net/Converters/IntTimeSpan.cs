﻿using System;
using System.Windows;
using System.Windows.Data;

namespace OodHelper.Converters
{
    class IntTimeSpan : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && value != null)
            {
                int seconds = (Int32)value;
                TimeSpan s = new TimeSpan(0, 0, seconds);
                if (s.Days > 0)
                    return s.ToString("UpdateUIDelegate\\ hh\\:mm\\:ss");
                return s.ToString("hh\\:mm\\:ss");
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
                return (int)resultDateTime.TotalSeconds;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}