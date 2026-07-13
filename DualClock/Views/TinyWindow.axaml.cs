using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using DualClock.Controls;
using DualClock.Modules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace DualClock;

public partial class TinyWindow : BaseWindow
{
    private DispatcherTimer _timer;
    private PixelPoint _lastSavedPosition;

    private List<ClockItem> _clockItems = new List<ClockItem>();
    private List<(TimeZoneInfo TimeZone, string Label)> _zones = new List<(TimeZoneInfo, string)>();
    private ClockItem _localClock = null!;
    private SecondItem _secondItem = null!;

    protected override bool EnableTooltip => true;
    protected override string TooltipText => "F 显示主窗全屏，A 表盘";

    public TinyWindow()
    {
        InitializeComponent();

        // 设置窗口属性
        LoadPosition();

        // 动态构建 UI
        BuildClockItems();
        
        // 加载时区配置并初始化时钟
        LoadConfigAndRefresh();

        // 设置定时器，每 200 毫秒刷新一次时钟和位置
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _timer.Tick += (s, e) =>
        {
            RefreshClocks();
            CheckAndSavePosition();
        };
        _timer.Start();

        this.Closed += (s, e) =>
        {
            WindowManager.OnOtherWindowClosed();
            _timer.Stop();
        };
        // 订阅 Deactivated 事件  
        this.Deactivated += (s, e) =>
        {
            // 在失去焦点时，强制刷新窗口状态
            // 注意：此处不能直接 Hide/Show，会引发循环，需要用 Dispatcher 延迟执行
            // 专门针对“切换到其他窗口后失效”的场景设计，社区验证有效。
            Dispatcher.UIThread.Post(() =>
            {
                if (this.Topmost)
                {
                    // 暂时取消置顶再恢复，以触发窗口管理器刷新
                    this.Topmost = false;
                    this.Topmost = true;
                    this.ShowInTaskbar = false;
                }
            }, DispatcherPriority.Background);
        };
    }

    private void LoadPosition()
    {
        this.Background = Brushes.Transparent;
        this.Topmost = true;
        this.ShowInTaskbar = false;

        // 加载保存的窗体位置
        var config = ClockConfig.Load();
        if (config.PrgSet.TinyWindowPosX.HasValue && config.PrgSet.TinyWindowPosY.HasValue)
        {
            Position = new PixelPoint(config.PrgSet.TinyWindowPosX.Value, config.PrgSet.TinyWindowPosY.Value);
        }
        else
        {
            var screen = Screens.Primary;
            if (screen != null)
            {
                var screenBounds = screen.Bounds;
                var windowWidth = this.Width;
                var windowHeight = this.Height;
                // 居中显示
                var centerX = (screenBounds.Width - windowWidth) / 2;
                var centerY = (screenBounds.Height - windowHeight) / 2;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Position = new PixelPoint((int)centerX, (int)centerY);
            }
        }
        _lastSavedPosition = Position;
        WindowStartupLocation = WindowStartupLocation.Manual;
    }

    private void BuildClockItems()
    {
        // 清空原有的子控件（除了可能已经存在的）
        TinyWrapPanel.Children.Clear();
        _clockItems.Clear();

        var config = ClockConfig.Load();
        int index = 0;
        // 为每个时区创建一个 ClockItem
        foreach (var zone in config.TimeZoneSet.Zones)
        {
            var clock = new ClockItem
            {
                Margin = new Thickness(0, 0,0, 0),
                TimeBackground = DefaultGradients[index % DefaultGradients.Length]
            };
            // 可以设置背景渐变
            // clock.TimeBackground = ...;
            TinyWrapPanel.Children.Add(clock);
            _clockItems.Add(clock);
            index++;
        }

        // 固定本地时钟（始终在最右侧）
        _localClock = new ClockItem
        {
            Margin = new Thickness(0, 0, 0, 0),
            TimeBackground = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
            {
                new GradientStop(Colors.Gold, 0),
                new GradientStop(Colors.Orange, 1)
            }
            }
        };
        TinyWrapPanel.Children.Add(_localClock);

        // 秒控件（可选）
        _secondItem = new SecondItem();
        TinyWrapPanel.Children.Add(_secondItem);

        UpdateWindowWidth();
    }
    private void CheckAndSavePosition()
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
   
    private void LoadConfigAndRefresh()
    {
        var config = ClockConfig.Load();
        _zones.Clear();
        foreach (var zone in config.TimeZoneSet.Zones)
        {
            var tz = GetTimeZoneById(zone.WinId, zone.IanaId);
            _zones.Add((tz, zone.Label));
        }

        //根据配置控制秒是否显示
        _secondItem.IsVisible = config.PrgSet.ShowSeconds;

        RefreshClocks();
        UpdateWindowWidth();
    }

    private void RefreshClocks()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localNow = DateTime.Now;
        var culture = new CultureInfo("zh-CN");

        for (int i = 0; i < _zones.Count && i < _clockItems.Count; i++)
        {
            var (tz, label) = _zones[i];
            var dt = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
            string dateStr = dt.ToString("MM-dd", culture);
            string weekStr = dt.ToString("ddd", culture).Replace("周", "");
            _clockItems[i].SetDate($"{label} {dateStr} {weekStr}");
            _clockItems[i].SetTime(dt.ToString("HH:mm"));
        }

        // 设置固定显示的本地时间
        string localDateStr = localNow.ToString("MM-dd", culture);
        string localWeekStr = localNow.ToString("ddd", culture).Replace("周", "");
        _localClock.SetDate($"本地 {localDateStr} {localWeekStr}");
        _localClock.SetTime(localNow.ToString("HH:mm"));

        _secondItem.SetSecond(localNow.ToString("ss"));
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
    private void UpdateWindowWidth()
    {
        double totalWidth = 0;
        foreach (Control child in TinyWrapPanel.Children)
        {
            double childWidth = child.Width;
            var margin = child.Margin;
            totalWidth += childWidth + margin.Left + margin.Right;
        }
        this.Width = totalWidth;
    }
    private static readonly IBrush[] DefaultGradients = new IBrush[]
{
    CreateGradientBrush("#FF416C", "#FF4B2B"),   // 红橙
    CreateGradientBrush("#00B4DB", "#0083B0"),   // 蓝紫
    CreateGradientBrush("#11998E", "#38EF7D")    // 翠绿
};

    private static IBrush CreateGradientBrush(string color1, string color2)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops
        {
            new GradientStop(Color.Parse(color1), 0),
            new GradientStop(Color.Parse(color2), 1)
        }
        };
    }
  
    //protected override void OnKeyDown(KeyEventArgs e)
    //{
    //    if (e.Key == Key.F)
    //    {
    //        this.Hide();
    //        WindowManager.ShowMainWindowFullScreen();
    //        e.Handled = true;
    //    }
    //    else if (e.Key == Key.A)
    //    {
    //        this.Hide();
    //        WindowManager.ShowMainWindowAnalogClock();
    //        e.Handled = true;
    //    }
    //}
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        // 确保每次窗口打开时都强制置顶且不在任务栏显示
        this.Topmost = true;
        this.ShowInTaskbar = false;
    }
    protected override void OnConfigUpdated()
    {
        BuildClockItems();
        LoadConfigAndRefresh();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.T)
        {
            // 不响应 T 键
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnFKeyPressed()
    {
        // 隐藏自己，显示主窗口并全屏
        WindowManager.ShowMainFullScreenAndHideCurrent(this);
    }
}
