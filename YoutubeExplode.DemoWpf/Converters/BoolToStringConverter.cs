﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace YoutubeExplode.DemoWpf.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToStringConverter : IValueConverter
    {
        public static BoolToStringConverter Instance { get; } = new BoolToStringConverter();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return default(string);

            var b = (bool) value;
            return b ? "yes" : "no";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}