using System;
using System.Windows.Data;

namespace OodHelper.Converters
{
    [Svn("$Id: DateTimeConverter.cs 224 2011-05-21 16:07:44Z woakesdavid $")]
    class MyDateTimeConverter : IValueConverter
    {
        DateTime? _date;

        public MyDateTimeConverter(DateTime d)
        {
            _date = d.Date;
        }

        public MyDateTimeConverter()
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
            if (strValue != null)
            {
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
            }
            return DBNull.Value;
        }
    }
}
