# Сборка и запуск

## Требования

- JDK 17 (Temurin/Corretto)
- Android SDK: platform 34, build-tools 34.0.0
- Gradle 8.2 (или Android Studio Hedgehog+ со встроенным Gradle)
- Устройство или эмулятор с Android 8.0+ (API 26). Для реального VPN нужен реальный телефон:
  эмуляторы не пропускают полноценный VpnService-трафик.

## Вариант 1: Android Studio (рекомендуется)

1. `File → Open` → папка `blockvpn`.
2. Дождись синхронизации Gradle (скачает зависимости с mavenCentral и google()).
3. Подключи телефон с включённой «Отладкой по USB» → `Run ▶`.

## Вариант 2: командная строка

```bash
# если есть gradle 8.2 в PATH
gradle wrapper --gradle-version 8.2   # создаст gradlew (в репозитории нет jar-обёртки)
./gradlew assembleDebug
# результат: app/build/outputs/apk/debug/app-debug.apk
adb install -r app/build/outputs/apk/debug/app-debug.apk
```

## Release-сборка

1. Создай keystore: `keytool -genkeypair -v -keystore blockvpn.keystore -alias blockvpn -keyalg RSA -keysize 4096 -validity 10000`
2. Пропиши `signingConfigs.release` в `app/build.gradle.kts` (пути и пароли — через
   переменные окружения, см. `SECRETS.md`). Не коммить keystore.
3. `./gradlew assembleRelease`.

## Первый запуск без серверов

Приложение стартует в **режиме разработки**: баннер «РЕЖИМ РАЗРАБОТКИ» на главном экране,
кнопка «Подключиться» неактивна, пока не добавлен хотя бы один профиль. Это честное
состояние — анимация «Защищено» без реального туннеля невозможна по построению:
статус приходит только из `VpnManager.state`, а тот — из WireGuard backend.

## Как попробовать настоящее подключение

1. Подними сервер по `docs/SERVER_SETUP.md` (или возьми готовый WireGuard-конфиг).
2. «Серверы» → «+ Добавить сервер» → вставь текст конфига.
3. «Подключиться» → согласись с системным диалогом VPN.
4. Проверь IP на любом сайте «my ip» — должен быть адрес сервера.
