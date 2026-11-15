using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BlockVPN.Core
{
    /// <summary>
    /// Хранилище профилей. Конфиги содержат приватные ключи, поэтому файл шифруется
    /// DPAPI (защита уровня текущего пользователя Windows) — в открытом виде на диске не лежит.
    /// </summary>
    public class ProfileStore
    {
        private static string Dir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlockVPN");

        private static string FilePath => Path.Combine(Dir, "profiles.bin");

        public List<VpnProfile> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new List<VpnProfile>();
                var encrypted = File.ReadAllBytes(FilePath);
                var plain = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return JsonSerializer.Deserialize<List<VpnProfile>>(plain) ?? new List<VpnProfile>();
            }
            catch (Exception)
            {
                // повреждённый файл не должен валить приложение; начинаем с пустого списка
                return new List<VpnProfile>();
            }
        }

        public void Save(List<VpnProfile> profiles)
        {
            Directory.CreateDirectory(Dir);
            var json = JsonSerializer.Serialize(profiles);
            var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(FilePath, encrypted);
        }
    }
}
