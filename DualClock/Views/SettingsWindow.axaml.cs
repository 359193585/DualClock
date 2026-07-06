using Avalonia.Controls;
using Avalonia.Interactivity;
using DualClock.Modules;
using System;

namespace DualClock;

public partial class SettingsWindow : Window
{
    public event Action? ConfigUpdated;

    public SettingsWindow()
    {
        InitializeComponent();
        this.Icon = App.AppIcon;

        LoadCurrentSelection();
    }

    private void LoadCurrentSelection()
    {
        var config = ClockConfig.Load();
        var zones = config.TimeZoneSet.Zones;

        var zone1 = zones.Count > 0 ? zones[0] : new TimeZoneItemConfig
        {
            WinId = "Pacific Standard Time",
            IanaId = "America/Los_Angeles",
            Label = "旧金山"
        };
        var zone2 = zones.Count > 1 ? zones[1] : new TimeZoneItemConfig
        {
            WinId = "China Standard Time",
            IanaId = "Asia/Shanghai",
            Label = "北京"
        };
        SetComboValue(ComboZone1, zone1.IanaId);
        SetComboValue(ComboZone2, zone2.IanaId);

        ComboStartWindow.SelectedIndex = config.PrgSet.StartWindow; // 0或1
        CheckAutoStart.IsChecked = config.PrgSet.AutoStart;

        CheckShowSeconds.IsChecked = config.PrgSet.ShowSeconds;
    }

    private void SetComboValue(ComboBox comboBox, string ianaId)
    {
        var items = comboBox.Items;
        if (items == null) return;
        foreach (object obj in items)  
        {
            if (obj is TimeZoneItem item && item.TagValue != null && item.TagValue.Contains(ianaId))
            {
                comboBox.SelectedItem = item;
                return;
            }
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (ComboZone1.SelectedItem is TimeZoneItem item1 && ComboZone2.SelectedItem is TimeZoneItem item2)
        {
            var parts1 = item1.TagValue.Split('|');
            var parts2 = item2.TagValue.Split('|');

            var config = ClockConfig.Load();

            // 构建或更新 Zones 列表：确保至少有两个元素
            var zones = config.TimeZoneSet.Zones;
            if (zones.Count == 0)
            {
                zones.Add(new TimeZoneItemConfig());
                zones.Add(new TimeZoneItemConfig());
            }
            else if (zones.Count == 1)
            {
                zones.Add(new TimeZoneItemConfig());
            }

            // 更新前两个时区，保留列表中其他时区（如果有的话），实现“保存全部配置”。
            zones[0].WinId = parts1[0];
            zones[0].IanaId = parts1[1];
            zones[0].Label = parts1[2];

            zones[1].WinId = parts2[0];
            zones[1].IanaId = parts2[1];
            zones[1].Label = parts2[2];


            // 其他配置保存
            config.PrgSet.StartWindow = ComboStartWindow.SelectedIndex; // 0 或 1
            config.PrgSet.AutoStart = CheckAutoStart.IsChecked == true;

            config.PrgSet.ShowSeconds = CheckShowSeconds.IsChecked == true;

            config.Save();

            ConfigUpdated?.Invoke();
            Close();

            if (config.PrgSet.AutoStart)
                AutoStartManager.Enable("DualClock");
            else
                AutoStartManager.Disable("DualClock");
        }
    }
}