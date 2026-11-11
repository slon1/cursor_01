using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class TokenSlotsTests
    {
        [Test]
        public void DifferentCells_AreCentered()
        {
            var left = new CellPos(0, 0);
            var right = new CellPos(1, 0);

            Assert.AreEqual(TokenSlot.Center, TokenSlots.GetSlot(LionId.A, left, right));
            Assert.AreEqual(TokenSlot.Center, TokenSlots.GetSlot(LionId.B, left, right));
        }

        [Test]
        public void SharedCell_PutsAOnTheLeft_AndBOnTheRight()
        {
            var cell = new CellPos(2, 3);

            Assert.AreEqual(TokenSlot.Left, TokenSlots.GetSlot(LionId.A, cell, cell));
            Assert.AreEqual(TokenSlot.Right, TokenSlots.GetSlot(LionId.B, cell, cell));
        }
    }
}
