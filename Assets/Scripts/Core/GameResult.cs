namespace Game.Core
{
    public readonly struct GameResult
    {
        public bool Won { get; }
        public int Level { get; }
        public int Coins { get; }

        public GameResult(bool won, int level, int coins)
        {
            Won = won;
            Level = level;
            Coins = coins;
        }
    }
}
