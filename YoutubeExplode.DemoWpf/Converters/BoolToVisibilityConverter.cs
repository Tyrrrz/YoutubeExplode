using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YoutubeExplode.DemoWpf.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public static BoolToVisibilityConverter Instance { get; } = new();

    public virtual object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var falseVisibility = parameter is Visibility visibilityParameter
            ? visibilityParameter
            : Visibility.Hidden;

        return value is bool boolValue && boolValue
            ? Visibility.Visible
            : falseVisibility;
    }

    public virtual object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}