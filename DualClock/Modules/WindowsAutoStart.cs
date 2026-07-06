using Microsoft.Win32;
using System;
using System.Runtime.Versioning;

namespace DualClock.Modules
{
    public static class WindowsAutoStart
    {
        // 注册表路径：当前用户的开机启动项
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        // 设置开机启动
        [SupportedOSPlatform("windows")]
        public static void Enable(string appName, string appPath)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                if (key != null)
                {
                    // 值数据为程序的完整路径，必要时可加启动参数
                    key.SetValue(appName, $"\"{appPath}\"");
                }
            }
            catch 
            {
                // 记录日志：设置开机启动失败
            }
        }

        // 取消开机启动
        [SupportedOSPlatform("windows")]
        public static void Disable(string appName)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                if (key?.GetValue(appName) != null)
                {
                    key.DeleteValue(appName);
                }
            }
            catch 
            {
                // 记录日志：取消开机启动失败
            }
        }
    }

}
