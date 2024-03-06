using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace YoutubeExplode.Demo.Gui.Converters;

public class BoolToYesNoStringConverter : IValueConverter
{
    public static BoolToYesNoStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is bool boolValue
            ? boolValue
                ? "yes"
                : "no"
            : default;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
