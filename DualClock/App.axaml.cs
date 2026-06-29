using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DualClock.Modules;

namespace DualClock
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
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
    }
}