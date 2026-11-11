namespace Game.Core
{
    public enum TokenSlot
    {
        Center,
        Left,
        Right
    }

    public static class TokenSlots
    {
        public static TokenSlot GetSlot(LionId lion, CellPos posA, CellPos posB)
        {
            if (posA != posB)
                return TokenSlot.Center;

            return lion == LionId.A ? TokenSlot.Left : TokenSlot.Right;
        }
    }
}
