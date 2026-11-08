using System;
using System.Collections.Generic;

namespace Game.Core
{
    public sealed class GameSession
    {
        readonly LevelData _level;

        public GameState State { get; private set; }
        public LionId ActiveLion { get; private set; }
        public int TotalCoins { get; }
        public int MinCoins => _level.MinCoins;
        public int CollectedCoins => State.Gold;

        public IReadOnlyList<MoveTarget> Targets =>
            State.Status == GameStatus.Playing
                ? RulesEngine.GetTargets(State, ActiveLion)
                : Array.Empty<MoveTarget>();

        public bool ExitReady =>
            State.Status == GameStatus.Playing && RulesEngine.ExitConditionsMet(State);

        public GameSession(LevelData level)
        {
            _level = level;
            TotalCoins = CountGold(level);
            Restart();
        }

        public void Restart()
        {
            State = GameState.FromLevel(_level);
            ActiveLion = LionId.A;
        }

        public void SelectLion(LionId lion) => ActiveLion = lion;

        public MoveResult TapCell(CellPos pos)
        {
            var targets = Targets;
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].Pos != pos)
                    continue;

                return RulesEngine.Move(State, ActiveLion, pos);
            }

            return new MoveResult(false, RejectReason.NotAdjacent, Array.Empty<GameEvent>());
        }

        public ExtractResult TryExtract() => RulesEngine.ConfirmExtract(State);

        static int CountGold(LevelData level)
        {
            var count = 0;
            for (var x = 0; x < level.Width; x++)
            {
                for (var y = 0; y < level.Height; y++)
                {
                    if (level.Cells[x, y].Item == ItemType.Gold)
                        count++;
                }
            }

            return count;
        }
    }
}
