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
        public void EveryAuthoredLevel_IsValidAndSolvable()
        {
            var set = AssetDatabase.LoadAssetAtPath<LevelSet>(SetPath);
            Assert.IsNotNull(set, "Нет " + SetPath);
            Assert.AreEqual(3, set.Count);

            for (var number = 1; number <= set.Count; number++)
            {
                Assert.IsTrue(set.Has(number), number.ToString());
                var config = set.Get(number);
                Assert.AreEqual(number, config.Number);
                var level = config.ToLevelData();
                Assert.IsEmpty(level.Validate(), "Уровень " + number);
                var solved = LevelSolver.TrySolve(level);
                Assert.IsTrue(solved.Solvable, "Уровень " + number);
                Assert.IsFalse(solved.Truncated, "Уровень " + number);
            }
        }
    }
}
