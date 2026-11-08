using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelDataTests
    {
        [Test]
        public void ValidLevel_HasNoErrors()
        {
            var level = LevelBuilder.Data("C.../..../E...", 1, 1, 2, 1);
            Assert.IsEmpty(level.Validate());
        }

        [Test]
        public void Size_BelowTwo_IsReported()
        {
            var level = LevelBuilder.Data("C.E.", 1, 0, 3, 0);
            Assert.That(level.Validate(), Does.Contain("Размер поля меньше 2."));
        }

        [Test]
        public void AlarmThreshold_MustBePositive()
        {
            var level = LevelBuilder.Data("C.../..../E...", 1, 1, 2, 1, alarmThreshold: 0);
            Assert.That(level.Validate(), Does.Contain("Порог тревоги должен быть больше 0."));
        }

        [Test]
        public void JumpCost_CannotBeNegative()
        {
            var level = LevelBuilder.Data("C.../..../E...", 1, 1, 2, 1, jumpCost: -1);
            Assert.That(level.Validate(), Does.Contain("Стоимость прыжка отрицательная."));
        }

        [Test]
        public void GateCost_CannotBeNegative()
        {
            var level = LevelBuilder.Data("C.../..../E...", 1, 1, 2, 1, gateBreakCost: -1);
            Assert.That(level.Validate(), Does.Contain("Стоимость решётки отрицательная."));
        }

        [Test]
        public void TrapCost_CannotBeNegative()
        {
            var level = LevelBuilder.Data("C.../..../E...", 1, 1, 2, 1, trapCost: -1);
            Assert.That(level.Validate(), Does.Contain("Стоимость ловушки отрицательная."));
        }

        [Test]
        public void Chest_MustBeExactlyOne()
        {
            var none = LevelBuilder.Data("..../..../E...", 1, 1, 2, 1);
            var two = LevelBuilder.Data("C.../..../EC..", 1, 1, 2, 1);
            Assert.That(none.Validate(), Does.Contain("На поле должен быть ровно один сундук."));
            Assert.That(two.Validate(), Does.Contain("На поле должен быть ровно один сундук."));
        }

        [Test]
        public void Exit_MustBeExactlyOne()
        {
            var none = LevelBuilder.Data("C.../..../....", 1, 1, 2, 1);
            var two = LevelBuilder.Data("C.../..../EE..", 1, 1, 2, 1);
            Assert.That(none.Validate(), Does.Contain("На поле должен быть ровно один выход."));
            Assert.That(two.Validate(), Does.Contain("На поле должен быть ровно один выход."));
        }

        [Test]
        public void Lion_OutOfBounds_IsReported()
        {
            var level = LevelBuilder.Data("C.../..../E...", -1, 0, 2, 1);
            Assert.That(level.Validate(), Does.Contain("Лев A стоит вне поля."));
        }

        [Test]
        public void Lion_OnClosedCell_IsReported()
        {
            var level = LevelBuilder.Data("C.../#.../E...", 0, 1, 2, 1);
            Assert.That(level.Validate(), Does.Contain("Лев A стоит на закрытой клетке."));
        }

        [Test]
        public void Lion_OnItem_IsReported()
        {
            var level = LevelBuilder.Data("C.../..../E...", 0, 0, 2, 1);
            Assert.That(level.Validate(), Does.Contain("Лев A стоит на клетке с предметом."));
        }

        [Test]
        public void Lions_OnTheSameCell_AreReported()
        {
            var level = LevelBuilder.Data("C.../..../E...", 1, 1, 1, 1);
            Assert.That(level.Validate(), Does.Contain("Львы стоят в одной клетке."));
        }
    }
}
