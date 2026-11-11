using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class MinCoinsTests
    {
        [Test]
        public void Validate_RejectsNegativeMinCoins_AndMinAboveTheBoard()
        {
            var negative = LevelMapParser.Parse("A.g/B.C/.E.", 10, 2, 3, 4, -1);
            Assert.That(negative.Validate(), Does.Contain("Минимальная добыча отрицательная."));

            var tooHigh = LevelMapParser.Parse("A.g/B.C/.E.", 10, 2, 3, 4, 2);
            Assert.That(tooHigh.Validate(), Does.Contain("Минимальная добыча больше числа монет на поле."));
        }

        [Test]
        public void Exit_StaysClosed_UntilGoldReachesMinCoins()
        {
            var session = new GameSession(LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4, 1));
            session.TapCell(new CellPos(0, 1));
            session.TapCell(new CellPos(1, 1));
            session.SelectLion(LionId.B);
            session.TapCell(new CellPos(2, 1));
            session.TapCell(new CellPos(1, 1));
            session.SelectLion(LionId.A);
            session.TapCell(new CellPos(1, 2));
            session.SelectLion(LionId.B);
            session.TapCell(new CellPos(1, 2));

            Assert.IsFalse(session.ExitReady);
            Assert.IsTrue(session.State.LionA == new CellPos(1, 2) && session.State.LionB == new CellPos(1, 2));
            Assert.IsFalse(session.TryExtract().Accepted);

            var ready = new GameSession(LevelMapParser.Parse("A.B/gC./.E.", 10, 2, 3, 4, 1));
            ready.TapCell(new CellPos(0, 1));
            ready.TapCell(new CellPos(1, 1));
            ready.SelectLion(LionId.B);
            ready.TapCell(new CellPos(2, 1));
            ready.TapCell(new CellPos(1, 1));
            ready.SelectLion(LionId.A);
            ready.TapCell(new CellPos(1, 2));
            ready.SelectLion(LionId.B);
            ready.TapCell(new CellPos(1, 2));

            Assert.AreEqual(1, ready.CollectedCoins);
            Assert.IsTrue(ready.ExitReady);
        }

        [Test]
        public void Solver_MapWithTooLittleGold_IsNotSolvable()
        {
            var level = LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4, 1);
            var result = LevelSolver.TrySolve(level, 20000);

            Assert.IsFalse(result.Solvable);
            Assert.IsFalse(result.Truncated);
        }
    }
}
