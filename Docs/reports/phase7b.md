# Фаза 7B. Экран общей клетки

Жетоны в общей клетке стоят рядом: `TokenSlots` плюс `SharedTokenScale` / `SharedTokenOffset` в настройках. Во время анимации слоты считаются из визуальных позиций `EventPlayer`, а не из конечного `GameSession`. Пульс EXTRACT — когда `ExitReady` стал истиной после хода; в `SfxId` нет звука готовности, пульс без звука. Серая EXTRACT только трясётся.

## Скриншоты

Game view 1080×2160, `Docs/reports/phase7b/`:

| Файл | Что видно |
| --- | --- |
| `shared-cell-5x5.png` | оба жетона рядом в одной пустой клетке 5×5 |
| `shared-cell-6x6.png` | то же на 6×6, жетоны внутри клетки |
| `chest-both.png` | оба на клетке сундука, сундук ещё виден |
| `exit-both-ready.png` | оба на выходе, золотая EXTRACT |
| `exit-both-notready.png` | оба на выходе без сундука, серая EXTRACT |
| `highlight-partner.png` | подсветка клетки партнёра |

`RevealPause` и `ConsumePause` перед коммитом снова 0.45 и 0.2.

## Play Mode

GameDev, уровень 1: оба льва на клетке сундука, затем оба на выходе, EXTRACT. Победа. Консоль: 0 ошибок, 0 предупреждений.

## Проверки

EditMode зелёные. Правила ядра и карты не менялись.

## Подсказка для экрана правил

«Take the chest: bring both lions onto the chest tile. Escape: after taking the chest and gathering the minimum loot, bring both lions onto the exit tile and tap EXTRACT.»

## Побочные незакоммиченные правки редактора

Не входят в 7B: `Assets/Fonts/Cinzel-Regular SDF.asset`, `Assets/Settings/Build Profiles/Android™.asset`, `Assets/Settings/DefaultVolumeProfile.asset`, `Assets/Settings/UniversalRP.asset`, `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/ProjectAuditorSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/UnityConnectSettings.asset`. Это локальный мусор редактора/сборки; в 7B не нужны. Часть Android/ProjectSettings может относиться к 6C и уйдёт в `phase/6c-android`, если владелец решит их туда занести. `UnityConnectSettings` (`m_Enabled`) не коммитить.
