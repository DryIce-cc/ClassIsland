using ClassIsland.Core.Extensions;
using ClassIsland.Services;
using ClassIsland.Shared.Models.Profile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
namespace ClassIsland.Controls;

/// <summary>
/// 时间规则编辑控件。
/// </summary>
public partial class TimeRuleEditControl
{
    public TimeRuleEditControl() => InitializeComponent();
    public SettingsService SettingsService { get; } = App.GetService<SettingsService>();
    public TimeRule TimeRule
    {
        get => (TimeRule)GetValue(TimeRuleProperty);
        set => SetValue(TimeRuleProperty, value);
    }
    public static readonly DependencyProperty TimeRuleProperty =
        DependencyProperty.Register(nameof(TimeRule), typeof(TimeRule), typeof(TimeRuleEditControl),
            new PropertyMetadata(null, static (o, args) =>
            {
                // OnRuleChanged
                if (o is not TimeRuleEditControl control) return;
                if (args.OldValue is TimeRule oldRule)
                {
                    control.WeekDayListBox.SelectionChanged -= control.WeekDayListBox_SelectionChanged;
                    control.WeekCountDivsListBox.SelectionChanged -= control.WeekCountDivsListBox_SelectionChanged;
                    oldRule.PropertyChanged -= control.OnRuleModified;
                }
                if (args.NewValue is TimeRule newRule)
                {
                    newRule.PropertyChanged += control.OnRuleModified;
                    control.UpdateWeekDayListBox();
                    control.UpdateWeekCountDivsListBox();
                    control.UpdateWeekCountDivTotalListBox();
                }
            }));

    private void OnRuleModified(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeRule.WeekCountDivTotal))
            UpdateWeekCountDivsListBox();
    }

    private void UpdateWeekDayListBox()
    {
        WeekDayListBox.SelectionChanged -= WeekDayListBox_SelectionChanged;

        // 设置选中项
        WeekDayListBox.SelectedItems.Clear();
        foreach (var value in TimeRule.WeekDays)
        {
            WeekDayListBox.SelectedItems.Add(WeekDayListBox.Items[value]);
        }

        WeekDayListBox.SelectionChanged += WeekDayListBox_SelectionChanged;
    }

    private void WeekDayListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 应用选中项
        var selectedValues = new List<int>();
        foreach (var item in WeekDayListBox.SelectedItems)
        {
            selectedValues.Add(Convert.ToInt32(((ListBoxItem)item).Tag));
        }
        TimeRule.WeekDays = selectedValues;
    }

    private void UpdateWeekCountDivsListBox()
    {
        WeekCountDivsListBox.SelectionChanged -= WeekCountDivsListBox_SelectionChanged;

        // 填充列表
        var divList = new ObservableCollection<ListBoxItem> { new() { Content = "不限", Tag = 0 } };
        for (var i = 1; i <= TimeRule.WeekCountDivTotal; i++)
        {
            divList.Add(TimeRule.WeekCountDivTotal switch
            {
                <= 2 when i == 1 => new() { Content = "单周", Tag = 1 },
                <= 2 when i == 2 => new() { Content = "双周", Tag = 2 },
                _ => new() { Content = $"第{i.ToChinese()}周", Tag = i }
            });
        }
        WeekCountDivsListBox.ItemsSource = divList;

        // 设置选中项
        WeekCountDivsListBox.SelectedItems.Clear();
        foreach (var item in TimeRule.WeekCountDivs)
        {
            if (item < divList.Count)
            {
                WeekCountDivsListBox.SelectedItems.Add(divList[item]);
            }
        }

        WeekCountDivsListBox.SelectionChanged += WeekCountDivsListBox_SelectionChanged;
    }

    private void WeekCountDivsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 应用选中项
        var selectedValues = new List<int>();
        foreach (var item in WeekCountDivsListBox.SelectedItems) 
        {
            selectedValues.Add((int)((ListBoxItem)item).Tag); 
        }
        TimeRule.WeekCountDivs = selectedValues;
    }

    private void UpdateWeekCountDivTotalListBox()
    {
        WeekCountDivTotalListBox.ClearValue(Selector.SelectedIndexProperty);

        // 填充列表
        var divTotalList = new ObservableCollection<ListBoxItem> {
            new() { Visibility = Visibility.Collapsed },
            new() { Visibility = Visibility.Collapsed },
            new() { Content = "两周" }, };

        var maxCycle = Math.Max(SettingsService.Settings.MultiWeekRotationMaxCycle, TimeRule.WeekCountDivTotal);
        for (var i = 3; i <= maxCycle; i++)
        {
            divTotalList.Add(new ListBoxItem() { Content = $"{i.ToChinese()}周" });
        }
        WeekCountDivTotalListBox.ItemsSource = divTotalList;

        WeekCountDivTotalListBox.SetBinding(Selector.SelectedIndexProperty, new Binding(nameof(TimeRule.WeekCountDivTotal)));
    }
}