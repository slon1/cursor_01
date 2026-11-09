namespace Game.Core
{
    public sealed class SettingsData
    {
        public bool Music = true;
        public bool Sound = true;
        public bool Vibration = true;
    }

    public interface ISettingsStorage
    {
        bool TryLoad(out bool music, out bool sound, out bool vibration);
        void Save(bool music, bool sound, bool vibration);
    }
}
