using System.Globalization;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class MenuScreen : MonoBehaviour, IAppScreen
    {
        public void Open(AppContext app, object args)
        {
            var stage = transform.Find("Stage");
            Button(stage, "Btn_Menu_Play").onClick.AddListener(() =>
            {
                // Play ведёт сразу на уровень 1, заменить на SelectLevel в 5B.
                app.Screens.ShowAsync(ScreenId.Game, app.Levels.Get(1), false).Forget();
            });
            Button(stage, "Btn_Menu_Rules").onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.Rules).Forget());
            Button(stage, "Btn_Menu_Settings").onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.Settings).Forget());

            var haul = stage.Find("HaulValue").GetComponent<TMP_Text>();
            haul.font = app.Theme.Heading;
            haul.color = app.Theme.Gold;
            haul.text = app.Progress.BalancePoints.ToString("N0", CultureInfo.InvariantCulture);
        }

        static Button Button(Transform stage, string name) => stage.Find(name).GetComponent<Button>();
    }
}
