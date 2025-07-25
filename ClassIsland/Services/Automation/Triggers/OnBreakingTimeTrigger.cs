using System;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;

namespace ClassIsland.Services.Automation.Triggers;

[TriggerInfo("classisland.lessons.onBreakingTime", "课间休息时", "\ue4c4")]
public class OnBreakingTimeTrigger(ILessonsService lessonsService) : TriggerBase
{
    private ILessonsService LessonsService { get; } = lessonsService;

    public override void Loaded()
    {
        LessonsService.OnBreakingTime += LessonsServiceOnOnBreakingTime;
    }
    public override void UnLoaded()
    {
        LessonsService.OnBreakingTime -= LessonsServiceOnOnBreakingTime;
    }

    private void LessonsServiceOnOnBreakingTime(object? sender, EventArgs e)
    {
        Trigger();
    }
}