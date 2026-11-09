namespace Game.Core
{
    public abstract record GameEvent;

    public sealed record LionMoved(LionId Lion, CellPos From, CellPos To, bool IsJump) : GameEvent;

    public sealed record LionsSwapped(CellPos PosA, CellPos PosB, LionId Initiator, bool IsJump) : GameEvent;

    public sealed record CellOpened(CellPos Pos) : GameEvent;

    public sealed record TrapTriggered(CellPos Pos, LionId Lion) : GameEvent;

    public sealed record GateBlocked(CellPos Pos, LionId Lion) : GameEvent;

    public sealed record GateBroken(CellPos Pos) : GameEvent;

    public sealed record GoldCollected(CellPos Pos, LionId Lion, int Total) : GameEvent;

    public sealed record LeverUsed(CellPos Pos, LionId Lion) : GameEvent;

    public sealed record AlarmChanged(int OldValue, int NewValue) : GameEvent;

    public sealed record ChestTaken(CellPos Pos, LionId Lion) : GameEvent;

    public sealed record ExitAttempted(bool ConditionsMet) : GameEvent;

    public sealed record GameWon : GameEvent;

    public sealed record GameLost : GameEvent;
}
