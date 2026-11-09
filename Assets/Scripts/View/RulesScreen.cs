using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class RulesScreen : MonoBehaviour, IAppScreen
    {
        [SerializeField] Button back;

        public void Open(AppContext app, object args)
        {
            back.onClick.AddListener(() => app.Screens.BackAsync().Forget());
        }
    }
}
