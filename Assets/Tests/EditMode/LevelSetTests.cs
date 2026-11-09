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
