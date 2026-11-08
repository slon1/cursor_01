using Game.Config;
using UnityEngine;

namespace Game.View
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameScreenPresenter screen;
        [SerializeField] LevelSet levels;
        [SerializeField] SpriteCatalog catalog;
        [SerializeField] GameSettingsConfig settings;
        [SerializeField] UiTheme theme;
        [SerializeField] int levelNumber = 1;

        void Start() => screen.Show(levels.Get(levelNumber), catalog, settings, theme);
    }
}
