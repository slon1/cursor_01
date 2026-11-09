using System.Diagnostics;
using Game.Config;
using Game.Core;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Game.EditorTools
{
    public static class LevelVerifyMenu
    {
        const string SetPath = "Assets/ScriptableObjects/LevelSet.asset";

        [MenuItem("Tools/Levels/Verify All Levels")]
        public static void VerifyAll()
        {
            var set = AssetDatabase.LoadAssetAtPath<LevelSet>(SetPath);
            if (set == null)
            {
                Debug.LogError("Нет " + SetPath);
                return;
            }

            for (var number = 1; number <= set.Count; number++)
            {
                var level = set.Get(number).ToLevelData();
                var errors = level.Validate();
                var watch = Stopwatch.StartNew();
                var solved = LevelSolver.TrySolve(level);
                watch.Stop();
                Debug.Log(
                    "Уровень " + number
                    + " Solvable=" + solved.Solvable
                    + " MinMoves=" + solved.MinMoves
                    + " MinAlarmAtWin=" + solved.MinAlarmAtWin
                    + " состояния=" + solved.StatesExplored
                    + " мс=" + watch.ElapsedMilliseconds
                    + (errors.Count == 0 ? "" : " проверка=" + string.Join("; ", errors)));
            }
        }
    }
}
