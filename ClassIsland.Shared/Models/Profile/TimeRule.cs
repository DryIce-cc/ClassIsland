using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ClassIsland.Shared.JsonConverters;
#pragma warning disable CS0618 // 类型或成员已过时
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
    ObservableCollection<int> _weekDays = [0];
    /// <summary>
    /// 在一周中的哪些天启用这个课表。
    /// </summary>
    /// <remarks>
    /// 受到 <see cref="TimeRuleJsonConverter"/> 作用。
    /// </remarks>
    public ObservableCollection<int> WeekDays
    {
        get => _weekDays;
        set
        {
            _weekDays.CollectionChanged -= OnWeekDaysChanged;
            _weekDays = value;
            _weekDays.CollectionChanged += OnWeekDaysChanged;
            OnPropertyChanged(nameof(WeekDays));
            OnPropertyChanged(nameof(WeekDay));
        }
    }
    void OnWeekDaysChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(WeekDays));
        OnPropertyChanged(nameof(WeekDay));
    }


    ObservableCollection<int> _weekCountDivs = [0];
    /// <summary>
    /// 在多周轮换中的哪些周启用这个课表。
    /// </summary>
    /// <remarks>
    /// 受到 <see cref="TimeRuleJsonConverter"/> 作用。
    /// </remarks>
    public ObservableCollection<int> WeekCountDivs
    {
        get => _weekCountDivs;
        set
        {
            _weekCountDivs.CollectionChanged -= OnWeekCountDivsChanged;
            _weekCountDivs = value;
            _weekCountDivs.CollectionChanged += OnWeekCountDivsChanged;
            OnPropertyChanged(nameof(WeekCountDivs));
            OnPropertyChanged(nameof(WeekCountDiv));
            OnPropertyChanged(nameof(IsCycleEnabled));
        }
    }
    void OnWeekCountDivsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(WeekCountDivs));
        OnPropertyChanged(nameof(WeekCountDiv));
        OnPropertyChanged(nameof(IsCycleEnabled));
    }

    /// <summary>
    /// 多周轮换总周数
    /// </summary>
    [ObservableProperty] int _weekCountDivTotal = 2;

    /// <summary>
    /// 兼容旧版 WeekDay { get; set; }
    /// </summary>
    [JsonIgnore, Obsolete($"迁移至支持多天的 {nameof(WeekDays)}。")]
    public int WeekDay
    {
        get => WeekDays.Count > 0 ? WeekDays.Min() : 0;
        set => WeekDays = [value];
    }

    /// <summary>
    /// 兼容旧版 WeekCountDiv { get; set; }
    /// </summary>
    [JsonIgnore, Obsolete($"迁移至支持多周的 {nameof(WeekCountDivs)}。")]
    public int WeekCountDiv
    {
        get => WeekCountDivs.Count > 0 ? WeekCountDivs.Min() : 0;
        set => WeekCountDivs = [value];
    }


    /// <summary>
    /// 是否启用多周轮换。
    /// </summary>
    public bool IsCycleEnabled => WeekCountDivs.Contains(0);
}
