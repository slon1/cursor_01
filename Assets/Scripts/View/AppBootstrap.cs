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
        [SerializeField] SoundManager sfx;

        void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

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
                ProgressStore = progressStorage,
                Options = new SettingsService(new PlayerPrefsSettingsStorage()),
                Screens = screens,
                Sfx = sfx
            };
            if (sfx != null)
                sfx.Bind(app.Options);
            app.Run = new GameRunController(app);
            screens.Bind(app);
            var back = GetComponent<BackHandler>();
            if (back == null)
                back = gameObject.AddComponent<BackHandler>();
            back.Bind(app);
            var lifecycle = GetComponent<AppLifecycle>();
            if (lifecycle == null)
                lifecycle = gameObject.AddComponent<AppLifecycle>();
            lifecycle.Bind(app);
            screens.ShowAsync(ScreenId.Menu, null, false).Forget();
        }
    }
}
