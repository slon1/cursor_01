namespace Game.Core
{
    public enum ItemType
    {
        None,
        Trap,
        Gate,
        Lever,
        Gold,
        Chest,
        Exit
    }

    public enum LionId
    {
        A,
        B
    }

    public enum GameStatus
    {
        Playing,
        Won,
        Lost
    }

    public enum MoveKind
    {
        Step,
        Jump
    }

    public enum RejectReason
    {
        None,
        OutOfBounds,
        NotAdjacent,
        GameOver,
        GateBlocksLionA,
        InvalidShape
    }
}
