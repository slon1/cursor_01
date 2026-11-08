using System.Collections.Generic;

namespace Game.Core
{
    public sealed record MoveResult(bool Accepted, RejectReason Reason, IReadOnlyList<GameEvent> Events);

    public sealed record ExtractResult(bool Accepted, IReadOnlyList<GameEvent> Events);

    public sealed record MoveTarget(CellPos Pos, MoveKind Kind);
}
