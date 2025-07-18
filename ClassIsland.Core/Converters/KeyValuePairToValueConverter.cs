﻿using System.Globalization;
using Avalonia.Data.Converters;


namespace ClassIsland.Core.Converters;

public class KeyValuePairToValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? null : ((KeyValuePair<object, object>)value).Value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}