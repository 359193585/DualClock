using System;
using System.IO;

namespace DualClock.Modules
{
    public static class MacOSAutoStart
    {
        private static readonly string LaunchAgentsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library/LaunchAgents"
        );

        public static void Enable(string appName, string appPath)
        {
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(LaunchAgentsDir);

                // plist 文件名：com.yourcompany.yourapp.plist
                var plistFileName = $"com.yourcompany.{appName}.plist";
                var plistFilePath = Path.Combine(LaunchAgentsDir, plistFileName);

                // 1. 创建 plist 内容
                var plistContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.yourcompany.{appName}</string>
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

                // 2. 写入 plist 文件
                File.WriteAllText(plistFilePath, plistContent);

                // 3. 加载并启动这个服务 (通过 bash 命令)
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
                var plistFileName = $"com.yourcompany.{appName}.plist";
                var plistFilePath = Path.Combine(LaunchAgentsDir, plistFileName);

                if (File.Exists(plistFilePath))
                {
                    // 1. 卸载服务
                    var unloadCmd = $"-c \"launchctl unload {plistFilePath}\"";
                    System.Diagnostics.Process.Start("/bin/bash", unloadCmd);

                    // 2. 删除 plist 文件
                    File.Delete(plistFilePath);
                }
            }
            catch 
            {
                // 记录日志：取消开机启动失败
            }
        }
    }
}
