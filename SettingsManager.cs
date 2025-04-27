using System.IO;
using Newtonsoft.Json;

namespace AIWareBuilder
{
    public static class SettingsManager
    {
    public class StubSettings
    {
        public string ServerUrl { get; set; }
        public int SampleSizeKB { get; set; }
        public int ValueThreshold { get; set; }
        public bool StealthEnabled { get; set; }
        public bool SelfDestructEnabled { get; set; }
        public string AiApiUrl { get; set; }
        public string AiApiKey { get; set; }
        public string IconPath { get; set; }
        public string ScanMode { get; set; } // "Default" or "Full"
        public bool CollectMediaFiles { get; set; }
        public bool PersistenceEnabled { get; set; }
    }

        public static void SaveSettings(StubSettings settings, string outputPath)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(outputPath, json);
        }
    }
}
