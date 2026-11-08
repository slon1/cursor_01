using Game.Core;
using UnityEngine;

namespace Game.View
{
    public sealed class PlayerPrefsProgressStorage : IProgressStorage
    {
        const string LevelKey = "mk.progress.level";
        const string BalanceKey = "mk.progress.balance";

        public bool TryLoad(out int highestUnlockedLevel, out int balancePoints)
        {
            highestUnlockedLevel = PlayerPrefs.GetInt(LevelKey, 1);
            balancePoints = PlayerPrefs.GetInt(BalanceKey, 0);
            return PlayerPrefs.HasKey(LevelKey);
        }

        public void Save(int highestUnlockedLevel, int balancePoints)
        {
            PlayerPrefs.SetInt(LevelKey, highestUnlockedLevel);
            PlayerPrefs.SetInt(BalanceKey, balancePoints);
            PlayerPrefs.Save();
        }
    }

    public sealed class PlayerPrefsSettingsStorage : ISettingsStorage
    {
        const string MusicKey = "mk.settings.music";
        const string SoundKey = "mk.settings.sound";
        const string VibrationKey = "mk.settings.vibration";

        public bool TryLoad(out bool music, out bool sound, out bool vibration)
        {
            music = PlayerPrefs.GetInt(MusicKey, 1) != 0;
            sound = PlayerPrefs.GetInt(SoundKey, 1) != 0;
            vibration = PlayerPrefs.GetInt(VibrationKey, 1) != 0;
            return PlayerPrefs.HasKey(MusicKey);
        }

        public void Save(bool music, bool sound, bool vibration)
        {
            PlayerPrefs.SetInt(MusicKey, music ? 1 : 0);
            PlayerPrefs.SetInt(SoundKey, sound ? 1 : 0);
            PlayerPrefs.SetInt(VibrationKey, vibration ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
