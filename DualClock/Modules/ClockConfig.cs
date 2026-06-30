using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DualClock.Modules
{
    public class TimeZoneItem
    {
        public string TagValue { get; set; } = string.Empty;   // "WinId|IanaId|中文名"
        public string DisplayName { get; set; } = string.Empty; // "北京 (中国)"
    }
    public class PrgConfig
    {
        public int StartWindow { get; set; } = 0;          // 启动时显示的窗口 (0=主窗口, 1=小窗)
        public bool AutoStart { get; set; } = true;        // 是否开机自启动
        
        // 小窗最后位置（屏幕坐标），null 表示未设置（首次居中）
        public int? TinyWindowPosX { get; set; }
        public int? TinyWindowPosY { get; set; }
    }
    public class TimeZoneConfig
    {
        public string TimeZone1_WinId { get; set; } = "Pacific Standard Time";
        public string TimeZone1_IanaId { get; set; } = "America/Los_Angeles";
        public string TimeZone1_Label { get; set; } = "旧金山";

        public string TimeZone2_WinId { get; set; } = "China Standard Time";
        public string TimeZone2_IanaId { get; set; } = "Asia/Shanghai";
        public string TimeZone2_Label { get; set; } = "北京";
    }
    /// <summary>
    /// 根配置类，映射 config.json 
    /// </summary>
    public class ClockConfig
    {
        private const string ConfigPath = "config.json";
        public PrgConfig PrgSet { get; set; } = new PrgConfig();
        public TimeZoneConfig TimeZoneSet { get; set; } = new TimeZoneConfig();

        public static TimeZoneItem[] AllZones { get; } = new[]
            {
        // 亚洲与中国主要城市
        new TimeZoneItem { TagValue = "China Standard Time|Asia/Shanghai|北京", DisplayName = "北京 (中国)" },
        new TimeZoneItem { TagValue = "China Standard Time|Asia/Hong_Kong|香港", DisplayName = "香港 (中国)" },
        new TimeZoneItem { TagValue = "Taipei Standard Time|Asia/Taipei|台北", DisplayName = "台北 (中国)" },
        new TimeZoneItem { TagValue = "Tokyo Standard Time|Asia/Tokyo|东京", DisplayName = "东京 (日本)" },
        new TimeZoneItem { TagValue = "Korea Standard Time|Asia/Seoul|首尔", DisplayName = "首尔 (韩国)" },
        new TimeZoneItem { TagValue = "Singapore Standard Time|Asia/Singapore|新加坡", DisplayName = "新加坡" },
        
        // 美洲主要城市
        new TimeZoneItem { TagValue = "Pacific Standard Time|America/Los_Angeles|旧金山", DisplayName = "旧金山 (美国太平洋时间)" },
        new TimeZoneItem { TagValue = "Pacific Standard Time|America/Vancouver|温哥语", DisplayName = "温哥华 (加拿大)" },
        new TimeZoneItem { TagValue = "Eastern Standard Time|America/New_York|纽约", DisplayName = "纽约 (美国东部时间)" },
        new TimeZoneItem { TagValue = "Central Standard Time|America/Chicago|芝加哥", DisplayName = "芝加哥 (美国中部时间)" },
        
        // 欧洲主要城市
        new TimeZoneItem { TagValue = "GMT Standard Time|Europe/London|伦敦", DisplayName = "伦敦 (英国/格林威治)" },
        new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Paris|巴黎", DisplayName = "巴黎 (法国/中欧时间)" },
        new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Berlin|柏林", DisplayName = "柏林 (德国)" },
        new TimeZoneItem { TagValue = "Russian Standard Time|Europe/Moscow|莫斯科", DisplayName = "莫斯科 (俄罗斯)" },
        
        // 大洋洲主要城市
        new TimeZoneItem { TagValue = "AUS Eastern Standard Time|Australia/Sydney|悉尼", DisplayName = "悉尼 (澳大利亚)" },
        new TimeZoneItem { TagValue = "New Zealand Standard Time|Pacific/Auckland|奥克兰", DisplayName = "奥克兰 (新西兰)" }
    };

        public static ClockConfig Load()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<ClockConfig>(json) ?? new ClockConfig();
                }
                catch { return new ClockConfig(); }
            }
            return new ClockConfig();
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 保留中文字符
                };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
