using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace BlockVPN.Core
{
    /// <summary>Локальная диагностика: DNS, адаптер, сервис, журнал туннеля. Ничего не отправляет наружу.</summary>
    public static class Diagnostics
    {
        public static string ResolveHost(string host)
        {
            try
            {
                var addrs = Dns.GetHostAddresses(host);
                var ips = addrs.Select(a => a.ToString()).Take(4);
                return host + " → " + string.Join(", ", ips);
            }
            catch (Exception ex)
            {
                return "DNS-ошибка для " + host + ": " + ex.Message;
            }
        }

        public static string AdapterStatusText()
        {
            try
            {
                var adapter = new Tunnel.Driver.Adapter(TunnelManager.TunnelName);
                var cfg = adapter.GetConfiguration();
                var peer = cfg.Peers.FirstOrDefault();
                if (peer == null) return "Адаптер найден, но пиров в нём нет.";
                var hs = peer.LastHandshake != default
                    ? "последний хендшейк: " + Format.FormatHandshakeAgo(peer.LastHandshake)
                    : "хендшейка ещё не было";
                return $"Адаптер найден. Трафик: {Format.FormatBytes(peer.RxBytes)} получено / {Format.FormatBytes(peer.TxBytes)} отправлено, {hs}.";
            }
            catch
            {
                return "Адаптер «" + TunnelManager.TunnelName + "» не найден — туннель сейчас не поднят.";
            }
        }

        public static string ServiceStatusText()
        {
            try
            {
                using var sc = new System.ServiceProcess.ServiceController("WireGuardTunnel$" + TunnelManager.TunnelName);
                return "Служба WireGuardTunnel$" + TunnelManager.TunnelName + ": " + sc.Status + ".";
            }
            catch
            {
                return "Служба туннеля не установлена (VPN сейчас выключен — это нормально).";
            }
        }

        public static string RecentLogLines(int maxLines)
        {
            try
            {
                var logPath = System.IO.Path.Combine(TunnelManager.ConfigDir, "log.bin");
                if (!System.IO.File.Exists(logPath))
                    return "Журнал туннеля пуст (туннель ещё не запускался).";
                var log = new Tunnel.Ringlogger(logPath, "UI");
                var cursor = Tunnel.Ringlogger.CursorAll;
                var lines = log.FollowFromCursor(ref cursor);
                var tail = lines.Skip(Math.Max(0, lines.Count - maxLines));
                return string.Join(Environment.NewLine, tail);
            }
            catch (Exception ex)
            {
                return "Не удалось прочитать журнал: " + ex.Message;
            }
        }
    }
}
