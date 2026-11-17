using System;
using System.IO;
using System.Text.Json;

namespace IslandDeck
{
    public class Settings
    {
        public bool IslandEnabled { get; set; } = true;
        public bool GlassTaskbar { get; set; } = true;
        public bool ShowWidgets { get; set; } = true;
        public string? WallpaperVideo { get; set; }

        // GitHub repozitár vo formáte "pouzivatel/repozitar", odkiaľ sa sťahujú aktualizácie.
        public string UpdateRepo { get; set; } = "";
        public bool AutoCheckUpdates { get; set; } = true;

        private static string FilePath
        {
            get
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IslandDeck");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "settings.json");
            }
        }

        public static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
            }
            catch { }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(FilePath,
                    JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}
