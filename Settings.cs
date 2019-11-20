using System.IO;
using System.ComponentModel;
using ff14bot.Helpers;
using Newtonsoft.Json;

namespace GatherUp
{
    class Settings : JsonSettings
    {
        public Settings() : base(Path.Combine(GetSettingsFilePath("GatherUp", "Settings.json")))
        {
            Directory.CreateDirectory(TempDirectory);
        }

        private static Settings _settings;

        public static Settings Current
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new Settings();
                }
                return _settings;
            }
            private set { }
        }


        [DefaultValue("")]
        public string ProfileDirectory { get; set; }

        [JsonIgnore]
        public string DataDirectory { get; private set; } = Path.Combine(GetSettingsFilePath("GatherUp", "Data"));

        [JsonIgnore]
        public string TempDirectory { get; private set; } = Path.Combine(GetSettingsFilePath("GatherUp", "Data", "Temp"));

        [JsonIgnore]
        public bool ProfileDirectoryExists => Directory.Exists(ProfileDirectory);
    }
}


