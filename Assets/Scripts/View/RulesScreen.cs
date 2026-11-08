using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class RulesScreen : MonoBehaviour, IAppScreen
    {
        public void Open(AppContext app, object args)
        {
            transform.Find("Stage/Btn_Rules_Back").GetComponent<Button>().onClick.AddListener(() =>
                app.Screens.BackAsync().Forget());
        }
    }
}
