using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    public struct SpriteCatalogEntry
    {
        public SpriteId Id;
        public Sprite Sprite;
    }

    [CreateAssetMenu(fileName = "SpriteCatalog", menuName = "Scriptable Objects/SpriteCatalog")]
    public class SpriteCatalog : ScriptableObject
    {
        public const int LevelCount = 10;

        public List<SpriteCatalogEntry> Entries = new List<SpriteCatalogEntry>();
        public Sprite[] LevelButtonOpen = new Sprite[LevelCount];
        public Sprite[] LevelButtonLocked = new Sprite[LevelCount];

        public Sprite Get(SpriteId id)
        {
            if (!TryGet(id, out var sprite))
                throw new InvalidOperationException("В SpriteCatalog не назначен спрайт " + id + ".");

            return sprite;
        }

        public bool TryGet(SpriteId id, out Sprite sprite)
        {
            if (Entries != null)
            {
                for (var i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].Id != id)
                        continue;

                    sprite = Entries[i].Sprite;
                    return sprite != null;
                }
            }

            sprite = null;
            return false;
        }

        public Sprite GetLevelButton(int level, bool locked)
        {
            if (level < 1 || level > LevelCount)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Уровень должен быть от 1 до 10.");

            if (locked && level == 1)
                return LevelButtonAt(LevelButtonOpen, 0);

            var source = locked ? LevelButtonLocked : LevelButtonOpen;
            return LevelButtonAt(source, level - 1);
        }

        static Sprite LevelButtonAt(Sprite[] source, int index)
        {
            if (source == null || index < 0 || index >= source.Length)
                throw new InvalidOperationException("Массив кнопок уровней в SpriteCatalog не заполнен.");

            return source[index];
        }
    }
}
