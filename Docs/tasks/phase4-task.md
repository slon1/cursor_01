# ФАЗА 4. Игровой экран

**Роль:** ты исполнительный программист проекта Unity 6000.5. Архитектор — Claude (проверяет через git, может смотреть картинки из репозитория).
**Ветка:** `phase/4-game-screen` (от `main` после мерджа `phase/3-config`).

**Прочитай перед работой:** `Docs/GAME_RULES.md`, `Docs/adr/ADR-001..003`, `Docs/TECH_DEBT.md`. Посмотри `Assets/Sprites/preview/Game.png` и `Assets/Sprites/Game/Game-bg.png`.

**Цель:** играбельный игровой экран на одном уровне из `LevelSet`. Окна победы, поражения и паузы в этой фазе заглушки.

## Решения архитектора и владельца проекта (не обсуждаются)
1. **Отдельного попапа EXTRACT нет.** Кнопка EXTRACT висит на экране постоянно (как на макете): серая `Btn_Extract_Locked`, пока `ExitReady == false`; золотая `Btn_Extract`, когда `ExitReady == true`. Нажатие на золотую = `TryExtract()` (победа). Нажатие на серую: кнопка трясётся, ничего не меняется. Событие `ExitAttempted` (ход на клетку выхода) даёт пульс кнопки.
2. Счётчик под шкалой: `{CollectedCoins * GoldPointsPerCoin}/{TotalCoins * GoldPointsPerCoin}`, цена монеты 100 очков (по умолчанию).
3. Шкала тревоги заполняется на `Alarm / AlarmThreshold`. Заголовок: `Alarm {Alarm}/{AlarmThreshold}`.
4. Окна победы, поражения и паузы: заглушки (фаза 5).

## ЧАСТЬ A. Game.Core: GameSession (чистый C#, тесты обязательны)
- `GameSession(LevelData level)`: `State` (GameState), `ActiveLion` (по умолчанию A), `Restart()`.
- `IReadOnlyList<MoveTarget> Targets`: цели `RulesEngine.GetTargets` для `ActiveLion`; пусто, если `Status != Playing`.
- `void SelectLion(LionId)`.
- `MoveResult TapCell(CellPos pos)`: если `pos` нет среди `Targets` (по позиции), вернуть `MoveResult` с `Accepted = false`, `Reason = NotAdjacent`, без событий; иначе `RulesEngine.Move(State, ActiveLion, pos)`.
- `bool ExitReady => RulesEngine.ExitConditionsMet(State) && State.Status == Playing`.
- `ExtractResult TryExtract() => RulesEngine.ConfirmExtract(State)`.
- `int TotalCoins` (число Gold на поле при старте, включая скрытые), `int CollectedCoins => State.Gold`.
- Тесты: стартовое состояние; `SelectLion` меняет `Targets`; `TapCell` вне целей ничего не меняет; `TapCell` по цели вызывает ход; `ExitReady` до и после выполнения условий; `TryExtract` в обоих случаях; `Restart` возвращает исходное состояние; `Targets` пусты после Won и Lost.

## ЧАСТЬ B. Настройки и холст
- `GameSettingsConfig`: добавь `int GoldPointsPerCoin = 100`.
- Canvas Scaler: референсное разрешение **1080×2160** (арт сделан под него; раньше стояло 1080×1920 по ошибке архитектора), Match 0.5, Screen Space - Camera.
- Компонент `StageFitter`: дочерний RectTransform 1080×2160 (**Stage**), масштабируется так, чтобы целиком вписаться в экран (contain), по центру. Вне Stage (поля при другом соотношении сторон) заливка цветом (0, 14, 25). Всё содержимое игрового экрана, включая `Bg_Game`, лежит ВНУТРИ Stage, чтобы рамка поля совпадала с клетками.

## ЧАСТЬ C. View (Game.View)
Тонкие MonoBehaviour, правила только в `GameSession`/`RulesEngine`. Все спрайты берутся ТОЛЬКО из `SpriteCatalog` по `SpriteId`, прямых ссылок на спрайты нет.

**GameScreen.prefab** (в `Assets/Prefabs/Screens/`), корень `GameScreenPresenter` с методом `Show(LevelConfig level, SpriteCatalog catalog, GameSettingsConfig settings)`.

