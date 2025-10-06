using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RXDKNeighborhood.Models
{

    public class Config
    {
        public List<XboxItem> XboxItemList { get; set; }

        public Config()
        {
            XboxItemList = [];
        }

        private static bool TryGetApplicationPath(ref string applicationPath)
        {
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            if (exePath == null)
            {
                return false;
            }

            var result = Path.GetDirectoryName(exePath);
            if (result == null)
            {
                return false;
            }

            applicationPath = result;
            return true;
        }

        public static bool TryLoadConfig(out Config? config)
        {
            string applicationPath = string.Empty;
            if (TryGetApplicationPath(ref applicationPath) == false)
            {
                config = null;
                return false;
            }
            var configPath = Path.Combine(applicationPath, "config.json");
            if (!File.Exists(configPath))
            {
                config = null;
                return false;
            }

            var configJson = File.ReadAllText(configPath);
            var deserializedConfig = JsonSerializer.Deserialize<Config>(configJson);
            if (deserializedConfig == null)
            {
                config = null;
                return false;
            }

            config = deserializedConfig;
            return true;
        }

        public static bool TrySaveConfig(Config config)
        {
            string applicationPath = string.Empty;
            if (TryGetApplicationPath(ref applicationPath) == false)
            {
                return false;
            }

            var configPath = Path.Combine(applicationPath, "config.json");
            var serializedConfig = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, serializedConfig);
            return true;
        }
    }
}
