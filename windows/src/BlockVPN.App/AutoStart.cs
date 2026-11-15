using Microsoft.Win32;

namespace BlockVPN.App
{
    /// <summary>
    /// Автозапуск через ключ Run в HKCU (только по явному согласию пользователя).
    /// Так как приложение требует прав администратора, при старте системы появится окно UAC —
    /// это ограничение Windows, честно описано в docs/WINDOWS.md.
    /// </summary>
    public static class AutoStart
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "BlockVPN";

        public static bool IsSet()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey);
                return key?.GetValue(ValueName) != null;
            }
            catch { return false; }
        }

        public static void Set(bool enabled)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (key == null) return;
                if (enabled)
                    key.SetValue(ValueName, System.Reflection.Assembly.GetExecutingAssembly().Location);
                else
                    key.DeleteValue(ValueName, false);
            }
            catch { }
        }
    }
}
