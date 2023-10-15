﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClassIsland.Controls;
using ClassIsland.ViewModels;
using MdXaml;
using Microsoft.AppCenter.Analytics;

namespace ClassIsland.Views;

/// <summary>
/// HelpsWindow.xaml 的交互逻辑
/// </summary>
public partial class HelpsWindow : MyWindow
{
    public HelpsViewModel ViewModel
    {
        get;
        set;
    } = new();

    public bool IsAutoNavigating
    {
        get;
        set;
    } = false;

    public HelpsWindow()
    {
        DataContext = this;
        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        RefreshDocuments();
        UpdateNavigationState();
        ViewModel.NavigationIndex = 0;
        base.OnContentRendered(e);
    }

    private void RefreshDocuments()
    {
        ViewModel.Document = new FlowDocument();
        ViewModel.HelpDocuments.Clear();

        //ViewModel.HelpDocuments.Add("测试", "/Assets/Documents/HelloWorld.md");
        ViewModel.HelpDocuments.Add("欢迎", "/Assets/Documents/Welcome.md");
        ViewModel.HelpDocuments.Add("基本", "/Assets/Documents/Basic.md");
        ViewModel.HelpDocuments.Add("简略信息", "/Assets/Documents/MiniInfo.md");
        ViewModel.HelpDocuments.Add("提醒", "/Assets/Documents/Notifications.md");
        ViewModel.HelpDocuments.Add("档案设置", "/Assets/Documents/ProfileSettingsPage.md");
        ViewModel.HelpDocuments.Add("课表", "/Assets/Documents/ClassPlan.md");
        ViewModel.HelpDocuments.Add("时间表", "/Assets/Documents/TimeLayout.md");
        ViewModel.HelpDocuments.Add("科目", "/Assets/Documents/Subject.md");

        ViewModel.SelectedDocumentName = "欢迎";
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedDocumentName != null && !IsAutoNavigating)
        {
            CoreNavigateTo(ViewModel.SelectedDocumentName);
            while (ViewModel.NavigationHistory.Count > ViewModel.NavigationIndex + 1)
            {
                ViewModel.NavigationHistory.RemoveAt(ViewModel.NavigationIndex + 1);
            }
            ViewModel.NavigationHistory.Add(ViewModel.SelectedDocumentName);
            ViewModel.NavigationIndex++;
            UpdateNavigationState();
        }
    }

    private void CoreNavigateTo(string name)
    {
        Analytics.TrackEvent("浏览帮助文档",
        new Dictionary<string, string>
        {
            { "Name", name }
        });
        ScrollViewerDocument.ScrollToTop();
        ConvertMarkdown(ViewModel.HelpDocuments[name]);
        ViewModel.SelectedDocumentName = name;
    }

    private void ConvertMarkdown(string path)
    {
        var r = Application.GetResourceStream(new Uri(path, UriKind.RelativeOrAbsolute))?.Stream;
        if (r == null)
        {
            return;
        }
        var md = new StreamReader(r).ReadToEnd();
        var e = new Markdown()
        {
            Heading1Style = (Style)FindResource("MarkdownHeadline1Style"),
            Heading2Style = (Style)FindResource("MarkdownHeadline2Style"),
            Heading3Style = (Style)FindResource("MarkdownHeadline3Style"),
            Heading4Style = (Style)FindResource("MarkdownHeadline4Style"),
            //CodeBlockStyle = (Style)FindResource("MarkdownCodeBlockStyle"),
            //NoteStyle = (Style)FindResource("MarkdownNoteStyle"),
            ImageStyle = (Style)FindResource("MarkdownImageStyle"),
        };
        var fd = e.Transform(md);
        fd.FontFamily = (FontFamily)FindResource("HarmonyOsSans");
        fd.MaxPageWidth = 750;
        ViewModel.Document = fd;
    }

    private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshDocuments();
    }

    private void HelpsWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        ViewModel.IsOpened = false;
    }

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!e.Handled)
        {
            // ListView拦截鼠标滚轮事件
            e.Handled = true;

            // 激发一个鼠标滚轮事件，冒泡给外层ListView接收到
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((System.Windows.Controls.Control)sender).Parent as UIElement;
            if (parent != null)
            {
                parent.RaiseEvent(eventArg);
            }
        }
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        IsAutoNavigating = true;
        Debug.WriteLine($"{ViewModel.NavigationIndex} {string.Join(' ', ViewModel.NavigationHistory)}");
        if (ViewModel.NavigationIndex <= 0)
        {
            return;
        }
        CoreNavigateTo(ViewModel.NavigationHistory[ViewModel.NavigationIndex - 1]);
        ViewModel.NavigationIndex--;
        IsAutoNavigating = false;
        UpdateNavigationState();
    }

    private void ButtonForward_OnClick(object sender, RoutedEventArgs e)
    {
        IsAutoNavigating = true;
        Debug.WriteLine($"{ViewModel.NavigationIndex} {string.Join(' ', ViewModel.NavigationHistory)}");
        if (ViewModel.NavigationIndex + 1 >= ViewModel.NavigationHistory.Count)
        {
            return;
        }
        CoreNavigateTo(ViewModel.NavigationHistory[ViewModel.NavigationIndex + 1]);
        ViewModel.NavigationIndex++;
        IsAutoNavigating = false;
        UpdateNavigationState();
    }

    private void UpdateNavigationState()
    {
        ViewModel.CanBack = ViewModel.NavigationIndex > 0;
        ViewModel.CanForward = ViewModel.NavigationIndex + 1 < ViewModel.NavigationHistory.Count;
    }
}