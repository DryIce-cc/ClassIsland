﻿using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Models.Automation.Triggers;
using ClassIsland.Models.EventArgs;

namespace ClassIsland.Services.Automation.Triggers;

[TriggerInfo("classisland.signal", "收到信号时", "\ue40e")]
public class SignalTrigger(SignalTriggerHandlerService signalTriggerHandlerService) : TriggerBase<SignalTriggerSettings>
{
    public SignalTriggerHandlerService SignalTriggerHandlerService { get; } = signalTriggerHandlerService;

    public override void Loaded()
    {
        SignalTriggerHandlerService.Handled += SignalTriggerHandlerServiceOnHandled;
    }


    public override void UnLoaded()
    {
        SignalTriggerHandlerService.Handled -= SignalTriggerHandlerServiceOnHandled;
    }

    private void SignalTriggerHandlerServiceOnHandled(object? sender, SignalTriggerEventArgs e)
    {
        if (e.SignalName != Settings.SignalName) return;

        if (e.Revert)
        {
            TriggerRevert();
        }
        else
        {
            Trigger();
        }
    }
}