using System;
using System.IO;

namespace DualClock.Modules
{
    public static class MacOSAutoStart
    {
        #region 常量和路径
        private static readonly string LaunchAgentsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library/LaunchAgents"
        );
        private const string BundleIdPrefix = "com.leison";
        private static string GetPlistFileName(string appName) => $"{BundleIdPrefix}.{appName}.plist";
        private static string GetPlistFilePath(string appName) => Path.Combine(LaunchAgentsDir, GetPlistFileName(appName));

        // 清理旧的错误文件（com.yourcompany.xxx.plist）
        private static void CleanOldPlist(string appName)
        {
            var oldFileName = $"com.yourcompany.{appName}.plist";
            var oldFilePath = Path.Combine(LaunchAgentsDir, oldFileName);
            if (File.Exists(oldFilePath))
            {
                try
                {
                    // 卸载旧服务
                    System.Diagnostics.Process.Start("/bin/bash", $"-c \"launchctl unload {oldFilePath}\"");
                    // 删除旧文件
                    File.Delete(oldFilePath);
                }
                catch { /* 忽略清理错误 */ }
            }
        }
        private static string CreatePlistContent(string appName, string appPath)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.leison.{appName}</string>
    <key>ProgramArguments</key>
    <array>
        <string>{appPath}</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <false/>
</dict>
</plist>";
        }
        #endregion

        #region 公共方法
        public static void Enable(string appName, string appPath)
        {
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(LaunchAgentsDir);

                // 清理可能存在的旧 plist（之前用 com.yourcompany 前缀的）
                CleanOldPlist(appName);

                var plistFilePath = GetPlistFilePath(appName);
                var bundleId = $"{BundleIdPrefix}.{appName}";
                string plistContent = CreatePlistContent(appName, appPath);
                File.WriteAllText(plistFilePath, plistContent);

                //  加载并启动这个服务 (通过 bash 命令)
                var loadCmd = $"-c \"launchctl load {plistFilePath}\"";
                System.Diagnostics.Process.Start("/bin/bash", loadCmd);
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
                var plistFilePath = GetPlistFilePath(appName);
                if (File.Exists(plistFilePath))
                {
                    var unloadCmd = $"-c \"launchctl unload {plistFilePath}\"";
                    System.Diagnostics.Process.Start("/bin/bash", unloadCmd);
                    File.Delete(plistFilePath);
                }
            }
            catch 
            {
                // 记录日志：取消开机启动失败
            }
        }
        #endregion


    }
}
