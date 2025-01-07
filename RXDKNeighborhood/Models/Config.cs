using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RXDKNeighborhood.Models
{

    public class Config
    {
        public List<ConsoleDetail> ConsoleDetailList { get; set; }

        public Config()
        {
            ConsoleDetailList = [];
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

        public static bool TryLoadConfig(ref Config config)
        {
            string applicationPath = string.Empty;
            if (TryGetApplicationPath(ref applicationPath) == false)
            {
                return false;
            }
            var configPath = Path.Combine(applicationPath, "config.json");
            if (!File.Exists(configPath))
            {
                return false;
            }

            var configJson = File.ReadAllText(configPath);
            var deserializedConfig = JsonSerializer.Deserialize<Config>(configJson);
            if (deserializedConfig == null)
            {
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
