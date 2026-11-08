using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class EditModeRunnerTests
    {
        [Test]
        public void EditModeRunner_IsWired()
        {
            Assert.IsNotNull(typeof(GameItemType));
        }
    }
}
