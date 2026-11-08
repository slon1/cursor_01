# Фаза 0

Дата: 2026-11-08. Ветка: `phase/0-setup`. Тег: `phase-0-done`.

## Сделано

- Папки: `Assets/Scripts/Core`, `View`, `Config`, `Editor`, `Assets/Tests/EditMode`, `Assets/ScriptableObjects`. `Assets/Prefabs` и `Assets/Scenes` уже были.
- Сборки: `Game.Core` (`noEngineReferences`), `Game.View` (Core, UniTask, DOTween.Modules), `Game.Config` (Core), `Game.Editor` (только Editor), `Game.Tests.EditMode` (NUnit, Core, платформа Editor). `autoReferenced` выключен.
- `GameItemType` перенесён в Core, `GameItem` в Config. GUID скриптов сохранены, порядок значений enum не менялся. Добавлены пространства имён `Game.Core` и `Game.Config`.
- В каждой сборке есть маркер, чтобы Unity её компилировала. В EditMode один тест `EditModeRunner_IsWired`: прошёл.
- Скомпилированы: Game.Core, Game.View, Game.Config, Game.Editor, Game.Tests.EditMode, UniTask, UniTask.DOTween, DOTween.Modules.
- Ориентация: только Portrait. Переворот и оба ландшафта выключены.
- Сцена `Assets/Scenes/Boot.unity` открывается. В ней камера, Canvas (Screen Space - Camera, 1080×1920, Scale With Screen Size, Match 0.5) и EventSystem с Input System. Boot — единственная сцена в билде. `SampleScene` остаётся на диске.
- Спрайты не переимпортировались: уже Sprite (2D and UI), mipmaps выключены, нарезка Multiple сохранена.
- `Docs/adr/ADR-000.md`, `Docs/TECH_DEBT.md`, этот отчёт. `.gitignore` для Unity.
- Консоль после обновления ассетов и теста: 0 ошибок, 0 предупреждений.

## Не получилось / не делалось

- `Assets/Sprites/MANIFEST.csv` в проекте нет. Каталог лежит в `Docs/MANIFEST.csv`. Содержимое не менял.
- `Game.Editor` без ссылок на другие сборки: в фазе 0 редакторских инструментов нет.
- Во время импорта asmdef, пока рядом ещё не было скриптов, консоль кратко показала три предупреждения «assembly will not be compiled, because it has no scripts». После появления скриптов они исчезли и при повторной проверке не вернулись.

## Вопросы

- `Game.View` не ссылается на `Game.Config`. Когда вью начнёт читать конфиги, ссылку нужно будет добавить. Так и задумано?
- В Boot камера ортографическая, size 5, сплошной фон, plane distance 10. В задании кадр камеры не задан.
- EventSystem добавлен, потому что в проекте включён только новый Input System. В тексте фазы его не было.
- `user.email` в git config пустой. Если коммит из-за этого не создастся, тег тоже не будет поставлен, пока не появится почта. Git config я не менял.
