using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YoutubeExplode.Demo.Gui.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public static BoolToVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var falseVisibility = parameter is Visibility visibilityParameter
            ? visibilityParameter
            : Visibility.Hidden;

        return value is true
            ? Visibility.Visible
            : falseVisibility;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}