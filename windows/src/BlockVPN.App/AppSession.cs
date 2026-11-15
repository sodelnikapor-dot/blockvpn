using System;
using System.Collections.Generic;
using BlockVPN.Core;

namespace BlockVPN.App
{
    /// <summary>Общее состояние приложения: менеджер, предпочтения, профили.</summary>
    public static class AppSession
    {
        public static TunnelManager Manager { get; } = new TunnelManager();
        public static UserPrefs Prefs { get; private set; }
        public static ProfileStore Store { get; } = new ProfileStore();
        public static List<VpnProfile> Profiles { get; private set; } = new List<VpnProfile>();

        public static event Action ProfilesChanged;
        public static event Action PrefsChanged;

        public static void Init()
        {
            Prefs = UserPrefs.Load();
            Profiles = Store.Load();
        }

        public static VpnProfile ActiveProfile()
        {
            if (Profiles == null || Profiles.Count == 0) return null;
            var byId = Profiles.Find(p => p.Id == Prefs.ActiveProfileId);
            return byId ?? Profiles[0];
        }

        public static void SaveProfiles()
        {
            Store.Save(Profiles);
            ProfilesChanged?.Invoke();
        }

        public static void SavePrefs()
        {
            Prefs.Save();
            PrefsChanged?.Invoke();
        }
    }
}
