namespace Game.Core
{
    public interface IProgressStorage
    {
        bool TryLoad(out int highestUnlockedLevel, out int balancePoints);
        void Save(int highestUnlockedLevel, int balancePoints);
    }
}
