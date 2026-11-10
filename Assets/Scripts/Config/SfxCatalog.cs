using UnityEngine;

namespace Game.Config
{
    public enum SfxId
    {
        Tap,
        Step,
        Jump,
        Open,
        Gold,
        Chest,
        Lever,
        Hit,
        Break,
        Win,
        Lose
    }

    [CreateAssetMenu(fileName = "SfxCatalog", menuName = "Scriptable Objects/SfxCatalog")]
    public sealed class SfxCatalog : ScriptableObject
    {
        public AudioClip Tap;
        public AudioClip Step;
        public AudioClip Jump;
        public AudioClip Open;
        public AudioClip Gold;
        public AudioClip Chest;
        public AudioClip Lever;
        public AudioClip Hit;
        public AudioClip Break;
        public AudioClip Win;
        public AudioClip Lose;

        public AudioClip Get(SfxId id)
        {
            switch (id)
            {
                case SfxId.Tap: return Tap;
                case SfxId.Step: return Step;
                case SfxId.Jump: return Jump;
                case SfxId.Open: return Open;
                case SfxId.Gold: return Gold;
                case SfxId.Chest: return Chest;
                case SfxId.Lever: return Lever;
                case SfxId.Hit: return Hit;
                case SfxId.Break: return Break;
                case SfxId.Win: return Win;
                case SfxId.Lose: return Lose;
                default: return null;
            }
        }
    }
}
