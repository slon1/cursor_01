namespace Game.Core
{
    public sealed class GameState
    {
        readonly Cell[,] _cells;

        public CellPos LionA { get; internal set; }
        public CellPos LionB { get; internal set; }
        public bool ChestOpened { get; internal set; }
        public int Alarm { get; internal set; }
        public int Gold { get; internal set; }
        public GameStatus Status { get; internal set; }
        public int AlarmThreshold { get; }
        public int JumpCost { get; }
        public int GateBreakCost { get; }
        public int TrapCost { get; }
        public int MinCoins { get; }
        public int Width => _cells.GetLength(0);
        public int Height => _cells.GetLength(1);

        GameState(
            Cell[,] cells,
            CellPos lionA,
            CellPos lionB,
            int alarmThreshold,
            int jumpCost,
            int gateBreakCost,
            int trapCost,
            int minCoins)
        {
            _cells = cells;
            LionA = lionA;
            LionB = lionB;
            AlarmThreshold = alarmThreshold;
            JumpCost = jumpCost;
            GateBreakCost = gateBreakCost;
            TrapCost = trapCost;
            MinCoins = minCoins;
            Status = GameStatus.Playing;
        }

        public static GameState FromLevel(LevelData level)
        {
            var copy = new Cell[level.Width, level.Height];
            for (var x = 0; x < level.Width; x++)
            {
                for (var y = 0; y < level.Height; y++)
                    copy[x, y] = level.Cells[x, y].Clone();
            }

            return new GameState(
                copy,
                level.LionAStart,
                level.LionBStart,
                level.AlarmThreshold,
                level.JumpCost,
                level.GateBreakCost,
                level.TrapCost,
                level.MinCoins);
        }

        public Cell CellAt(CellPos pos) => _cells[pos.X, pos.Y];

        public bool InBounds(CellPos pos) =>
            pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;

        public CellPos Position(LionId lion) => lion == LionId.A ? LionA : LionB;

        public GameState Clone()
        {
            var copy = new Cell[Width, Height];
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                    copy[x, y] = _cells[x, y].Clone();
            }

            var clone = new GameState(copy, LionA, LionB, AlarmThreshold, JumpCost, GateBreakCost, TrapCost, MinCoins);
            clone.ChestOpened = ChestOpened;
            clone.Alarm = Alarm;
            clone.Gold = Gold;
            clone.Status = Status;
            return clone;
        }

        internal void SetPosition(LionId lion, CellPos pos)
        {
            if (lion == LionId.A)
                LionA = pos;
            else
                LionB = pos;
        }
    }
}
