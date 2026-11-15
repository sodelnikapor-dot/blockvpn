using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockVPN.Core
{
    /// <summary>
    /// Машина состояний VPN. Единственный источник правды о статусе:
    /// UI никогда не рисует «Защищено», если менеджер его не сообщил.
    /// Реального статуса три источника: (1) сервис запущен, (2) адаптер существует,
    /// (3) свежий хендшейк WireGuard (не старше 180 секунд).
    /// </summary>
    public class TunnelManager : IDisposable
    {
        public const string TunnelName = "blockvpn";

        public static string ConfigDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BlockVPN");
        public static string ConfigPath => Path.Combine(ConfigDir, TunnelName + ".conf");

        private const int MaxReconnectAttempts = 5;
        private const double HandshakeFreshSeconds = 180;
        private const double ConnectTimeoutSeconds = 45;

        private readonly object _sync = new object();
        private readonly System.Threading.Timer _watch;

        private DateTime? _connectedAt;
        private DateTime? _lastHandshake;
        private DateTime _connectStarted;
        private int _reconnectAttempts;
        private int _reconnecting; // Interlocked флаг активной переподключалки

        private Tunnel.Driver.Adapter _adapter;

        public VpnState State { get; private set; } = VpnState.Disconnected;
        public string LastError { get; private set; }

        public event Action<VpnState, string> StateChanged;
        public event Action<ulong, ulong, DateTime?, TimeSpan?> StatsUpdated;

        public TunnelManager()
        {
            _watch = new System.Threading.Timer(_ => SafeWatch(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public bool IsActive =>
            State == VpnState.Connecting || State == VpnState.Secured || State == VpnState.Reconnecting;

        public void Connect(VpnProfile profile)
        {
            if (profile == null) return;
            lock (_sync)
            {
                if (IsActive) return;
                _reconnectAttempts = 0;
                _lastHandshake = null;
                _connectedAt = null;
                LastError = null;
                SetState(VpnState.Connecting, null);
            }

            Task.Run(() =>
            {
                try
                {
                    // валидация перед запуском: не запускаем мусор
                    WgConfigParser.Parse(profile.ConfigText);
                    Directory.CreateDirectory(ConfigDir);
                    File.WriteAllText(ConfigPath, profile.ConfigText, new UTF8Encoding(false));
                    Tunnel.Service.Add(ConfigPath, true);
                    _connectStarted = DateTime.UtcNow;
                    _watch.Change(1500, 2000);
                }
                catch (Exception ex)
                {
                    LastError = Friendly(ex);
                    SetState(VpnState.Error, LastError);
                    TryDeleteConfig();
                }
            });
        }

        public void Disconnect()
        {
            _watch.Change(Timeout.Infinite, Timeout.Infinite);
            Task.Run(() =>
            {
                try { Tunnel.Service.Remove(ConfigPath, true); }
                catch { }
                CloseAdapter();
                TryDeleteConfig();
                SetState(VpnState.Disconnected, null);
            });
        }

        /// <summary>Вызывается при закрытии приложения: останавливает туннель (эфемерный режим).</summary>
        public void ShutdownCleanup()
        {
            try
            {
                _watch.Change(Timeout.Infinite, Timeout.Infinite);
                if (ServiceExists())
                    Tunnel.Service.Remove(ConfigPath, false);
            }
            catch { }
        }

        public bool ServiceExists()
        {
            try
            {
                using var sc = new System.ServiceProcess.ServiceController("WireGuardTunnel$" + TunnelName);
                _ = sc.Status; // бросает при отсутствии сервиса
                return true;
            }
            catch { return false; }
        }

        // ---------- внутреннее ----------

        private void SafeWatch()
        {
            try { Watch(); } catch { }
        }

        private void Watch()
        {
            if (!IsActive) return;

            ulong rx = 0, tx = 0;
            DateTime? handshake = null;
            bool adapterUp = TryGetStats(out rx, out tx, out handshake);

            if (!adapterUp)
            {
                if (State == VpnState.Secured || State == VpnState.Reconnecting)
                {
                    TryReconnect("Туннель пропал — переподключаюсь");
                }
                else if (State == VpnState.Connecting &&
                         (DateTime.UtcNow - _connectStarted).TotalSeconds > ConnectTimeoutSeconds)
                {
                    LastError = "Сервис туннеля не запустился за 45 секунд. Проверь конфиг и интернет.";
                    SetState(VpnState.Error, LastError);
                }
                return;
            }

            _lastHandshake = handshake;
            bool fresh = handshake.HasValue &&
                         (DateTime.UtcNow - handshake.Value).TotalSeconds <= HandshakeFreshSeconds;

            if (fresh)
            {
                if (_connectedAt == null) _connectedAt = DateTime.UtcNow;
                _reconnectAttempts = 0;
                SetState(VpnState.Secured, null);
            }
            else
            {
                if (State == VpnState.Secured) SetState(VpnState.Reconnecting, "Хендшейк устарел");
                var sinceStart = (DateTime.UtcNow - _connectStarted).TotalSeconds;
                if (sinceStart > ConnectTimeoutSeconds)
                    TryReconnect("Хендшейк не приходит — переподключаюсь");
            }

            var session = _connectedAt != null ? DateTime.UtcNow - _connectedAt : (TimeSpan?)null;
            try { StatsUpdated?.Invoke(rx, tx, handshake, session); } catch { }
        }

        private void TryReconnect(string message)
        {
            if (Interlocked.CompareExchange(ref _reconnecting, 1, 0) != 0) return;
            try
            {
                _reconnectAttempts++;
                if (_reconnectAttempts > MaxReconnectAttempts)
                {
                    LastError = "Не удалось восстановить соединение после " + MaxReconnectAttempts + " попыток. Нажми «Повторить» или проверь сервер.";
                    SetState(VpnState.Error, LastError);
                    return;
                }
                SetState(VpnState.Reconnecting, message);
                CloseAdapter();
                try { Tunnel.Service.Remove(ConfigPath, false); } catch { }
                Task.Delay(2000).Wait();
                Tunnel.Service.Add(ConfigPath, true);
                _connectStarted = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                LastError = Friendly(ex);
            }
            finally
            {
                Interlocked.Exchange(ref _reconnecting, 0);
            }
        }

        private bool TryGetStats(out ulong rx, out ulong tx, out DateTime? handshake)
        {
            rx = 0; tx = 0; handshake = null;
            try
            {
                if (_adapter == null)
                    _adapter = new Tunnel.Driver.Adapter(TunnelName);
                var cfg = _adapter.GetConfiguration();
                foreach (var p in cfg.Peers)
                {
                    rx += p.RxBytes;
                    tx += p.TxBytes;
                    if (p.LastHandshake != default && (handshake == null || p.LastHandshake > handshake.Value))
                        handshake = p.LastHandshake;
                }
                return true;
            }
            catch
            {
                _adapter = null;
                return false;
            }
        }

        private void CloseAdapter()
        {
            _adapter = null; // финализатор Driver.Adapter освободит хендл
        }

        private void TryDeleteConfig()
        {
            try { if (File.Exists(ConfigPath)) File.Delete(ConfigPath); } catch { }
        }

        private void SetState(VpnState state, string message)
        {
            State = state;
            try { StateChanged?.Invoke(state, message); } catch { }
        }

        public static string Friendly(Exception ex)
        {
            var msg = ex.Message ?? "";
            if (msg.Contains("Отказано в доступе") || msg.Contains("Access is denied") ||
                msg.Contains("access denied") || msg.Contains("denied"))
                return "Нужны права администратора: правый клик по BlockVPN.exe → «Запуск от имени администратора».";
            if (msg.Contains("service", StringComparison.OrdinalIgnoreCase) && msg.Contains("exist", StringComparison.OrdinalIgnoreCase))
                return "Служба туннеля не установлена. Перезапусти приложение и попробуй ещё раз.";
            return "Ошибка: " + msg;
        }

        public void Dispose()
        {
            _watch?.Dispose();
        }
    }
}
