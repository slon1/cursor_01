# Фаза 4

Ветка: `phase/4-game-screen`. База: `main` на `d3066ce` (фаза 3).

## Сделано

- `GameSession` в `Game.Core`: выбранный лев, цели только из `GetTargets`, `TapCell`, `TryExtract`, счётчики золота, `Restart`.
- `SpriteCatalog.TryGet` строит словарь при первом обращении и сбрасывает его в `OnEnable`, `OnValidate` и после импорта манифеста. Список в ассете тот же.
- `GameSettingsConfig.GoldPointsPerCoin = 100`. Длительности не переименовывались: `MoveDuration` — ход, прыжок, обмен; `FadeDuration` — появление, исчезновение, шкала и счётчик; `PopupDuration` — пульсы, тряска, заглушки WIN/LOSE.
- Экран: `StageFitter`, `CellView`, `EventPlayer`, `GameScreenPresenter`, `GameBootstrap`. Префаб `Assets/Prefabs/Screens/GameScreen.prefab`. Спрайты экрана только из каталога.
- `GameDev` после `Boot` в Build Settings. Холст Boot и GameDev: 1080×2160, Match 0.5, Screen Space Camera. В Boot больше ничего не менялось. Камера GameDev и поля вокруг Stage — RGB (0, 14, 25).
- `ReferenceOverlay` только в `GameDev`, выключенный, тег `EditorOnly`, дочерний к Stage. В префабе его нет, `preview/Game.png` в префаб не входит.
- `Docs/adr/ADR-004.md`. В `Docs/TECH_DEBT.md` закрыт пункт про словарь, снят `AssemblyMarker`, добавлены шрифт, заглушки паузы/победы/поражения и смысл `MinAlarmAtWin`.

## Проверка

EditMode: 97 тестов зелёные (прежние 89, 7 тестов `GameSession`, тест словаря каталога). Консоль Unity после игры: 0 ошибок и 0 предупреждений.

Уровень 1 пройден мышью в Game view от старта до победы: тревога 4, статус Won. Повторный запуск с заглушки вернул старт. Три шага льва A по верхнему ряду собрали закрытое золото, счётчик стал 100/100.

Слово Alarm нарисовано в спрайте рамки. Числа `Alarm/порог` стоят справа от него, на экране читается `Alarm 0/10`. Счётчик золота — справа от медальона, чтобы не закрывать его.

## Скриншоты

- `start.png` — старт, EXTRACT серая.
- `opened.png` — открытые клетки и сундук, EXTRACT ещё серая.
- `gold.png` — собрано золото, 100/100.
- `extract-ready.png` — EXTRACT золотая, тревога 4/10.
- `win.png` — заглушка WIN.
- `aspect-9x16.png` — Game view 540×960, сцена целиком, поля по бокам.
- `aspect-9x20.png` — Game view 540×1200, сцена целиком, поля сверху и снизу.
