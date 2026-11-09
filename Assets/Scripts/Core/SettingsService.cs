namespace Game.Core
{
    public sealed class SettingsService
    {
        readonly ISettingsStorage _storage;

        public SettingsData Data { get; } = new SettingsData();
        public event System.Action Changed;

        public SettingsService(ISettingsStorage storage)
        {
            _storage = storage;
            if (storage != null && storage.TryLoad(out var music, out var sound, out var vibration))
            {
                Data.Music = music;
                Data.Sound = sound;
                Data.Vibration = vibration;
            }
        }

        public void SetMusic(bool value) => Set(value, Data.Sound, Data.Vibration);
        public void SetSound(bool value) => Set(Data.Music, value, Data.Vibration);
        public void SetVibration(bool value) => Set(Data.Music, Data.Sound, value);

        void Set(bool music, bool sound, bool vibration)
        {
            Data.Music = music;
            Data.Sound = sound;
            Data.Vibration = vibration;
            _storage?.Save(music, sound, vibration);
            Changed?.Invoke();
        }
    }
}
