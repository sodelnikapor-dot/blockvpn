# Руководство по настройке VPN-сервера (WireGuard)

MVP работает с обычным WireGuard-сервером: приложение импортирует стандартный `.conf`.
Ниже — минимальный проверенный путь для Ubuntu 22.04/24.04 на VPS с одним IPv4.

## 1. Установка wg на сервере

```bash
sudo apt update && sudo apt install -y wireguard
wg genkey | tee server_private | wg pubkey > server_public
```

`/etc/wireguard/wg0.conf` на сервере:

```ini
[Interface]
Address = 10.66.66.1/24
ListenPort = 51820
PrivateKey = <СОДЕРЖИМОЕ server_private>
# DNS-сервер, который будет предлагаться клиентам, задаётся в их конфиге

PostUp   = iptables -A FORWARD -i wg0 -j ACCEPT; iptables -t nat -A POSTROUTING -o eth0 -j MASQUERADE
PostDown = iptables -D FORWARD -i wg0 -j ACCEPT; iptables -t nat -D POSTROUTING -o eth0 -j MASQUERADE
```

Включаем форвардинг и запускаем:

```bash
echo 'net.ipv4.ip_forward=1' | sudo tee /etc/sysctl.d/99-wg.conf
sudo sysctl --system
sudo systemctl enable --now wg-quick@wg0
```

Открой порт в фаерволе провайдера: `udp/51820`.

## 2. Клиентский конфиг (то, что вставляется в приложение)

Ключи клиента генерируются **на устройстве пользователя** в идеале; для ручного MVP
можно сгенерировать на сервере и сразу удалить приватный ключ оттуда:

```bash
wg genkey | tee client_private | wg pubkey > client_public
sudo wg set wg0 peer <СОДЕРЖИМОЕ client_public> allowed-ips 10.66.66.2/32
sudo wg-quick save wg0
```

Конфиг для клиента:

```ini
[Interface]
PrivateKey = <client_private>
Address = 10.66.66.2/32
DNS = 1.1.1.1

[Peer]
PublicKey = <server_public>
Endpoint = <IP_СЕРВЕРА>:51820
AllowedIPs = 0.0.0.0/0, ::/0
PersistentKeepalive = 25
```

`AllowedIPs = 0.0.0.0/0, ::/0` — весь трафик через туннель (включая DNS → защита
от DNS-утечек). Указание `DNS` в `[Interface]` обязательно: приложение использует его
внутри туннеля.

## 3. Проверка на сервере

```bash
sudo wg show            # должен появиться peer, last handshake недавно
sudo tcpdump -ni eth0 udp port 51820   # видны пакеты клиента
```

## 4. Дальше (этап 2+, требует отдельного подтверждения)

- Автоматизация выдачи конфигов: небольшой API (Go) с токенами устройств; клиент
  генерирует пару ключей локально и отправляет серверу только публичный.
- Ротация peer-ключей, ограничение срока жизни, health-check для списка серверов.
- Домен + TLS (Let's Encrypt) для API и канала подписанных обновлений.
- Несколько регионов: по одному VPS на регион, единая БД peer-ов или независимые узлы.

> Инфраструктура не покупается и не создаётся этим проектом без твоего отдельного подтверждения.
