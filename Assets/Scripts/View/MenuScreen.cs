using System.Globalization;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class MenuScreen : MonoBehaviour, IAppScreen
    {
        [SerializeField] Button play;
        [SerializeField] Button rules;
        [SerializeField] Button settings;
        [SerializeField] TMP_Text haulValue;

        public void Open(AppContext app, object args)
        {
            play.onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.SelectLevel).Forget());
            rules.onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.Rules).Forget());
            settings.onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.Settings).Forget());

            haulValue.font = app.Theme.Heading;
            haulValue.color = app.Theme.Gold;
            haulValue.fontSize = 59f;
            haulValue.text = app.Progress.BalancePoints.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}
