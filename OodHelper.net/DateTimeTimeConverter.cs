using System;
using System.Windows.Data;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class DateTimeTimeConverter : IValueConverter
    {
        DateTime _date;

        public DateTimeTimeConverter(DateTime d)
        {
            _date = d.Date;
        }

        public DateTimeTimeConverter()
        {
            _date = DateTime.Today;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && typeof(DateTime) == value.GetType())
            {
                DateTime x = (DateTime)value;
                return x.ToString("HH:mm:ss");
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
            if (TimeSpan.TryParse(strValue, out resultDateTime) || TimeSpan.TryParseExact(strValue, "hh\\ mm\\ ss", null, out resultDateTime))
            {
                return _date + resultDateTime;
            }
            return DBNull.Value; // DependencyProperty.UnsetValue;
        }
    }
}