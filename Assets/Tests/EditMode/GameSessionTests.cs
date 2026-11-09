using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class GameSessionTests
    {
        [Test]
        public void Start_SelectsLionA_AndCountsHiddenGold()
        {
            var session = new GameSession(LevelMapParser.Parse("A.g/B.C/.E.", 10, 2, 3, 4));

            Assert.AreEqual(LionId.A, session.ActiveLion);
            Assert.AreEqual(GameStatus.Playing, session.State.Status);
            Assert.AreEqual(new CellPos(0, 0), session.State.LionA);
            Assert.AreEqual(1, session.TotalCoins);
            Assert.AreEqual(0, session.CollectedCoins);
            Assert.IsFalse(session.ExitReady);
            Assert.IsNotEmpty(session.Targets);
        }

        [Test]
        public void SelectLion_ChangesTargets()
        {
            var session = new GameSession(LevelMapParser.Parse("A.../B.C./.E..", 10, 2, 3, 4));
            session.SelectLion(LionId.B);
            Assert.IsFalse(HasJump(session.Targets));

            session.SelectLion(LionId.A);
            Assert.IsTrue(HasJump(session.Targets));
        }

        [Test]
        public void TapCell_OutsideTargets_ChangesNothing()
        {
            var session = new GameSession(LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4));
            var result = session.TapCell(new CellPos(2, 2));

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(RejectReason.NotAdjacent, result.Reason);
            Assert.AreEqual(0, result.Events.Count);
            Assert.AreEqual(new CellPos(0, 0), session.State.LionA);
            Assert.AreEqual(0, session.State.Alarm);
        }

        [Test]
        public void TapCell_OnTarget_Moves()
        {
            var session = new GameSession(LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4));
            var result = session.TapCell(new CellPos(0, 1));

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(0, 1), session.State.LionA);
        }

        [Test]
        public void ExitReady_AndExtract_FollowTheRules()
        {
            var session = new GameSession(LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4));
            Assert.IsFalse(session.ExitReady);
            Assert.IsFalse(session.TryExtract().Accepted);

            session.TapCell(new CellPos(0, 1));
            session.SelectLion(LionId.B);
            session.TapCell(new CellPos(2, 1));
            session.SelectLion(LionId.A);
            session.TapCell(new CellPos(1, 1));
            session.SelectLion(LionId.B);
            session.TapCell(new CellPos(2, 2));

            Assert.IsTrue(session.ExitReady);
            var extract = session.TryExtract();
            Assert.IsTrue(extract.Accepted);
            Assert.AreEqual(GameStatus.Won, session.State.Status);
            Assert.IsEmpty(session.Targets);
        }

        [Test]
        public void Restart_RestoresTheLevel()
        {
            var session = new GameSession(LevelMapParser.Parse("Ag./B.C/.E.", 10, 2, 3, 4));
            session.TapCell(new CellPos(1, 0));
            session.SelectLion(LionId.B);
            Assert.AreEqual(1, session.CollectedCoins);

            session.Restart();

            Assert.AreEqual(LionId.A, session.ActiveLion);
            Assert.AreEqual(new CellPos(0, 0), session.State.LionA);
            Assert.AreEqual(0, session.CollectedCoins);
            Assert.AreEqual(ItemType.Gold, session.State.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(GameStatus.Playing, session.State.Status);
        }

        [Test]
        public void Targets_AreEmptyAfterLoss()
        {
            var session = new GameSession(LevelMapParser.Parse("AT./B.C/.E.", 1, 2, 3, 1));
            var result = session.TapCell(new CellPos(1, 0));

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(GameStatus.Lost, session.State.Status);
            Assert.IsEmpty(session.Targets);
            Assert.IsFalse(session.TryExtract().Accepted);
        }

        static bool HasJump(System.Collections.Generic.IReadOnlyList<MoveTarget> targets)
        {
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].Kind == MoveKind.Jump)
                    return true;
            }

            return false;
        }
    }
}
