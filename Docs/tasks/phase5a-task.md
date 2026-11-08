# ФАЗА 5A. Правила (минимальная добыча), тема и шрифты, навигация, прогресс, экраны Menu / Settings / Rules

**Роль:** ты исполнительный программист проекта Unity 6000.5. Архитектор — Claude (проверяет через git, может смотреть картинки из репозитория).
**Ветка:** `phase/5a-flow` (от `main` после мерджа `phase/4-game-screen`).

**Прочитай перед работой:** `Docs/GAME_RULES.md`, `Docs/adr/ADR-001..004`, `Docs/TECH_DEBT.md`. Посмотри макеты в `Assets/Sprites/preview/` (`Menu.png`, `Settings.png`, `Rules.png`) и фоны экранов в `Assets/Sprites/Menu`, `Settings`, `Rules`.

**Цель:** внести в ядро правило минимальной добычи, переписать уровни, довести игровой экран до макета, и собрать каркас приложения (ScreenManager, прогресс, настройки, тема) с тремя экранами. Выбор уровня, попапы паузы, победы и поражения будут в фазе 5B.

## Решения владельца проекта (не обсуждаются)
1. **Минимальная добыча** это правило: у каждого уровня есть `MinCoins`. Золотая кнопка EXTRACT доступна, только когда выполнены ВСЕ условия: сундук открыт, оба льва в клетках, смежных с выходом (4 стороны), клетка выхода открыта и `Gold >= MinCoins`.
2. **Все клетки в начале закрыты**, кроме двух стартовых клеток львов. Предметы скрыты, пока клетка не открыта (это уже поддерживает движок; меняется контент уровней).
3. Золото сохраняется в баланс игрока **только при победе**. При поражении собранное на уровне пропадает.
4. Валюта в интерфейсе: монета = `GoldPointsPerCoin` (100) очков. Баланс хранится в очках.
5. **Экрана Preload нет**: первый экран приложения Menu. Макет `preview/Preload.png` не используется.
6. В проект добавлены шрифты Cinzel, Inter, Luckiest_Guy (найди их в Assets, скорее всего `Assets/Fonts`). Используем Cinzel (заголовки, числа) и Inter (мелкие подписи). Luckiest Guy пока не нужен.

## ЧАСТЬ 0. Ядро и уровни

**0.1 Game.Core**
- `LevelData.MinCoins` (int, >= 0). `LevelData.Validate()` добавляет ошибки: `MinCoins < 0`; `MinCoins` больше числа клеток с Gold на поле.
- `RulesEngine.ExitConditionsMet`: добавить условие `state.Gold >= state.MinCoins` (скопируй `MinCoins` в `GameState` при `FromLevel`). Событие `ExitAttempted(conditionsMet)` и `GameSession.ExitReady` работают через это условие. `GameSession` получает `int MinCoins`.
- `LevelMapParser.Parse(...)` принимает `minCoins`.
- Решатель (`LevelSolver`) уже опирается на `ExitAttempted(true)`: убедись, что он корректно учитывает золото (состояние включает Gold). `SolveResult` не менять.
- Тесты: выход не готов при `Gold < MinCoins` и готов при `Gold == MinCoins`; `ExitAttempted(false)` при нехватке золота; `Validate` для `MinCoins`; решатель: карта, где золота меньше `MinCoins`, непроходима (Validate должен поймать раньше, а решатель на ручном LevelData без Validate возвращает Solvable == false).

**0.2 Game.Config и уровни**
- `LevelConfig.MinCoins` (int). `ToLevelData()` передаёт его.
- Перепиши три уровня (в клетках `#` закрытая пустая, строчные буквы закрытые предметы, `A` и `B` открытые клетки старта; остальное по алфавиту из ADR-003). Менять карты и числа нельзя, только внести как есть:
  - **Level_01:** `A#g##/#####/##c#g/B####/e#g##`, AlarmThreshold 10, JumpCost 2, GateBreakCost 3, TrapCost 4, **MinCoins 2**.
  - **Level_02:** `A###B/#g###/rrrrr/#g#c#/g#e##`, AlarmThreshold 8, JumpCost 2, GateBreakCost 3, TrapCost 4, **MinCoins 2**.
  - **Level_03:** `A#t#l/###g#/##t##/#c##g/B##e#`, AlarmThreshold 8, JumpCost 2, GateBreakCost 3, TrapCost 4, **MinCoins 2**.
