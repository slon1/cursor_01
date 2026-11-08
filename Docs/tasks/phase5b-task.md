# ФАЗА 5B. Выбор уровня, попапы паузы / победы / поражения, полный поток игры

**Роль:** ты исполнительный программист проекта Unity 6000.5. Архитектор — Claude (проверяет через git, может смотреть картинки из репозитория).
**Ветка:** `phase/5b-select-popups` (от `main` после мерджа `phase/5a-flow`).

**Прочитай перед работой:** `Docs/GAME_RULES.md` (v0.3), `Docs/adr/ADR-004..006`, `Docs/TECH_DEBT.md`. Посмотри макеты в `Assets/Sprites/preview/`: `Select level.png`, `Select level-popup.png`, `popup-pause.png`, `popup-win.png`, `popup-lose.png`.

**Цель:** собрать оставшиеся экраны и замкнуть игровой цикл: Menu → выбор уровня → игра → пауза / победа / поражение → дальше. Использовать каркас из 5A: `ScreenManager`, `PlayerProgress`, `SettingsService`, `UiTheme`, Stage-экраны.

## Решения владельца проекта (не обсуждаются)
1. Золото сохраняется в баланс только при победе (`PlayerProgress.CompleteLevel`). При поражении собранное пропадает.
2. Уровни открываются по порядку. Открытый уровень без конфига в `LevelSet` (сейчас 4–10) показывается закрытым спрайтом и не открывает попап.
3. В выборе уровня и в меню число баланса форматируется как `2,450` (`InvariantCulture`, `ToString("N0")`).
4. Текст «TARGET REACHERD» запечён в спрайтах окон победы и поражения; владелец проекта удалит его позже. Число после точки-разделителя рисуем отдельным TMP-текстом; когда запечённый текст уберут, подпись «Target» будет добавлена отдельно (отметь в `TECH_DEBT`).

## ЧАСТЬ 1. Экран выбора уровня (`preview/Select level.png`)
- `Bg_SelectLevel` внутри Stage. `Btn_Select_Back` (вверху слева) → назад. `Balance_Frame` (вверху справа, 339×117) с числом баланса (Heading, `Gold`).
- Десять узлов уровней на тропе: `SpriteCatalog.GetLevelButton(n, locked)`, позиции задай `Vector2[10]` в данных префаба по макету (через `ReferenceOverlay`); уровень 1 внизу. Узел открыт, если `PlayerProgress.IsUnlocked(n)` и `LevelSet.Has(n)`; иначе закрытый спрайт, тап трясёт узел без реакции.
- Тап по открытому узлу открывает попап (`preview/Select level-popup.png`): затемнение (`UiTheme.Dim`, анимация `PopupDuration`), `Popup_SelectLevel` (1074×716) по центру; в верхней части заголовок «VAULT NN» (Heading, `Cream`, двузначный номер с нулём) между запечёнными львами; «MINIMUM LOOT» и монета в попапе уже запечены, число `MinCoins * GoldPointsPerCoin` (Heading, `Gold`) рядом с монетой справа; `Btn_SelectPopup_Enter` внизу, `Btn_SelectPopup_Close` в правом верхнем углу. Под выбранным узлом на время попапа пульсирует `Fx_LevelGlow` (DOTween).
- ENTER → `ScreenManager.ShowAsync(ScreenId.Game, LevelConfig)`. Закрытие попапа не меняет стек.
- Menu: кнопка Play теперь ведёт в SelectLevel.

## ЧАСТЬ 2. Игровой экран: убрать заглушки
В `GameScreenPresenter` заменить заглушки WIN/LOSE/пауза на события и попапы. Присутствуют попапы в префабе Game (оверлеи в Stage).
- **Пауза:** `Btn_Pause` открывает попап паузы (`preview/popup-pause.png`): затемнение, `Popup_Pause_Frame` (974×1156) по центру («PAUSED» запечено), кнопки `Btn_Pause_Resume`, `Btn_Pause_Restart`, `Btn_Pause_Menu`. Resume закрывает попап; Restart = `GameSession.Restart()` и пересборка экрана; Main Menu = `ScreenManager.ShowAsync(ScreenId.Menu, pushToHistory: false)` с очисткой стека. Пока открыт попап, ввод на поле заблокирован.
- **Победа** (`preview/popup-win.png`): при `GameWon` сначала `PlayerProgress.CompleteLevel(level, coins, pointsPerCoin)`, затем попап `Popup_Win_Bg` на весь Stage с анимацией `PopupDuration`. Числа: «Total Loot» = `Gold * GoldPointsPerCoin` справа от монеты; число цели = `MinCoins * GoldPointsPerCoin` после запечённой точки в строке «TARGET …». Кнопки: `Btn_Win_Next` (если у следующего уровня есть конфиг; иначе скрыть и центрировать `Btn_Win_SelectLevel`) и `Btn_Win_SelectLevel`. Next = Game со следующим `LevelConfig` (без записи в историю), Level Select = SelectLevel.
- **Поражение** (`preview/popup-lose.png`): при `GameLost` попап `Popup_Lose_Bg`. Прогресс не сохранять. Числа: «Unsaved loot lost» = `Gold * GoldPointsPerCoin`, цель = `MinCoins * GoldPointsPerCoin`. Кнопки: `Btn_Lose_TryAgain` (рестарт), `Btn_Lose_SelectLevel`.
- Поток результата: `GameScreenPresenter` отдаёт событие `Finished(GameResult)` (`Won`/`Lost`, монеты, номер уровня); связывание с прогрессом и экранами делает контроллер потока (тонкий класс, не внутри вью); логику прогресса тестируй в Core.
- Вибрация: если `SettingsData.Vibration`, на `TrapTriggered` и `GameLost` вызвать `Handheld.Vibrate()` (только `#if UNITY_ANDROID || UNITY_IOS`; в редакторе пусто). Музыку и звук не подключаем (нет ассетов): сохраняются только флаги.

## ЧАСТЬ 3. Документы и проверка
- `ADR-007.md` (поток результата, попапы, выбор уровня, правила разблокировки), `TECH_DEBT.md` (аудио, подпись «Target» после исправления спрайтов).
- Отчёт `Docs/reports/phase5b.md`.
- Скриншоты в `Docs/reports/phase5b/` при **Game view 1080×2160**: выбор уровня (с открытыми и закрытыми узлами, после победы на уровнях 1–2), попап уровня, игра, пауза, победа, поражение.
- Тесты: контроллер потока через фейки (победа обновляет прогресс и баланс, поражение нет; Next существует только когда есть конфиг следующего уровня), разблокировка узлов по `PlayerProgress` и `LevelSet`.

## Запрещено
Менять правила ядра; прямые ссылки на спрайты вне `SpriteCatalog`; сохранять золото при поражении; логика в `MonoBehaviour`, где её можно вынести в Core.

## Критерии готовности
Консоль 0 ошибок и 0 предупреждений (через MCP); все тесты зелёные; полный путь без остановок: Boot → Menu → Play → выбор уровня 1 → победа → Next → уровень 2 → поражение → Try Again → пауза → Main Menu; баланс и открытые уровни сохраняются между запусками. Тег `phase-5b-done`, push ветки и тега.
