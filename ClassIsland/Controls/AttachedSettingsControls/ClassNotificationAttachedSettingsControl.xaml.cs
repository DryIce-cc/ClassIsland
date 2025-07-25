#if false
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums;

namespace ClassIsland.Controls.AttachedSettingsControls;

/// <summary>
/// ClassNotificationAttachedSettingsControl.xaml 的交互逻辑
/// </summary>
[AttachedSettingsUsage(AttachedSettingsTargets.ClassPlan | AttachedSettingsTargets.TimeLayout |
                       AttachedSettingsTargets.Lesson | AttachedSettingsTargets.Subject |
                       AttachedSettingsTargets.TimePoint)]
[AttachedSettingsControlInfo("08F0D9C3-C770-4093-A3D0-02F3D90C24BC", "上课提醒设置", MaterialIconKind.BellNotificationOutline)]
public partial class ClassNotificationAttachedSettingsControl
{
    public ClassNotificationAttachedSettingsControl()
    {
        InitializeComponent();
    }
}
#endif
