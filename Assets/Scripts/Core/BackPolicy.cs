namespace Game.Core
{
    public enum AppPlace
    {
        Menu,
        Settings,
        Rules,
        SelectLevel,
        Game
    }

    public enum BackCommand
    {
        Ignore,
        Quit,
        GoBack,
        ClosePopup,
        Resume,
        OpenPause
    }

    public readonly struct BackState
    {
        public AppPlace Place { get; init; }
        public bool TransitionBusy { get; init; }
        public bool MoveBusy { get; init; }
        public bool PopupOpen { get; init; }
        public bool PauseOpen { get; init; }
        public bool RoundOver { get; init; }
    }

    public static class BackPolicy
    {
        public static BackCommand Decide(BackState state)
        {
            if (state.TransitionBusy || state.MoveBusy)
                return BackCommand.Ignore;

            switch (state.Place)
            {
                case AppPlace.Menu:
                    return BackCommand.Quit;
                case AppPlace.Settings:
                case AppPlace.Rules:
                    return BackCommand.GoBack;
                case AppPlace.SelectLevel:
                    return state.PopupOpen ? BackCommand.ClosePopup : BackCommand.GoBack;
                case AppPlace.Game:
                    if (state.RoundOver)
                        return BackCommand.Ignore;
                    return state.PauseOpen ? BackCommand.Resume : BackCommand.OpenPause;
                default:
                    return BackCommand.Ignore;
            }
        }
    }
}
