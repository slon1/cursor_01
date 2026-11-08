using Game.Core;
using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        public int Number;
        [TextArea(5, 8)] public string Map;
        public int AlarmThreshold;
        public int JumpCost;
        public int GateBreakCost;
        public int TrapCost;

        public LevelData ToLevelData() =>
            LevelMapParser.Parse(Map, AlarmThreshold, JumpCost, GateBreakCost, TrapCost);
    }
}
