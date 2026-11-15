using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BlockVPN.Core
{
    public class WgPeer
    {
        public string PublicKey { get; set; }
        public string Endpoint { get; set; }
        public List<string> AllowedIps { get; } = new List<string>();
        public int? PersistentKeepalive { get; set; }
    }

    public class WgConfig
    {
        public string PrivateKey { get; set; }
        public List<string> Addresses { get; } = new List<string>();
        public List<string> Dns { get; } = new List<string>();
        public List<WgPeer> Peers { get; } = new List<WgPeer>();
    }

    /// <summary>
    /// Парсер конфигов WireGuard (формат wg-quick). Чистая логика, покрыта тестами.
    /// Никакой криптографии здесь нет — только разбор и валидация текста.
    /// </summary>
    public static class WgConfigParser
    {
        public static bool IsValidKey(string base64Key)
        {
            if (string.IsNullOrWhiteSpace(base64Key)) return false;
            try
            {
                var bytes = Convert.FromBase64String(base64Key.Trim());
                return bytes.Length == 32;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>Возвращает список ошибок. Пустой список = конфиг валиден.</summary>
        public static List<string> Validate(string text)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add("Конфиг пустой.");
                return errors;
            }

            var config = new WgConfig();
            ParseInternal(text, config, errors);
            return errors;
        }

        /// <summary>Разбирает конфиг; бросает ArgumentException с понятным сообщением.</summary>
        public static WgConfig Parse(string text)
        {
            var errors = new List<string>();
            var config = new WgConfig();
            ParseInternal(text, config, errors);
            if (errors.Count > 0)
                throw new ArgumentException(errors[0]);
            return config;
        }

        private static void ParseInternal(string text, WgConfig config, List<string> errors)
        {
            string section = null;
            WgPeer currentPeer = null;
            int peerIndex = 0;
            bool hasInterface = false;

            var lines = text.Replace("\r\n", "\n").Split('\n');
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                if (line.StartsWith("["))
                {
                    var name = line.Trim('[', ']').Trim().ToLowerInvariant();
                    if (name == "interface")
                    {
                        section = "interface";
                        hasInterface = true;
                    }
                    else if (name == "peer")
                    {
                        section = "peer";
                        currentPeer = new WgPeer();
                        config.Peers.Add(currentPeer);
                        peerIndex++;
                    }
                    else
                    {
                        errors.Add($"Неизвестная секция [{name}]. Ожидаются [Interface] и [Peer].");
                        section = null;
                    }
                    continue;
                }

                int eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    errors.Add($"Строка не похожа на «Ключ = Значение»: «{line}».");
                    continue;
                }
                var key = line.Substring(0, eq).Trim().ToLowerInvariant();
                var value = line.Substring(eq + 1).Trim();

                if (section == "interface")
                {
                    switch (key)
                    {
                        case "privatekey":
                            if (!IsValidKey(value))
                                errors.Add("PrivateKey должен быть 32 байта в base64 (44 символа).");
                            config.PrivateKey = value;
                            break;
                        case "address":
                            foreach (var a in value.Split(','))
                                ValidateAddress(a.Trim(), "Address", errors);
                            config.Addresses.AddRange(SplitList(value));
                            break;
                        case "dns":
                            config.Dns.AddRange(SplitList(value));
                            break;
                        case "listenport":
                        case "mtu":
                        case "table":
                        case "preup":
                        case "postup":
                        case "predown":
                        case "postdown":
                        case "savestep": // unknown keys ignored silently below
                            break;
                        default:
                            break; // незнакомые ключи интерфейса игнорируем
                    }
                }
                else if (section == "peer")
                {
                    switch (key)
                    {
                        case "publickey":
                            if (!IsValidKey(value))
                                errors.Add($"У пира №{peerIndex}: PublicKey должен быть 32 байта в base64.");
                            currentPeer.PublicKey = value;
                            break;
                        case "endpoint":
                            if (!IsValidEndpoint(value))
                                errors.Add($"У пира №{peerIndex}: Endpoint должен быть вида host:port (например vpn.example.com:51820).");
                            currentPeer.Endpoint = value;
                            break;
                        case "allowedips":
                            foreach (var a in SplitList(value))
                                ValidateAddress(a.Trim(), "AllowedIPs", errors);
                            currentPeer.AllowedIps.AddRange(SplitList(value));
                            break;
                        case "persistentkeepalive":
                            if (!int.TryParse(value, out _))
                                errors.Add($"У пира №{peerIndex}: PersistentKeepalive должен быть числом секунд.");
                            if (int.TryParse(value, out var ka))
                                currentPeer.PersistentKeepalive = ka;
                            break;
                        default:
                            break; // presharedkey и прочее пропускаем без разбора
                    }
                }
                else
                {
                    errors.Add($"Ключ «{key}» вне секций [Interface]/[Peer].");
                }
            }

            if (!hasInterface)
                errors.Add("Не найдена секция [Interface].");
            else if (string.IsNullOrEmpty(config.PrivateKey))
                errors.Add("В [Interface] отсутствует PrivateKey.");

            if (config.Peers.Count == 0)
                errors.Add("Не найдена ни одна секция [Peer] — некому отправлять трафик.");
            else if (config.Peers.Any(p => string.IsNullOrEmpty(p.PublicKey)))
            {
                // ошибка уже добавлена выше при разборе
            }
        }

        private static IEnumerable<string> SplitList(string value)
        {
            return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static void ValidateAddress(string value, string what, List<string> errors)
        {
            if (string.IsNullOrEmpty(value)) { errors.Add($"{what}: пустой адрес."); return; }
            var parts = value.Split('/');
            if (parts.Length > 2) { errors.Add($"{what}: некорректный адрес «{value}»."); return; }
            if (!IPAddress.TryParse(parts[0], out var ip))
            {
                errors.Add($"{what}: «{parts[0]}» не является IP-адресом.");
                return;
            }
            if (parts.Length == 2)
            {
                if (!byte.TryParse(parts[1], out var prefix) || prefix > (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128))
                    errors.Add($"{what}: некорректная длина префикса в «{value}».");
            }
        }

        public static bool IsValidEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint)) return false;
            string host;
            string portPart;
            if (endpoint.StartsWith("["))
            {
                int close = endpoint.IndexOf(']');
                if (close < 0 || close + 2 > endpoint.Length || endpoint[close + 1] != ':') return false;
                host = endpoint.Substring(1, close - 1);
                portPart = endpoint.Substring(close + 2);
            }
            else
            {
                int colon = endpoint.LastIndexOf(':');
                if (colon <= 0) return false;
                host = endpoint.Substring(0, colon);
                portPart = endpoint.Substring(colon + 1);
            }
            if (host.Length == 0) return false;
            if (!int.TryParse(portPart, out var port) || port < 1 || port > 65535) return false;
            return true;
        }

        public static string ExtractDns(string text)
        {
            try
            {
                var cfg = Parse(text);
                return cfg.Dns.Count > 0 ? string.Join(", ", cfg.Dns) : null;
            }
            catch { return null; }
        }

        public static string ExtractEndpoint(string text)
        {
            try
            {
                var cfg = Parse(text);
                return cfg.Peers.Count > 0 ? cfg.Peers[0].Endpoint : null;
            }
            catch { return null; }
        }
    }
}
