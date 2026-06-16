using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace DualClock;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    // 5 个文本对应的后台字段
    private string _sfTimeDisplay = "旧金山: 00:00:00";
    private string _sfDateDisplay = "Loading...";
    private string _bjTimeDisplay = "北京: 00:00:00";
    private string _bjDateDisplay = "Loading...";
    private string _localTimeDisplay = "Local: 00:00:00";

    private readonly TimeZoneInfo _sfTimeZone;
    private readonly TimeZoneInfo _beijingTimeZone;

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

        _sfTimeZone = GetTimeZoneById("Pacific Standard Time", "America/Los_Angeles");
        _beijingTimeZone = GetTimeZoneById("China Standard Time", "Asia/Shanghai");

        DataContext = this;

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

    private void RefreshClocks()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localNow = DateTime.Now; 

        DateTime sfLocal = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _sfTimeZone);
        DateTime bjLocal = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _beijingTimeZone);

        SfTimeDisplay = $"SF: {sfLocal:HH:mm:ss}";
        BjTimeDisplay = $"BJ: {bjLocal:HH:mm:ss}";

        // 指定 CultureInfo("zh-CN") 确保在任何系统语言下都强制输出中文的“星期几”
        var culture = new CultureInfo("zh-CN");
        SfDateDisplay = sfLocal.ToString("MM月dd日 dddd", culture);
        BjDateDisplay = bjLocal.ToString("MM月dd日 dddd", culture);

        LocalTimeDisplay = $"本地时间: {localNow:yyyy-MM-dd HH:mm:ss}";
    }

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

    // handle ESC event to exit full screen or close the app
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
    }
}