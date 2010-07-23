﻿using System;
using System.Windows.Data;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && value != null)
            {
                int i = (Int32)value;
                return i.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            int i;
            if (Int32.TryParse(strValue, out i))
            {
                return i;
            }
            return DBNull.Value;
        }
    }
}
