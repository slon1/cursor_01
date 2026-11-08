using System;
using Cysharp.Threading.Tasks;
using Game.Config;
using Game.Core;
using UnityEngine;

namespace Game.View
{
    public sealed class ScreenManager : MonoBehaviour
    {
        [Serializable]
        public struct Binding
        {
            public ScreenId Id;
            public GameObject Prefab;
        }

        [SerializeField] CanvasGroup fade;
        [SerializeField] GameSettingsConfig settings;
        [SerializeField] Binding[] screens;

        readonly NavigationStack<ScreenId> _back = new NavigationStack<ScreenId>();
        ScreenId? _current;
        GameObject _instance;
        AppContext _app;
        bool _busy;

        public void Bind(AppContext app)
        {
            _app = app;
            if (app.Settings != null)
                settings = app.Settings;
        }

        public async UniTask ShowAsync(ScreenId id, object args = null, bool pushToHistory = true)
        {
            if (_busy)
                return;

            _busy = true;
            try
            {
                var first = !_current.HasValue;
                if (!first)
                    await Fade(1f);

                if (pushToHistory && _current.HasValue)
                    _back.Push(_current.Value);

                Place(id, args);

                if (first)
                {
                    fade.alpha = 0f;
                    fade.blocksRaycasts = false;
                }
                else
                {
                    await Fade(0f);
                }
            }
            finally
            {
                _busy = false;
            }
        }

        public async UniTask BackAsync()
        {
            if (_busy || _back.Count == 0)
                return;

            var previous = _back.Pop();
            await ShowAsync(previous, null, false);
        }

        void Place(ScreenId id, object args)
        {
            if (_instance != null)
                Destroy(_instance);

            GameObject prefab = null;
            for (var i = 0; i < screens.Length; i++)
            {
                if (screens[i].Id == id)
                {
                    prefab = screens[i].Prefab;
                    break;
                }
            }

            _instance = Instantiate(prefab, transform);
            _current = id;
            var views = _instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < views.Length; i++)
            {
                if (views[i] is IAppScreen screen)
                    screen.Open(_app, args);
            }

            fade.transform.SetAsLastSibling();
        }

        async UniTask Fade(float target)
        {
            fade.blocksRaycasts = true;
            var start = fade.alpha;
            var duration = settings != null ? settings.FadeDuration : 0.3f;
            var time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                fade.alpha = Mathf.Lerp(start, target, duration <= 0f ? 1f : time / duration);
                await UniTask.Yield(destroyCancellationToken);
            }

            fade.alpha = target;
            fade.blocksRaycasts = target > 0.01f;
        }
    }
}
