using System;
using System.Globalization;

namespace BlockVPN.Core
{
    public static class Format
    {
        public static string FormatBytes(ulong bytes)
        {
            decimal d = bytes;
            string unit = "Б";
            foreach (var u in new[] { "КиБ", "МиБ", "ГиБ", "ТиБ", "ПиБ" })
            {
                if (d < 1024) break;
                d /= 1024;
                unit = u;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", d, unit);
        }

        public static string FormatDuration(TimeSpan? duration)
        {
            if (duration == null) return "—";
            var t = duration.Value;
            if (t < TimeSpan.Zero) t = TimeSpan.Zero;
            return string.Format("{0}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds);
        }

        public static string FormatHandshakeAgo(DateTime? lastHandshakeUtc)
        {
            if (lastHandshakeUtc == null) return "—";
            var ago = DateTime.UtcNow - lastHandshakeUtc.Value;
            if (ago < TimeSpan.Zero) ago = TimeSpan.Zero;
            if (ago.TotalSeconds < 60) return $"{(int)ago.TotalSeconds} с назад";
            if (ago.TotalMinutes < 60) return $"{(int)ago.TotalMinutes} мин назад";
            return $"{(int)ago.TotalHours} ч назад";
        }
    }
}
