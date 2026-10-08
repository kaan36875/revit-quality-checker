using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RevitQualityChecker.Core.Configuration
{
    public class UserSettings
    {
        public HashSet<string> DisabledRuleIds { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public string CustomRulesFilePath { get; set; }

        private static string GetSettingsPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "RevitQualityChecker");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, "settings.json");
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(GetSettingsPath(), json);
        }

        public static UserSettings Load()
        {
            var path = GetSettingsPath();
            if (!File.Exists(path))
                return new UserSettings();

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<UserSettings>(json) ?? new UserSettings();
            }
            catch
            {
                return new UserSettings();
            }
        }
    }
}
