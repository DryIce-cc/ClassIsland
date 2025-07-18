using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Xaml.Interactions.DragAndDrop;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Models;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Controls;
using ClassIsland.Core.Controls.Ruleset;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.Core.Helpers.UI;
using ClassIsland.Core.Models.Components;
using ClassIsland.Services;
using ClassIsland.Shared;
using ClassIsland.Shared.Helpers;
using ClassIsland.ViewModels.SettingsPages;
using FluentAvalonia.UI.Controls;

namespace ClassIsland.Views.SettingPages;

[SettingsPageInfo("components", "组件", "\ue06f", "\ue06f", SettingsPageCategory.Internal)]
public partial class ComponentsSettingsPage : SettingsPageBase
{
    public ComponentsSettingsViewModel ViewModel { get; } = IAppHost.GetService<ComponentsSettingsViewModel>();
    
    public ComponentsSettingsPage()
    {
        InitializeComponent();
        DataContext = this;
    }
    
    private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ComponentsService.RefreshConfigs();
    }

    private async void ButtonCreateConfig_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.CreateProfileName = "";

        var textBox = new TextBox()
        {
            Text = ""
        };
        var dialogResult = await new ContentDialog()
        {
            Title = "创建组件配置",
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "创建",
            SecondaryButtonText = "取消",
            Content = new Field()
            {
                Content = textBox,
                Label = "组件名",
                Suffix = ".json"
            }
        }.ShowAsync();

        ViewModel.CreateProfileName = textBox.Text;
        var path = Path.Combine(ClassIsland.Services.ComponentsService.ComponentSettingsPath,
            ViewModel.CreateProfileName + ".json");
        if (dialogResult != ContentDialogResult.Primary || File.Exists(path))
        {
            return;
        }
        ConfigureFileHelper.SaveConfig(path, ClassIsland.Services.ComponentsService.DefaultComponentProfile);
        ViewModel.ComponentsService.RefreshConfigs();
        ViewModel.SettingsService.Settings.CurrentComponentConfig = ViewModel.CreateProfileName;
    }
    
    private void SelectorComponents_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedComponentSettingsMain != null)
        {
            ViewModel.SelectedComponentSettings = ViewModel.SelectedComponentSettingsMain;
            ViewModel.SelectedComponentSettingsChild = null;
        }
        UpdateSettingsVisibility();
    }
    
    private void UpdateSettingsVisibility()
    {
        if (ViewModel.SelectedComponentSettings == null)
        {
            ViewModel.IsComponentAdvancedSettingsVisible = false;
            ViewModel.IsComponentSettingsVisible = false;
            ViewModel.SettingsTabControlIndex = 0;
            return;
        }

        ViewModel.IsComponentAdvancedSettingsVisible = true;
        if (ViewModel.SelectedComponentSettings.AssociatedComponentInfo.SettingsType == null)
        {
            ViewModel.IsComponentSettingsVisible = false;
            ViewModel.SettingsTabControlIndex = ViewModel.SettingsTabControlIndex == 1 ? 0 : ViewModel.SettingsTabControlIndex;
            return;
        }
        ViewModel.IsComponentSettingsVisible = true;
        ViewModel.SettingsTabControlIndex = ViewModel.SettingsTabControlIndex == 0 ? 1 : ViewModel.SettingsTabControlIndex;
    }

    private void ButtonOpenConfigFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = Path.GetFullPath(ClassIsland.Services.ComponentsService.ComponentSettingsPath),
            UseShellExecute = true
        });
    }
    
    private void ButtonRemoveSelectedComponent_OnClick(object sender, RoutedEventArgs e)
    {
        var remove = ViewModel.SelectedComponentSettings;
        if (remove == null)
            return;
        if (ViewModel.SelectedComponentSettings == ViewModel.SelectedRootComponent)
        {
            CloseComponentChildrenView();
        }
        ViewModel.SelectedComponentSettings = null;
        if (ViewModel.SelectedComponentSettingsMain != null)
        {
            foreach (var line in ViewModel.ComponentsService.CurrentComponents.Lines)
            {
                if (line.Children.Remove(remove))
                {
                    break;
                }
            }
        } else if (ViewModel.SelectedComponentSettingsChild != null)
        {
            ViewModel.SelectedComponentContainerChildren.Remove(remove);
        }
    }
    
    private void ButtonChildrenViewClose_OnClick(object sender, RoutedEventArgs e)
    {
        CloseComponentChildrenView();
    }
    
    private void CloseComponentChildrenView()
    {
        ViewModel.IsComponentChildrenViewOpen = false;
        ViewModel.ChildrenComponentSettingsNavigationStack.Clear();
        ViewModel.CanChildrenNavigateBack = false;
        ViewModel.SelectedComponentContainerChildren = [];
        ViewModel.SelectedRootComponent = null;
    }
    
    private void ButtonOpenRuleset_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.FindResource("RulesetControl") is not RulesetControl control ||
            ViewModel.SelectedComponentSettings == null) 
            return;
        control.Ruleset = ViewModel.SelectedComponentSettings.HidingRules;
        OpenDrawer("RulesetControl");
    }
    
    private void ButtonShowChildrenComponents_OnClick(object sender, RoutedEventArgs e)
    {
        SetCurrentSelectedComponentContainer(ViewModel.SelectedComponentSettings);
    }

    private void SetCurrentSelectedComponentContainer(ComponentSettings? componentSettings, bool isBack=false)
    {
        if (componentSettings?.AssociatedComponentInfo?.IsComponentContainer != true)
        {
            return;
        }

        if (componentSettings.Settings is not IComponentContainerSettings settings)
        {
            return;
        }
        if (componentSettings == ViewModel.SelectedComponentSettingsMain)
        {
            ViewModel.ChildrenComponentSettingsNavigationStack.Clear();
            ViewModel.SelectedRootComponent = componentSettings;
        } else if (ViewModel.SelectedContainerComponent != null && !isBack)
        {
            ViewModel.ChildrenComponentSettingsNavigationStack.Push(ViewModel.SelectedContainerComponent);
        }
        ViewModel.SelectedComponentContainerChildren = settings.Children;
        ViewModel.SelectedContainerComponent = componentSettings;
        ViewModel.IsComponentChildrenViewOpen = true;
        ViewModel.CanChildrenNavigateBack = ViewModel.ChildrenComponentSettingsNavigationStack.Count >= 1;
    }

    private void ListBoxComponents_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // 这里应该把指针按下的事件吞掉，这样在拖动这里面的在组件时就不会把事件往上传送到上级，让上级 ListBoxItem 也跟着被拖动。
        e.Handled = true;
    }

    private void ButtonCreateMainWindowLine_OnClick(object? sender, RoutedEventArgs e)
    {
        var index = ViewModel.SelectedMainWindowLineSettings == null
            ? 0
            : Math.Max(0,
                ViewModel.ComponentsService.CurrentComponents.Lines.IndexOf(ViewModel.SelectedMainWindowLineSettings) + 1);
        var lineSettings = new MainWindowLineSettings();
        ViewModel.ComponentsService.CurrentComponents.Lines.Insert(index, lineSettings);
        ViewModel.SelectedMainWindowLineSettings = lineSettings;
    }

    private void ButtonRemoveSelectedMainWindowLine_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.ComponentsService.CurrentComponents.Lines.Count <= 1)
        {
            this.ShowWarningToast("至少需要保留 1 个主界面行。");
            return;
        }

        if (ViewModel.SelectedMainWindowLineSettings != null)
            ViewModel.ComponentsService.CurrentComponents.Lines.Remove(ViewModel.SelectedMainWindowLineSettings);
    }

    private void ToggleButtonIsNotificationEnabled_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (!ViewModel.ComponentsService.CurrentComponents.Lines.Any(x => x.IsNotificationEnabled))
        {
            this.ShowWarningToast("您已经禁用了所有主界面行的提醒显示功能。如果没有插件注册其它提醒消费者，提醒将不会显示，也不会播放提醒音效、特效和语音。");
        }
    }

    private void ToggleButtonIsMainLine_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton button)
        {
            return;
        }

        foreach (var line in ViewModel.ComponentsService.CurrentComponents.Lines.Where(x => !Equals(button.DataContext, x)))
        {
            line.IsMainLine = false;
        }

        if (button.IsChecked == false)
        {
            var firstLine = ViewModel.ComponentsService.CurrentComponents.Lines.FirstOrDefault();
            if (firstLine != null) 
                firstLine.IsMainLine = true;
            this.ShowToast("已将第一行设置为主要行。");
        }
    }
}