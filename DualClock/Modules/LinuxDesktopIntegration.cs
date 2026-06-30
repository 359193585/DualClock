using System;
using System.IO;

namespace DualClock.Modules;

public static class LinuxDesktopIntegration
{
    public static void RegisterDesktopEntry()
    {
        if (!OperatingSystem.IsLinux()) return;

        try
        {
            var appDir = AppContext.BaseDirectory;
            var appName = "DualClock";
            var execPath = Path.Combine(appDir, appName); // 可执行文件路径
            var iconPath = Path.Combine(appDir, "Assets", "icon.png"); // 图标路径

            // 用户级 .desktop 文件目录
            var desktopDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local/share/applications"
            );
            Directory.CreateDirectory(desktopDir);

            var desktopFilePath = Path.Combine(desktopDir, $"{appName}.desktop");
            // 如果已存在，不覆盖（避免反复写入）
            if (File.Exists(desktopFilePath)) return;

            var desktopContent = $@"[Desktop Entry]
Name=DualClock
Comment=双时钟工具
Exec={execPath}
Icon={iconPath}
Type=Application
Categories=Utility;
StartupNotify=true";

            File.WriteAllText(desktopFilePath, desktopContent);
            // 可选：赋予执行权限（通常不需要）
        }
        catch
        {
            // 忽略错误，不影响程序运行
        }
    }
}