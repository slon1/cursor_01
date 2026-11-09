using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "GameSettingsConfig", menuName = "Scriptable Objects/GameSettingsConfig")]
    public sealed class GameSettingsConfig : ScriptableObject
    {
        public float FadeDuration = 0.3f;
        public float MoveDuration = 0.18f;
        public float PopupDuration = 0.25f;
        public float RevealPause = 0.45f;
        public float ConsumePause = 0.2f;
        public int GoldPointsPerCoin = 100;
        public float HighlightAlpha = 0.5f;
        public bool DefaultSoundOn = true;
        public bool DefaultMusicOn = true;
    }
}
