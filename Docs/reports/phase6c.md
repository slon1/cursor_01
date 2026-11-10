# Фаза 6C. Android

Ветка `phase/6c-android`. В `main` уровней 4–10 не было, поэтому влита `phase/6b-levels`. Конфликтов нет. Тег `phase-6c-done` не ставился.

## Сборка

`Tools/Build/Build Android Bundle (Release)` до сборки не дошёл: файла `keystore.local.json` и переменных `MK_KEYSTORE_*` нет. Сообщение скрипта: «Нет подписи релиза. Нужен keystore.local.json с полями path, alias, keystorePass, keyPass или переменные MK_KEYSTORE_PATH, MK_KEYSTORE_PASS, MK_KEY_ALIAS, MK_KEY_PASS.» AAB не создан.

`Tools/Build/Build Android APK (Development)` собрался. SDK и NDK лежат в модуле Android Player редактора 6000.5.9f1. Лог скрипта: «Сборка Builds/MaritKing-dev.apk Succeeded 1100799299 байт, предупреждений 444, ошибок 0». `totalSize` в отчёте Unity — 1 100 799 299 байт. Сам файл `Builds/MaritKing-dev.apk` — 64 590 484 байт. В `Editor.log` из этих предупреждений явно напечатаны устаревание круглых и legacy-иконок и флаг clang `-x c++` без эффекта. Остальные 444 считаются отчётом сборки и в лог редактора построчно не попали. Ошибок сборки 0.

## minSdk 26

Задание просило API 24. Редактор 6000.5 поддерживает Android 8.0 (API 26) и новее и отвергает более низкий уровень: «Minimum supported Android API level is 26 (Android 8.0)». Поэтому `AndroidConfigure` ставит API 26, целевой уровень — Automatic.

## Память текстур

`Tools/Build/Report Sprite Memory`, 86 текстур в `Assets/Sprites`. До сжатия (RGBA) 191 482 444 байт, после оценки ASTC 22 314 224 байт. Это меньше 150 МБ, отдельные каталоги не предлагаются.

Десять самых больших, все ASTC 6×6, 1080×2160, 935 712 байт:

1. `Assets/Sprites/preview/Menu.png`
2. `Assets/Sprites/popup-win/popup-win-bg.png`
3. `Assets/Sprites/popup-lose/popup-lose-bg.png`
4. `Assets/Sprites/Menu/Menu-bg.png`
5. `Assets/Sprites/preview/popup-pause.png`
6. `Assets/Sprites/preview/Game.png`
7. `Assets/Sprites/preview/popup-win.png`
8. `Assets/Sprites/preview/Preload.png`
9. `Assets/Sprites/preview/Rules.png`
10. `Assets/Sprites/preview/Select level-popup.png`

## Шрифты

По GUID в `Assets` и в `UiTheme` игра ссылается на SDF `Assets/Fonts`: Cinzel `789d9e4d97b34d84796f1994b02dc99e`, Inter `611f991c0998004459a061925d819e65`. Эти атласы смотрели на статические `Cinzel-Regular.ttf` и `Inter_18pt-Regular.ttf`. Источник переведён на variable-файлы, запечённые глифы не пересобирались. Папки `static` у Inter и Cinzel, курсивный variable Inter вместе с его SDF и каталог `Luckiest_Guy` ссылок больше не имели и удалены. Variable-шрифты и игровые SDF Cinzel и Inter остались.

Снимки Game view 1080×2160 после удаления: [menu.png](phase6c/menu.png), [game.png](phase6c/game.png). Подписи PLAY, HOW TO PLAY, BEST HAUL, Alarm, Select Leon и EXTRACT на месте.

## Сетка

Сторона клетки `min(176, (912 - (N - 1) * 8) / N)`. Для 5×5 это 176, сетка 912. Для 6×6 это 145,33, сетка тоже 912. Врезка предмета 8 px.

[board-5x5.png](phase6c/board-5x5.png) — уровень 1. [board-6x6.png](phase6c/board-6x6.png) — уровень 7, тревога 0/12.

## Прочее

Safe area, кнопка «Назад», пауза при сворачивании и сжатие текстур — в [ADR-010](../adr/ADR-010.md). Чек-лист телефона — [ANDROID_CHECKLIST.md](../ANDROID_CHECKLIST.md), отметки пустые. Составная иконка — [icon.png](phase6c/icon.png). Симулятор: [safe-ios-classic.png](phase6c/safe-ios-classic.png), [safe-ios-notch.png](phase6c/safe-ios-notch.png), [safe-punch-hole.png](phase6c/safe-punch-hole.png).
