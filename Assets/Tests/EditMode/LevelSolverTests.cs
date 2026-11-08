using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelSolverTests
    {
        [Test]
        public void Clone_IsIndependent()
        {
            var original = LevelBuilder.Play(".G../....", 0, 0, 3, 1, alarmThreshold: 10, jumpCost: 2);
            var clone = original.Clone();

            RulesEngine.Move(clone, LionId.A, new CellPos(1, 0));

            Assert.AreEqual(0, original.Gold);
            Assert.AreEqual(1, clone.Gold);
            Assert.AreEqual(new CellPos(0, 0), original.LionA);
            Assert.AreEqual(new CellPos(1, 0), clone.LionA);
            Assert.AreEqual(ItemType.Gold, original.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(ItemType.None, clone.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(original.AlarmThreshold, clone.AlarmThreshold);
            Assert.AreEqual(original.JumpCost, clone.JumpCost);
            Assert.AreEqual(original.GateBreakCost, clone.GateBreakCost);
            Assert.AreEqual(original.TrapCost, clone.TrapCost);
            Assert.AreEqual(GameStatus.Playing, original.Status);
        }

        [Test]
        public void SolvableMap_FindsAWin()
        {
            var level = LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4);
            var result = LevelSolver.TrySolve(level);

            Assert.IsTrue(result.Solvable);
            Assert.IsFalse(result.Truncated);
            Assert.Greater(result.MinMoves, 0);
            Assert.GreaterOrEqual(result.MinAlarmAtWin, 0);
            Assert.Greater(result.StatesExplored, 1);
        }

        [Test]
        public void UnreachableChestAndExit_AreNotSolvable()
        {
            var level = LevelMapParser.Parse("A.B/RRR/RRR/RRR/.CE", 5, 2, 3, 4);
            var result = LevelSolver.TrySolve(level);

            Assert.IsFalse(result.Solvable);
            Assert.IsFalse(result.Truncated);
            Assert.AreEqual(0, result.MinMoves);
            Assert.AreEqual(0, result.MinAlarmAtWin);
        }

        [Test]
        public void LowStateCap_Truncates()
        {
            var level = LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4);
            var result = LevelSolver.TrySolve(level, 1);

            Assert.IsTrue(result.Truncated);
            Assert.IsFalse(result.Solvable);
            Assert.AreEqual(1, result.StatesExplored);
        }
    }
}