Размещение внутри Stage (координаты Stage, начало слева сверху). Сверяй наложением `Sprites/preview/Game.png`: добавь в префаб **выключенный** объект `ReferenceOverlay` с этой картинкой и прозрачностью 0.4; он не должен участвовать в логике и билде.
- `Bg_Game` на весь Stage.
- Заголовок: `Alarm_Frame` (630×210) по центру сверху; поверх TMP-текст `Alarm {Alarm}/{AlarmThreshold}` (кремовый `#F3E5B5`, жирный, по центру рамки). Справа `Btn_Pause` (145×134); нажатие пока пишет в лог.
- Шкала: `Alarm_BarFrame` (929×207) по центру под заголовком; внутри `Alarm_BarFill` (929×68) как Image Filled / Horizontal, `fillAmount = Alarm / AlarmThreshold` (анимация DOTween). Под шкалой в рамке TMP-текст счётчика золота (см. решение 2).
- Поле: квадрат ориентировочно 940×940 с центром примерно (540, 1010) в рамке из фона. Размер клетки = `min(176, (внутренний размер - (N-1)*отступ)/N)`, отступ 8. Поддержка любого N×M (не только 5×5). Клетки создаются в рантайме из `GameState`: `CellView` = Image основы (`Cell_Closed` или `Cell_Open`) + Image предмета (`Item_*`, только на открытой клетке; на закрытой скрыт).
- Жетоны львов: `LionA_Token`, `LionB_Token` (Image поверх клеток, масштаб cellSize/176 от родных размеров).
- Подсветка: `Hl_Step` на целях типа Step, `Hl_Jump` на целях типа Jump; пул Image, `raycastTarget = false`; обновляется после каждого хода и смены льва.
- Выбор льва: два Button ниже поля, A слева (`LionA_Icon_Active/Inactive`), B справа (`LionB_Icon_Active/Inactive`); у активного Active-спрайт, у другого Inactive. Ориентировочные центры (404, 1716) и (676, 1716).
- Кнопка EXTRACT внизу, центр примерно (540, 1976), поведение по решению 1.

**Ввод:** тап по `CellView` вызывает `GameSession.TapCell`, только если ввод не заблокирован анимацией. Используй `IPointerClickHandler`/`Button` (Input System UI Module уже стоит).

**Воспроизведение событий** (`EventPlayer`, UniTask + DOTween, длительности из `GameSettingsConfig`, `CancellationToken` привязан к уничтожению объекта): после `TapCell` или `TryExtract` показать события последовательно, ввод заблокирован до конца. Источник правды для картинки состояния — `GameState`: после проигрывания событий вызвать `Refresh(GameState)` (синхронизация клеток, жетонов, шкалы, счётчика, подсветки, кнопки EXTRACT), так ошибки анимации не накапливаются.
- `LionMoved`: жетон плавно к клетке; при `isJump` дугой с лёгким увеличением. `LionsSwapped`: оба жетона меняются местами (используй `Initiator` и `IsJump`).
- `CellOpened`: клетка меняет спрайт на открытый (быстрое схлопывание по X и обратно), затем появляется предмет (fade in).
- `TrapTriggered`: жетон трясётся и вспышка красным. `GateBlocked`: жетон трясётся. `GateBroken`: решётка исчезает (fade/scale). `GoldCollected`: монета затухает вверх, счётчик обновляется. `LeverUsed`: рубильник исчезает, шкала до нуля. `AlarmChanged`: шкала и текст плавно.
- `ChestOpened`: пульс сундука. `ExitAttempted`: пульс кнопки EXTRACT.
- `GameWon` / `GameLost`: заглушка. Затемнение и по центру Stage TMP-текст "WIN" / "LOSE" с подсказкой "tap to restart"; тап = `GameSession.Restart()` и пересборка экрана. Настоящие попапы в фазе 5.

**Шрифт:** пока стандартный TMP; замена на засечочный шрифт в `TECH_DEBT.md` (файл `.ttf` предоставит владелец проекта).

## ЧАСТЬ D. Тестовая сцена
- `Assets/Scenes/GameDev.unity`: Canvas (Screen Space - Camera, как в Boot) + `GameScreen.prefab` + `GameBootstrap` (поля: `LevelSet`, `SpriteCatalog`, `GameSettingsConfig`, `int levelNumber = 1`), который вызывает `Show`. Добавь сцену в Build Settings после Boot. Boot не менять.

## ЧАСТЬ E. Документы и проверка
- `Docs/adr/ADR-004.md`: архитектура View (GameSession в Core, тонкие MonoBehaviour, Refresh как источник правды, Stage с contain-масштабом, референс 1080×2160, постоянная кнопка EXTRACT вместо попапа).
- `Docs/TECH_DEBT.md`: шрифт; заглушки Pause/Win/Lose (фаза 5); удалить `AssemblyMarker` в Game.View, раз появился код; `LevelSolver.MinAlarmAtWin` показывает тревогу при победе за минимум ходов, а не минимальную возможную (позже добавить `MinPeakAlarm` и кнопку проверки уровней в редакторе, фаза 6).
- Отчёт `Docs/reports/phase4.md`.
- По возможности (через MCP/Game view) сохрани скриншоты в `Docs/reports/phase4/`: уровень 1 на старте; после открытия клеток и сбора золота; золотая EXTRACT; серая EXTRACT; Game view в соотношениях 9:16 и 9:20. Если скриншоты сделать нельзя, напиши об этом в отчёте.

## Запрещено
Менять правила ядра и ADR-002; менять карты уровней; прямые ссылки на спрайты вне `SpriteCatalog`; логика правил во View; попап EXTRACT.

## Критерии готовности
- Консоль 0 ошибок и 0 предупреждений (через MCP).
- Все тесты зелёные (старые 89 + новые).
- Уровень 1 проходится от начала до победы мышью в редакторе.
- Тег `phase-4-done`, push ветки и тега.
