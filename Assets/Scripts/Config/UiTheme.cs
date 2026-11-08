using TMPro;
using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "UiTheme", menuName = "Scriptable Objects/UiTheme")]
    public sealed class UiTheme : ScriptableObject
    {
        public TMP_FontAsset Heading;
        public TMP_FontAsset Body;
        public Color Cream = new Color(0.953f, 0.898f, 0.710f, 1f);
        public Color Gold = new Color(0.910f, 0.769f, 0.408f, 1f);
        public Color Dim = new Color(0f, 0f, 0f, 0.6f);
    }
}
