using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DualClock.Modules;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DualClock;

public partial class MainWindow : BaseWindow, INotifyPropertyChanged
{
    private string _sfCityDisplay = "SF:";
    private string _sfTimeDisplay = "Loading...";
    private string _sfDateDisplay = "Loading...";

    private string _bjCityDisplay = "BJ:";
    private string _bjTimeDisplay = "Loading...";
    private string _bjDateDisplay = "Loading...";

    private string _localTimeDisplay = "Local: 00:00:00";

    private TimeZoneInfo _timeZone1 = TimeZoneInfo.Utc;
    private TimeZoneInfo _timeZone2 = TimeZoneInfo.Utc;
    private string _label1 = "Zone 1";
    private string _label2 = "Zone 2";

    public new event PropertyChangedEventHandler? PropertyChanged;

    #region 属性公开给 XAML 绑定
    public string SfCityDisplay
    {
        get => _sfCityDisplay;
        set { if (_sfCityDisplay != value) { _sfCityDisplay = value; OnPropertyChanged(); } }
    }
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

    public string BjCityDisplay
    {
        get => _bjCityDisplay;
        set { if (_bjCityDisplay != value) { _bjCityDisplay = value; OnPropertyChanged(); } }
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
            SfCityDisplay = "美国西部湾区城市旧金山";
            SfTimeDisplay = "08:00:00";
            SfDateDisplay = "10月24日 星期五";

            BjCityDisplay = "北京";
            BjTimeDisplay = "23:00:00";
            BjDateDisplay = "10月24日 星期五";

            LocalTimeDisplay = "本地时间: 2026-06-15 15:00:00";
        }
    }
    protected override void OnConfigUpdated()
    {
        LoadConfig();
        RefreshClocks();
    }
    private void LoadConfig()
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
        _timeZone1 = GetTimeZoneById(zone1.WinId, zone1.IanaId);
        _timeZone2 = GetTimeZoneById(zone2.WinId, zone2.IanaId);
        _label1 = zone1.Label;
        _label2 = zone2.Label;
    }
    private void RefreshClocks()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localNow = DateTime.Now;

        DateTime t1 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _timeZone1);
        DateTime t2 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _timeZone2);

        SfCityDisplay = $"{_label1} ";
        SfTimeDisplay = $"{t1:HH:mm:ss}";
        BjCityDisplay = $"{_label2} ";
        BjTimeDisplay = $"{t2:HH:mm:ss}";

        var culture = new CultureInfo("zh-CN");
        SfDateDisplay = t1.ToString("MM月dd日 dddd", culture);
        BjDateDisplay = t2.ToString("MM月dd日 dddd", culture);

        LocalTimeDisplay = $"本地时间: {localNow:yyyy-MM-dd HH:mm:ss}";
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
    public void ShowAndFullScreen()
    {
        if (!this.IsVisible)
            this.Show();

        if (WindowState != WindowState.FullScreen)
        {
            WindowState = WindowState.FullScreen;
            Cursor = new Cursor(StandardCursorType.None);
        }

        this.Focus();
    }

    protected override void OnClosed(EventArgs e)
    {
        WindowManager.CloseAllWindows();

    }
    //  ESC, f event to exit full screen or close the app ,this handleed in basewindow
    // now hadle T to show tinywindow
    //protected override void OnKeyDown(KeyEventArgs e)
    //{
    //    base.OnKeyDown(e);
    //    if (e.Handled) return;
    //    if (e.Key == Key.Escape)
    //    {
    //        if (WindowState == WindowState.FullScreen)
    //        {
    //            WindowState = WindowState.Normal;
    //            Cursor = Cursor.Default;
    //        }
    //        else
    //        {
    //            WindowManager.CloseAllWindows();

    //        }
    //        e.Handled = true;
    //    }
    //    else if (e.Key == Key.F)
    //    {
    //        if (WindowState == WindowState.FullScreen)
    //        {
    //            WindowState = WindowState.Normal;
    //            Cursor = Cursor.Default;
    //        }
    //        else
    //        {
    //            WindowState = WindowState.FullScreen;
    //            Cursor = new Cursor(StandardCursorType.None);
    //        }
    //        e.Handled = true;
    //    }
    //    else if (e.Key == Key.T)
    //    {
    //        WindowManager.ToggleTinyWindow(this);
    //        e.Handled = true;
    //    }
    //}
    protected override void OnFKeyPressed()
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
