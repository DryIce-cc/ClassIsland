﻿using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Abstractions.Services.Management;
using ClassIsland.Core.Abstractions.Services.Metadata;
using ClassIsland.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassIsland.ViewModels.SettingsPages;

public class GeneralSettingsViewModel(
    SettingsService settingsService,
    IManagementService managementService,
    IExactTimeService exactTimeService,
    ISplashService splashService,
    IAnnouncementService announcementService) : ObservableRecipient
{
    public SettingsService SettingsService { get; } = settingsService;
    public IManagementService ManagementService { get; } = managementService;
    public IExactTimeService ExactTimeService { get; } = exactTimeService;
    public ISplashService SplashService { get; } = splashService;
    public IAnnouncementService AnnouncementService { get; } = announcementService;
    private bool _isWeekOffsetSettingsOpen = false;

    public bool IsWeekOffsetSettingsOpen
    {
        get => _isWeekOffsetSettingsOpen;
        set
        {
            if (value == _isWeekOffsetSettingsOpen) return;
            _isWeekOffsetSettingsOpen = value;
            OnPropertyChanged();
        }
    }
}