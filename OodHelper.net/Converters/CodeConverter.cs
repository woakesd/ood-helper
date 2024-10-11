using System;
using System.Windows.Data;

namespace OodHelper.Converters
{
    class CodeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value && value != null)
            {
                return value.ToString()!;
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            if (strValue != null && strValue != string.Empty)
            {
                strValue = strValue.ToUpper();
                switch (strValue)
                {
                    case "RTD":
                        strValue = "RET";
                        break;
                }
                return strValue;
            }
            return DBNull.Value;
        }
    }
}
