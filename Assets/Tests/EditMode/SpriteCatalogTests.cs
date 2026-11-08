using System;
using Game.Config;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class SpriteCatalogTests
    {
        const string CatalogPath = "Assets/ScriptableObjects/SpriteCatalog.asset";

        SpriteCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _catalog = AssetDatabase.LoadAssetAtPath<SpriteCatalog>(CatalogPath);
            Assert.IsNotNull(_catalog, "Нет " + CatalogPath + ". Запустите Tools/Sprites/Import Manifest.");
        }

        [Test]
        public void EverySpriteId_HasSprite()
        {
            foreach (SpriteId id in Enum.GetValues(typeof(SpriteId)))
            {
                Assert.IsTrue(_catalog.TryGet(id, out var sprite), id.ToString());
                Assert.IsNotNull(sprite, id.ToString());
                Assert.AreSame(sprite, _catalog.Get(id));
            }
        }

        [Test]
        public void LevelButtons_HaveExpectedSlots()
        {
            Assert.AreEqual(SpriteCatalog.LevelCount, _catalog.LevelButtonOpen.Length);
            Assert.AreEqual(SpriteCatalog.LevelCount, _catalog.LevelButtonLocked.Length);

            for (var i = 0; i < SpriteCatalog.LevelCount; i++)
                Assert.IsNotNull(_catalog.LevelButtonOpen[i], "Open " + (i + 1));

            Assert.IsNull(_catalog.LevelButtonLocked[0]);
            for (var i = 1; i < SpriteCatalog.LevelCount; i++)
                Assert.IsNotNull(_catalog.LevelButtonLocked[i], "Locked " + (i + 1));
        }

        [Test]
        public void GetLevelButton_ReturnsSlotOrThrows()
        {
            for (var level = 1; level <= SpriteCatalog.LevelCount; level++)
            {
                Assert.AreSame(_catalog.LevelButtonOpen[level - 1], _catalog.GetLevelButton(level, false));
                if (level == 1)
                    Assert.AreSame(_catalog.LevelButtonOpen[0], _catalog.GetLevelButton(1, true));
                else
                    Assert.AreSame(_catalog.LevelButtonLocked[level - 1], _catalog.GetLevelButton(level, true));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => _catalog.GetLevelButton(0, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => _catalog.GetLevelButton(11, true));
        }
    }
}
