using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualClock.Modules
{
    public static class AutoStartManager
    {
        public static void Enable(string appName)
        {
            var appPath = Environment.ProcessPath!;

            if (OperatingSystem.IsWindows())
            {
                WindowsAutoStart.Enable(appName, appPath);
            }
            else if (OperatingSystem.IsMacOS())
            {
                MacOSAutoStart.Enable(appName, appPath);
            }
            else if (OperatingSystem.IsLinux())
            {
                LinuxAutoStart.Enable(appName, appPath);
            }
        }

        public static void Disable(string appName)
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsAutoStart.Disable(appName);
            }
            else if (OperatingSystem.IsMacOS())
            {
                MacOSAutoStart.Disable(appName);
            }
            else if (OperatingSystem.IsLinux())
            {
                LinuxAutoStart.Disable(appName);
            }
        }
    }
}
