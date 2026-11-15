# BlockVPN for Windows (MVP)

Клиент WireGuard для Windows 10/11 в уличном стиле BlockVPN. Тёмная тема, три палитры,
«Мой тег», локальные стикеры, диагностика.

## Как это работает

- WireGuard поднимается через **официальный embeddable `tunnel.dll`** из проекта
  [wireguard-windows](https://git.zx2c4.com/wireguard-windows/about/embeddable-dll-service/)
  (тот же userspace-движок, что у официального клиента). Собственного крипто-кода нет.
- `tunnel.dll` требует `wireguard.dll` (драйвер wireguard-nt) — берётся с официального
  сервера загрузок WireGuard с проверкой SHA256 в CI.
- Приложение запускается с правами администратора (UAC) — требование Windows для
  системных туннелей, так же работает официальный клиент WireGuard.
- При подключении приложение регистрирует эфемерную службу `WireGuardTunnel$blockvpn`,
  которая запускает наш exe с аргументом `/service`. При закрытии приложения туннель
  останавливается.

## Сборка

Нужен .NET 8 SDK (Windows) и файлы `tunnel.dll` + `wireguard.dll` рядом с `BlockVPN.exe`
(при publish из CI подкладываются автоматически).

```powershell
dotnet test windows/tests/BlockVPN.Tests/BlockVPN.Tests.csproj
dotnet publish windows/src/BlockVPN.App/BlockVPN.App.csproj -c Release -r win-x64 --self-contained true -o out
copy <путь-к-tunnel.dll> out\tunnel.dll
copy <путь-к-wireguard.dll> out\wireguard.dll
```

## Установка (из артефакта CI)

1. Скачай артефакт `BlockVPN-windows-x64` со страницы Actions.
2. Распакуй zip в отдельную папку.
3. Запусти `BlockVPN.exe`, согласись на запрос прав администратора.
4. Вкладка «Сервера» → «+ Добавить» → вставь WireGuard-конфиг.

## Безопасность

- Конфиги (с приватными ключами) хранятся в `%APPDATA%\BlockVPN\profiles.bin`,
  зашифрованные DPAPI текущего пользователя.
- Настройки вида — `%APPDATA%\BlockVPN\prefs.json` (несекретные).
- Журнал туннеля — `%ProgramData%\BlockVPN\log.bin`, только локально.
- Никакой телеметрии.

## Известные ограничения MVP

- Kill switch в Windows не автоматизирован (см. вкладку «Настройки»).
- Автозапуск через HKCU Run — при старте системы будет окно UAC.
- Туннель останавливается при закрытии приложения (эфемерный режим).
- Файлы не подписаны сертификатом — SmartScreen может предупредить при первом запуске.
