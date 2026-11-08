using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ProgressTests
    {
        [Test]
        public void CompleteLevel_UnlocksTheNext_AndAddsBalance()
        {
            var progress = new PlayerProgress();
            progress.CompleteLevel(1, 2, 100);

            Assert.AreEqual(2, progress.HighestUnlockedLevel);
            Assert.AreEqual(200, progress.BalancePoints);
            Assert.IsTrue(progress.IsUnlocked(2));
            Assert.IsFalse(progress.IsUnlocked(3));

            progress.CompleteLevel(1, 1, 100);
            Assert.AreEqual(2, progress.HighestUnlockedLevel);
            Assert.AreEqual(300, progress.BalancePoints);
        }

        [Test]
        public void Storage_RoundTrip_KeepsProgress()
        {
            var storage = new MemoryProgress();
            var progress = new PlayerProgress();
            progress.CompleteLevel(2, 3, 100);
            storage.Save(progress.HighestUnlockedLevel, progress.BalancePoints);

            var loaded = new PlayerProgress();
            Assert.IsTrue(storage.TryLoad(out var level, out var balance));
            loaded.Restore(level, balance);
            Assert.AreEqual(3, loaded.HighestUnlockedLevel);
            Assert.AreEqual(300, loaded.BalancePoints);
        }

        [Test]
        public void Settings_DefaultOn_AndTogglePersists()
        {
            var storage = new MemorySettings();
            var service = new SettingsService(storage);
            Assert.IsTrue(service.Data.Music);
            Assert.IsTrue(service.Data.Sound);
            Assert.IsTrue(service.Data.Vibration);

            var changes = 0;
            service.Changed += () => changes++;
            service.SetSound(false);

            Assert.IsFalse(service.Data.Sound);
            Assert.AreEqual(1, changes);
            var again = new SettingsService(storage);
            Assert.IsFalse(again.Data.Sound);
            Assert.IsTrue(again.Data.Music);
        }

        [Test]
        public void NavigationStack_PushPopPeekClear()
        {
            var stack = new NavigationStack<int>();
            stack.Push(1);
            stack.Push(2);
            Assert.AreEqual(2, stack.Peek());
            Assert.AreEqual(2, stack.Pop());
            Assert.AreEqual(1, stack.Count);
            stack.Clear();
            Assert.AreEqual(0, stack.Count);
        }

        sealed class MemoryProgress : IProgressStorage
        {
            int _level;
            int _balance;
            bool _saved;

            public bool TryLoad(out int highestUnlockedLevel, out int balancePoints)
            {
                highestUnlockedLevel = _level;
                balancePoints = _balance;
                return _saved;
            }

            public void Save(int highestUnlockedLevel, int balancePoints)
            {
                _level = highestUnlockedLevel;
                _balance = balancePoints;
                _saved = true;
            }
        }

        sealed class MemorySettings : ISettingsStorage
        {
            bool _music = true;
            bool _sound = true;
            bool _vibration = true;
            bool _saved;

            public bool TryLoad(out bool music, out bool sound, out bool vibration)
            {
                music = _music;
                sound = _sound;
                vibration = _vibration;
                return _saved;
            }

            public void Save(bool music, bool sound, bool vibration)
            {
                _music = music;
                _sound = sound;
                _vibration = vibration;
                _saved = true;
            }
        }
    }
}
