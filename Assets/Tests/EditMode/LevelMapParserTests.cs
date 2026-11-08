using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelMapParserTests
    {
        [Test]
        public void ParseCell_CoversTheAlphabet()
        {
            AssertCell('.', true, ItemType.None);
            AssertCell('#', false, ItemType.None);
            AssertCell('T', true, ItemType.Trap);
            AssertCell('t', false, ItemType.Trap);
            AssertCell('R', true, ItemType.Gate);
            AssertCell('r', false, ItemType.Gate);
            AssertCell('L', true, ItemType.Lever);
            AssertCell('l', false, ItemType.Lever);
            AssertCell('G', true, ItemType.Gold);
            AssertCell('g', false, ItemType.Gold);
            AssertCell('C', true, ItemType.Chest);
            AssertCell('c', false, ItemType.Chest);
            AssertCell('E', true, ItemType.Exit);
            AssertCell('e', false, ItemType.Exit);
        }

        [Test]
        public void Parse_ReadsBothLions()
        {
            var level = LevelMapParser.Parse("  A.B  \n\n .cE ", 10, 2, 3, 4);

            Assert.AreEqual(3, level.Width);
            Assert.AreEqual(2, level.Height);
            Assert.AreEqual(new CellPos(0, 0), level.LionAStart);
            Assert.AreEqual(new CellPos(2, 0), level.LionBStart);
            Assert.IsTrue(level.Cells[0, 0].IsOpen);
            Assert.AreEqual(ItemType.None, level.Cells[0, 0].Item);
            Assert.IsFalse(level.Cells[1, 1].IsOpen);
            Assert.AreEqual(ItemType.Chest, level.Cells[1, 1].Item);
            Assert.AreEqual(ItemType.Exit, level.Cells[2, 1].Item);
            Assert.AreEqual(10, level.AlarmThreshold);
            Assert.AreEqual(2, level.JumpCost);
            Assert.AreEqual(3, level.GateBreakCost);
            Assert.AreEqual(4, level.TrapCost);
        }

        [Test]
        public void Parse_AcceptsSlashRows()
        {
            var level = LevelMapParser.Parse("A.B/.C.", 8, 1, 1, 1);
            Assert.AreEqual(new CellPos(0, 0), level.LionAStart);
            Assert.AreEqual(new CellPos(2, 0), level.LionBStart);
            Assert.AreEqual(ItemType.Chest, level.Cells[1, 1].Item);
        }

        [Test]
        public void UnevenRows_Throw()
        {
            var error = Assert.Throws<FormatException>(() => LevelMapParser.Parse("A.B/..", 10, 1, 1, 1));
            StringAssert.Contains("Ряд 2", error.Message);
        }

        [Test]
        public void MissingLionA_Throws()
        {
            var error = Assert.Throws<FormatException>(() => LevelMapParser.Parse("..B/.C.", 10, 1, 1, 1));
            StringAssert.Contains("льва A", error.Message);
        }

        [Test]
        public void TwoLionB_Throw()
        {
            var error = Assert.Throws<FormatException>(() => LevelMapParser.Parse("A.B/B.C", 10, 1, 1, 1));
            StringAssert.Contains("льва B", error.Message);
            StringAssert.Contains("ряду 2", error.Message);
            StringAssert.Contains("колонке 1", error.Message);
        }

        [Test]
        public void UnknownSymbol_ThrowsWithPosition()
        {
            var error = Assert.Throws<FormatException>(() => LevelMapParser.Parse("A?B/.C.", 10, 1, 1, 1));
            StringAssert.Contains("'?'", error.Message);
            StringAssert.Contains("ряду 1", error.Message);
            StringAssert.Contains("колонке 2", error.Message);
        }

        [Test]
        public void LowercaseLion_Throws()
        {
            var error = Assert.Throws<FormatException>(() => LevelMapParser.Parse("a.B/.C.", 10, 1, 1, 1));
            StringAssert.Contains("'a'", error.Message);
            StringAssert.Contains("ряду 1", error.Message);
            StringAssert.Contains("колонке 1", error.Message);
        }

        static void AssertCell(char symbol, bool isOpen, ItemType item)
        {
            var cell = LevelMapParser.ParseCell(symbol);
            Assert.AreEqual(isOpen, cell.IsOpen, symbol.ToString());
            Assert.AreEqual(item, cell.Item, symbol.ToString());
        }
    }
}
