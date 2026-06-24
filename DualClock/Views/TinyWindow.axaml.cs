using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DualClock.Modules;
using System;
using System.Globalization;

namespace DualClock;

public partial class TinyWindow : BaseWindow
{
    private TimeZoneInfo _timeZone1 = TimeZoneInfo.Utc;
    private TimeZoneInfo _timeZone2 = TimeZoneInfo.Utc;
    private string _label1 = "Zone1";
    private string _label2 = "Zone2";

    private DispatcherTimer _timer;

    private Action? _showMainFullScreen; // 回调：显示主窗并全屏
    public TinyWindow()
    {
        InitializeComponent();
        this.Background = Brushes.Transparent;
        this.PointerPressed += OnPointerPressed;

        LoadConfigAndRefresh();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => RefreshClocks();
        _timer.Start();

    }
    public void SetMainWindowCallbacks(Action showMainFullScreen)
    {
        _showMainFullScreen = showMainFullScreen;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }
    private void LoadConfigAndRefresh()
    {
        var config = ClockConfig.Load();

        _timeZone1 = GetTimeZoneById(config.TimeZone1_WinId, config.TimeZone1_IanaId);
        _timeZone2 = GetTimeZoneById(config.TimeZone2_WinId, config.TimeZone2_IanaId);
        _label1 = config.TimeZone1_Label;
        _label2 = config.TimeZone2_Label;

        RefreshClocks();
    }

    private void RefreshClocks()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localNow = DateTime.Now;

        DateTime t1 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _timeZone1);
        DateTime t2 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _timeZone2);

        var culture = new CultureInfo("zh-CN");

        // 设置时钟1（配置时区1）
        Clock1.SetDate($"{_label1} {t1.ToString("MM/dd", culture)}");
        Clock1.SetTime(t1.ToString("HH:mm"));  

        // 设置时钟2（配置时区2）
        Clock2.SetDate($"{_label2} {t2.ToString("MM/dd", culture)}");
        Clock2.SetTime(t2.ToString("HH:mm"));

        // 设置时钟3（本地时间）
        Clock3.SetDate($"本地 {localNow.ToString("MM/dd", culture)}");
        Clock3.SetTime(localNow.ToString("HH:mm"));
    }

    protected override void OnConfigUpdated()
    {
        LoadConfigAndRefresh();
    }

    protected override void OnClosed(EventArgs e)
    {
        _timer?.Stop();
        _showMainFullScreen = null;
        base.OnClosed(e);
    }

    private TimeZoneInfo GetTimeZoneById(string winId, string ianaId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(winId);
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F)
        {
            this.Hide();  // 隐藏自身

            // 调用回调，让 MainWindow 显示并全屏
            _showMainFullScreen?.Invoke();

            e.Handled = true;
        }
    }

}