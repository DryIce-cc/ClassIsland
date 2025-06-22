using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace ClassIsland.Core.Converters;

public class ListToStringConverter : IValueConverter
{
    /// <summary>
    /// 默认分隔符
    /// </summary>
    public string Separator { get; set; } = " ";

    /// <summary>
    /// 当输入空集合时返回值
    /// </summary>
    public string EmptyValue { get; set; } = string.Empty;

    /// <summary>
    /// 输出时是否进行排序
    /// </summary>
    public bool EnableOutputSorting { get; set; } = false;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 处理集合类型
        if (value is not IEnumerable enumerable)
            return value?.ToString() ?? EmptyValue;

        var items = new List<string>();
        foreach (var item in enumerable)
        {
            items.Add(item?.ToString() ?? string.Empty);
        }

        if (EnableOutputSorting) items.Sort();
        return items.Count > 0
            ? string.Join(Separator, items)
            : EmptyValue;
    }

    object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}