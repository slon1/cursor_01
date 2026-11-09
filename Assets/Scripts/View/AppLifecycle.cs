using UnityEngine;

namespace Game.View
{
    public sealed class AppLifecycle : MonoBehaviour
    {
        AppContext _app;

        public void Bind(AppContext app) => _app = app;

        void OnApplicationPause(bool paused)
        {
            if (paused)
                Persist();
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused)
                Persist();
        }

        void Persist()
        {
            PlayerPrefs.Save();
            if (_app == null || _app.Screens == null || _app.Screens.Current != ScreenId.Game)
                return;

            var instance = _app.Screens.CurrentInstance;
            if (instance == null)
                return;
            var game = instance.GetComponent<GameScreenPresenter>();
            if (game != null)
                game.SuspendMatch();
        }
    }
}
