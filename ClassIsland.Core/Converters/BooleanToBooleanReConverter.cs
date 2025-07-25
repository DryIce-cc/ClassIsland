﻿using System.Globalization;

using Avalonia.Data.Converters;

namespace ClassIsland.Core.Converters;

public class BooleanToBooleanReConverter : IValueConverter  
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
}