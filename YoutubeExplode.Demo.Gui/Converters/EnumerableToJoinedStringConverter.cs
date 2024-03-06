using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace YoutubeExplode.Demo.Gui.Converters;

public class EnumerableToJoinedStringConverter : IValueConverter
{
    public static EnumerableToJoinedStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is IEnumerable enumerableValue
            ? string.Join(parameter as string ?? ", ", enumerableValue.Cast<object>())
            : default;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
