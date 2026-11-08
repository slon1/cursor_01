namespace Game.Core
{
    public sealed class PlayerProgress
    {
        public int HighestUnlockedLevel { get; private set; } = 1;
        public int BalancePoints { get; private set; }

        public void Restore(int highestUnlockedLevel, int balancePoints)
        {
            HighestUnlockedLevel = highestUnlockedLevel < 1 ? 1 : highestUnlockedLevel;
            BalancePoints = balancePoints < 0 ? 0 : balancePoints;
        }

        public void CompleteLevel(int level, int coins, int pointsPerCoin)
        {
            var next = level + 1;
            if (next > HighestUnlockedLevel)
                HighestUnlockedLevel = next;

            BalancePoints += coins * pointsPerCoin;
        }

        public bool IsUnlocked(int level) => level >= 1 && level <= HighestUnlockedLevel;
    }

    public interface IProgressStorage
    {
        bool TryLoad(out int highestUnlockedLevel, out int balancePoints);
        void Save(int highestUnlockedLevel, int balancePoints);
    }

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

    public sealed class NavigationStack<T>
    {
        readonly System.Collections.Generic.Stack<T> _items = new System.Collections.Generic.Stack<T>();

        public int Count => _items.Count;

        public void Push(T item) => _items.Push(item);

        public T Peek() => _items.Peek();

        public T Pop() => _items.Pop();

        public void Clear() => _items.Clear();
    }
}
