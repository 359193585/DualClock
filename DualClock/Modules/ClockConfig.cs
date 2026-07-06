using System;
using System.Collections.Generic;
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
        public bool ShowSeconds { get; set; } = true; // 默认显示
    }
    public class TimeZoneConfig
    {
        public List<TimeZoneItemConfig> Zones { get; set; } = new List<TimeZoneItemConfig>();
    }

    public class TimeZoneItemConfig
    {
        public string WinId { get; set; } = "Pacific Standard Time";
        public string IanaId { get; set; } = "America/Los_Angeles";
        public string Label { get; set; } = "旧金山";
    }
    /// <summary>
    /// 根配置类，映射 config.json 
    /// </summary>
    public class ClockConfig
    {
        // 配置文件路径：始终与可执行文件在同一目录， 便于跨平台部署和更新
        private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "config.json");
        public PrgConfig PrgSet { get; set; } = new PrgConfig();
        public TimeZoneConfig TimeZoneSet { get; set; } = new TimeZoneConfig();

        public static TimeZoneItem[] AllZones { get; } = new[]
 {
    // ==================== 亚洲与中东 ====================
    new TimeZoneItem { TagValue = "China Standard Time|Asia/Shanghai|北京", DisplayName = "北京 (中国)" },
    new TimeZoneItem { TagValue = "China Standard Time|Asia/Hong_Kong|香港", DisplayName = "香港 (中国)" },
    new TimeZoneItem { TagValue = "Taipei Standard Time|Asia/Taipei|台北", DisplayName = "台北 (中国)" },
    new TimeZoneItem { TagValue = "Tokyo Standard Time|Asia/Tokyo|东京", DisplayName = "东京 (日本)" },
    new TimeZoneItem { TagValue = "Korea Standard Time|Asia/Seoul|首尔", DisplayName = "首尔 (韩国)" },
    new TimeZoneItem { TagValue = "Singapore Standard Time|Asia/Singapore|新加坡", DisplayName = "新加坡" },
    new TimeZoneItem { TagValue = "India Standard Time|Asia/Kolkata|孟买", DisplayName = "孟买 (印度)" },
    new TimeZoneItem { TagValue = "India Standard Time|Asia/Colombo|科伦坡", DisplayName = "科伦坡 (斯里兰卡)" },
    new TimeZoneItem { TagValue = "SE Asia Standard Time|Asia/Bangkok|曼谷", DisplayName = "曼谷 (泰国)" },
    new TimeZoneItem { TagValue = "SE Asia Standard Time|Asia/Jakarta|雅加达", DisplayName = "雅加达 (印尼)" },
    new TimeZoneItem { TagValue = "Arabian Standard Time|Asia/Dubai|迪拜", DisplayName = "迪拜 (阿联酋)" },
    new TimeZoneItem { TagValue = "Arab Standard Time|Asia/Riyadh|利雅得", DisplayName = "利雅得 (沙特)" },
    new TimeZoneItem { TagValue = "Iran Standard Time|Asia/Tehran|德黑兰", DisplayName = "德黑兰 (伊朗)" },
    new TimeZoneItem { TagValue = "Turkey Standard Time|Europe/Istanbul|伊斯坦布尔", DisplayName = "伊斯坦布尔 (土耳其)" },
    new TimeZoneItem { TagValue = "Israel Standard Time|Asia/Jerusalem|耶路撒冷", DisplayName = "耶路撒冷 (以色列)" },

    // ==================== 美洲 ====================
    // --- 美国 & 加拿大 ---
    new TimeZoneItem { TagValue = "Pacific Standard Time|America/San Francisco|旧金山", DisplayName = "旧金山 (美国太平洋时间)" },
    new TimeZoneItem { TagValue = "Pacific Standard Time|America/Los_Angeles|洛杉矶", DisplayName = "洛杉矶 (美国太平洋时间)" },
    new TimeZoneItem { TagValue = "Pacific Standard Time|America/Vancouver|温哥华", DisplayName = "温哥华 (加拿大)" },
    new TimeZoneItem { TagValue = "Pacific Standard Time|America/Seattle|西雅图", DisplayName = "西雅图 (美国太平洋时间)" },
    new TimeZoneItem { TagValue = "Mountain Standard Time|America/Denver|丹佛", DisplayName = "丹佛 (美国山地时间)" },
    new TimeZoneItem { TagValue = "Mountain Standard Time|America/Phoenix|凤凰城", DisplayName = "凤凰城 (美国山地时间, 无夏令时)" },
    new TimeZoneItem { TagValue = "Eastern Standard Time|America/Pittsburgh|匹兹堡", DisplayName = "匹兹堡 (美国东部时间)" },
    new TimeZoneItem { TagValue = "Central Standard Time|America/Chicago|芝加哥", DisplayName = "芝加哥 (美国中部时间)" },
    new TimeZoneItem { TagValue = "Central Standard Time|America/Mexico_City|墨西哥城", DisplayName = "墨西哥城 (墨西哥)" },
    new TimeZoneItem { TagValue = "Eastern Standard Time|America/New_York|纽约", DisplayName = "纽约 (美国东部时间)" },
    new TimeZoneItem { TagValue = "Eastern Standard Time|America/Toronto|多伦多", DisplayName = "多伦多 (加拿大)" },
    new TimeZoneItem { TagValue = "Eastern Standard Time|America/Washington|华盛顿", DisplayName = "华盛顿 (美国东部时间)" },
    new TimeZoneItem { TagValue = "Eastern Standard Time|America/Miami|迈阿密", DisplayName = "迈阿密 (美国东部时间)" },
    // --- 阿拉斯加 & 夏威夷 ---
    new TimeZoneItem { TagValue = "Alaskan Standard Time|America/Anchorage|安克雷奇", DisplayName = "安克雷奇 (美国阿拉斯加时间)" },
    new TimeZoneItem { TagValue = "Hawaiian Standard Time|Pacific/Honolulu|火奴鲁鲁", DisplayName = "火奴鲁鲁 (美国夏威夷-阿留申时间)" },
    // --- 其他美洲 ---
    new TimeZoneItem { TagValue = "Atlantic Standard Time|America/Halifax|哈利法克斯", DisplayName = "哈利法克斯 (加拿大大西洋时间)" },
    new TimeZoneItem { TagValue = "Newfoundland Standard Time|America/St_Johns|圣约翰斯", DisplayName = "圣约翰斯 (加拿大纽芬兰)" },
    new TimeZoneItem { TagValue = "SA Eastern Standard Time|America/Argentina/Buenos_Aires|布宜诺斯艾利斯", DisplayName = "布宜诺斯艾利斯 (阿根廷)" },
    new TimeZoneItem { TagValue = "SA Pacific Standard Time|America/Bogota|波哥大", DisplayName = "波哥大 (哥伦比亚)" },
    new TimeZoneItem { TagValue = "SA Western Standard Time|America/La_Paz|拉巴斯", DisplayName = "拉巴斯 (玻利维亚)" },
    new TimeZoneItem { TagValue = "E. South America Standard Time|America/Sao_Paulo|圣保罗", DisplayName = "圣保罗 (巴西)" },

    // ==================== 欧洲 ====================
    new TimeZoneItem { TagValue = "GMT Standard Time|Europe/London|伦敦", DisplayName = "伦敦 (英国/格林威治)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Lisbon|里斯本", DisplayName = "里斯本 (葡萄牙/西欧时间)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Dublin|都柏林", DisplayName = "都柏林 (爱尔兰)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Paris|巴黎", DisplayName = "巴黎 (法国/中欧时间)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Berlin|柏林", DisplayName = "柏林 (德国)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Rome|罗马", DisplayName = "罗马 (意大利)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Madrid|马德里", DisplayName = "马德里 (西班牙)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Amsterdam|阿姆斯特丹", DisplayName = "阿姆斯特丹 (荷兰)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Brussels|布鲁塞尔", DisplayName = "布鲁塞尔 (比利时)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Vienna|维也纳", DisplayName = "维也纳 (奥地利)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Prague|布拉格", DisplayName = "布拉格 (捷克)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Warsaw|华沙", DisplayName = "华沙 (波兰)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Copenhagen|哥本哈根", DisplayName = "哥本哈根 (丹麦)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Stockholm|斯德哥尔摩", DisplayName = "斯德哥尔摩 (瑞典)" },
    new TimeZoneItem { TagValue = "W. Europe Standard Time|Europe/Oslo|奥斯陆", DisplayName = "奥斯陆 (挪威)" },
    new TimeZoneItem { TagValue = "E. Europe Standard Time|Europe/Helsinki|赫尔辛基", DisplayName = "赫尔辛基 (芬兰/东欧时间)" },
    new TimeZoneItem { TagValue = "E. Europe Standard Time|Europe/Riga|里加", DisplayName = "里加 (拉脱维亚)" },
    new TimeZoneItem { TagValue = "E. Europe Standard Time|Europe/Tallinn|塔林", DisplayName = "塔林 (爱沙尼亚)" },
    new TimeZoneItem { TagValue = "E. Europe Standard Time|Europe/Vilnius|维尔纽斯", DisplayName = "维尔纽斯 (立陶宛)" },
    new TimeZoneItem { TagValue = "GTB Standard Time|Europe/Bucharest|布加勒斯特", DisplayName = "布加勒斯特 (罗马尼亚)" },
    new TimeZoneItem { TagValue = "GTB Standard Time|Europe/Sofia|索菲亚", DisplayName = "索菲亚 (保加利亚)" },
    new TimeZoneItem { TagValue = "GTB Standard Time|Europe/Athens|雅典", DisplayName = "雅典 (希腊)" },
    new TimeZoneItem { TagValue = "Russian Standard Time|Europe/Moscow|莫斯科", DisplayName = "莫斯科 (俄罗斯)" },
    new TimeZoneItem { TagValue = "Russian Standard Time|Europe/Minsk|明斯克", DisplayName = "明斯克 (白俄罗斯)" },

    // ==================== 大洋洲 ====================
    new TimeZoneItem { TagValue = "AUS Eastern Standard Time|Australia/Sydney|悉尼", DisplayName = "悉尼 (澳大利亚)" },
    new TimeZoneItem { TagValue = "AUS Eastern Standard Time|Australia/Melbourne|墨尔本", DisplayName = "墨尔本 (澳大利亚)" },
    new TimeZoneItem { TagValue = "AUS Eastern Standard Time|Australia/Brisbane|布里斯班", DisplayName = "布里斯班 (澳大利亚)" },
    new TimeZoneItem { TagValue = "AUS Central Standard Time|Australia/Adelaide|阿德莱德", DisplayName = "阿德莱德 (澳大利亚)" },
    new TimeZoneItem { TagValue = "W. Australia Standard Time|Australia/Perth|珀斯", DisplayName = "珀斯 (澳大利亚)" },
    new TimeZoneItem { TagValue = "New Zealand Standard Time|Pacific/Auckland|奥克兰", DisplayName = "奥克兰 (新西兰)" },
    new TimeZoneItem { TagValue = "Fiji Standard Time|Pacific/Fiji|苏瓦", DisplayName = "苏瓦 (斐济)" },

    // ==================== 非洲 ====================
    new TimeZoneItem { TagValue = "South Africa Standard Time|Africa/Johannesburg|约翰内斯堡", DisplayName = "约翰内斯堡 (南非)" },
    new TimeZoneItem { TagValue = "South Africa Standard Time|Africa/Cape_Town|开普敦", DisplayName = "开普敦 (南非)" },
    new TimeZoneItem { TagValue = "E. Africa Standard Time|Africa/Nairobi|内罗毕", DisplayName = "内罗毕 (肯尼亚)" },
    new TimeZoneItem { TagValue = "W. Central Africa Standard Time|Africa/Lagos|拉各斯", DisplayName = "拉各斯 (尼日利亚)" },
    new TimeZoneItem { TagValue = "Morocco Standard Time|Africa/Casablanca|卡萨布兰卡", DisplayName = "卡萨布兰卡 (摩洛哥)" },
    new TimeZoneItem { TagValue = "Egypt Standard Time|Africa/Cairo|开罗", DisplayName = "开罗 (埃及)" }
};

        public static ClockConfig Load()
        {
            var path = ConfigPath;
            var logPath = Path.Combine(Path.GetTempPath(), "DualClock.log");

            // 写入诊断信息
            File.AppendAllText(logPath, $"[{DateTime.Now}] Load() called. ConfigPath: {path}, Exists: {File.Exists(path)}\n");

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
