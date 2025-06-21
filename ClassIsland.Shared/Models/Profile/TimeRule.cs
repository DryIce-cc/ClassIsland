using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ClassIsland.Shared.JsonConverters;
namespace ClassIsland.Shared.Models.Profile;

/// <summary>
/// 代表一个课表<see cref="ClassPlan"/>触发规则。
/// </summary>
/// <remarks>
/// 受到 <see cref="TimeRuleJsonConverter"/> 作用。
/// </remarks>
[JsonConverter(typeof(TimeRuleJsonConverter))]
public partial class TimeRule : ObservableRecipient
{
    /// <summary>
    /// 在一周中的哪些天启用这个课表。
    /// </summary>
    /// <remarks>
    /// 受到 <see cref="TimeRuleJsonConverter"/> 作用。
    /// </remarks>
    [ObservableProperty] ObservableCollection<int> _weekDays = [0];

    /// <summary>
    /// 在多周轮换中的哪些周启用这个课表。
    /// </summary>
    /// <remarks>
    /// 受到 <see cref="TimeRuleJsonConverter"/> 作用。
    /// </remarks>
    [ObservableProperty] ObservableCollection<int> _weekCountDivs = [0];

    /// <summary>
    /// 多周轮换总周数
    /// </summary>
    [ObservableProperty] int _weekCountDivTotal = 2;

    /// <summary>
    /// 兼容旧版 WeekDay { get; set; }
    /// </summary>
    [JsonIgnore, Obsolete($"{nameof(WeekDay)} 已过时：迁移至支持多天的 {nameof(WeekDays)}。")]
    public int WeekDay
    {
        get => WeekDays.Count > 0 ? WeekDays[0] : 0;
        set => WeekDays = [value];
    }

    /// <summary>
    /// 兼容旧版 WeekCountDiv { get; set; }
    /// </summary>
    [JsonIgnore, Obsolete($"{nameof(WeekCountDiv)} 已过时：迁移至支持多周的 {nameof(WeekCountDivs)}。")]
    public int WeekCountDiv
    {
        get => WeekCountDivs.Count > 0 ? WeekCountDivs[0] : 0;
        set => WeekCountDivs = [value];
    }
}