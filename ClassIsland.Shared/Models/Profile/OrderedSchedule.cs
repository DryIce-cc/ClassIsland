using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassIsland.Shared.Models.Profile;

/// <summary>
/// 代表一个预定的课表。
/// </summary>
public class OrderedSchedule : ObservableRecipient
{
    private Guid _classPlanId = Guid.Empty;

    /// <summary>
    /// 预定课表 ID
    /// </summary>
    public Guid ClassPlanId
    {
        get => _classPlanId;
        set
        {
            if (value == _classPlanId) return;
            _classPlanId = value;
            OnPropertyChanged();
        }
    }
}