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
            musicButton.onClick.AddListener(() => _options.SetMusic(!_options.Data.Music));
            soundButton.onClick.AddListener(() => _options.SetSound(!_options.Data.Sound));
            vibrationButton.onClick.AddListener(() => _options.SetVibration(!_options.Data.Vibration));
            rules.onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.Rules).Forget());
            back.onClick.AddListener(() => app.Screens.BackAsync().Forget());
            _options.Changed += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (_options != null)
                _options.Changed -= Refresh;
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
