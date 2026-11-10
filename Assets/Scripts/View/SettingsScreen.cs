using Cysharp.Threading.Tasks;
using Game.Config;
using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class SettingsScreen : MonoBehaviour, IAppScreen
    {
        [SerializeField] Button musicButton;
        [SerializeField] Button soundButton;
        [SerializeField] Button vibrationButton;
        [SerializeField] Image musicImage;
        [SerializeField] Image soundImage;
        [SerializeField] Image vibrationImage;
        [SerializeField] Button rules;
        [SerializeField] Button back;

        SettingsService _options;
        SpriteCatalog _catalog;

        public void Open(AppContext app, object args)
        {
            _options = app.Options;
            _catalog = app.Catalog;
            musicButton.onClick.AddListener(() => { app.Sfx?.Play(SfxId.Tap); _options.SetMusic(!_options.Data.Music); });
            soundButton.onClick.AddListener(() => ToggleSound(app));
            vibrationButton.onClick.AddListener(() => { app.Sfx?.Play(SfxId.Tap); _options.SetVibration(!_options.Data.Vibration); });
            rules.onClick.AddListener(() => { app.Sfx?.Play(SfxId.Tap); app.Screens.ShowAsync(ScreenId.Rules).Forget(); });
            back.onClick.AddListener(() => { app.Sfx?.Play(SfxId.Tap); app.Screens.BackAsync().Forget(); });
            _options.Changed += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (_options != null)
                _options.Changed -= Refresh;
        }

        void ToggleSound(AppContext app)
        {
            if (_options.Data.Sound)
            {
                app.Sfx?.Play(SfxId.Tap);
                _options.SetSound(false);
                return;
            }

            _options.SetSound(true);
            app.Sfx?.Play(SfxId.Tap);
        }

        void Refresh()
        {
            Paint(musicImage, _options.Data.Music);
            Paint(soundImage, _options.Data.Sound);
            Paint(vibrationImage, _options.Data.Vibration);
        }

        void Paint(Image image, bool on)
        {
            image.sprite = _catalog.Get(on ? SpriteId.Switch_On : SpriteId.Switch_Off);
        }
    }
}
