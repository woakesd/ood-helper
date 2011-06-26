using System;
using System.Windows.Data;

namespace OodHelper.Converters
{
    class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && value != null)
            {
                double i = (double)value;
                return i.ToString("0.#");
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            double i;
            if (Double.TryParse(strValue, out i))
            {
                return i;
            }
            return DBNull.Value;
        }
    }
}
