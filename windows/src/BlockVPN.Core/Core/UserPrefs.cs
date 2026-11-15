using System;
using System.IO;
using System.Text.Json;

namespace BlockVPN.Core
{
    /// <summary>Несекретные настройки интерфейса. JSON в %APPDATA%\BlockVPN.</summary>
    public class UserPrefs
    {
        public string TagText { get; set; } = "BLOCK";
        public int TagColorIndex { get; set; } = 0;
        public int TagStyleIndex { get; set; } = 0;
        public int ThemeIndex { get; set; } = 0;
        public bool Animations { get; set; } = true;
        public bool Decor { get; set; } = true;
        public bool AutoStart { get; set; } = false;
        public bool AutoConnect { get; set; } = false;
        public string ActiveProfileId { get; set; }
        public int TotalConnects { get; set; } = 0;
        public double TotalMinutes { get; set; } = 0;

        private static string Dir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlockVPN");
        private static string FilePath => Path.Combine(Dir, "prefs.json");

        public static UserPrefs Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<UserPrefs>(File.ReadAllText(FilePath)) ?? new UserPrefs();
            }
            catch { }
            return new UserPrefs();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
            }
            catch { }
        }
    }
}
