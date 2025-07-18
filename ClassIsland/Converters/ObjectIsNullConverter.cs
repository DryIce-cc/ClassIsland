﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ClassIsland.Converters;

public class ObjectIsNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}