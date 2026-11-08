Роль: ты исполнительный программист. Архитектор — Claude. Перед работой прочитай docs/GAME_RULES.md и Assets/Sprites/MANIFEST.csv.
ФАЗА 0. Основа проекта. Ветка: phase/0-setup.
Цель: чистая основа проекта, в которой ничего не сломается при дальнейших фазах.
Сделай:

1. Структура папок в Assets/: Scripts/Core, Scripts/View, Scripts/Config, Scripts/Editor, Tests/EditMode, ScriptableObjects, Prefabs, Scenes.
2. Assembly definitions:
   - Game.Core (БЕЗ ссылок на UnityEngine: noEngineReferences = true),
   - Game.View (ссылается на Game.Core, UniTask, DOTween),
   - Game.Config (SO; ссылается на Game.Core),
   - Game.Editor (только Editor),
   - Game.Tests.EditMode (NUnit, ссылается на Game.Core).
3. Проверь, что UniTask и DOTween подключены и компилируются.
4. Player Settings: ориентация только Portrait, референсное разрешение Canvas 1080x1920, Canvas Scaler Scale With Screen Size, Match 0.5.
5. Сцена Boot (пустая, с камерой). Canvas в режиме Screen Space - Camera.
6. Для спрайтов: импорт-настройки Sprite (2D and UI), без mipmaps.
7. Каталог документов: docs/adr/ (шаблон ADR-000.md), docs/TECH_DEBT.md, docs/reports/.
8. .gitignore для Unity, первый коммит и тег phase-0-done.
   Запрещено: писать игровую логику, трогать содержимое MANIFEST.csv, менять правила.
   Критерии готовности: проект компилируется без ошибок и предупреждений (проверь через MCP), asmdef настроены, сцена Boot открывается.
   В конце: docs/reports/phase0.md — что сделано, что не получилось, вопросы ко мне.
