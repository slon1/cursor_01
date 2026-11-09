using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.View
{
    public sealed class BackHandler : MonoBehaviour
    {
        AppContext _app;

        public void Bind(AppContext app) => _app = app;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
                return;
            if (_app == null || _app.Screens == null || !_app.Screens.Current.HasValue)
                return;

            var screens = _app.Screens;
            var instance = screens.CurrentInstance;
            var select = instance != null ? instance.GetComponent<SelectLevelScreen>() : null;
            var game = instance != null ? instance.GetComponent<GameScreenPresenter>() : null;
            var command = BackPolicy.Decide(new BackState
            {
                Place = PlaceOf(screens.Current.Value),
                TransitionBusy = screens.IsBusy,
                MoveBusy = game != null && game.MoveBusy,
                PopupOpen = select != null && select.PopupOpen,
                PauseOpen = game != null && game.PauseOpen,
                RoundOver = game != null && game.RoundOver
            });

            switch (command)
            {
                case BackCommand.Quit:
                    Application.Quit();
                    break;
                case BackCommand.GoBack:
                    screens.BackAsync().Forget();
                    break;
                case BackCommand.ClosePopup:
                    select.ClosePopup();
                    break;
                case BackCommand.Resume:
                    game.ResumeFromBack();
                    break;
                case BackCommand.OpenPause:
                    game.OpenPauseFromBack();
                    break;
            }
        }

        static AppPlace PlaceOf(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Settings: return AppPlace.Settings;
                case ScreenId.Rules: return AppPlace.Rules;
                case ScreenId.SelectLevel: return AppPlace.SelectLevel;
                case ScreenId.Game: return AppPlace.Game;
                default: return AppPlace.Menu;
            }
        }
    }
}
