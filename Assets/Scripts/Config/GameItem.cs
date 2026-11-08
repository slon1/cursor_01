using Game.Core;
using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "GameItem", menuName = "Scriptable Objects/GameItem")]
    public class GameItem : ScriptableObject
    {
        public Sprite sprite;
        public GameItemType type;
    }
}
