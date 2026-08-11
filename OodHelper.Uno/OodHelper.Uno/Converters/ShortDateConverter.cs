using System;
using Microsoft.UI.Xaml.Data;

namespace OodHelper.Uno.Converters
{
    /// <summary>
    /// Formats a nullable <see cref="DateTime"/> as a short date for display. Illustrates the
    /// mechanical WPF→WinUI converter port: the interface is
    /// <c>Microsoft.UI.Xaml.Data.IValueConverter</c> (not <c>System.Windows.Data</c>) and the culture
    /// argument is a <c>string language</c> rather than a <c>CultureInfo</c>. WinUI bindings never
    /// surface <c>DBNull</c>, so the old DBNull guards are dropped.
    /// </summary>
    public sealed class ShortDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is DateTime dt ? dt.ToString("d") : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
