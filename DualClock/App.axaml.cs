using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using DualClock.Modules;
using System;

namespace DualClock
{
    public partial class App : Application
    {
        public static WindowIcon? AppIcon { get; private set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;

                // 加载图标(跨平台)
                AppIcon = GetPlatformIcon();

                // 注册 Linux 桌面图标（仅首次运行）
                LinuxDesktopIntegration.RegisterDesktopEntry();

                // 加载配置，读取启动窗口设置
                var config = ClockConfig.Load();
                var startWindow = config.PrgSet.StartWindow; // 0=主窗口, 1=小窗

                Window mainWindow;
                if (startWindow == 1)
                {
                    mainWindow = WindowManager.GetOrCreateTinyWindow();
                }
                else
                {
                    mainWindow = WindowManager.GetOrCreateMainWindow(); // 默认启动主窗口
                }

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
        private static WindowIcon GetPlatformIcon()
        {
            string iconPath = OperatingSystem.IsWindows() ? "avares://DualClock/Assets/icon.ico"
       : OperatingSystem.IsMacOS() ? "avares://DualClock/Assets/icon.png"
       : "avares://DualClock/Assets/icon.png";

            var uri = new Uri(iconPath);
            using var stream = AssetLoader.Open(uri);
            return new WindowIcon(stream);
        }

    }
}