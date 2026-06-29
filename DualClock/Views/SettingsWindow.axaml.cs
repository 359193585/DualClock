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

        SetComboValue(ComboZone1, config.TimeZoneSet.TimeZone1_IanaId);
        SetComboValue(ComboZone2, config.TimeZoneSet.TimeZone2_IanaId);

        ComboStartWindow.SelectedIndex = config.PrgSet.StartWindow; // 0或1
        CheckAutoStart.IsChecked = config.PrgSet.AutoStart;
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

            var config = ClockConfig.Load();

            // 更新时区设置（TimeZoneSet）
            config.TimeZoneSet.TimeZone1_WinId = parts1[0];
            config.TimeZoneSet.TimeZone1_IanaId = parts1[1];
            config.TimeZoneSet.TimeZone1_Label = parts1[2];

            config.TimeZoneSet.TimeZone2_WinId = parts2[0];
            config.TimeZoneSet.TimeZone2_IanaId = parts2[1];
            config.TimeZoneSet.TimeZone2_Label = parts2[2];

            config.PrgSet.StartWindow = ComboStartWindow.SelectedIndex; // 0 或 1
            config.PrgSet.AutoStart = CheckAutoStart.IsChecked == true;

            config.Save();

            ConfigUpdated?.Invoke();
            Close();
        }
    }
}