using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class BackPolicyTests
    {
        [Test]
        public void Menu_Quits()
        {
            Assert.AreEqual(BackCommand.Quit, BackPolicy.Decide(new BackState { Place = AppPlace.Menu }));
        }

        [Test]
        public void SettingsAndRules_GoBack()
        {
            Assert.AreEqual(BackCommand.GoBack, BackPolicy.Decide(new BackState { Place = AppPlace.Settings }));
            Assert.AreEqual(BackCommand.GoBack, BackPolicy.Decide(new BackState { Place = AppPlace.Rules }));
        }

        [Test]
        public void SelectLevel_ClosesPopupBeforeLeaving()
        {
            Assert.AreEqual(BackCommand.ClosePopup, BackPolicy.Decide(new BackState
            {
                Place = AppPlace.SelectLevel,
                PopupOpen = true
            }));
            Assert.AreEqual(BackCommand.GoBack, BackPolicy.Decide(new BackState { Place = AppPlace.SelectLevel }));
        }

        [Test]
        public void Game_OpensPause_ThenResumes_AndIgnoresTheResult()
        {
            Assert.AreEqual(BackCommand.OpenPause, BackPolicy.Decide(new BackState { Place = AppPlace.Game }));
            Assert.AreEqual(BackCommand.Resume, BackPolicy.Decide(new BackState
            {
                Place = AppPlace.Game,
                PauseOpen = true
            }));
            Assert.AreEqual(BackCommand.Ignore, BackPolicy.Decide(new BackState
            {
                Place = AppPlace.Game,
                RoundOver = true,
                PauseOpen = true
            }));
        }

        [Test]
        public void TransitionOrMove_IgnoresEveryPlace()
        {
            Assert.AreEqual(BackCommand.Ignore, BackPolicy.Decide(new BackState
            {
                Place = AppPlace.Menu,
                TransitionBusy = true
            }));
            Assert.AreEqual(BackCommand.Ignore, BackPolicy.Decide(new BackState
            {
                Place = AppPlace.Game,
                MoveBusy = true
            }));
        }
    }
}
