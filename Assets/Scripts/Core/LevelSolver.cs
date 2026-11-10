using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game.Core
{
    public enum SolveMode
    {
        FindAny,
        Shortest
    }

    public sealed class SolveOptions
    {
        public SolveMode Mode;
        public int MaxStates = 5_000_000;
        public int TimeLimitMs = 60_000;
    }

    public readonly struct SolverMove
    {
        public LionId Lion { get; }
        public CellPos Target { get; }

        public SolverMove(LionId lion, CellPos target)
        {
            Lion = lion;
            Target = target;
        }
    }

    public sealed record SolveResult(
        bool Solvable,
        bool Truncated,
        int MinMoves,
        int AlarmAtWin,
        int StatesExplored,
        IReadOnlyList<SolverMove> Path,
        long ElapsedMs);

    public sealed record MinThresholdResult(int Value, bool Exact, bool Truncated);

    public static class LevelSolver
    {
        const int CellLimit = 64;

        public static SolveResult Solve(LevelData level, SolveOptions options)
        {
            if (options.MaxStates < 1)
                return Empty(true, 0, 0);

            if (level.Width * level.Height > CellLimit)
                throw new InvalidOperationException("Решатель держит маски клеток в ulong, поле больше 64 клеток.");

            var watch = Stopwatch.StartNew();
            return options.Mode == SolveMode.Shortest
                ? BreadthFirst(level, options, watch)
                : BestFirst(level, options, watch);
        }

        public static SolveResult TrySolve(LevelData level, int maxStates = 2_000_000) =>
            Solve(level, new SolveOptions { Mode = SolveMode.Shortest, MaxStates = maxStates });

        public static MinThresholdResult MinSolvableThreshold(LevelData level, int maxThreshold, SolveOptions options)
        {
            var truncated = false;
            for (var threshold = 1; threshold <= maxThreshold; threshold++)
            {
                var solved = Solve(level.WithAlarmThreshold(threshold), new SolveOptions
                {
                    Mode = SolveMode.FindAny,
                    MaxStates = options.MaxStates,
                    TimeLimitMs = options.TimeLimitMs
                });
                if (solved.Truncated)
                {
                    truncated = true;
                    continue;
                }

                if (solved.Solvable)
                    return new MinThresholdResult(threshold, !truncated, truncated);
            }

            return new MinThresholdResult(-1, false, truncated);
        }

        static SolveResult BestFirst(LevelData level, SolveOptions options, Stopwatch watch)
        {
            var goal = FindGoals(level);
            var board = GameState.FromLevel(level);
            var start = Capture(level, board);
            var bestAlarm = new Dictionary<StateKey, int> { { start, board.Alarm } };
            var nodes = new List<Node> { Node.Start(start, board.Alarm) };
            var stack = new Stack<int>();
            var children = new List<Child>();
            stack.Push(0);
            var sequence = 0;
            var truncated = false;

            while (stack.Count > 0)
            {
                if (watch.ElapsedMilliseconds >= options.TimeLimitMs)
                {
                    truncated = true;
                    break;
                }

                var index = stack.Pop();
                var node = nodes[index];
                if (!bestAlarm.TryGetValue(node.Key, out var known) || node.Alarm != known)
                    continue;

                children.Clear();
                LionId winLion = default;
                var winTarget = new CellPos();
                var winAlarm = 0;
                var winScore = int.MaxValue;
                var winSequence = int.MaxValue;
                var foundWin = false;
                for (var lionIndex = 0; lionIndex < 2; lionIndex++)
                {
                    var lion = lionIndex == 0 ? LionId.A : LionId.B;
                    Apply(board, level, node.Key, node.Alarm);
                    var targets = RulesEngine.GetTargets(board, lion);
                    for (var i = 0; i < targets.Count; i++)
                    {
                        var target = targets[i].Pos;
                        Apply(board, level, node.Key, node.Alarm);
                        var result = RulesEngine.Move(board, lion, target);
                        if (!result.Accepted)
                            continue;

                        var order = sequence++;
                        if (IsWinningExit(result.Events) && RulesEngine.ConfirmExtract(board).Accepted)
                        {
                            var score = node.Moves + 1;
                            if (score < winScore || (score == winScore && order < winSequence))
                            {
                                foundWin = true;
                                winLion = lion;
                                winTarget = target;
                                winAlarm = board.Alarm;
                                winScore = score;
                                winSequence = order;
                            }

                            continue;
                        }

                        if (board.Status == GameStatus.Lost)
                            continue;

                        var key = Capture(level, board);
                        if (bestAlarm.TryGetValue(key, out var seen) && board.Alarm >= seen)
                            continue;

                        if (!bestAlarm.ContainsKey(key) && bestAlarm.Count >= options.MaxStates)
                            return Empty(true, bestAlarm.Count, watch.ElapsedMilliseconds);

                        var alarm = board.Alarm;
                        bestAlarm[key] = alarm;
                        var moves = node.Moves + 1;
                        var child = nodes.Count;
                        nodes.Add(new Node(key, alarm, moves, index, lion, target));
                        children.Add(new Child(moves + 3 * Heuristic(level, goal, key), order, child));
                    }
                }

                if (foundWin)
                    return Win(nodes, index, winLion, winTarget, winAlarm, bestAlarm.Count, watch);

                children.Sort(CompareChildren);
                for (var i = children.Count - 1; i >= 0; i--)
                    stack.Push(children[i].Index);
            }

            return Empty(truncated, bestAlarm.Count, watch.ElapsedMilliseconds);
        }

        static int CompareChildren(Child left, Child right)
        {
            var score = left.Score.CompareTo(right.Score);
            return score != 0 ? score : left.Sequence.CompareTo(right.Sequence);
        }

        static SolveResult BreadthFirst(LevelData level, SolveOptions options, Stopwatch watch)
        {
            var board = GameState.FromLevel(level);
            var start = Capture(level, board);
            var seen = new HashSet<AlarmKey> { new AlarmKey(start, board.Alarm) };
            var nodes = new List<Node> { Node.Start(start, board.Alarm) };
            var queue = new Queue<int>();
            queue.Enqueue(0);

            while (queue.Count > 0)
            {
                if (watch.ElapsedMilliseconds >= options.TimeLimitMs)
                    return Empty(true, seen.Count, watch.ElapsedMilliseconds);

                var index = queue.Dequeue();
                var node = nodes[index];
                for (var lionIndex = 0; lionIndex < 2; lionIndex++)
                {
                    var lion = lionIndex == 0 ? LionId.A : LionId.B;
                    Apply(board, level, node.Key, node.Alarm);
                    var targets = RulesEngine.GetTargets(board, lion);
                    for (var i = 0; i < targets.Count; i++)
                    {
                        var target = targets[i].Pos;
                        Apply(board, level, node.Key, node.Alarm);
                        var result = RulesEngine.Move(board, lion, target);
                        if (!result.Accepted)
                            continue;

                        if (IsWinningExit(result.Events) && RulesEngine.ConfirmExtract(board).Accepted)
                            return Win(nodes, index, lion, target, board.Alarm, seen.Count, watch);

                        if (board.Status == GameStatus.Lost)
                            continue;

                        var alarm = board.Alarm;
                        var key = new AlarmKey(Capture(level, board), alarm);
                        if (seen.Contains(key))
                            continue;

                        if (seen.Count >= options.MaxStates)
                            return Empty(true, seen.Count, watch.ElapsedMilliseconds);

                        seen.Add(key);
                        var child = nodes.Count;
                        nodes.Add(new Node(key.Key, alarm, node.Moves + 1, index, lion, target));
                        queue.Enqueue(child);
                    }
                }
            }

            return Empty(false, seen.Count, watch.ElapsedMilliseconds);
        }

        static int Heuristic(LevelData level, Goals goal, StateKey key)
        {
            var deficit = level.MinCoins - RemovedGold(level, key.Removed);
            if (deficit < 0)
                deficit = 0;

            var focus = key.Chest ? goal.Exit : goal.Chest;
            return Approach(new CellPos(key.Ax, key.Ay), focus, level.Width, level.Height)
                + Approach(new CellPos(key.Bx, key.By), focus, level.Width, level.Height)
                + 2 * deficit;
        }

        static int Approach(CellPos from, CellPos goal, int width, int height)
        {
            var best = int.MaxValue;
            Consider(goal.X, goal.Y - 1);
            Consider(goal.X + 1, goal.Y);
            Consider(goal.X, goal.Y + 1);
            Consider(goal.X - 1, goal.Y);
            return best == int.MaxValue ? 0 : best;

            void Consider(int x, int y)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                    return;

                var distance = Math.Abs(from.X - x) + Math.Abs(from.Y - y);
                if (distance < best)
                    best = distance;
            }
        }

        static Goals FindGoals(LevelData level)
        {
            var chest = new CellPos(0, 0);
            var exit = new CellPos(0, 0);
            var foundChest = false;
            var foundExit = false;
            for (var y = 0; y < level.Height; y++)
            {
                for (var x = 0; x < level.Width; x++)
                {
                    var item = level.Cells[x, y].Item;
                    if (item == ItemType.Chest)
                    {
                        chest = new CellPos(x, y);
                        foundChest = true;
                    }
                    else if (item == ItemType.Exit)
                    {
                        exit = new CellPos(x, y);
                        foundExit = true;
                    }
                }
            }

            if (!foundChest || !foundExit)
                throw new InvalidOperationException("Решателю нужны сундук и выход на карте.");

            return new Goals(chest, exit);
        }

        static StateKey Capture(LevelData level, GameState state)
        {
            ulong removed = 0;
            var chestOpen = false;
            var bit = 0;
            for (var y = 0; y < level.Height; y++)
            {
                for (var x = 0; x < level.Width; x++)
                {
                    var cell = state.CellAt(new CellPos(x, y));
                    if (level.Cells[x, y].Item == ItemType.Chest)
                        chestOpen = cell.IsOpen;
                    if (level.Cells[x, y].Item != ItemType.None && cell.Item == ItemType.None)
                        removed |= 1UL << bit;
                    bit++;
                }
            }

            return new StateKey(
                chestOpen,
                removed,
                (byte)state.LionA.X,
                (byte)state.LionA.Y,
                (byte)state.LionB.X,
                (byte)state.LionB.Y,
                state.ChestTaken);
        }

        static void Apply(GameState state, LevelData level, StateKey key, int alarm)
        {
            state.SetPosition(LionId.A, new CellPos(key.Ax, key.Ay));
            state.SetPosition(LionId.B, new CellPos(key.Bx, key.By));
            state.ChestTaken = key.Chest;
            state.Alarm = alarm;
            state.Status = GameStatus.Playing;
            var gold = 0;
            var bit = 0;
            for (var y = 0; y < level.Height; y++)
            {
                for (var x = 0; x < level.Width; x++)
                {
                    var cell = state.CellAt(new CellPos(x, y));
                    var original = level.Cells[x, y].Item;
                    cell.IsOpen = original == ItemType.Chest && key.ChestOpen;
                    if ((key.Removed & (1UL << bit)) != 0)
                    {
                        if (original == ItemType.Gold)
                            gold++;
                        cell.Item = ItemType.None;
                    }
                    else
                        cell.Item = original;

                    bit++;
                }
            }

            state.Gold = gold;
        }

        static int RemovedGold(LevelData level, ulong removed)
        {
            var gold = 0;
            var bit = 0;
            for (var y = 0; y < level.Height; y++)
            {
                for (var x = 0; x < level.Width; x++)
                {
                    if (level.Cells[x, y].Item == ItemType.Gold && (removed & (1UL << bit)) != 0)
                        gold++;
                    bit++;
                }
            }

            return gold;
        }

        static bool IsWinningExit(IReadOnlyList<GameEvent> events)
        {
            for (var i = 0; i < events.Count; i++)
            {
                if (events[i] is ExitAttempted attempt && attempt.ConditionsMet)
                    return true;
            }

            return false;
        }

        static SolveResult Win(List<Node> nodes, int parent, LionId lion, CellPos target, int alarm, int states, Stopwatch watch)
        {
            var path = new List<SolverMove>();
            var index = parent;
            while (index >= 0 && nodes[index].Parent >= 0)
            {
                path.Add(new SolverMove(nodes[index].Lion, nodes[index].Target));
                index = nodes[index].Parent;
            }

            path.Reverse();
            path.Add(new SolverMove(lion, target));
            return new SolveResult(true, false, path.Count, alarm, states, path, watch.ElapsedMilliseconds);
        }

        static SolveResult Empty(bool truncated, int states, long elapsed) =>
            new SolveResult(false, truncated, 0, 0, states, Array.Empty<SolverMove>(), elapsed);

        readonly struct Goals
        {
            public readonly CellPos Chest;
            public readonly CellPos Exit;

            public Goals(CellPos chest, CellPos exit)
            {
                Chest = chest;
                Exit = exit;
            }
        }

        readonly struct Node
        {
            public readonly StateKey Key;
            public readonly int Alarm;
            public readonly int Moves;
            public readonly int Parent;
            public readonly LionId Lion;
            public readonly CellPos Target;

            public Node(StateKey key, int alarm, int moves, int parent, LionId lion, CellPos target)
            {
                Key = key;
                Alarm = alarm;
                Moves = moves;
                Parent = parent;
                Lion = lion;
                Target = target;
            }

            public static Node Start(StateKey key, int alarm) =>
                new Node(key, alarm, 0, -1, LionId.A, default);
        }

        readonly struct StateKey : IEquatable<StateKey>
        {
            public readonly bool ChestOpen;
            public readonly ulong Removed;
            public readonly byte Ax;
            public readonly byte Ay;
            public readonly byte Bx;
            public readonly byte By;
            public readonly bool Chest;

            public StateKey(bool chestOpen, ulong removed, byte ax, byte ay, byte bx, byte by, bool chest)
            {
                ChestOpen = chestOpen;
                Removed = removed;
                Ax = ax;
                Ay = ay;
                Bx = bx;
                By = by;
                Chest = chest;
            }

            public bool Equals(StateKey other) =>
                ChestOpen == other.ChestOpen
                && Removed == other.Removed
                && Ax == other.Ax
                && Ay == other.Ay
                && Bx == other.Bx
                && By == other.By
                && Chest == other.Chest;

            public override bool Equals(object obj) => obj is StateKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = ChestOpen ? 1 : 0;
                    hash = hash * 397 ^ (int)Removed ^ (int)(Removed >> 32);
                    hash = hash * 397 ^ Ax;
                    hash = hash * 397 ^ Ay;
                    hash = hash * 397 ^ Bx;
                    hash = hash * 397 ^ By;
                    hash = hash * 397 ^ (Chest ? 1 : 0);
                    return hash;
                }
            }
        }

        readonly struct AlarmKey : IEquatable<AlarmKey>
        {
            public readonly StateKey Key;
            public readonly int Alarm;

            public AlarmKey(StateKey key, int alarm)
            {
                Key = key;
                Alarm = alarm;
            }

            public bool Equals(AlarmKey other) => Alarm == other.Alarm && Key.Equals(other.Key);

            public override bool Equals(object obj) => obj is AlarmKey other && Equals(other);

            public override int GetHashCode() => unchecked(Key.GetHashCode() * 397 ^ Alarm);
        }

        readonly struct Child
        {
            public readonly int Score;
            public readonly int Sequence;
            public readonly int Index;

            public Child(int score, int sequence, int index)
            {
                Score = score;
                Sequence = sequence;
                Index = index;
            }
        }
    }
}
