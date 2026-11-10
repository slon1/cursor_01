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
            Assert.GreaterOrEqual(result.AlarmAtWin, 0);
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
            Assert.AreEqual(0, result.AlarmAtWin);
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

        [Test]
        public void FindAny_SmallMap_PathWinsWhenReplayed()
        {
            var level = LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4);
            var result = LevelSolver.Solve(level, new SolveOptions { Mode = SolveMode.FindAny });

            Assert.IsTrue(result.Solvable);
            Assert.IsFalse(result.Truncated);
            Assert.Greater(result.Path.Count, 0);
            var session = Replay(level, result.Path);
            Assert.AreEqual(GameStatus.Won, session.State.Status);
            Assert.AreEqual(result.AlarmAtWin, session.State.Alarm);
        }

        [Test]
        public void FindAny_UnreachableChestAndExit_IsNotSolvable()
        {
            var level = LevelMapParser.Parse("A.B/RRR/RRR/RRR/.CE", 5, 2, 3, 4);
            var result = LevelSolver.Solve(level, new SolveOptions { Mode = SolveMode.FindAny });

            Assert.IsFalse(result.Solvable);
            Assert.IsFalse(result.Truncated);
            Assert.AreEqual(0, result.Path.Count);
        }

        [Test]
        public void FindAny_LowStateCap_Truncates()
        {
            var level = LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4);
            var result = LevelSolver.Solve(level, new SolveOptions { Mode = SolveMode.FindAny, MaxStates = 1 });

            Assert.IsTrue(result.Truncated);
            Assert.IsFalse(result.Solvable);
            Assert.AreEqual(1, result.StatesExplored);
        }

        [Test]
        public void FindAny_TwoRuns_ReturnTheSamePath()
        {
            var level = LevelMapParser.Parse("A.B/.C./gE.", 10, 2, 3, 4, 1);
            var first = LevelSolver.Solve(level, new SolveOptions { Mode = SolveMode.FindAny });
            var second = LevelSolver.Solve(level, new SolveOptions { Mode = SolveMode.FindAny });

            Assert.IsTrue(first.Solvable);
            Assert.AreEqual(first.Path.Count, second.Path.Count);
            for (var i = 0; i < first.Path.Count; i++)
            {
                Assert.AreEqual(first.Path[i].Lion, second.Path[i].Lion);
                Assert.AreEqual(first.Path[i].Target, second.Path[i].Target);
            }
        }

        [Test]
        public void FindAny_ClosedChest_OpensBeforeTake()
        {
            var level = LevelMapParser.Parse("A.B/.c./.E.", 10, 2, 3, 4);
            var result = LevelSolver.Solve(level, new SolveOptions { Mode = SolveMode.FindAny });

            Assert.IsTrue(result.Solvable);
            Assert.IsFalse(result.Truncated);
            var session = new GameSession(level);
            var opened = false;
            var taken = false;
            for (var i = 0; i < result.Path.Count; i++)
            {
                session.SelectLion(result.Path[i].Lion);
                var move = session.TapCell(result.Path[i].Target);
                Assert.IsTrue(move.Accepted);
                var openedNow = false;
                var takenNow = false;
                for (var e = 0; e < move.Events.Count; e++)
                {
                    if (move.Events[e] is CellOpened cell && cell.Item == ItemType.Chest)
                        openedNow = true;
                    if (move.Events[e] is ChestTaken)
                        takenNow = true;
                }

                if (openedNow)
                {
                    Assert.IsFalse(takenNow);
                    Assert.IsFalse(session.State.ChestTaken);
                    opened = true;
                }

                if (takenNow)
                {
                    Assert.IsTrue(opened);
                    taken = true;
                }
            }

            Assert.IsTrue(opened);
            Assert.IsTrue(taken);
            Assert.IsTrue(session.ExitReady);
            Assert.IsTrue(session.TryExtract().Accepted);
            Assert.AreEqual(GameStatus.Won, session.State.Status);
        }

        [Test]
        public void Shortest_OpenBitsExceptChest_DoNotChangeLength()
        {
            var options = new SolveOptions { Mode = SolveMode.Shortest, TimeLimitMs = 5000 };
            var closed = LevelSolver.Solve(LevelMapParser.Parse("A#B/#c#/#e#", 10, 2, 3, 4), options);
            var open = LevelSolver.Solve(LevelMapParser.Parse("A.B/.c./.e.", 10, 2, 3, 4), options);

            Assert.IsTrue(closed.Solvable);
            Assert.IsTrue(open.Solvable);
            Assert.IsFalse(closed.Truncated);
            Assert.IsFalse(open.Truncated);
            Assert.AreEqual(open.MinMoves, closed.MinMoves);
            Assert.AreEqual(open.AlarmAtWin, closed.AlarmAtWin);
        }

        [Test]
        public void Shortest_ClosedChest_TakesOneExtraMove()
        {
            var options = new SolveOptions { Mode = SolveMode.Shortest, TimeLimitMs = 5000 };
            var closed = LevelSolver.Solve(LevelMapParser.Parse("A.B/.c./.E.", 10, 2, 3, 4), options);
            var open = LevelSolver.Solve(LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4), options);

            Assert.IsTrue(closed.Solvable);
            Assert.IsTrue(open.Solvable);
            Assert.IsFalse(closed.Truncated);
            Assert.IsFalse(open.Truncated);
            Assert.AreEqual(open.MinMoves + 1, closed.MinMoves);
        }

        [Test]
        public void WithAlarmThreshold_SharesCellsAndKeepsOriginal()
        {
            var level = LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4);
            var copy = level.WithAlarmThreshold(4);

            Assert.AreEqual(10, level.AlarmThreshold);
            Assert.AreEqual(4, copy.AlarmThreshold);
            Assert.AreSame(level.Cells, copy.Cells);
            Assert.AreEqual(level.JumpCost, copy.JumpCost);
            Assert.AreEqual(level.MinCoins, copy.MinCoins);
        }

        [Test]
        public void MinSolvableThreshold_GateWall_Is4()
        {
            var level = LevelMapParser.Parse("A.B./rrrr/..C./.E..", 10, 2, 3, 4);
            var result = LevelSolver.MinSolvableThreshold(level, 10, new SolveOptions());

            Assert.AreEqual(4, result.Value);
            Assert.IsTrue(result.Exact);
            Assert.IsFalse(result.Truncated);
        }

        [Test]
        public void MinSolvableThreshold_StateCap_IsNotAFailure()
        {
            var level = LevelMapParser.Parse("A.B/.C./.E.", 10, 2, 3, 4);
            var result = LevelSolver.MinSolvableThreshold(level, 3, new SolveOptions { MaxStates = 1 });

            Assert.AreEqual(-1, result.Value);
            Assert.IsFalse(result.Exact);
            Assert.IsTrue(result.Truncated);
        }

        [Test]
        public void MinSolvableThreshold_Unreachable_IsExactMiss()
        {
            var level = LevelMapParser.Parse("A.B/RRR/RRR/RRR/.CE", 5, 2, 3, 4);
            var result = LevelSolver.MinSolvableThreshold(level, 5, new SolveOptions());

            Assert.AreEqual(-1, result.Value);
            Assert.IsFalse(result.Exact);
            Assert.IsFalse(result.Truncated);
        }

        [Test]
        public void MinSolvableThreshold_FirstThreeLevels_MatchPort()
        {
            var set = UnityEditor.AssetDatabase.LoadAssetAtPath<Game.Config.LevelSet>("Assets/ScriptableObjects/LevelSet.asset");
            Assert.IsNotNull(set);
            var expected = new[] { 1, 4, 1 };
            for (var number = 1; number <= 3; number++)
            {
                var level = set.Get(number).ToLevelData();
                var result = LevelSolver.MinSolvableThreshold(level, level.AlarmThreshold, new SolveOptions());
                Assert.AreEqual(expected[number - 1], result.Value, "Уровень " + number);
                Assert.IsTrue(result.Exact, "Уровень " + number);
                Assert.IsFalse(result.Truncated, "Уровень " + number);
            }
        }

        [Test]
        public void FindAny_AuthoredLevels_PathsWin()
        {
            var set = UnityEditor.AssetDatabase.LoadAssetAtPath<Game.Config.LevelSet>("Assets/ScriptableObjects/LevelSet.asset");
            Assert.IsNotNull(set);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (var number = 1; number <= set.Count; number++)
            {
                var level = set.Get(number).ToLevelData();
                var started = watch.ElapsedMilliseconds;
                var result = LevelSolver.Solve(level, new SolveOptions { Mode = SolveMode.FindAny });
                var levelMs = watch.ElapsedMilliseconds - started;
                TestContext.WriteLine("Уровень " + number + " мс=" + levelMs + " состояния=" + result.StatesExplored);
                Assert.LessOrEqual(levelMs, 10000, "Уровень " + number + " дольше 10 секунд");
                Assert.IsTrue(result.Solvable, "Уровень " + number);
                Assert.IsFalse(result.Truncated, "Уровень " + number);
                var session = Replay(level, result.Path);
                Assert.AreEqual(GameStatus.Won, session.State.Status, "Уровень " + number);
                Assert.AreEqual(result.AlarmAtWin, session.State.Alarm, "Уровень " + number);
            }

            Assert.LessOrEqual(watch.ElapsedMilliseconds, 60000, "Весь тест дольше 60 секунд");
        }

        static GameSession Replay(LevelData level, System.Collections.Generic.IReadOnlyList<SolverMove> path)
        {
            var session = new GameSession(level);
            for (var i = 0; i < path.Count; i++)
            {
                session.SelectLion(path[i].Lion);
                var move = session.TapCell(path[i].Target);
                Assert.IsTrue(move.Accepted, path[i].Lion + " " + path[i].Target);
            }

            Assert.IsTrue(session.ExitReady);
            Assert.IsTrue(session.TryExtract().Accepted);
            return session;
        }
    }
}
