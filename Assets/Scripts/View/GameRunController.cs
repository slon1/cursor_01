using Cysharp.Threading.Tasks;
using Game.Core;

namespace Game.View
{
    public sealed class GameRunController
    {
        readonly AppContext _app;

        public GameRunController(AppContext app) => _app = app;

        public void Apply(GameResult result) =>
            RunFlow.Finish(_app.Progress, _app.ProgressStore, result.Won, result.Level, result.Coins, _app.Settings.GoldPointsPerCoin);

        public bool HasNext(int level) => RunFlow.ShowNext(level, _app.Levels.Has(level + 1));

        public UniTask Next(int level) =>
            _app.Screens.ShowAsync(ScreenId.Game, _app.Levels.Get(level), false);

        public UniTask Select()
        {
            _app.Screens.SetReturnTo(ScreenId.Menu);
            return _app.Screens.ShowAsync(ScreenId.SelectLevel, null, false);
        }

        public UniTask Menu()
        {
            _app.Screens.ClearHistory();
            return _app.Screens.ShowAsync(ScreenId.Menu, null, false);
        }
    }
}
