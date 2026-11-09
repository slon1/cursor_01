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
        public static void VerifyAll() => Verify(SolveMode.FindAny);

        [MenuItem("Tools/Levels/Verify All Levels (Shortest)")]
        public static void VerifyShortest() => Verify(SolveMode.Shortest);

        static void Verify(SolveMode mode)
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
                var solved = LevelSolver.Solve(level, new SolveOptions { Mode = mode });
                Debug.Log(
                    "Уровень " + number
                    + " режим=" + mode
                    + " Solvable=" + solved.Solvable
                    + " ходов=" + solved.MinMoves
                    + " AlarmAtWin=" + solved.AlarmAtWin
                    + " состояния=" + solved.StatesExplored
                    + " мс=" + solved.ElapsedMs
                    + (solved.Truncated ? " Truncated" : "")
                    + (errors.Count == 0 ? "" : " проверка=" + string.Join("; ", errors)));
            }
        }
    }
}
