﻿using System.Collections.ObjectModel;
using System.Windows;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Services.Logging;
using ClassIsland.Core.Models.Logging;
using DynamicData;

namespace ClassIsland.Services.Logging;

public class AppLogService : IAppLogService
{
    public static readonly int MaxLogEntries = 1000;

    public SourceList<LogEntry> Logs { get; } = new();

    public void AddLog(LogEntry log)
    {
        var dispatcher = Dispatcher.UIThread;
        _ = dispatcher?.InvokeAsync(() =>
        {
            Logs.Add(log);
            while (Logs.Count > MaxLogEntries)
            {
                Logs.RemoveAt(0);
            }
        });
    }
}