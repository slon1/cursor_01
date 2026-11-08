using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "LevelSet", menuName = "Scriptable Objects/LevelSet")]
    public sealed class LevelSet : ScriptableObject
    {
        public List<LevelConfig> Levels = new List<LevelConfig>();

        public int Count => Levels == null ? 0 : Levels.Count;

        public bool Has(int number) =>
            number >= 1 && Levels != null && number <= Levels.Count && Levels[number - 1] != null;

        public LevelConfig Get(int number)
        {
            if (number < 1 || Levels == null || number > Levels.Count)
                throw new ArgumentOutOfRangeException(nameof(number), "Уровень " + number + " вне диапазона 1.." + Count + ".");

            var level = Levels[number - 1];
            if (level == null)
                throw new InvalidOperationException("Слот уровня " + number + " пуст.");

            return level;
        }
    }
}
