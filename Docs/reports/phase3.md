# Фаза 3

Ветка: `phase/3-config`. База: `main` после fast-forward `phase/2-core` (`27503bb`).

## Сделано

- `LevelMapParser` читает карту в `LevelData`. `A` и `B` — старты львов на открытой пустой клетке. `LevelBuilder` берёт клетки из `ParseCell`, свой алфавит удалён.
- `GameState.Clone` копирует поле, львов, флаг сундука, тревогу, золото, статус и стоимости.
- `LevelSolver` ищет кратчайшую победу через публичный `RulesEngine`. При равном числе ходов оставляет меньшую тревогу. Решения записаны в `Docs/adr/ADR-003.md`.
- `LevelConfig`, `LevelSet`, `GameSettingsConfig`. Ассеты: `Assets/ScriptableObjects/Levels/Level_01.asset`, `Level_02.asset`, `Level_03.asset`, `Assets/ScriptableObjects/LevelSet.asset`, `Assets/ScriptableObjects/GameSettingsConfig.asset`. Карты не менялись.
- EditMode: 89 тестов зелёные, включая старые. Консоль Unity: 0 ошибок и 0 предупреждений.
- Новый техдолг не появился, `Docs/TECH_DEBT.md` оставлен как был.

## Проходимость

`MinMoves` — число ходов `Move` до `ExitAttempted(true)` включительно. EXTRACT после этого отдельным ходом не считается.

| Уровень | MinMoves | MinAlarmAtWin | Состояний |
| --- | --- | --- | --- |
| 1 | 8 | 4 | 6570 |
| 2 | 10 | 7 | 7900 |
| 3 | 7 | 4 | 4665 |

Все три уровня проходимы, поиск не упёрся в лимит. Порог уровня 2 равен 8, найденная победа заканчивается тревогой 7.

## Вопрос

Блокирующих вопросов нет. Позже имеет смысл кнопка редактора, которая гоняет `LevelSolver` по `LevelSet` сразу при правке карты, не дожидаясь тестов.
