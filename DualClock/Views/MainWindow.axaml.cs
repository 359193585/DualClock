using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DualClock.Modules;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DualClock;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string _sfTimeDisplay = "Loading...";
    private string _sfDateDisplay = "Loading...";
    private string _bjTimeDisplay = "Loading...";
    private string _bjDateDisplay = "Loading...";
    private string _localTimeDisplay = "Local: 00:00:00";

    private TimeZoneInfo _timeZone1 = TimeZoneInfo.Utc;
    private TimeZoneInfo _timeZone2 = TimeZoneInfo.Utc;
    private string _label1 = "Zone 1";
    private string _label2 = "Zone 2";


    public new event PropertyChangedEventHandler? PropertyChanged;

    #region 属性公开给 XAML 绑定
    public string SfTimeDisplay
    {
        get => _sfTimeDisplay;
        set { if (_sfTimeDisplay != value) { _sfTimeDisplay = value; OnPropertyChanged(); } }
    }

    public string SfDateDisplay
    {
        get => _sfDateDisplay;
        set { if (_sfDateDisplay != value) { _sfDateDisplay = value; OnPropertyChanged(); } }
    }

    public string BjTimeDisplay
    {
        get => _bjTimeDisplay;
        set { if (_bjTimeDisplay != value) { _bjTimeDisplay = value; OnPropertyChanged(); } }
    }

    public string BjDateDisplay
    {
        get => _bjDateDisplay;
        set { if (_bjDateDisplay != value) { _bjDateDisplay = value; OnPropertyChanged(); } }
    }

    public string LocalTimeDisplay
    {
        get => _localTimeDisplay;
        set { if (_localTimeDisplay != value) { _localTimeDisplay = value; OnPropertyChanged(); } }
    }
    #endregion

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        LoadConfig();

        if (!Design.IsDesignMode)
        {
            Cursor = new Cursor(StandardCursorType.None);
            
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => RefreshClocks();
            timer.Start();

            RefreshClocks();
        }
        else
        {
            // 设计器预览
            SfTimeDisplay = "SF: 08:00:00";
            SfDateDisplay = "10月24日 星期五";
            BjTimeDisplay = "BJ: 23:00:00";
            BjDateDisplay = "10月24日 星期五";
            LocalTimeDisplay = "本地时间: 2026-06-15 15:00:00";
        }
    }
    private void OnContextMenuOpened(object? sender, RoutedEventArgs e)
    {
        // 菜单弹出时，立刻恢复默认鼠标指针，让用户在全屏下也能看清并操作菜单
        Cursor = Cursor.Default;
    }
    private void LoadConfig()
    {
        var config = ClockConfig.Load();

        _timeZone1 = GetTimeZoneById(config.TimeZone1_WinId, config.TimeZone1_IanaId);
        _timeZone2 = GetTimeZoneById(config.TimeZone2_WinId, config.TimeZone2_IanaId);
        _label1 = config.TimeZone1_Label;
        _label2 = config.TimeZone2_Label;
    }
    private void RefreshClocks()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localNow = DateTime.Now;

        DateTime t1 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _timeZone1);
        DateTime t2 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _timeZone2);

        SfTimeDisplay = $"{_label1}: {t1:HH:mm:ss}";
        BjTimeDisplay = $"{_label2}: {t2:HH:mm:ss}";

        var culture = new CultureInfo("zh-CN");
        SfDateDisplay = t1.ToString("MM月dd日 dddd", culture);
        BjDateDisplay = t2.ToString("MM月dd日 dddd", culture);

        LocalTimeDisplay = $"本地时间: {localNow:yyyy-MM-dd HH:mm:ss}";
    }
    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        Cursor = Cursor.Default;

        var settingsWin = new SettingsWindow();
        settingsWin.ConfigUpdated += () => {
            LoadConfig();      
            RefreshClocks();   
        };
        settingsWin.ShowDialog(this);
        settingsWin.Closed += (s, ev) => {
            if (WindowState == WindowState.FullScreen) Cursor = new Cursor(StandardCursorType.None);
        };
    }
    private void MenuClose(object sender, RoutedEventArgs e) => Close();
    private static TimeZoneInfo GetTimeZoneById(string windowsId, string ianaId)
    {
        string id = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? windowsId : ianaId;
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch { return TimeZoneInfo.Utc; }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // handle ESC, f event to exit full screen or close the app
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            if (WindowState == WindowState.FullScreen)
            {
                WindowState = WindowState.Normal; 
                Cursor = Cursor.Default;            
            }
            else
            {
                Close(); 
            }
        }
        if (e.Key == Key.F)
        {
            if (WindowState == WindowState.FullScreen)
            {
                WindowState = WindowState.Normal;
                Cursor = Cursor.Default;
            }
            else
            {
                WindowState = WindowState.FullScreen;
                Cursor = new Cursor(StandardCursorType.None); 
            }
        }
    }
}