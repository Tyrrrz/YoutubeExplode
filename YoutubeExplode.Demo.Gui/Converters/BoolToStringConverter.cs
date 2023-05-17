using System;
using System.Globalization;
using System.Windows.Data;

namespace YoutubeExplode.Demo.Gui.Converters;

[ValueConversion(typeof(bool), typeof(string))]
public class BoolToStringConverter : IValueConverter
{
    public static BoolToStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool boolValue
            ? boolValue ? "yes" : "no"
            : default;

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}