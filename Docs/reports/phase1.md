# Фаза 1

Ветка: `phase/1-sprite-catalog`. База: `main` после мерджа `phase/0-setup` (коммит `1cef1d2`).

## Сделано

- Шаг 0 уже был в `phase-0.1-done` (`b177fd7`): ссылки asmdef, перенос `Docs/tasks/todo-F0.md`, удаление `Docs/gameplay.md`, пункты в `Docs/TECH_DEBT.md`. `GameItemType`, `GameItem` и ассеты `GameItem_*` не менялись.
- `SpriteId` в `Game.Config`: все `sprite_id` манифеста, кроме категорий AppIcon, Preview и LevelButton. Имена совпадают с CSV.
- `SpriteCatalog`: список пар `SpriteId` + `Sprite`, `Get` / `TryGet`, массивы `LevelButtonOpen` и `LevelButtonLocked` по 10 элементов. `GetLevelButton(1, true)` возвращает открытую кнопку уровня 1.
- Меню `Tools/Sprites/Import Manifest`: CSV-парсер с кавычками, пропуск AppIcon и Preview, кнопки уровней в массивы, файлы вне манифеста не сканируются. Повторный запуск оставляет 50 уникальных записей, без дублей.
- Ассет `Assets/ScriptableObjects/SpriteCatalog.asset` заполнен импортом.
- Итог импорта: записано 69 спрайтов (50 в списке каталога и 19 кнопок уровней), пропущено 12 (3 AppIcon и 9 Preview).
- Тесты: `LevelButtons_HaveExpectedSlots` и `GetLevelButton_ReturnsSlotOrThrows` зелёные. `EditModeRunner_IsWired` зелёный. `EverySpriteId_HasSprite` падает на `Menu_Logo`. `Rules_Frame` тоже не назначен.
- Предупреждений компиляции нет. В консоли остаются ошибки импорта по двум файлам с несколькими нарезками (см. вопрос). Поэтому тег `phase-1-done` не ставился.

## Вопросы

- `Menu/logo.png` (строка 31) нарезан на 20 спрайтов `logo_0`…`logo_19`. `Rules/Frame-rules.png` (строка 44) нарезан на 3: `Frame-rules_0`, `Frame-rules_1`, `Frame-rules_2`. По заданию импорт пишет ошибку и не выбирает срез сам. Какой срез класть в каталог, или эти файлы нужно переимпортировать как один Sprite?
- В `Docs/MANIFEST.csv` не менял. Заметки, которые просят проверки: `push_large_icon.png` (описание «adaptive background» против имени push) и пара `Settings/switch-on.png` / `switch-off.png` (в описании они, возможно, перепутаны).
- `Select level/light.png` и `preview/Preload.png` есть на диске и не входят в манифест. Импорт их молча не трогает.
