using System;
using System.Windows.Data;

namespace OodHelper.net
{
    class DateTimeConverter : IValueConverter
    {
        DateTime? _date;

        public DateTimeConverter(DateTime d)
        {
            _date = d.Date;
        }

        public DateTimeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && typeof(DateTime) == value.GetType())
            {
                DateTime x = (DateTime)value;
                return x.ToString("dd MMM yyyy HH:mm:ss");
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
            DateTime resultDate;
            if (TimeSpan.TryParseExact(strValue, "hh\\:mm\\:ss", null, out resultTime) || TimeSpan.TryParseExact(strValue, "hh\\ mm\\ ss", null, out resultTime))
            {
                return _date + resultTime;
            }
            if (DateTime.TryParseExact(strValue, "dd MMM yyyy HH mm ss", null, System.Globalization.DateTimeStyles.None, out resultDate)
                || DateTime.TryParseExact(strValue, "dd MMM yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out resultDate))
            {
                return resultDate;
            }
            return DBNull.Value;
        }
    }
}
