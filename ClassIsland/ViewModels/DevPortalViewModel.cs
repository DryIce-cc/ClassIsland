using System;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassIsland.ViewModels;

public partial class DevPortalViewModel(
    INotificationHostService notificationHostService,
    SettingsService settingsService,
    IExactTimeService exactTimeService) : ObservableObject
{
    public INotificationHostService NotificationHostService { get; } = notificationHostService;
    public SettingsService SettingsService { get; } = settingsService;
    public IExactTimeService ExactTimeService { get; } = exactTimeService;

    [ObservableProperty] private string _notificationMaskText = "";
    
    [ObservableProperty] private string _notificationOverlayText = "";

    [ObservableProperty] private DateTime _targetDate = DateTime.Now;
    
    [ObservableProperty] private TimeSpan _targetTime = TimeSpan.Zero;

    [ObservableProperty] private bool _isTargetDateLoaded = false;

    [ObservableProperty] private bool _isTargetTimeLoaded = false;

    [ObservableProperty] private string _toastTitle = "";
    [ObservableProperty] private string _toastMessage = "";
    [ObservableProperty] private bool _toastHaveActions = false;
    [ObservableProperty] private bool _toastCanUserClose = true;

    public bool IsTargetDateTimeLoaded => IsTargetDateLoaded && IsTargetTimeLoaded;
}