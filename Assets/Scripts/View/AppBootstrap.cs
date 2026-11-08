using Cysharp.Threading.Tasks;
using Game.Config;
using Game.Core;
using UnityEngine;

namespace Game.View
{
    public sealed class AppBootstrap : MonoBehaviour
    {
        [SerializeField] SpriteCatalog catalog;
        [SerializeField] LevelSet levels;
        [SerializeField] GameSettingsConfig settings;
        [SerializeField] UiTheme theme;
        [SerializeField] ScreenManager screens;

        void Start()
        {
            var progress = new PlayerProgress();
            var progressStorage = new PlayerPrefsProgressStorage();
            if (progressStorage.TryLoad(out var level, out var balance))
                progress.Restore(level, balance);

            var app = new AppContext
            {
                Catalog = catalog,
                Levels = levels,
                Settings = settings,
                Theme = theme,
                Progress = progress,
                Options = new SettingsService(new PlayerPrefsSettingsStorage()),
                Screens = screens
            };
            screens.Bind(app);
            screens.ShowAsync(ScreenId.Menu, null, false).Forget();
        }
    }
}
