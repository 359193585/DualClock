// BaseWindow.axaml.cs
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Input;
using DualClock.Views;
using System;

namespace DualClock
{
    public class BaseWindow : Window
    {
        public BaseWindow()
        {
            ContextMenu globalMenu = CreateContentMenu();

            this.ContextMenu = globalMenu;
            this.Icon = App.AppIcon;
        }

        private ContextMenu CreateContentMenu()
        {
            var globalMenu = new ContextMenu();
            globalMenu.Cursor = new Cursor(StandardCursorType.Arrow);

            var itemSettings = new MenuItem
            {
                Header = "程序设置..."
            };
            itemSettings.Cursor = new Cursor(StandardCursorType.Arrow);
            itemSettings.Click += OnGlobalSettingsClicked;

            var itemAbout = new MenuItem
            {
                Header = "关于"
            };
            itemAbout.Cursor = new Cursor(StandardCursorType.Arrow);
            itemAbout.Click += OnGlobalAboutClicked;

            var separator = new Separator();

            var itemExit = new MenuItem
            {
                Header = "退出程序"
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

            settingsWin.ShowDialog(this);

            settingsWin.Closed += (s, ev) =>
            {
                // 全屏光标恢复（基类通用）
                if (WindowState == WindowState.FullScreen)
                    Cursor = new Cursor(StandardCursorType.None);
            };

        }

        private void OnGlobalExitClicked(object? sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        protected virtual void OnConfigUpdated()
        {
        }

        
    }
}