- Тест по всем уровням набора: `Validate()` пусто, `TrySolve` даёт `Solvable == true` и `!Truncated`. Если уровень непроходим, ничего не исправляй сам: опиши в отчёте (я проверил руками, что все три проходимы без урона).

**0.3 Документы**
- Обнови `Docs/GAME_RULES.md`: в «Поле и предметы» добавь «В начале уровня все клетки закрыты, кроме стартовых клеток львов»; в «Сундук и выход» условие выхода «золото собрано не меньше `MinCoins` уровня»; в «Счёт» «при победе золото добавляется к балансу, при поражении пропадает». Создай `ADR-005.md` с этими решениями.

## ЧАСТЬ 1. Правки игрового экрана (по ревью фазы 4)
1. **Подсветка:** спрайты `Hl_Step` и `Hl_Jump` непрозрачные. Добавь `GameSettingsConfig.HighlightAlpha = 0.5f` и применяй как альфу цвета Image подсветки.
2. **Счётчик золота:** показывает `{Gold*GoldPointsPerCoin}/{MinCoins*GoldPointsPerCoin}` (было «всего монет»). Расположение: монета на макете в рамке уже есть, число должно стоять справа от неё, визуально по центру плашки (центр числа около x=570 в координатах Stage). Сверь с `Game.png` в ReferenceOverlay.
3. **Заголовок тревоги:** число `N/порог` на одной вертикальной линии с запечённым словом «Alarm».
4. `GameSession`: `TotalCoins` оставь (понадобится позже), но на экране используется `MinCoins`.

## ЧАСТЬ 2. Тема и шрифты
- В `Game.Config` ScriptableObject `UiTheme`: `TMP_FontAsset Heading` (Cinzel), `TMP_FontAsset Body` (Inter), цвета `Cream = #F3E5B5`, `Gold = #E8C468`, `Dim = #000000` с альфой 0.6 (затемнение под попапами). Добавь в asmdef `Game.Config` ссылку `Unity.TextMeshPro` при необходимости.
- Сгенерируй TMP Font Asset из шрифтов Cinzel и Inter (SDF, символы: латиница, цифры, знаки `/ , . : • ✓ -`). Положи в `Assets/Fonts/` (или рядом со шрифтами). Скрипт-меню `Tools/Fonts/Create TMP Assets` (идемпотентно) или ручная генерация через MCP, как удобнее.
- Все TMP-тексты игрового экрана переведи на тему (счётчики: Heading; заглушки: Heading). Закрой пункт про шрифт в `TECH_DEBT.md`.

## ЧАСТЬ 3. Каркас приложения
**Прогресс и настройки (чистая логика в Game.Core, хранение в Game.View):**
- `PlayerProgress`: `int HighestUnlockedLevel` (>= 1, по умолчанию 1), `int BalancePoints`. Методы: `CompleteLevel(int level, int coins, int pointsPerCoin)` (`HighestUnlockedLevel = max(текущий, level + 1)`; `BalancePoints += coins * pointsPerCoin`), `bool IsUnlocked(int level)`.
- `IProgressStorage` (Load/Save) и `PlayerPrefsProgressStorage` во View. Для тестов фейковое хранилище.
- `SettingsData`: `bool Music, Sound, Vibration` (по умолчанию все true), `ISettingsStorage` и `PlayerPrefsSettingsStorage` аналогично; `SettingsService` с событием `Changed`.
- Тесты: `CompleteLevel` (разблокировка, баланс, не уменьшает `HighestUnlockedLevel`, суммирует баланс), сохранение/загрузка через фейковое хранилище, настройки по умолчанию и переключение.

