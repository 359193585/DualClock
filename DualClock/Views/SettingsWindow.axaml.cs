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
        LoadCurrentSelection();
    }

    private void LoadCurrentSelection()
    {
        var config = ClockConfig.Load(); 

        SetComboValue(ComboZone1, config.TimeZone1_IanaId);
        SetComboValue(ComboZone2, config.TimeZone2_IanaId);
    }

    private void SetComboValue(ComboBox comboBox, string ianaId)
    {
        foreach (TimeZoneItem item in comboBox.Items)
        {
            if (item.TagValue.Contains(ianaId))
            {
                comboBox.SelectedItem = item;
                break;
            }
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (ComboZone1.SelectedItem is TimeZoneItem item1 && ComboZone2.SelectedItem is TimeZoneItem item2)
        {
            var parts1 = item1.TagValue.Split('|');
            var parts2 = item2.TagValue.Split('|');

            // 组装新配置并利用独立类的 Save 存储
            var config = new ClockConfig
            {
                TimeZone1_WinId = parts1[0],
                TimeZone1_IanaId = parts1[1],
                TimeZone1_Label = parts1[2],
                TimeZone2_WinId = parts2[0],
                TimeZone2_IanaId = parts2[1],
                TimeZone2_Label = parts2[2]
            };
            config.Save();

            ConfigUpdated?.Invoke();
            Close();
        }
    }
}