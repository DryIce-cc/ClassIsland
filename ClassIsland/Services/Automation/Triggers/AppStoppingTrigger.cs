using System;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;

namespace ClassIsland.Services.Automation.Triggers;

[TriggerInfo("classisland.lifetime.stopping", "应用退出时", "\ue0df")]
public class AppStoppingTrigger : TriggerBase
{
    public override void Loaded()
    {
        AppBase.Current.AppStopping += CurrentOnAppStarted;
    }

    public override void UnLoaded()
    {
        AppBase.Current.AppStopping -= CurrentOnAppStarted;
    }

    private void CurrentOnAppStarted(object? sender, EventArgs e)
    {
        Trigger();
    }
}