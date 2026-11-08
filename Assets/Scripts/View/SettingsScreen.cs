using Cysharp.Threading.Tasks;
using Game.Config;
using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class SettingsScreen : MonoBehaviour, IAppScreen
    {
        SettingsService _options;
        SpriteCatalog _catalog;
        Image _music;
        Image _sound;
        Image _vibration;

        public void Open(AppContext app, object args)
        {
            _options = app.Options;
            _catalog = app.Catalog;
            var stage = transform.Find("Stage");
            _music = stage.Find("Switch_Music").GetComponent<Image>();
            _sound = stage.Find("Switch_Sound").GetComponent<Image>();
            _vibration = stage.Find("Switch_Vibration").GetComponent<Image>();

            stage.Find("Switch_Music").GetComponent<Button>().onClick.AddListener(() => _options.SetMusic(!_options.Data.Music));
            stage.Find("Switch_Sound").GetComponent<Button>().onClick.AddListener(() => _options.SetSound(!_options.Data.Sound));
            stage.Find("Switch_Vibration").GetComponent<Button>().onClick.AddListener(() => _options.SetVibration(!_options.Data.Vibration));
            stage.Find("Btn_Settings_Rules").GetComponent<Button>().onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.Rules).Forget());
            stage.Find("Btn_Settings_Back").GetComponent<Button>().onClick.AddListener(() => app.Screens.BackAsync().Forget());

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
            Paint(_music, _options.Data.Music);
            Paint(_sound, _options.Data.Sound);
            Paint(_vibration, _options.Data.Vibration);
        }

        void Paint(Image image, bool on)
        {
            image.sprite = _catalog.Get(on ? SpriteId.Switch_On : SpriteId.Switch_Off);
        }
    }
}
