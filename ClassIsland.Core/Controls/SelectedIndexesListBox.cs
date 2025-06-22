using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
namespace ClassIsland.Core.Controls;

/// <summary>
/// 支持使用 SelectedIndexes 进行多选的 ListBox。已禁止自身滚动。
/// </summary>
public class SelectedIndexesListBox : NonScrollingListBox
{
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        SelectedIndexes ??= new();
        if (_updatingSelection) return;

        _updatingSelection = true;
        SelectedIndexes.Clear();

        foreach (var item in SelectedItems)
        {
            var index = Items.IndexOf(item);
            if (index >= 0) SelectedIndexes.Add(index);
        }
        _updatingSelection = false;

        base.OnSelectionChanged(e);
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        FilterInvalidIndexes();
        OnSelectedIndexesChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    }

    private static void OnSelectedIndexesPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        if (o is not SelectedIndexesListBox control) return;
        if (args.OldValue is ObservableCollection<int> oldIndexes)
        {
            oldIndexes.CollectionChanged -= control.OnSelectedIndexesChanged;
        }
        if (args.NewValue is ObservableCollection<int> newIndexes)
        {
            newIndexes.CollectionChanged += control.OnSelectedIndexesChanged;
            control.FilterInvalidIndexes();
            control.OnSelectedIndexesChanged(control, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        else
        {
            // 当设置为null时，清空选中项
            control.SelectedItems.Clear();
        }
    }

    private void OnSelectedIndexesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedIndexes ??= new();
        if (_updatingSelection) return;
        _updatingSelection = true;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (int index in e.NewItems)
                {
                    if (index >= 0 && index < Items.Count)
                    {
                        SelectedItems.Add(Items[index]);
                    }
                    else
                    {
                        SelectedIndexes.Remove(index);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (int index in e.OldItems)
                {
                    if (index >= 0 && index < Items.Count)
                    {
                        SelectedItems.Remove(Items[index]);
                    }
                    else
                    {
                        SelectedIndexes.Remove(index);
                    }
                }
                break;

            default:
                SelectedItems.Clear();
                var selectedIndexes = SelectedIndexes.ToList();
                foreach (var index in selectedIndexes)
                {
                    if (index >= 0 && index < Items.Count)
                    {
                        SelectedItems.Add(Items[index]);
                    }
                }
                break;
        }

        _updatingSelection = false;
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_updatingSelection) return;
        _updatingSelection = true;
        base.OnItemsChanged(e);
        _updatingSelection = false;
    }

    private void FilterInvalidIndexes()
    {
        if (SelectedIndexes == null) return;
        if (_updatingSelection) return;
        _updatingSelection = true;

        var invalidIndexes = SelectedIndexes
            .Where(i => i < 0 || i > Items.Count).ToList();

        foreach (var index in invalidIndexes)
            SelectedIndexes.Remove(index);

        _updatingSelection = false;
    }

    private bool _updatingSelection;

    static SelectedIndexesListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SelectedIndexesListBox),
            new FrameworkPropertyMetadata(typeof(ListBox)));
    }

    public static readonly DependencyProperty SelectedIndexesProperty =
        DependencyProperty.Register(nameof(SelectedIndexes),
            typeof(ObservableCollection<int>),
            typeof(SelectedIndexesListBox),
            new FrameworkPropertyMetadata(new ObservableCollection<int>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexesPropertyChanged));

    public ObservableCollection<int> SelectedIndexes
    {
        get => (ObservableCollection<int>)GetValue(SelectedIndexesProperty);
        set => SetValue(SelectedIndexesProperty, value);
    }
}