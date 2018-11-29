using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DemoWpf.Converters
{
    [ValueConversion(typeof(IEnumerable), typeof(string))]
    public class ArrayToStringConverter : IValueConverter
    {
        public static ArrayToStringConverter Instance { get; } = new ArrayToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var enumerable = (IEnumerable) value;
            var separator = parameter as string ?? ", ";
            return string.Join(separator, enumerable.Cast<object>());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}