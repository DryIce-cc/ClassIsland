using ClassIsland.Core.Controls;
using ClassIsland.Core.Extensions;
using ClassIsland.Services;
using ClassIsland.Shared.Models.Profile;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    public TimeRuleEditControl()
    {
        InitializeComponent();
        Loaded += (_, _) => SettingsService.Settings.PropertyChanged += OnSettingsPropertyChanged;
        Unloaded += (_, _) => SettingsService.Settings.PropertyChanged -= OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsService.Settings.MultiWeekRotationMaxCycle))
            UpdateWeekCountDivTotalListBox();
    }

    public SettingsService SettingsService { get; } = App.GetService<SettingsService>();
    public TimeRule TimeRule
    {
        get => (TimeRule)GetValue(TimeRuleProperty);
        set => SetValue(TimeRuleProperty, value);
    }
    public static readonly DependencyProperty TimeRuleProperty =
        DependencyProperty.Register(nameof(TimeRule), typeof(TimeRule), typeof(TimeRuleEditControl),
            new PropertyMetadata(null, OnTimeRulePropertyChanged));

    private static void OnTimeRulePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        if (o is not TimeRuleEditControl control) return;
        if (args.OldValue is TimeRule oldRule)
        {
            oldRule.PropertyChanged -= control.OnTimeRuleModified;
        }

        if (args.NewValue is TimeRule newRule)
        {
            newRule.PropertyChanged += control.OnTimeRuleModified;
            control.UpdateWeekCountDivsListBox();
            control.UpdateWeekCountDivTotalListBox();
        }
    }

    private void OnTimeRuleModified(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeRule.WeekCountDivTotal))
            UpdateWeekCountDivsListBox();
    }

    private void UpdateWeekCountDivsListBox()
    {
        WeekCountDivsListBox.SelectionChanged -= WeekCountDivsListBox_FilterInvalidIndexes;
        WeekCountDivsListBox.ClearValue(SelectedIndexesListBox.SelectedIndexesProperty);

        // 填充列表
        var divList = new ObservableCollection<string> { "不限" };
        for (var i = 1; i <= TimeRule.WeekCountDivTotal; i++)
        {
            divList.Add(TimeRule.WeekCountDivTotal switch
            {
                <= 2 when i == 1 => "单周",
                <= 2 when i == 2 => "双周",
                _ => $"第{i.ToChinese()}周"
            });
        }
        WeekCountDivsListBox.ItemsSource = divList;

        FilterInvalidWeekCountDivs();

        WeekCountDivsListBox.SetBinding(SelectedIndexesListBox.SelectedIndexesProperty, new Binding(nameof(TimeRule.WeekCountDivs)));
        WeekCountDivsListBox.SelectionChanged += WeekCountDivsListBox_FilterInvalidIndexes;
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

    private void WeekCountDivsListBox_FilterInvalidIndexes(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not SelectedIndexesListBox listBox) return;
        if (e.AddedItems.Count > 0 && e.AddedItems[0]?.ToString() != "不限" && listBox.SelectedItems.Contains("不限"))
        {
            listBox.SelectedIndexes.Remove(0);
        }
        else if (e.AddedItems.Count > 0 && e.AddedItems[0]?.ToString() == "不限")
            //|| listBox.SelectedIndexes.Count == TimeRule.WeekCountDivTotal - 1
        {
            listBox.SelectedIndexes = [0];
        }
        FilterInvalidWeekCountDivs();
    }

    private void FilterInvalidWeekCountDivs()
    {
        var invalidIndexes = TimeRule.WeekCountDivs.ToList()
            .Where(i => i < 0 || i > TimeRule.WeekCountDivTotal).ToList();

        foreach (var index in invalidIndexes)
            TimeRule.WeekCountDivs.Remove(index);

        if (TimeRule.WeekCountDivs.Count == 0)
        {
            TimeRule.WeekCountDivs.Add(0);
        }
    }
}