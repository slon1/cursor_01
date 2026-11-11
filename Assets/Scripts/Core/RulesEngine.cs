using System;
using System.Collections.Generic;

namespace Game.Core
{
    public static class RulesEngine
    {
        public static MoveResult Move(GameState state, LionId lion, CellPos target)
        {
            if (state.Status != GameStatus.Playing)
                return Reject(RejectReason.GameOver);

            var origin = state.Position(lion);
            if (!state.InBounds(target))
                return Reject(RejectReason.OutOfBounds);

            if (!TryGetKind(lion, origin, target, out var kind, out var shapeReason))
                return Reject(shapeReason);

            var cell = state.CellAt(target);
            if (cell.Item == ItemType.Gate && lion == LionId.A && cell.IsOpen)
                return Reject(RejectReason.GateBlocksLionA);

            return ResolveLanding(state, lion, origin, target, kind);
        }

        public static ExtractResult ConfirmExtract(GameState state)
        {
            if (state.Status != GameStatus.Playing || !ExitConditionsMet(state))
                return new ExtractResult(false, Array.Empty<GameEvent>());

            state.Status = GameStatus.Won;
            return new ExtractResult(true, new GameEvent[] { new GameWon() });
        }

        public static IReadOnlyList<MoveTarget> GetTargets(GameState state, LionId lion)
        {
            if (state.Status != GameStatus.Playing)
                return Array.Empty<MoveTarget>();

            var origin = state.Position(lion);
            var targets = new List<MoveTarget>();
            AddRange(state, lion, origin, 1, MoveKind.Step, targets);
            if (lion == LionId.A)
                AddRange(state, lion, origin, 2, MoveKind.Jump, targets);

            return targets;
        }

        public static bool ExitConditionsMet(GameState state)
        {
            if (!state.ChestTaken || !TryFind(state, ItemType.Exit, out var exit))
                return false;

            return state.LionA == exit && state.LionB == exit && state.Gold >= state.MinCoins;
        }

        static MoveResult ResolveLanding(GameState state, LionId lion, CellPos origin, CellPos target, MoveKind kind)
        {
            var events = new List<GameEvent>();
            var alarmBefore = state.Alarm;
            var cell = state.CellAt(target);
            var startedClosed = !cell.IsOpen;
            var revealed = cell.Item;
            if (startedClosed)
            {
                cell.IsOpen = true;
                events.Add(new CellOpened(target, revealed));
            }

            var landed = false;
            var alarmCost = 0;
            var resetAlarm = false;
            switch (cell.Item)
            {
                case ItemType.Trap:
                    events.Add(new TrapTriggered(target, lion));
                    alarmCost += state.TrapCost;
                    break;
                case ItemType.Gate when lion == LionId.A:
                    events.Add(new GateBlocked(target, lion));
                    break;
                case ItemType.Gate:
                    cell.Item = ItemType.None;
                    events.Add(new GateBroken(target));
                    alarmCost += state.GateBreakCost;
                    break;
                case ItemType.Gold:
                    state.SetPosition(lion, target);
                    state.Gold += 1;
                    cell.Item = ItemType.None;
                    events.Add(new LionMoved(lion, origin, target, kind == MoveKind.Jump));
                    events.Add(new GoldCollected(target, lion, state.Gold));
                    landed = true;
                    break;
                case ItemType.Lever:
                    state.SetPosition(lion, target);
                    cell.Item = ItemType.None;
                    events.Add(new LionMoved(lion, origin, target, kind == MoveKind.Jump));
                    events.Add(new LeverUsed(target, lion));
                    landed = true;
                    resetAlarm = true;
                    break;
                case ItemType.Chest:
                case ItemType.Exit:
                case ItemType.None:
                    state.SetPosition(lion, target);
                    events.Add(new LionMoved(lion, origin, target, kind == MoveKind.Jump));
                    landed = true;
                    break;
            }

            if (landed
                && !state.ChestTaken
                && TryFind(state, ItemType.Chest, out var chest)
                && state.LionA == chest
                && state.LionB == chest)
            {
                state.CellAt(chest).Item = ItemType.None;
                state.ChestTaken = true;
                events.Add(new ChestTaken(chest, lion));
            }

            if (landed && kind == MoveKind.Jump)
                alarmCost += state.JumpCost;

            state.Alarm += alarmCost;
            if (resetAlarm)
                state.Alarm = 0;

            Finish(state, alarmBefore, events);
            return Accept(events);
        }

        static void Finish(GameState state, int alarmBefore, List<GameEvent> events)
        {
            if (state.Alarm != alarmBefore)
                events.Add(new AlarmChanged(alarmBefore, state.Alarm));

            if (state.Alarm >= state.AlarmThreshold)
            {
                state.Status = GameStatus.Lost;
                events.Add(new GameLost());
            }
        }

        static void AddRange(GameState state, LionId lion, CellPos origin, int distance, MoveKind kind, List<MoveTarget> targets)
        {
            Consider(state, lion, new CellPos(origin.X, origin.Y - distance), kind, targets);
            Consider(state, lion, new CellPos(origin.X + distance, origin.Y), kind, targets);
            Consider(state, lion, new CellPos(origin.X, origin.Y + distance), kind, targets);
            Consider(state, lion, new CellPos(origin.X - distance, origin.Y), kind, targets);
        }

        static void Consider(GameState state, LionId lion, CellPos pos, MoveKind kind, List<MoveTarget> targets)
        {
            if (state.InBounds(pos) && IsListed(state, lion, pos))
                targets.Add(new MoveTarget(pos, kind));
        }

        static bool IsListed(GameState state, LionId lion, CellPos pos)
        {
            var cell = state.CellAt(pos);
            return !(cell.IsOpen && lion == LionId.A && cell.Item == ItemType.Gate);
        }

        static bool TryGetKind(LionId lion, CellPos from, CellPos to, out MoveKind kind, out RejectReason reason)
        {
            kind = MoveKind.Step;
            reason = RejectReason.None;
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            if ((dx != 0 && dy != 0) || (dx == 0 && dy == 0))
            {
                reason = RejectReason.InvalidShape;
                return false;
            }

            var distance = Math.Abs(dx) + Math.Abs(dy);
            if (distance == 1)
                return true;

            if (distance == 2 && lion == LionId.A)
            {
                kind = MoveKind.Jump;
                return true;
            }

            reason = RejectReason.NotAdjacent;
            return false;
        }

        static bool TryFind(GameState state, ItemType item, out CellPos pos)
        {
            for (var x = 0; x < state.Width; x++)
            {
                for (var y = 0; y < state.Height; y++)
                {
                    pos = new CellPos(x, y);
                    if (state.CellAt(pos).Item == item)
                        return true;
                }
            }

            pos = default;
            return false;
        }

        static MoveResult Accept(List<GameEvent> events) =>
            new MoveResult(true, RejectReason.None, events);

        static MoveResult Reject(RejectReason reason) =>
            new MoveResult(false, reason, Array.Empty<GameEvent>());
    }
}
