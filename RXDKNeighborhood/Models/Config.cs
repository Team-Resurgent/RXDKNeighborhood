using RXDKXBDM.Models;
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
            try
            {
                var userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RXDKNeighborhood");
                if (!Directory.Exists(userFolder))
                {
                    Directory.CreateDirectory(userFolder);
                }
                applicationPath = userFolder;
                return true;
            }
            catch
            {
                return false;
            }
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
