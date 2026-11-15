# Архитектура

## Слои

```
UI (Jetpack Compose, тёмные «уличные» темы, «Мой тег», стикеры)
  ↕ StateFlow
Логика (MainActivity: экраны, разрешения VPN, стикеры-достижения, live-статистика)
  ↕
VpnManager (автомат состояний, автоповтор, монитор туннеля)
  ↕ com.wireguard.android:tunnel
GoBackend (userspace WireGuard, системный VpnService)
```

## Компоненты

| Файл | Роль |
|---|---|
| `vpn/VpnManager.kt` | Подключение/отключение, retry (5 попыток, backoff), монитор-цикл каждые 3 с, статистика `totalRx/totalTx` |
| `vpn/ProfileStore.kt` | Профили серверов; JSON, зашифрованный AES-GCM ключом Keystore (`profiles.enc`) |
| `vpn/KeystoreBox.kt` | AES-256-GCM ключ в AndroidKeyStore, шифрование/дешифрование |
| `prefs/UserPrefs.kt` | Несекретные настройки UI (тег, тема, анимации, автоподключение) |
| `diag/Diagnostics.kt` | Проверка TRANSPORT_VPN, разрешение DNS, разбор DNS/Endpoint из конфига |
| `ui/screens/MainScreen.kt` | Статус-круг, кнопка, статистика, тег, стикеры, нижняя навигация |
| `ui/screens/ServersScreen.kt` | Список профилей, добавление с валидацией `Config.parse` |
| `ui/screens/TagScreen.kt` | Редактор «Мой тег» (текст ≤ 12 симв., 6 цветов, 3 стиля) |
| `ui/screens/SettingsScreen.kt` | Темы, анимации, декор, автоподключение, kill switch-хук, приватность |
| `ui/screens/HelpScreen.kt` | FAQ, диагностика, версия |
| `ui/theme/Theme.kt` | 3 палитры: Ночной квартал, Неон-молл, Ржавый вагон |

## Потоки данных

- `VpnManager.state: StateFlow<VpnUiState>` — единственный источник истины о статусе.
  UI никогда не «рисует» состояние сам.
- Статистика опрашивается раз в секунду только в состоянии SECURED.
- Профили читаются из зашифрованного файла при изменении списка.

## Почему WireGuard и GoBackend

- WireGuard — аудированный протокол, эталонная реализация; библиотека поддерживается
  проектом WireGuard (`wireguard-android`), включая GoBackend для userspace-туннеля
  через системный `VpnService`.
- Собственная криптография не используется вообще.

## Kill switch

Реализация возложена на системные механизмы Android («VPN всегда активен» +
«Блокировать соединения без VPN») — это надёжнее любых самодельных правил.
Приложение объясняет и открывает нужный экран настроек. DNS берётся из туннельного
конфига, что исключает DNS-утечки при активном туннеле.

## Дорожная карта платформ

- Android (этот репозиторий) — MVP.
- Windows — тот же UI-подход на Compose Multiplatform, ядро WireGuardNT/wintun, отдельный сервис.
- iOS/macOS — NetworkExtension + WireGuardKit, нужен платный Apple Developer-аккаунт.
- Серверный контур (API конфигураций, подписанные обновления) — общий для всех платформ,
  описан в `SERVER_SETUP.md`.
