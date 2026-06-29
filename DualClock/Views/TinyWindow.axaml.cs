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
    private PixelPoint _lastSavedPosition; // 记录上次保存的位置，避免重复写入

    public TinyWindow()
    {
        InitializeComponent();
        this.Background = Brushes.Transparent;
        this.PointerPressed += OnPointerPressed;

        LoadConfigAndRefresh();

        // 加载保存的位置
        var config = ClockConfig.Load();
        if (config.PrgSet.TinyWindowPosX.HasValue && config.PrgSet.TinyWindowPosY.HasValue)
        {
            Position = new PixelPoint(config.PrgSet.TinyWindowPosX.Value, config.PrgSet.TinyWindowPosY.Value);
            _lastSavedPosition = Position; // 初始化
            WindowStartupLocation = WindowStartupLocation.Manual;
        }

        // 监听位置变化，自动保存
        this.LayoutUpdated += OnLayoutUpdated;
        this.Closed += (s, e) => this.LayoutUpdated -= OnLayoutUpdated;

        this.Closed += (s, e) => WindowManager.OnTinyWindowClosed();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => RefreshClocks();
        _timer.Start();

    }
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        var currentPos = Position;
        if (currentPos != _lastSavedPosition)
        {
            _lastSavedPosition = currentPos;
            var config = ClockConfig.Load();
            config.PrgSet.TinyWindowPosX = currentPos.X;
            config.PrgSet.TinyWindowPosY = currentPos.Y;
            config.Save();
        }
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

        _timeZone1 = GetTimeZoneById(config.TimeZoneSet.TimeZone1_WinId, config.TimeZoneSet.TimeZone1_IanaId);
        _timeZone2 = GetTimeZoneById(config.TimeZoneSet.TimeZone2_WinId, config.TimeZoneSet.TimeZone2_IanaId);
        _label1 = config.TimeZoneSet.TimeZone1_Label;
        _label2 = config.TimeZoneSet.TimeZone2_Label;

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
            this.Hide();
            WindowManager.ShowMainWindowFullScreen();
            e.Handled = true;
        }
    }

}