using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YoutubeExplode.DemoWpf.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public static BoolToVisibilityConverter Instance { get; } = new BoolToVisibilityConverter();

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var falseVisibility = Visibility.Hidden;
            if (parameter is Visibility parameterVisibility)
                falseVisibility = parameterVisibility;

            if (value is null)
                return falseVisibility;

            var valueBool = (bool) value;
            return valueBool ? Visibility.Visible : falseVisibility;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}