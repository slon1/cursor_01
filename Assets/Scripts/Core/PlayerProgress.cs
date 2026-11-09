namespace Game.Core
{
    public sealed class PlayerProgress
    {
        public int HighestUnlockedLevel { get; private set; } = 1;
        public int BalancePoints { get; private set; }

        public void Restore(int highestUnlockedLevel, int balancePoints)
        {
            HighestUnlockedLevel = highestUnlockedLevel < 1 ? 1 : highestUnlockedLevel;
            BalancePoints = balancePoints < 0 ? 0 : balancePoints;
        }

        public void CompleteLevel(int level, int coins, int pointsPerCoin)
        {
            var next = level + 1;
            if (next > HighestUnlockedLevel)
                HighestUnlockedLevel = next;

            BalancePoints += coins * pointsPerCoin;
        }

        public bool IsUnlocked(int level) => level >= 1 && level <= HighestUnlockedLevel;
    }
}
