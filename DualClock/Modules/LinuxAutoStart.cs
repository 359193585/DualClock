using System;
using System.IO;

namespace DualClock.Modules
{
    public static class LinuxAutoStart
    {
        private static readonly string AutoStartDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config/autostart"
        );

        public static void Enable(string appName, string appPath)
        {
            try
            {
                Directory.CreateDirectory(AutoStartDir);

                var desktopFileName = $"{appName}.desktop";
                var desktopFilePath = Path.Combine(AutoStartDir, desktopFileName);

                // 1. 创建 .desktop 文件内容
                var desktopContent = $@"[Desktop Entry]
Type=Application
Name={appName}
Exec={appPath}
Hidden=false
NoDisplay=false
X-GNOME-Autostart-enabled=true";

                // 2. 写入 .desktop 文件
                File.WriteAllText(desktopFilePath, desktopContent);

                // 3. 赋予可执行权限 (有时需要)
                System.Diagnostics.Process.Start("chmod", $"+x \"{desktopFilePath}\"");
            }
            catch 
            {
                // 记录日志：设置开机启动失败
            }
        }

        public static void Disable(string appName)
        {
            try
            {
                var desktopFileName = $"{appName}.desktop";
                var desktopFilePath = Path.Combine(AutoStartDir, desktopFileName);

                if (File.Exists(desktopFilePath))
                {
                    File.Delete(desktopFilePath);
                }
            }
            catch 
            {
                // 记录日志：取消开机启动失败
            }
        }
    }
}
