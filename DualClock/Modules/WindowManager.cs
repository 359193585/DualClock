using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace DualClock
{
    public static class WindowManager
    {
        private static MainWindow? _mainWindow;
        private static TinyWindow? _tinyWindow;

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

        // 获取或创建小窗
        public static TinyWindow GetOrCreateTinyWindow()
        {
            if (_tinyWindow == null )
            {
                _tinyWindow = new TinyWindow();
                _tinyWindow.Closed += (s, e) => _tinyWindow = null;
            }
            return _tinyWindow;
        }

        // 显示主窗口并全屏
        public static void ShowMainWindowFullScreen()
        {
            var main = GetOrCreateMainWindow();
            main.Show();
            if (main.WindowState != WindowState.FullScreen)
            {
                main.WindowState = WindowState.FullScreen;
                main.Cursor = new Cursor(StandardCursorType.None);
            }
            main.Focus();
        }

        // 切换小窗显隐（由 MainWindow 调用）
        public static void ToggleTinyWindow(MainWindow caller)
        {
            var tiny = GetOrCreateTinyWindow();

            if (tiny.IsVisible)
            {
                // 隐藏小窗，显示主窗口
                tiny.Hide();
                // 如果调用方主窗口已经隐藏，则显示它
                if (!caller.IsVisible)
                {
                    caller.Show();
                    caller.Focus();
                }
            }
            else
            {
                // 显示小窗，隐藏主窗口
                tiny.Topmost = true;
                tiny.ShowInTaskbar = false;
                tiny.Show();
                caller.Hide();
            }
        }

        // 当 TinyWindow 被关闭时，尝试显示主窗口
        public static void OnTinyWindowClosed()
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