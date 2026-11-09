using Game.Config;
using Game.Core;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests.EditMode
{
    public class RunFlowTests
    {
        [Test]
        public void Win_SavesProgress_Loss_LeavesIt()
        {
            var progress = new PlayerProgress();
            var storage = new Memory();
            RunFlow.Finish(progress, storage, false, 1, 4, 100);
            Assert.AreEqual(1, progress.HighestUnlockedLevel);
            Assert.AreEqual(0, progress.BalancePoints);
            Assert.IsFalse(storage.Saved);

            RunFlow.Finish(progress, storage, true, 1, 3, 100);
            Assert.AreEqual(2, progress.HighestUnlockedLevel);
            Assert.AreEqual(300, progress.BalancePoints);
            Assert.AreEqual(2, storage.Level);
            Assert.AreEqual(300, storage.Balance);

            RunFlow.Finish(progress, storage, true, 1, 1, 100);
            Assert.AreEqual(2, progress.HighestUnlockedLevel);
            Assert.AreEqual(400, progress.BalancePoints);
        }

        [Test]
        public void ShowNext_OnlyWhenTheNextLevelExists()
        {
            Assert.IsTrue(RunFlow.ShowNext(1, 3));
            Assert.IsTrue(RunFlow.ShowNext(2, 3));
            Assert.IsFalse(RunFlow.ShowNext(3, 3));
        }

        [Test]
        public void Nodes_FollowProgress_AndAuthoredLevels()
        {
            var set = AssetDatabase.LoadAssetAtPath<LevelSet>("Assets/ScriptableObjects/LevelSet.asset");
            var progress = new PlayerProgress();
            Assert.IsTrue(RunFlow.NodeOpen(progress, set.Has(1), 1));
            Assert.IsFalse(RunFlow.NodeOpen(progress, set.Has(2), 2));
            Assert.IsFalse(RunFlow.NodeOpen(progress, set.Has(4), 4));

            progress.CompleteLevel(1, 1, 100);
            progress.CompleteLevel(2, 1, 100);
            Assert.IsTrue(RunFlow.NodeOpen(progress, set.Has(3), 3));
            Assert.IsFalse(RunFlow.NodeOpen(progress, set.Has(4), 4));
        }

        sealed class Memory : IProgressStorage
        {
            public int Level;
            public int Balance;
            public bool Saved;

            public bool TryLoad(out int highestUnlockedLevel, out int balancePoints)
            {
                highestUnlockedLevel = Level;
                balancePoints = Balance;
                return Saved;
            }

            public void Save(int highestUnlockedLevel, int balancePoints)
            {
                Level = highestUnlockedLevel;
                Balance = balancePoints;
                Saved = true;
            }
        }
    }
}
