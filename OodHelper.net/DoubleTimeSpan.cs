﻿using System;
using System.Windows;
using System.Windows.Data;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class DoubleTimeSpan : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value)
            {
                double seconds = (double)value;
                if (seconds < 999999)
                {
                    TimeSpan s = new TimeSpan(0, 0, 0, (int)Math.Truncate(seconds),
                        (int)Math.Round((seconds - Math.Truncate(seconds)) * 1000));
                    return s.ToString().Replace("00000", string.Empty);
                }
                else
                    return seconds.ToString();
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