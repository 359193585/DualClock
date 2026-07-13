using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DualClock.Controls;

namespace DualClock
{
    public partial class AnalogWindow : BaseWindow
    {
        private DispatcherTimer _timer;

        public AnalogWindow()
        {
            InitializeComponent();

            // 初始化时间和定时器
            UpdateTime();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateTime();
            _timer.Start();

            // 窗口关闭时
            this.Closed += (s, e) =>
            {
                WindowManager.OnOtherWindowClosed();
                _timer.Stop();
            };
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            // 更新表盘
            AnalogClockControl.Time = now;
            
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnFKeyPressed()
        {
            WindowManager.ShowMainFullScreenAndHideCurrent(this);
        }
    }
}
