using System.Collections.Generic;

namespace Game.Core
{
    public sealed record SolveResult(bool Solvable, bool Truncated, int MinMoves, int MinAlarmAtWin, int StatesExplored);

    public static class LevelSolver
    {
        public static SolveResult TrySolve(LevelData level, int maxStates = 2_000_000)
        {
            if (maxStates < 1)
                return new SolveResult(false, true, 0, 0, 0);

            var start = GameState.FromLevel(level);
            var visited = new HashSet<string> { Key(start) };
            var queue = new Queue<SearchNode>();
            queue.Enqueue(new SearchNode(start, 0));

            var solvable = false;
            var truncated = false;
            var bestMoves = int.MaxValue;
            var bestAlarm = int.MaxValue;

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Moves >= bestMoves)
                    break;

                if (!Expand(node, maxStates, visited, queue, ref solvable, ref truncated, ref bestMoves, ref bestAlarm))
                    break;
            }

            return new SolveResult(
                solvable,
                truncated,
                solvable ? bestMoves : 0,
                solvable ? bestAlarm : 0,
                visited.Count);
        }

        static bool Expand(
            SearchNode node,
            int maxStates,
            HashSet<string> visited,
            Queue<SearchNode> queue,
            ref bool solvable,
            ref bool truncated,
            ref int bestMoves,
            ref int bestAlarm)
        {
            if (!TryLion(node, LionId.A, maxStates, visited, queue, ref solvable, ref truncated, ref bestMoves, ref bestAlarm))
                return false;
            return TryLion(node, LionId.B, maxStates, visited, queue, ref solvable, ref truncated, ref bestMoves, ref bestAlarm);
        }

        static bool TryLion(
            SearchNode node,
            LionId lion,
            int maxStates,
            HashSet<string> visited,
            Queue<SearchNode> queue,
            ref bool solvable,
            ref bool truncated,
            ref int bestMoves,
            ref int bestAlarm)
        {
            var targets = RulesEngine.GetTargets(node.State, lion);
            for (var i = 0; i < targets.Count; i++)
            {
                var next = node.State.Clone();
                var result = RulesEngine.Move(next, lion, targets[i].Pos);
                if (!result.Accepted)
                    continue;

                if (IsWinningExit(result.Events))
                {
                    var alarm = next.Alarm;
                    if (RulesEngine.ConfirmExtract(next).Accepted)
                        ConsiderWin(node.Moves + 1, alarm, ref solvable, ref bestMoves, ref bestAlarm);
                    continue;
                }

                if (next.Status == GameStatus.Lost)
                    continue;

                if (solvable && node.Moves + 1 >= bestMoves)
                    continue;

                var key = Key(next);
                if (visited.Contains(key))
                    continue;

                if (visited.Count >= maxStates)
                {
                    truncated = true;
                    return false;
                }

                visited.Add(key);
                queue.Enqueue(new SearchNode(next, node.Moves + 1));
            }

            return true;
        }

        static void ConsiderWin(int moves, int alarm, ref bool solvable, ref int bestMoves, ref int bestAlarm)
        {
            if (!solvable || moves < bestMoves || (moves == bestMoves && alarm < bestAlarm))
            {
                solvable = true;
                bestMoves = moves;
                bestAlarm = alarm;
            }
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

        static string Key(GameState state)
        {
            var text = state.LionA.X + "," + state.LionA.Y + ";"
                + state.LionB.X + "," + state.LionB.Y + ";"
                + (state.ChestOpened ? "1" : "0") + ";"
                + state.Alarm + ";"
                + state.Gold + ";";
            for (var y = 0; y < state.Height; y++)
            {
                for (var x = 0; x < state.Width; x++)
                {
                    var cell = state.CellAt(new CellPos(x, y));
                    text += cell.IsOpen ? "1" : "0";
                    text += (int)cell.Item;
                }
            }

            return text;
        }

        readonly struct SearchNode
        {
            public GameState State { get; }
            public int Moves { get; }

            public SearchNode(GameState state, int moves)
            {
                State = state;
                Moves = moves;
            }
        }
    }
}
