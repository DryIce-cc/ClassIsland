﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Data.Converters;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Models.Components;
using ClassIsland.Services;
using ClassIsland.Views;
using ClassIsland.Views.SettingPages;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassIsland.ViewModels.SettingsPages;

public partial class ComponentsSettingsViewModel : ObservableRecipient
{
    public IComponentsService ComponentsService { get; }
    public SettingsService SettingsService { get; }

    public ComponentsSettingsPageDropHandler DropHandler { get; }
    
    [ObservableProperty] private ComponentSettings? _selectedComponentSettings;
    [ObservableProperty] private string _createProfileName = "";
    [ObservableProperty] private int _settingsTabControlIndex = 0;
    [ObservableProperty] private bool _isComponentSettingsVisible = false;
    [ObservableProperty] private bool _isComponentAdvancedSettingsVisible = false;
    [ObservableProperty] private ObservableCollection<ComponentSettings> _selectedComponentContainerChildren = new();
    [ObservableProperty] private bool _isComponentChildrenViewOpen = false;
    [ObservableProperty] private bool _isSelectedInChildrenListBox = false;
    [ObservableProperty] private ComponentSettings? _selectedComponentSettingsMain;
    [ObservableProperty] private ComponentSettings? _selectedComponentSettingsChild;
    [ObservableProperty] private Stack<ComponentSettings> _childrenComponentSettingsNavigationStack = new();
    [ObservableProperty] private ComponentSettings? _selectedContainerComponent;
    [ObservableProperty] private bool _canChildrenNavigateBack = false;
    [ObservableProperty] private ComponentSettings? _selectedRootComponent;
    [ObservableProperty] private MainWindowLineSettings? _selectedMainWindowLineSettings;

    public static FuncValueConverter<int, double> ComponentsEditorHeightConverter { get; } = new(x => x * 43.2);

    /// <inheritdoc/>
    public ComponentsSettingsViewModel(IComponentsService componentsService, SettingsService settingsService)
    {
        ComponentsService = componentsService;
        SettingsService = settingsService;
        DropHandler = new ComponentsSettingsPageDropHandler(this);
    }
}