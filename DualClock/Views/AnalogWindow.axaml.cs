using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using DualClock.Controls;
using DualClock.Modules;

namespace DualClock
{
    public partial class AnalogWindow : BaseWindow
    {
        private DispatcherTimer _timer;
        private string _currentTime = DateTime.Now.ToString("HH:mm:ss");
        public new event PropertyChangedEventHandler? PropertyChanged;
        protected override bool EnableTooltip => true;
        protected override string TooltipText => "F 显示主窗全屏，T 小窗";
        public string CurrentTime
        {
            get => _currentTime;
            set { if (_currentTime != value) { _currentTime = value; OnPropertyChanged(); } }
        }

        public AnalogWindow()
        {
            InitializeComponent();

            LoadPosition();

            // 初始化时间和定时器
            UpdateTime();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateTime();
            _timer.Start();


            this.Loaded += (s, e) => ApplyBackgroundFallback();

            // 订阅 Deactivated 事件，处理窗口管理器可能重置状态的情况
            this.Deactivated += (s, e) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (this.Topmost)
                    {
                        this.Topmost = false;
                        this.Topmost = true;
                        this.ShowInTaskbar = false; //维持任务栏隐藏
                    }
                }, DispatcherPriority.Background);
            };

            // 窗口关闭时
            this.Closed += (s, e) =>
            {
                WindowManager.OnOtherWindowClosed();
                _timer.Stop();
            };
        }
        private PixelPoint _lastSavedPosition;

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
        // 窗口加载时，根据平台修改背景色
        protected override void ApplyBackgroundFallback()
        {
            // 如果不支持透明（macOS / UOS），则从资源获取表盘颜色，设置为窗口背景
            if (OperatingSystem.IsMacOS() || BaseWindow.IsUOS())
            {
                if (this.TryFindResource("ClockFaceColor", out object? resource) && resource is SolidColorBrush brush)
                {
                    var color = brush.Color;

                    double factor = 0.85;
                    byte r = (byte)Math.Clamp(color.R * factor, 0, 255);
                    byte g = (byte)Math.Clamp(color.G * factor, 0, 255);
                    byte b = (byte)Math.Clamp(color.B * factor, 0, 255);
                    this.Background = new SolidColorBrush(Color.FromArgb(color.A, r, g, b));
                }
                else
                {
                    this.Background = new SolidColorBrush(Color.Parse("#F0E68C"));
                    
                }
            }
            else
            {
                // 在支持透明的平台上，窗口背景透明
                this.Background = Brushes.Transparent;
            }
        }
        private void UpdateTime()
        {
            var now = DateTime.Now;
            // 更新表盘
            AnalogClockControl.Time = now;
            
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.A || e.Key == Key.Escape)
            {
                // 不响应 A Escape 键
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            // 确保每次窗口打开时都强制置顶且不在任务栏显示
            this.Topmost = true;
            this.ShowInTaskbar = false;
        }
      
        protected override void OnFKeyPressed()
        {
            WindowManager.ShowMainFullScreenAndHideCurrent(this);
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
