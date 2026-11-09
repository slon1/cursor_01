namespace Game.Core
{
    public static class RunFlow
    {
        public static void Finish(PlayerProgress progress, IProgressStorage storage, bool won, int level, int coins, int pointsPerCoin)
        {
            if (!won)
                return;

            progress.CompleteLevel(level, coins, pointsPerCoin);
            storage.Save(progress.HighestUnlockedLevel, progress.BalancePoints);
        }

        public static bool ShowNext(int level, bool hasNext) => level >= 1 && hasNext;

        public static bool NodeOpen(PlayerProgress progress, bool hasConfig, int level) =>
            progress.IsUnlocked(level) && hasConfig;
    }
}
