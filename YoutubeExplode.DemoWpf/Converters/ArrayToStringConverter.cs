using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace YoutubeExplode.DemoWpf.Converters;

[ValueConversion(typeof(IEnumerable), typeof(string))]
public class ArrayToStringConverter : IValueConverter
{
    public static ArrayToStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is IEnumerable enumerableValue
            ? string.Join(
                parameter as string ?? ", ",
                enumerableValue.Cast<object>()
            )
            : default;

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}