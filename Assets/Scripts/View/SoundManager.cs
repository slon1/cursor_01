using Game.Config;
using Game.Core;
using UnityEngine;

namespace Game.View
{
    public sealed class SoundManager : MonoBehaviour
    {
        [SerializeField] SfxCatalog catalog;
        [SerializeField] AudioClip music;

        AudioSource _effects;
        AudioSource _music;
        SettingsService _options;

        public void Bind(SettingsService options)
        {
            if (ReferenceEquals(_options, options))
            {
                ApplyMusic();
                return;
            }

            if (_options != null)
                _options.Changed -= ApplyMusic;
            _options = options;
            if (_options != null)
                _options.Changed += ApplyMusic;
            ApplyMusic();
        }

        void Awake()
        {
            EnsureSources();
            _options = new SettingsService(new PlayerPrefsSettingsStorage());
            _options.Changed += ApplyMusic;
        }

        void Start() => ApplyMusic();

        void EnsureSources()
        {
            var sources = GetComponents<AudioSource>();
            _effects = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
            _music = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
            _effects.playOnAwake = false;
            _effects.spatialBlend = 0f;
            _music.playOnAwake = false;
            _music.spatialBlend = 0f;
            _music.loop = true;
            _music.clip = music;
        }

        void OnDestroy()
        {
            if (_options != null)
                _options.Changed -= ApplyMusic;
        }

        void ApplyMusic()
        {
            if (_music == null || _music.clip == null)
                return;

            if (_options == null || _options.Data.Music)
            {
                if (!_music.isPlaying)
                    _music.Play();
                return;
            }

            if (_music.isPlaying)
                _music.Stop();
        }

        public void Play(SfxId id)
        {
            if (catalog == null || _effects == null)
                return;
            if (_options != null && !_options.Data.Sound)
                return;

            var clip = catalog.Get(id);
            if (clip != null)
                _effects.PlayOneShot(clip);
        }
    }
}
