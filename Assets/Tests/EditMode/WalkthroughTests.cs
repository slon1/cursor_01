using Game.Config;
using Game.Core;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests.EditMode
{
    public static class LevelWalkthroughs
    {
        public readonly struct Step
        {
            public readonly LionId Lion;
            public readonly int X;
            public readonly int Y;

            public Step(LionId lion, int x, int y)
            {
                Lion = lion;
                X = x;
                Y = y;
            }
        }

        public readonly struct Walk
        {
            public readonly Step[] Moves;
            public readonly bool Extract;
            public readonly int Alarm;

            public Walk(Step[] moves, bool extract, int alarm)
            {
                Moves = moves;
                Extract = extract;
                Alarm = alarm;
            }
        }

        public static Walk Level(int number)
        {
            switch (number)
            {
                case 1: return Level1;
                case 2: return Level2;
                case 3: return Level3;
                default: throw new System.ArgumentOutOfRangeException(nameof(number));
            }
        }

        static readonly Walk Level1 = new Walk(new[]
        {
            A(1, 0), A(2, 0), A(2, 1), A(2, 2),
            B(1, 3), B(2, 3),
            A(2, 2), A(3, 2), A(4, 2),
            B(1, 3), B(0, 3),
            A(4, 3), A(4, 4), A(3, 4), A(2, 4), A(1, 4), A(0, 4)
        }, true, 0);

        static readonly Walk Level2 = new Walk(new[]
        {
            A(0, 1), A(1, 1), A(1, 3), A(2, 3), A(3, 3),
            B(4, 1), B(4, 2), B(4, 2), B(4, 3),
            A(3, 3), A(3, 4),
            B(3, 3), B(2, 3),
            A(2, 4)
        }, true, 5);

        static readonly Walk Level3 = new Walk(new[]
        {
            A(1, 0), A(1, 1), A(1, 2), A(1, 3),
            B(0, 3),
            A(1, 3), A(1, 2), A(1, 1), A(2, 1), A(3, 1), A(4, 1), A(4, 2), A(4, 3), A(3, 3),
            B(1, 3), B(1, 4), B(2, 4),
            A(3, 4)
        }, true, 0);

        static Step A(int x, int y) => new Step(LionId.A, x, y);

        static Step B(int x, int y) => new Step(LionId.B, x, y);
    }

    public class WalkthroughTests
    {
        const string SetPath = "Assets/ScriptableObjects/LevelSet.asset";

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Walkthrough_LevelN_Wins(int number)
        {
            var set = AssetDatabase.LoadAssetAtPath<LevelSet>(SetPath);
            var session = new GameSession(set.Get(number).ToLevelData());
            var walk = LevelWalkthroughs.Level(number);
            for (var i = 0; i < walk.Moves.Length; i++)
            {
                var step = walk.Moves[i];
                session.SelectLion(step.Lion);
                var move = session.TapCell(new CellPos(step.X, step.Y));
                Assert.IsTrue(move.Accepted, number + " ход " + (i + 1) + " " + step.Lion + " " + step.X + "," + step.Y);
            }

            Assert.IsTrue(walk.Extract);
            Assert.IsTrue(session.ExitReady, "Уровень " + number);
            Assert.IsTrue(session.TryExtract().Accepted, "Уровень " + number);
            Assert.AreEqual(GameStatus.Won, session.State.Status);
            Assert.AreEqual(walk.Alarm, session.State.Alarm);
        }
    }
}
