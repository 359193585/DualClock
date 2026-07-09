using Avalonia.Controls;
using Avalonia.Interactivity;
using DualClock.Modules;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DualClock;

/// <summary>
/// 用于动态时区列表的条目包装类
/// </summary>
public class ZoneEntry : INotifyPropertyChanged
{
    private TimeZoneItem? _selectedZone;
    public TimeZoneItem? SelectedZone
    {
        get => _selectedZone;
        set
        {
            if (_selectedZone != value)
            {
                _selectedZone = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
public partial class SettingsWindow : Window
{
    public event Action? ConfigUpdated;

    // 绑定到 ItemsControl 的集合
    public ObservableCollection<ZoneEntry> ZonesCollection { get; } = new();

    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = this;
        LoadCurrentSelection();
    }

    private void LoadCurrentSelection()
    {
        var config = ClockConfig.Load();

        // 加载时区列表
        ZonesCollection.Clear();
        foreach (var zone in config.TimeZoneSet.Zones)
        {
            var entry = new ZoneEntry();
            var matched = ClockConfig.AllZones.FirstOrDefault(
                z => z.TagValue.Contains(zone.IanaId) || z.TagValue.Contains(zone.WinId));
            entry.SelectedZone = matched;
            ZonesCollection.Add(entry);
        }

        if (ZonesCollection.Count == 0)
        {
            ZonesCollection.Add(new ZoneEntry());
        }

        ComboStartWindow.SelectedIndex = config.PrgSet.StartWindow;
        CheckAutoStart.IsChecked = config.PrgSet.AutoStart;
        CheckShowSeconds.IsChecked = config.PrgSet.ShowSeconds;
    }

    private void OnAddZoneClick(object sender, RoutedEventArgs e)
    {
        ZonesCollection.Add(new ZoneEntry());
    }

    private void RemoveZone(ZoneEntry entry)
    {
        if (ZonesCollection.Count <= 1)
        {
            // 至少保留一个时区项
            return;
        }
        ZonesCollection.Remove(entry);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var config = ClockConfig.Load();

        // 更新时区列表
        config.TimeZoneSet.Zones.Clear();
        foreach (var entry in ZonesCollection)
        {
            if (entry.SelectedZone == null) continue;
            var parts = entry.SelectedZone.TagValue.Split('|');
            if (parts.Length == 3)
            {
                config.TimeZoneSet.Zones.Add(new TimeZoneItemConfig
                {
                    WinId = parts[0],
                    IanaId = parts[1],
                    Label = parts[2]
                });
            }
        }

        // 如果列表为空，添加默认时区
        if (config.TimeZoneSet.Zones.Count == 0)
        {
            config.TimeZoneSet.Zones.Add(new TimeZoneItemConfig
            {
                WinId = "China Standard Time",
                IanaId = "Asia/Shanghai",
                Label = "北京"
            });
        }

        // 更新程序设置
        config.PrgSet.StartWindow = ComboStartWindow.SelectedIndex;
        config.PrgSet.AutoStart = CheckAutoStart.IsChecked == true;
        config.PrgSet.ShowSeconds = CheckShowSeconds.IsChecked == true;

        config.Save();

        ConfigUpdated?.Invoke();
        Close();
    }

    // 删除按钮的命令处理（通过 XAML 的 Command 绑定）
    private void OnRemoveZoneClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ZoneEntry entry)
        {
            RemoveZone(entry);
        }
    }
}