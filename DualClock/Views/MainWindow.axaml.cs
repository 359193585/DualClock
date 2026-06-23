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

public partial class MainWindow : BaseWindow, INotifyPropertyChanged
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

    private TinyWindow? _tinyWindow;
    private bool _isMainWindowClosed = false; // 标记主窗口是否已关闭
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
    protected override void OnConfigUpdated()
    {
        LoadConfig();
        RefreshClocks();
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
   

    //  ESC, f event to exit full screen or close the app ,this handleed in basewindow
    // now hadle T to show tinywindow
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;
        if (e.Key == Key.Escape)
        {
            if (WindowState == WindowState.FullScreen)
            {
                WindowState = WindowState.Normal;
                Cursor = Cursor.Default;
            }
            else
            {
                Close(); // 退出程序
            }
            e.Handled = true;
        }
        else if (e.Key == Key.F)
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
            e.Handled = true;
        }
        else if (e.Key == Key.T)
        {
            if (_tinyWindow == null)
            {
                _tinyWindow = new TinyWindow();
                _tinyWindow.SetMainWindowCallbacks(showMainFullScreen: ShowAndFullScreen);
                _tinyWindow.Closed += (s, args) =>
                {
                    _tinyWindow = null;      // 清空引用，允许下次按 T 重建

                    if (!_isMainWindowClosed && !this.IsVisible)
                    {
                        this.Show();
                        this.Focus();
                    }
                };
            }
            if (_tinyWindow.IsVisible)
            {
                _tinyWindow.Hide();
                if (!this.IsVisible)
                    this.Show();
            }
            else
            {
                _tinyWindow.Show();
                this.Hide();
            }

            e.Handled = true;
        }
       
    }

    protected override void OnClosed(EventArgs e)
    {
        _isMainWindowClosed = true;
        if (_tinyWindow != null)
        {
            _tinyWindow.Closed -= TinyWindow_Closed;
            _tinyWindow.Close();
            _tinyWindow = null;
        }
        base.OnClosed(e);

    }
    private void TinyWindow_Closed(object? sender, EventArgs e)
    {
        _tinyWindow = null; // 清理引用

        // 如果主窗口没有被关闭且当前不可见，则显示它
        if (!_isMainWindowClosed && !this.IsVisible)
        {
            this.Show();
            this.Focus();
        }
    }
}