**Навигация:**
- В `Game.Core` `NavigationStack<T>` (push/pop/peek/Clear) с тестами.
- `ScreenManager` (MonoBehaviour в сцене Boot): держит `ScreenId` (Menu, Settings, Rules, SelectLevel, Game), префабы экранов, стек возврата и полноэкранный чёрный `FadeOverlay` (Image + CanvasGroup). API: `UniTask ShowAsync(ScreenId id, object args = null, bool pushToHistory = true)` и `UniTask BackAsync()`. Смена: затемнение (`FadeDuration`) → замена → осветление; на время перехода `FadeOverlay` блокирует ввод. Подготовь точки расширения для экранов SelectLevel и Game (заглушки-пустые префабы допустимы, они появятся в 5B; Game уже есть из фазы 4: подключи его к `ScreenManager` с аргументом `LevelConfig`).
- Каждый экран это префаб с корнем `Stage` (контейн-масштаб из фазы 4, `StageFitter`), фон внутри Stage. Использовать тот же приём `ReferenceOverlay` (выключен, тег `EditorOnly`, только в тестовой сцене, не в префабе) для сверки положения с макетом.
- `AppBootstrap` в Boot: собирает сервисы (`SpriteCatalog`, `LevelSet`, `GameSettingsConfig`, `UiTheme`, прогресс, настройки) и запускает `ScreenManager`, который сразу показывает Menu (без затемнения на входе, `FadeOverlay` стартует прозрачным). Boot становится стартовой сценой. `GameDev` остаётся для отладки, из Build Settings убрать.

## ЧАСТЬ 4. Экраны
Все спрайты только из `SpriteCatalog` (`SpriteId`). Положение элементов брать по макету через `ReferenceOverlay`.

**Menu** (`preview/Menu.png`): `Bg_Menu`; `Btn_Menu_Play` → SelectLevel (пока заглушка: пишет в лог и остаётся в Menu); `Btn_Menu_Rules` → Rules; `Btn_Menu_Settings` → Settings. Внизу текст «BEST HAUL {баланс}» (Body, число Heading, формат с разделителем тысяч: `2,450`, `CultureInfo.InvariantCulture`, `ToString("N0")`). Лого `Menu_Logo`, если оно не запечено в фон (проверь по макету и спрайту: на макете герб «MK» вверху).

**Settings** (`preview/Settings.png`): `Bg_Settings` (заголовок, подписи Music/Sound/Vibration, иконки и «v. 1.0» уже запечены в фон, не дублировать; номер версии не рисуем). Три переключателя (`Switch_On` / `Switch_Off` как Button/Image) напротив строк Music, Sound, Vibration; клик переключает `SettingsService`. `Btn_Settings_Rules` («HOW TO PLAY») → Rules. `Btn_Settings_Back` (стрелка влево вверху слева) → назад по стеку.

**Rules** (`preview/Rules.png`): `Bg_Rules` и `Rules_Frame` (одно изображение 996×1467, по макету), `Btn_Rules_Back` → назад по стеку. Текст внутри `Rules_Frame` запечён, его владелец проекта исправит сам. Не менять.

## ЧАСТЬ 5. Документы и проверка
- `ADR-006.md`: навигация и экраны, прогресс и настройки, хранение (PlayerPrefs). Экрана Preload в проекте нет (решение владельца): первый экран Menu.
- `TECH_DEBT.md`: нет аудио (Music/Sound только сохраняются), вибрация не подключена (5B), «TARGET REACHERD» запечена в спрайтах попапов (владелец исправит).
- Отчёт `Docs/reports/phase5a.md`.
- Скриншоты в `Docs/reports/phase5a/`: **Game view с фиксированным разрешением 1080×2160** (создай пресет в Game view), чтобы экран был виден целиком. Сохрани: Menu, Settings (с переключателями в разных положениях), Rules, игровой экран на уровне 1 (с серой и золотой кнопкой EXTRACT, с подсветкой).

## Запрещено
Менять карты уровней и числа из части 0.2; менять правила кроме описанных; прямые ссылки на спрайты вне `SpriteCatalog` (исключение: ReferenceOverlay в тестовых сценах); логика в `MonoBehaviour`, где её можно вынести в Core.

## Критерии готовности
Консоль 0 ошибок и 0 предупреждений (через MCP); все тесты зелёные (старые 97 + новые); из Boot сразу открывается Menu; можно пройти Menu → Settings → назад, Menu → Rules → назад, Settings → Rules → назад; игровой экран открывается через `ScreenManager` (временно по заглушке, например через отладочную кнопку или параметр AppBootstrap). Тег `phase-5a-done`, push ветки и тега.
