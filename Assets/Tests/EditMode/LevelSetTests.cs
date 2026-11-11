using Game.Config;
using Game.Core;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests.EditMode
{
    public class LevelSetTests
    {
        const string SetPath = "Assets/ScriptableObjects/LevelSet.asset";

        [Test]
        public void EveryAuthoredLevel_IsValid()
        {
            var set = Load();
            for (var number = 1; number <= set.Count; number++)
            {
                Assert.IsTrue(set.Has(number), number.ToString());
                var config = set.Get(number);
                Assert.AreEqual(number, config.Number);
                Assert.IsEmpty(config.ToLevelData().Validate(), "Уровень " + number);
            }
        }

        [Test]
        public void EveryAuthoredLevel_MatchesTheFrozenTable()
        {
            var set = Load();
            var maps = new[]
            {
                "A#g##/#####/##c#g/B####/e#g##",
                "A###B/#g###/rrrrr/#g#c#/g#e##",
                "A#t#l/###g#/##t##/#c##g/B##e#",
                "A#t##/#tg#t/##c##/g#t#B/##eg#",
                "A#r#g/#gr##/##rc#/B#r#g/l#r#e",
                "A#B#g/g#let/rrrrr/rrrtr/ctgg#",
                "AgB##g/#####t/rrrttt/rrrrrr/##tt#g/#e#g#c",
                "A#et#B/#g#t#g/rrtrrt/rr#rrr/##cggt/l###tt",
                "#A#rBg/tget##/rrrrtr/rrrrrr/###ttl/c##tgg",
                "AggteB/g#tgl#/rrrrrr/rrrrtt/rrttrr/#c###g"
            };
            var thresholds = new[] { 10, 8, 8, 9, 9, 12, 12, 12, 14, 18 };
            var minCoins = new[] { 2, 2, 2, 2, 2, 3, 3, 3, 3, 4 };
            var widths = new[] { 5, 5, 5, 5, 5, 5, 6, 6, 6, 6 };

            for (var number = 1; number <= set.Count; number++)
            {
                var config = set.Get(number);
                var index = number - 1;
                Assert.AreEqual(maps[index], config.Map, "Уровень " + number);
                Assert.AreEqual(widths[index], config.ToLevelData().Width, "Уровень " + number);
                Assert.AreEqual(widths[index], config.ToLevelData().Height, "Уровень " + number);
                Assert.AreEqual(thresholds[index], config.AlarmThreshold, "Уровень " + number);
                Assert.AreEqual(minCoins[index], config.MinCoins, "Уровень " + number);
                Assert.AreEqual(2, config.JumpCost, "Уровень " + number);
                Assert.AreEqual(3, config.GateBreakCost, "Уровень " + number);
                Assert.AreEqual(4, config.TrapCost, "Уровень " + number);
            }
        }

        [Test]
        [Explicit]
        [Category("Slow")]
        [Timeout(900000)]
        public void EveryAuthoredLevel_IsSolvable()
        {
            var set = Load();
            for (var number = 1; number <= set.Count; number++)
            {
                var solved = LevelSolver.Solve(
                    set.Get(number).ToLevelData(),
                    new SolveOptions { Mode = SolveMode.FindAny });
                Assert.IsTrue(solved.Solvable, "Уровень " + number);
                Assert.IsFalse(solved.Truncated, "Уровень " + number);
            }
        }

        static LevelSet Load()
        {
            var set = AssetDatabase.LoadAssetAtPath<LevelSet>(SetPath);
            Assert.IsNotNull(set, "Нет " + SetPath);
            Assert.AreEqual(10, set.Count);
            return set;
        }
    }
}
