using Avalonia.Controls;
using Avalonia.Threading;
using DualClock.Controls;
using System;

namespace DualClock.Views
{
    public partial class AnalogWindow : Window
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

            // 窗口关闭时停止定时器
            this.Closed += (s, e) => _timer.Stop();
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            // 更新表盘
            AnalogClockControl.Time = now;
            
        }
    }
}