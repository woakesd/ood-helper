using System;
using System.Windows.Data;

namespace OodHelper.Converters
{
    [Svn("$Id$")]
    class MyBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && typeof(bool) == value.GetType() && (bool)value ||
                typeof(int) == value.GetType() && (int)value != 0)
                return System.Windows.Visibility.Visible;
            else
                return System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
