using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace DualClock
{
    public static class WindowManager
    {
        private static MainWindow? _mainWindow;
        private static TinyWindow? _tinyWindow;
        private static AnalogWindow? _analogWindow;

        // 获取或创建主窗口
        public static MainWindow GetOrCreateMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                // 订阅关闭事件，以便清理引用
                _mainWindow.Closed += (s, e) => _mainWindow = null;
            }
            return _mainWindow;
        }

        // 通用的切换逻辑
        private static void ToggleWindow<T>(ref T? window, Window current, Func<T> create) where T : Window
        {
            var target = window ?? create();
            if (target.IsVisible)
            {
                // 隐藏目标，显示主窗口
                target.Hide();
                var main = GetOrCreateMainWindow();
                if (!main.IsVisible) main.Show();
                main.Focus();
            }
            else
            {
                // 隐藏当前窗口（如果当前不是目标）
                if (current != target && current.IsVisible)
                    current.Hide();
                target.Show();
                target.Focus();
            }
        }

        public static void ToggleTinyWindow(Window current)
            => ToggleWindow(ref _tinyWindow, current, () => new TinyWindow());

        public static void ToggleAnalogWindow(Window current)
            => ToggleWindow(ref _analogWindow, current, () => new AnalogWindow());

        public static void ShowMainFullScreenAndHideCurrent(Window current)
        {
            if (current != null && current.IsVisible)
                current.Hide();
            var main = GetOrCreateMainWindow();
            main.Show();
            if (main.WindowState != WindowState.FullScreen)
            {
                main.WindowState = WindowState.FullScreen;
                main.Cursor = new Cursor(StandardCursorType.None);
            }
            main.Focus();
        }
      
        // 当 TinyWindow等被关闭时，尝试显示主窗口
        public static void OnOtherWindowClosed()
        {
            try
            {
                // 如果主窗口未关闭且隐藏，则显示它
                if (_mainWindow != null && !_mainWindow.IsVisible)
                {
                    _mainWindow.Show();
                    _mainWindow.Focus();
                }
            }
            catch { }
        }
        public static void CloseAllWindows()
        {
            _tinyWindow?.Close();
            _mainWindow?.Close();
            Environment.Exit(0);
        }

    }
}
