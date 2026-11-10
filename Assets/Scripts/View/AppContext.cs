using Game.Config;
using Game.Core;

namespace Game.View
{
    public sealed class AppContext
    {
        public SpriteCatalog Catalog;
        public LevelSet Levels;
        public GameSettingsConfig Settings;
        public UiTheme Theme;
        public PlayerProgress Progress;
        public IProgressStorage ProgressStore;
        public GameRunController Run;
        public SettingsService Options;
        public ScreenManager Screens;
        public SoundManager Sfx;
    }

    public interface IAppScreen
    {
        void Open(AppContext app, object args);
    }

    public enum ScreenId
    {
        Menu,
        Settings,
        Rules,
        SelectLevel,
        Game
    }
}
