using System.Linq;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class RulesEngineTests
    {
        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoOtherLion_SharesTheCell(LionId lion)
        {
            var state = lion == LionId.A
                ? LevelBuilder.Play("..../....", 0, 0, 1, 0)
                : LevelBuilder.Play("..../....", 1, 0, 0, 0);
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(1, 0), state.LionA);
            Assert.AreEqual(new CellPos(1, 0), state.LionB);
            var moved = (LionMoved)result.Events.Single();
            Assert.AreEqual(lion, moved.Lion);
            Assert.IsFalse(moved.IsJump);
            Assert.AreEqual(0, state.Alarm);
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoEmpty_Occupies(LionId lion)
        {
            var state = ActorAtOrigin(lion, "..../....");
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
            Assert.AreEqual(0, state.Alarm);
            var moved = (LionMoved)result.Events.Single();
            Assert.IsFalse(moved.IsJump);
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoTrap_StaysAndKeepsTrap(LionId lion)
        {
            var state = ActorAtOrigin(lion, ".T../....");
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(0, 0), state.Position(lion));
            Assert.AreEqual(ItemType.Trap, state.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(4, state.Alarm);
            Assert.IsInstanceOf<TrapTriggered>(result.Events[0]);
        }

        [Test]
        public void Step_GateBlocksLionA()
        {
            var state = ActorAtOrigin(LionId.A, ".R../....");
            var result = Move(state, LionId.A, 1, 0);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(RejectReason.GateBlocksLionA, result.Reason);
            Assert.AreEqual(new CellPos(0, 0), state.LionA);
            Assert.AreEqual(ItemType.Gate, state.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(0, state.Alarm);
            Assert.IsTrue(state.CellAt(new CellPos(1, 0)).IsOpen);
            Assert.AreEqual(0, result.Events.Count);
        }

        [Test]
        public void Step_LionBBreaksGate()
        {
            var state = ActorAtOrigin(LionId.B, ".R../....");
            var result = Move(state, LionId.B, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(0, 0), state.LionB);
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(3, state.Alarm);
            Assert.IsInstanceOf<GateBroken>(result.Events[0]);
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoGold_Collects(LionId lion)
        {
            var state = ActorAtOrigin(lion, ".G../....");
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
            Assert.AreEqual(1, state.Gold);
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsFalse(result.Events.OfType<CellOpened>().Any());
            var moved = (LionMoved)result.Events[0];
            Assert.AreEqual(lion, moved.Lion);
            Assert.IsFalse(moved.IsJump);
            Assert.IsInstanceOf<GoldCollected>(result.Events[1]);
            Assert.AreEqual(2, result.Events.Count);
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoLever_ResetsAlarm(LionId lion)
        {
            var state = ActorAtOrigin(lion, ".L../T...");
            Move(state, lion, 0, 1);
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(0, state.Alarm);
            Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsFalse(result.Events.OfType<CellOpened>().Any());
            Assert.IsInstanceOf<LionMoved>(result.Events[0]);
            Assert.IsInstanceOf<LeverUsed>(result.Events[1]);
            Assert.IsInstanceOf<AlarmChanged>(result.Events[2]);
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoOpenChest_EntersAndLeavesTheChest(LionId lion)
        {
            var state = ActorAtOrigin(lion, ".C../....");
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
            Assert.IsFalse(state.ChestTaken);
            Assert.AreEqual(ItemType.Chest, state.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(0, state.Alarm);
            Assert.IsInstanceOf<LionMoved>(result.Events.Single());
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoClosedChest_OpensWithoutCost(LionId lion)
        {
            var state = ActorAtOrigin(lion, ".c../....");
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(RejectReason.None, result.Reason);
            Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
            Assert.AreEqual(0, state.Alarm);
            Assert.IsFalse(state.ChestTaken);
            Assert.IsTrue(state.CellAt(new CellPos(1, 0)).IsOpen);
            Assert.AreEqual(ItemType.Chest, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsInstanceOf<CellOpened>(result.Events[0]);
            Assert.IsInstanceOf<LionMoved>(result.Events[1]);
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoExit_Enters(LionId lion)
        {
            var state = ActorAtOrigin(lion, ".E../....");
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
            Assert.AreEqual(ItemType.Exit, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsFalse(RulesEngine.ExitConditionsMet(state));
            Assert.IsInstanceOf<LionMoved>(result.Events.Single());
        }

        [TestCase(LionId.A, "t")]
        [TestCase(LionId.B, "t")]
        [TestCase(LionId.A, "#")]
        [TestCase(LionId.B, "#")]
        [TestCase(LionId.A, "r")]
        [TestCase(LionId.B, "r")]
        [TestCase(LionId.A, "l")]
        [TestCase(LionId.B, "l")]
        [TestCase(LionId.A, "g")]
        [TestCase(LionId.B, "g")]
        [TestCase(LionId.A, "c")]
        [TestCase(LionId.B, "c")]
        [TestCase(LionId.A, "e")]
        [TestCase(LionId.B, "e")]
        public void Step_OntoClosedCell_DependsOnContents(LionId lion, string contents)
        {
            var state = ActorAtOrigin(lion, "." + contents + "../....");
            var result = Move(state, lion, 1, 0);
            var cell = state.CellAt(new CellPos(1, 0));
            if (contents == "c" || contents == "e")
            {
                Assert.IsTrue(result.Accepted);
                Assert.IsTrue(cell.IsOpen);
                Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
                Assert.AreEqual(0, state.Alarm);
                Assert.IsInstanceOf<CellOpened>(result.Events[0]);
                Assert.IsInstanceOf<LionMoved>(result.Events[1]);
                Assert.AreEqual(contents == "c" ? ItemType.Chest : ItemType.Exit, cell.Item);
                Assert.IsFalse(state.ChestTaken);
                return;
            }

            Assert.IsTrue(cell.IsOpen);
            Assert.IsInstanceOf<CellOpened>(result.Events[0]);
            switch (contents)
            {
                case "t":
                    Assert.AreEqual(ItemType.Trap, cell.Item);
                    Assert.AreEqual(new CellPos(0, 0), state.Position(lion));
                    Assert.AreEqual(4, state.Alarm);
                    break;
                case "#":
                    Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
                    Assert.IsInstanceOf<LionMoved>(result.Events[1]);
                    break;
                case "r" when lion == LionId.A:
                    Assert.IsTrue(result.Accepted);
                    Assert.AreEqual(RejectReason.None, result.Reason);
                    Assert.AreEqual(ItemType.Gate, cell.Item);
                    Assert.AreEqual(new CellPos(0, 0), state.Position(lion));
                    Assert.AreEqual(0, state.Alarm);
                    Assert.IsInstanceOf<GateBlocked>(result.Events[1]);
                    Assert.AreEqual(2, result.Events.Count);
                    break;
                case "r":
                    Assert.AreEqual(ItemType.None, cell.Item);
                    Assert.AreEqual(new CellPos(0, 0), state.Position(lion));
                    Assert.AreEqual(3, state.Alarm);
                    break;
                case "l":
                    Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
                    Assert.AreEqual(ItemType.None, cell.Item);
                    Assert.AreEqual(0, state.Alarm);
                    break;
                case "g":
                    Assert.AreEqual(new CellPos(1, 0), state.Position(lion));
                    Assert.AreEqual(1, state.Gold);
                    break;
            }
        }

        [TestCase("t", typeof(TrapTriggered))]
        [TestCase("#", typeof(LionMoved))]
        [TestCase("r", typeof(GateBlocked))]
        [TestCase("l", typeof(LeverUsed))]
        [TestCase("g", typeof(GoldCollected))]
        [TestCase("e", typeof(LionMoved))]
        public void Jump_OntoClosedCell(string contents, System.Type interaction)
        {
            var state = LevelBuilder.Play(".#" + contents + "./....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, 2, 0);
            var between = state.CellAt(new CellPos(1, 0));

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(RejectReason.None, result.Reason);
            Assert.IsFalse(between.IsOpen);
            Assert.AreEqual(ItemType.None, between.Item);
            var opened = (CellOpened)result.Events[0];
            Assert.AreEqual(new CellPos(2, 0), opened.Pos);
            if (contents == "g" || contents == "l")
            {
                var moved = (LionMoved)result.Events[1];
                Assert.IsTrue(moved.IsJump);
                Assert.IsInstanceOf(interaction, result.Events[2]);
            }
            else
                Assert.IsInstanceOf(interaction, result.Events[1]);
            if (contents == "r")
                Assert.AreEqual(0, state.Alarm);
            Assert.AreEqual(
                contents == "t" || contents == "r" ? new CellPos(0, 0) : new CellPos(2, 0),
                state.LionA);
        }

        [Test]
        public void Jump_OntoClosedChest_LandsAndPays()
        {
            var state = LevelBuilder.Play("..c./....", 0, 0, 3, 1, jumpCost: 9);
            var result = Move(state, LionId.A, 2, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(2, 0), state.LionA);
            Assert.AreEqual(9, state.Alarm);
            Assert.IsFalse(state.ChestTaken);
            Assert.AreEqual(ItemType.Chest, state.CellAt(new CellPos(2, 0)).Item);
            Assert.IsInstanceOf<CellOpened>(result.Events[0]);
            var moved = (LionMoved)result.Events[1];
            Assert.IsTrue(moved.IsJump);
            Assert.IsInstanceOf<AlarmChanged>(result.Events[2]);
        }

        [Test]
        public void Step_OntoClosedGate_LionA_IsAccepted()
        {
            var state = ActorAtOrigin(LionId.A, ".r../....");
            var result = Move(state, LionId.A, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(RejectReason.None, result.Reason);
            Assert.AreEqual(new CellPos(0, 0), state.LionA);
            Assert.AreEqual(0, state.Alarm);
            Assert.AreEqual(ItemType.Gate, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsTrue(state.CellAt(new CellPos(1, 0)).IsOpen);
            Assert.IsInstanceOf<CellOpened>(result.Events[0]);
            Assert.IsInstanceOf<GateBlocked>(result.Events[1]);
            Assert.AreEqual(2, result.Events.Count);
        }

        [Test]
        public void Jump_OffTheBoard_IsOutOfBounds()
        {
            var state = LevelBuilder.Play("..../....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, -2, 0);

            Assert.AreEqual(RejectReason.OutOfBounds, result.Reason);
            Assert.AreEqual(new CellPos(0, 0), state.LionA);
        }

        [Test]
        public void Jump_OverLionB_DoesNotTouchHim()
        {
            var state = LevelBuilder.Play("T.../....", 0, 1, 1, 1);
            var result = Move(state, LionId.A, 2, 1);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(2, 1), state.LionA);
            Assert.AreEqual(new CellPos(1, 1), state.LionB);
            Assert.AreEqual(ItemType.Trap, state.CellAt(new CellPos(0, 0)).Item);
            Assert.AreEqual(2, state.Alarm);
            var moved = (LionMoved)result.Events[0];
            Assert.IsTrue(moved.IsJump);
        }

        [Test]
        public void Jump_OntoLionB_SharesTheCellAndPaysJump()
        {
            var state = LevelBuilder.Play(".T../....", 0, 0, 2, 0);
            var result = Move(state, LionId.A, 2, 0);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(new CellPos(2, 0), state.LionA);
            Assert.AreEqual(new CellPos(2, 0), state.LionB);
            Assert.AreEqual(ItemType.Trap, state.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(2, state.Alarm);
            var moved = (LionMoved)result.Events[0];
            Assert.AreEqual(LionId.A, moved.Lion);
            Assert.IsTrue(moved.IsJump);
        }

        [Test]
        public void Jump_OntoEmpty_PaysJumpCost()
        {
            var state = LevelBuilder.Play("..../....", 0, 0, 3, 1, jumpCost: 2);
            var result = Move(state, LionId.A, 2, 0);

            Assert.AreEqual(2, state.Alarm);
            Assert.AreEqual(new CellPos(2, 0), state.LionA);
            Assert.IsInstanceOf<AlarmChanged>(result.Events.Last());
        }

        [Test]
        public void Jump_OntoTrap_PaysOnlyTrap()
        {
            var state = LevelBuilder.Play("..T./....", 0, 0, 3, 1, jumpCost: 9, trapCost: 4);
            Move(state, LionId.A, 2, 0);

            Assert.AreEqual(new CellPos(0, 0), state.LionA);
            Assert.AreEqual(4, state.Alarm);
        }

        [Test]
        public void Jump_OntoGate_PaysNothingForLionA()
        {
            var blocked = LevelBuilder.Play("..R./....", 0, 0, 3, 1, jumpCost: 9, gateBreakCost: 3);
            var result = Move(blocked, LionId.A, 2, 0);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(RejectReason.GateBlocksLionA, result.Reason);
            Assert.AreEqual(0, result.Events.Count);
            Assert.AreEqual(0, blocked.Alarm);
            Assert.AreEqual(new CellPos(0, 0), blocked.LionA);
            Assert.AreEqual(ItemType.Gate, blocked.CellAt(new CellPos(2, 0)).Item);
        }

        [Test]
        public void Jump_OntoGold_PaysOnlyJump()
        {
            var state = LevelBuilder.Play("..G./....", 0, 0, 3, 1, jumpCost: 2);
            Move(state, LionId.A, 2, 0);

            Assert.AreEqual(1, state.Gold);
            Assert.AreEqual(2, state.Alarm);
            Assert.AreEqual(new CellPos(2, 0), state.LionA);
        }

        [Test]
        public void Jump_OntoLever_EndsAtZero()
        {
            var state = LevelBuilder.Play("T.../..../.L..", 1, 0, 3, 0, alarmThreshold: 20, jumpCost: 5, trapCost: 4);
            Move(state, LionId.A, 0, 0);
            Assert.AreEqual(4, state.Alarm);

            var result = Move(state, LionId.A, 1, 2);
            Assert.AreEqual(0, state.Alarm);
            Assert.AreEqual(GameStatus.Playing, state.Status);
            var alarm = result.Events.OfType<AlarmChanged>().Single();
            Assert.AreEqual(4, alarm.OldValue);
            Assert.AreEqual(0, alarm.NewValue);
        }

        [Test]
        public void Alarm_ExactThreshold_Loses()
        {
            var state = LevelBuilder.Play(".T../....", 0, 0, 3, 1, alarmThreshold: 4, trapCost: 4);
            var result = Move(state, LionId.A, 1, 0);

            Assert.AreEqual(GameStatus.Lost, state.Status);
            Assert.IsInstanceOf<GameLost>(result.Events.Last());
        }

        [Test]
        public void Lever_BeforeThreshold_Saves()
        {
            var state = LevelBuilder.Play(".TL./....", 0, 0, 3, 1, alarmThreshold: 5, trapCost: 4);
            Move(state, LionId.A, 1, 0);
            Assert.AreEqual(4, state.Alarm);

            var saved = Move(state, LionId.A, 2, 0);
            Assert.IsFalse(saved.Events.OfType<TrapTriggered>().Any());
            Assert.AreEqual(0, state.Alarm);
            Assert.AreEqual(GameStatus.Playing, state.Status);

            Move(state, LionId.A, 1, 0);
            Assert.AreEqual(4, state.Alarm);
            Assert.AreEqual(GameStatus.Playing, state.Status);
        }

        [TestCase(LionId.A)]
        [TestCase(LionId.B)]
        public void Step_OntoOpenChest_TakesWhenPartnerIsThere(LionId lion)
        {
            var state = lion == LionId.A
                ? LevelBuilder.Play(".C../....", 0, 0, 1, 0)
                : LevelBuilder.Play(".C../....", 1, 0, 0, 0);
            var result = Move(state, lion, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.IsTrue(state.ChestTaken);
            Assert.AreEqual(new CellPos(1, 0), state.LionA);
            Assert.AreEqual(new CellPos(1, 0), state.LionB);
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(0, state.Alarm);
            Assert.IsInstanceOf<LionMoved>(result.Events[0]);
            Assert.IsInstanceOf<ChestTaken>(result.Events[1]);
            var taken = (ChestTaken)result.Events[1];
            Assert.AreEqual(new CellPos(1, 0), taken.Pos);
            Assert.AreEqual(lion, taken.Lion);
        }

        [Test]
        public void Jump_OntoOpenChest_PaysJump_AndTakes()
        {
            var state = LevelBuilder.Play("..C./....", 0, 0, 2, 0, jumpCost: 2);
            var result = Move(state, LionId.A, 2, 0);

            Assert.IsTrue(result.Accepted);
            Assert.IsTrue(state.ChestTaken);
            Assert.AreEqual(new CellPos(2, 0), state.LionA);
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(2, 0)).Item);
            Assert.AreEqual(2, state.Alarm);
            Assert.IsInstanceOf<LionMoved>(result.Events[0]);
            Assert.IsTrue(((LionMoved)result.Events[0]).IsJump);
            Assert.IsInstanceOf<ChestTaken>(result.Events[1]);
            var alarm = (AlarmChanged)result.Events[2];
            Assert.AreEqual(0, alarm.OldValue);
            Assert.AreEqual(2, alarm.NewValue);
            Assert.AreEqual(3, result.Events.Count);
        }

        [Test]
        public void Step_OntoOpenChest_WithoutPartner_LeavesTheChest()
        {
            var state = LevelBuilder.Play(".C../....", 0, 0, 2, 1);
            var result = Move(state, LionId.A, 1, 0);

            Assert.IsTrue(result.Accepted);
            Assert.IsFalse(state.ChestTaken);
            Assert.AreEqual(new CellPos(1, 0), state.LionA);
            Assert.AreEqual(new CellPos(2, 1), state.LionB);
            Assert.AreEqual(ItemType.Chest, state.CellAt(new CellPos(1, 0)).Item);
            Assert.AreEqual(0, state.Alarm);
            Assert.IsFalse(result.Events.OfType<ChestTaken>().Any());
        }

        [Test]
        public void ClosedChest_OpensAndEnters_ThenPartnerTakesIt()
        {
            var state = LevelBuilder.Play(".c../....", 0, 0, 1, 1);
            var opened = Move(state, LionId.A, 1, 0);

            Assert.IsTrue(opened.Accepted);
            Assert.IsFalse(state.ChestTaken);
            Assert.AreEqual(new CellPos(1, 0), state.LionA);
            Assert.AreEqual(ItemType.Chest, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsInstanceOf<CellOpened>(opened.Events[0]);
            Assert.IsInstanceOf<LionMoved>(opened.Events[1]);

            var left = Move(state, LionId.A, 0, 0);
            Assert.IsFalse(state.ChestTaken);
            Assert.AreEqual(new CellPos(0, 0), state.LionA);

            Move(state, LionId.A, 1, 0);
            var taken = Move(state, LionId.B, 1, 0);
            Assert.IsTrue(state.ChestTaken);
            Assert.AreEqual(new CellPos(1, 0), state.LionA);
            Assert.AreEqual(new CellPos(1, 0), state.LionB);
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsInstanceOf<LionMoved>(taken.Events[0]);
            Assert.IsInstanceOf<ChestTaken>(taken.Events[1]);
        }

        [Test]
        public void AdjacentLions_DoNotTakeTheChestByThemselves()
        {
            var state = LevelBuilder.Play(".C../....", 1, 1, 0, 0);
            var result = Move(state, LionId.A, 2, 1);

            Assert.IsTrue(result.Accepted);
            Assert.IsFalse(state.ChestTaken);
            Assert.AreEqual(ItemType.Chest, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsFalse(result.Events.OfType<ChestTaken>().Any());
        }

        [Test]
        public void GetTargets_IncludesPartnerChestAndExit_ButNotOpenGateForLionA()
        {
            var state = LevelBuilder.Play(".CE./..../R...", 0, 0, 0, 1);
            var targets = RulesEngine.GetTargets(state, LionId.A);

            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(0, 1) && t.Kind == MoveKind.Step));
            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(1, 0) && t.Kind == MoveKind.Step));
            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(2, 0) && t.Kind == MoveKind.Jump));
            Assert.IsFalse(targets.Any(t => t.Pos == new CellPos(0, 2)));
        }

        [Test]
        public void Exit_NeedsBothLionsOnTheCell_AfterTheChest()
        {
            var state = GameState.FromLevel(LevelMapParser.Parse("ACBE", 20, 2, 3, 4));
            Move(state, LionId.B, 3, 0);
            Move(state, LionId.A, 1, 0);
            Move(state, LionId.A, 3, 0);
            Assert.IsFalse(state.ChestTaken);
            Assert.IsFalse(RulesEngine.ExitConditionsMet(state));
            Assert.IsFalse(RulesEngine.ConfirmExtract(state).Accepted);

            Move(state, LionId.A, 1, 0);
            Move(state, LionId.B, 2, 0);
            Move(state, LionId.B, 1, 0);
            Assert.IsTrue(state.ChestTaken);
            Assert.IsFalse(RulesEngine.ExitConditionsMet(state));

            Move(state, LionId.A, 3, 0);
            Move(state, LionId.B, 2, 0);
            Move(state, LionId.B, 3, 0);
            Assert.IsTrue(RulesEngine.ExitConditionsMet(state));
            var extract = RulesEngine.ConfirmExtract(state);
            Assert.IsTrue(extract.Accepted);
            Assert.AreEqual(GameStatus.Won, state.Status);
        }

        [Test]
        public void Exit_ShortGold_AndLeaving_CloseItAgain()
        {
            var poor = GameState.FromLevel(LevelMapParser.Parse("ACBE", 20, 2, 3, 4, 1));
            Move(poor, LionId.A, 1, 0);
            Move(poor, LionId.B, 1, 0);
            Move(poor, LionId.A, 3, 0);
            Move(poor, LionId.B, 2, 0);
            Move(poor, LionId.B, 3, 0);

            Assert.IsTrue(poor.ChestTaken);
            Assert.IsFalse(RulesEngine.ExitConditionsMet(poor));
            Assert.IsFalse(RulesEngine.ConfirmExtract(poor).Accepted);
            Assert.AreEqual(GameStatus.Playing, poor.Status);

            var ready = GameState.FromLevel(LevelMapParser.Parse("ACBE", 20, 2, 3, 4));
            Move(ready, LionId.A, 1, 0);
            Move(ready, LionId.B, 1, 0);
            Move(ready, LionId.A, 3, 0);
            Move(ready, LionId.B, 2, 0);
            Move(ready, LionId.B, 3, 0);
            Assert.IsTrue(RulesEngine.ExitConditionsMet(ready));

            Move(ready, LionId.A, 2, 0);
            Assert.IsFalse(RulesEngine.ExitConditionsMet(ready));
            Assert.IsFalse(RulesEngine.ConfirmExtract(ready).Accepted);

            Move(ready, LionId.A, 3, 0);
            var extract = RulesEngine.ConfirmExtract(ready);
            Assert.IsTrue(extract.Accepted);
            Assert.IsInstanceOf<GameWon>(extract.Events.Single());
            Assert.AreEqual(GameStatus.Won, ready.Status);
        }

        [Test]
        public void SharedGold_FirstLionTakesIt()
        {
            var state = LevelBuilder.Play(".G../....", 0, 0, 2, 0);
            var first = Move(state, LionId.A, 1, 0);

            Assert.AreEqual(1, state.Gold);
            Assert.IsInstanceOf<LionMoved>(first.Events[0]);
            Assert.IsInstanceOf<GoldCollected>(first.Events[1]);

            var second = Move(state, LionId.B, 1, 0);
            Assert.AreEqual(new CellPos(1, 0), state.LionA);
            Assert.AreEqual(new CellPos(1, 0), state.LionB);
            Assert.AreEqual(1, state.Gold);
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(1, 0)).Item);
            Assert.IsInstanceOf<LionMoved>(second.Events.Single());
        }

        [Test]
        public void Moves_AfterLoss_AreRejected()
        {
            var state = LevelBuilder.Play(".T../....", 0, 0, 3, 1, alarmThreshold: 4, trapCost: 4);
            Move(state, LionId.A, 1, 0);
            var result = Move(state, LionId.A, 0, 1);

            Assert.AreEqual(RejectReason.GameOver, result.Reason);
            Assert.IsFalse(RulesEngine.ConfirmExtract(state).Accepted);
            Assert.IsEmpty(RulesEngine.GetTargets(state, LionId.A));
        }

        [Test]
        public void GetTargets_HidesClosedContents_AndSkipsVisibleBlocks()
        {
            var state = LevelBuilder.Play("..C../..R../r..TE/..c../.....", 2, 2, 2, 4);
            var targets = RulesEngine.GetTargets(state, LionId.A);

            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(2, 0)));
            Assert.IsFalse(targets.Any(t => t.Pos == new CellPos(2, 1)));
            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(2, 3) && t.Kind == MoveKind.Step));
            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(0, 2) && t.Kind == MoveKind.Jump));
            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(3, 2) && t.Kind == MoveKind.Step));
            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(2, 4) && t.Kind == MoveKind.Jump));
        }

        [Test]
        public void GetTargets_LionB_IncludesOpenGate()
        {
            var state = LevelBuilder.Play("R.../....", 3, 1, 1, 0);
            var targets = RulesEngine.GetTargets(state, LionId.B);

            Assert.IsTrue(targets.Any(t => t.Pos == new CellPos(0, 0) && t.Kind == MoveKind.Step));
        }

        [Test]
        public void Loss_DoesNotTakeTheChest()
        {
            var state = LevelBuilder.Play("T.../.C../....", 1, 0, 2, 1, alarmThreshold: 4, trapCost: 4);
            var result = Move(state, LionId.A, 0, 0);

            Assert.IsInstanceOf<TrapTriggered>(result.Events[0]);
            Assert.IsInstanceOf<AlarmChanged>(result.Events[1]);
            Assert.IsInstanceOf<GameLost>(result.Events[2]);
            Assert.AreEqual(3, result.Events.Count);
            Assert.IsFalse(state.ChestTaken);
            Assert.AreEqual(ItemType.Chest, state.CellAt(new CellPos(1, 1)).Item);
            Assert.AreEqual(GameStatus.Lost, state.Status);
        }

        [Test]
        public void ClosedTrap_Loss_EventOrder()
        {
            var state = LevelBuilder.Play(".t../....", 0, 0, 3, 1, alarmThreshold: 4, trapCost: 4);
            var result = Move(state, LionId.A, 1, 0);

            Assert.IsInstanceOf<CellOpened>(result.Events[0]);
            Assert.IsInstanceOf<TrapTriggered>(result.Events[1]);
            Assert.IsInstanceOf<AlarmChanged>(result.Events[2]);
            Assert.IsInstanceOf<GameLost>(result.Events[3]);
            Assert.AreEqual(4, result.Events.Count);
        }

        [Test]
        public void FromLevel_CopiesCells()
        {
            var level = LevelBuilder.Data(".G../....", 0, 0, 3, 1);
            var state = GameState.FromLevel(level);
            Move(state, LionId.A, 1, 0);

            Assert.AreEqual(ItemType.Gold, level.Cells[1, 0].Item);
            Assert.AreEqual(ItemType.None, state.CellAt(new CellPos(1, 0)).Item);
        }

        [Test]
        public void InvalidShape_AndNotAdjacent_AreDistinct()
        {
            var state = LevelBuilder.Play("..../....", 0, 0, 3, 1);
            Assert.AreEqual(RejectReason.InvalidShape, Move(state, LionId.A, 1, 1).Reason);
            Assert.AreEqual(RejectReason.InvalidShape, Move(state, LionId.A, 0, 0).Reason);
            Assert.AreEqual(RejectReason.NotAdjacent, Move(state, LionId.B, 1, 1).Reason);
            Assert.AreEqual(RejectReason.NotAdjacent, Move(state, LionId.A, 3, 0).Reason);
        }

        [Test]
        public void ClosedGold_RevealsBeforeTheLionLands()
        {
            var state = LevelBuilder.Play(".g../....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, 1, 0);

            var opened = (CellOpened)result.Events[0];
            Assert.AreEqual(ItemType.Gold, opened.Item);
            var moved = (LionMoved)result.Events[1];
            Assert.AreEqual(new CellPos(0, 0), moved.From);
            Assert.AreEqual(new CellPos(1, 0), moved.To);
            Assert.IsFalse(moved.IsJump);
            Assert.IsInstanceOf<GoldCollected>(result.Events[2]);
            Assert.AreEqual(3, result.Events.Count);
        }

        [Test]
        public void ClosedLever_RevealsBeforeTheLionLands()
        {
            var state = LevelBuilder.Play(".l../T...", 0, 0, 3, 1);
            Move(state, LionId.A, 0, 1);
            var result = Move(state, LionId.A, 1, 0);

            var opened = (CellOpened)result.Events[0];
            Assert.AreEqual(ItemType.Lever, opened.Item);
            Assert.IsInstanceOf<LionMoved>(result.Events[1]);
            Assert.IsInstanceOf<LeverUsed>(result.Events[2]);
            Assert.IsInstanceOf<AlarmChanged>(result.Events[3]);
        }

        [Test]
        public void ClosedGate_LionB_OpensWithoutAMove()
        {
            var state = LevelBuilder.Play(".r../....", 3, 1, 0, 0);
            var result = Move(state, LionId.B, 1, 0);

            var opened = (CellOpened)result.Events[0];
            Assert.AreEqual(ItemType.Gate, opened.Item);
            Assert.IsInstanceOf<GateBroken>(result.Events[1]);
            Assert.IsFalse(result.Events.OfType<LionMoved>().Any());
            Assert.AreEqual(new CellPos(0, 0), state.LionB);
        }

        [Test]
        public void ClosedGate_LionA_OpensWithoutAMove()
        {
            var state = LevelBuilder.Play(".r../....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, 1, 0);

            Assert.AreEqual(ItemType.Gate, ((CellOpened)result.Events[0]).Item);
            Assert.IsInstanceOf<GateBlocked>(result.Events[1]);
            Assert.IsFalse(result.Events.OfType<LionMoved>().Any());
            Assert.AreEqual(2, result.Events.Count);
        }

        [Test]
        public void ClosedTrap_OpensWithoutAMove()
        {
            var state = LevelBuilder.Play(".t../....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, 1, 0);

            Assert.AreEqual(ItemType.Trap, ((CellOpened)result.Events[0]).Item);
            Assert.IsInstanceOf<TrapTriggered>(result.Events[1]);
            Assert.IsFalse(result.Events.OfType<LionMoved>().Any());
            Assert.AreEqual(new CellPos(0, 0), state.LionA);
        }

        [Test]
        public void ClosedEmpty_OpensThenTheLionSteps()
        {
            var state = LevelBuilder.Play(".#../....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, 1, 0);

            Assert.AreEqual(ItemType.None, ((CellOpened)result.Events[0]).Item);
            Assert.IsInstanceOf<LionMoved>(result.Events[1]);
            Assert.AreEqual(2, result.Events.Count);
        }

        [Test]
        public void OpenGold_MovesBeforeItIsCollected()
        {
            var state = LevelBuilder.Play(".G../....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, 1, 0);

            Assert.IsFalse(result.Events.OfType<CellOpened>().Any());
            Assert.IsInstanceOf<LionMoved>(result.Events[0]);
            Assert.IsInstanceOf<GoldCollected>(result.Events[1]);
        }

        [Test]
        public void Jump_OntoClosedGold_RevealsThenJumps()
        {
            var state = LevelBuilder.Play(".#g./....", 0, 0, 3, 1);
            var result = Move(state, LionId.A, 2, 0);

            Assert.AreEqual(ItemType.Gold, ((CellOpened)result.Events[0]).Item);
            var moved = (LionMoved)result.Events[1];
            Assert.IsTrue(moved.IsJump);
            Assert.AreEqual(new CellPos(0, 0), moved.From);
            Assert.AreEqual(new CellPos(2, 0), moved.To);
            Assert.IsInstanceOf<GoldCollected>(result.Events[2]);
        }

        static GameState ActorAtOrigin(LionId lion, string map)
        {
            return lion == LionId.A
                ? LevelBuilder.Play(map, 0, 0, 3, 1)
                : LevelBuilder.Play(map, 3, 1, 0, 0);
        }

        static MoveResult Move(GameState state, LionId lion, int x, int y) =>
            RulesEngine.Move(state, lion, new CellPos(x, y));

        static LionId Other(LionId lion) => lion == LionId.A ? LionId.B : LionId.A;
    }
}
