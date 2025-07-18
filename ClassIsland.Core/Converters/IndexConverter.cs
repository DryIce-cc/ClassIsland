﻿using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;


namespace ClassIsland.Core.Converters;

public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var item = (ListBoxItem)value;
        var listBox = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
        return listBox!.ItemContainerGenerator.IndexFromContainer(item);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}