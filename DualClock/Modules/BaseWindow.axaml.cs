// BaseWindow.axaml.cs
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DualClock.Views;

namespace DualClock
{
    public class BaseWindow : Window
    {

        public BaseWindow()
        {
            this.Icon = App.AppIcon;

            ContextMenu globalMenu = CreateContentMenu();
            this.ContextMenu = globalMenu;

            this.Loaded += (s, e) => ApplyBackgroundFallback();

            if (EnableDrag)
            {
                this.PointerPressed += OnPointerPressed;
            }

            // tooltip on mouse hover
            if (EnableTooltip)
            {
                ToolTip.SetTip(this, TooltipText);
                ToolTip.SetPlacement(this, PlacementMode.Bottom);
                ToolTip.SetShowDelay(this, 800);
            }

        }

        #region ==== 窗体的图标管理 ====
        private Image? LoadIcon(string fileName)
        {
            try
            {
                var uri = new Uri($"avares://DualClock/Assets/{fileName}");
                if (!AssetLoader.Exists(uri))
                    return null;

                using var stream = AssetLoader.Open(uri);
                return new Image
                {
                    Source = new Bitmap(stream),
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(0, 0, 4, 0)
                };
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region ==== 全局菜单事件处理 ====
        private ContextMenu CreateContentMenu()
        {
            var globalMenu = new ContextMenu();
            globalMenu.Cursor = new Cursor(StandardCursorType.Arrow);

            var itemSettings = new MenuItem
            {
                Header = "程序设置...",
                Icon = LoadIcon("settings.ico")
            };
            itemSettings.Cursor = new Cursor(StandardCursorType.Arrow);
            itemSettings.Click += OnGlobalSettingsClicked;

            var itemAbout = new MenuItem
            {
                Header = "关于",
                Icon = LoadIcon("about.ico")
            };
            itemAbout.Cursor = new Cursor(StandardCursorType.Arrow);
            itemAbout.Click += OnGlobalAboutClicked;

            var separator = new Separator();

            var itemExit = new MenuItem
            {
                Header = "退出程序",
                Icon = LoadIcon("exit.ico")
            };
            itemExit.Cursor = new Cursor(StandardCursorType.Arrow);
            itemExit.Click += OnGlobalExitClicked;

            globalMenu.Items.Add(itemSettings);
            globalMenu.Items.Add(itemAbout);
            globalMenu.Items.Add(separator);
            globalMenu.Items.Add(itemExit);
            return globalMenu;
        }
        private async void OnGlobalAboutClicked(object? sender, EventArgs e)
        {
            var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var mainWindow = lifetime?.MainWindow;
            if (mainWindow != null)
            {
                var aboutWindow = new AboutWindow();
                await aboutWindow.ShowDialog(mainWindow);
            }
        }
        private void OnGlobalSettingsClicked(object? sender, EventArgs e)
        {
            Cursor = Cursor.Default;
            var settingsWin = new SettingsWindow();
            settingsWin.ConfigUpdated += () =>
            {
                OnConfigUpdated();
            };

            settingsWin.Closed += (s, ev) =>
            {
                // 全屏光标恢复（基类通用）
                if (WindowState == WindowState.FullScreen)
                    Cursor = new Cursor(StandardCursorType.None);
            };
            settingsWin.ShowDialog(this);


        }

        private void OnGlobalExitClicked(object? sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion

        #region ==== 窗体拖拽 ====
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        }
        #endregion

        #region ==== 背景透明兼容处理 ====
        protected virtual void ApplyBackgroundFallback()
        {
            if (!ShouldApplyBackgroundFallback) return;

            if (OperatingSystem.IsMacOS() || IsUOS())
            {
                this.Background = new SolidColorBrush(Color.Parse("#333333"));
            }
        }
        protected static bool IsUOS()
        {
            try
            {
                var osReleasePath = "/etc/os-release";
                if (File.Exists(osReleasePath))
                {
                    var lines = File.ReadAllLines(osReleasePath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("ID=", StringComparison.OrdinalIgnoreCase))
                        {
                            var id = line.Substring(3).Trim('"');
                            return id == "uos" || id == "deepin";
                        }
                    }
                }
            }
            catch { }
            return false;
        }
        #endregion

        #region ==== 键盘事件处理 ====
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled) return;

            switch (e.Key)
            {
                case Key.Escape:
                    // 退出全屏 / 关闭窗口
                    if (WindowState == WindowState.FullScreen)
                    {
                        WindowState = WindowState.Normal;
                        Cursor = Cursor.Default;
                    }
                    else
                    {
                        Close();
                    }
                    e.Handled = true;
                    break;

                case Key.F:
                    OnFKeyPressed();
                    e.Handled = true;
                    break;

                case Key.T:
                    WindowManager.ToggleTinyWindow(this);
                    e.Handled = true;
                    break;

                case Key.A:
                    WindowManager.ToggleAnalogWindow(this);
                    e.Handled = true;
                    break;
            }
        }
        #endregion

        #region ==== 可被子类重写的配置 ====
        protected virtual bool EnableDrag => true;
        protected virtual bool ShouldApplyBackgroundFallback => true;
        protected virtual void OnConfigUpdated()
        {
        }
        protected virtual bool EnableTooltip => false;
        protected virtual string TooltipText => "按 F 全屏，T 置顶小窗， A 表盘，ESC 退出，右键单击显示菜单";
        protected virtual void OnFKeyPressed()
        {
        }
        #endregion

    }
}
