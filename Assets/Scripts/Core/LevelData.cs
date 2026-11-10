using System.Collections.Generic;

namespace Game.Core
{
    public sealed class LevelData
    {
        public int Width { get; }
        public int Height { get; }
        public Cell[,] Cells { get; }
        public CellPos LionAStart { get; }
        public CellPos LionBStart { get; }
        public int AlarmThreshold { get; }
        public int JumpCost { get; }
        public int GateBreakCost { get; }
        public int TrapCost { get; }
        public int MinCoins { get; }

        public LevelData(
            int width,
            int height,
            Cell[,] cells,
            CellPos lionAStart,
            CellPos lionBStart,
            int alarmThreshold,
            int jumpCost,
            int gateBreakCost,
            int trapCost,
            int minCoins = 0)
        {
            Width = width;
            Height = height;
            Cells = cells;
            LionAStart = lionAStart;
            LionBStart = lionBStart;
            AlarmThreshold = alarmThreshold;
            JumpCost = jumpCost;
            GateBreakCost = gateBreakCost;
            TrapCost = trapCost;
            MinCoins = minCoins;
        }

        public LevelData WithAlarmThreshold(int alarmThreshold) =>
            new LevelData(
                Width,
                Height,
                Cells,
                LionAStart,
                LionBStart,
                alarmThreshold,
                JumpCost,
                GateBreakCost,
                TrapCost,
                MinCoins);

        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (Width < 2 || Height < 2)
                errors.Add("Размер поля меньше 2.");

            if (AlarmThreshold <= 0)
                errors.Add("Порог тревоги должен быть больше 0.");

            if (JumpCost < 0)
                errors.Add("Стоимость прыжка отрицательная.");

            if (GateBreakCost < 0)
                errors.Add("Стоимость решётки отрицательная.");

            if (TrapCost < 0)
                errors.Add("Стоимость ловушки отрицательная.");

            if (MinCoins < 0)
                errors.Add("Минимальная добыча отрицательная.");

            var chest = 0;
            var exit = 0;
            var gold = 0;
            if (Cells != null && Cells.GetLength(0) == Width && Cells.GetLength(1) == Height)
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        var item = Cells[x, y].Item;
                        if (item == ItemType.Chest)
                            chest++;
                        else if (item == ItemType.Exit)
                            exit++;
                        else if (item == ItemType.Gold)
                            gold++;
                    }
                }
            }
            else
            {
                errors.Add("Поле клеток не совпадает с размером.");
            }

            if (chest != 1)
                errors.Add("На поле должен быть ровно один сундук.");

            if (exit != 1)
                errors.Add("На поле должен быть ровно один выход.");

            if (MinCoins > gold)
                errors.Add("Минимальная добыча больше числа монет на поле.");

            ValidateLion(LionAStart, "A", errors);
            ValidateLion(LionBStart, "B", errors);
            if (LionAStart == LionBStart)
                errors.Add("Львы стоят в одной клетке.");

            return errors;
        }

        void ValidateLion(CellPos pos, string name, List<string> errors)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= Width || pos.Y >= Height)
            {
                errors.Add("Лев " + name + " стоит вне поля.");
                return;
            }

            if (Cells == null || Cells.GetLength(0) != Width || Cells.GetLength(1) != Height)
                return;

            var cell = Cells[pos.X, pos.Y];
            if (!cell.IsOpen)
                errors.Add("Лев " + name + " стоит на закрытой клетке.");

            if (cell.Item != ItemType.None)
                errors.Add("Лев " + name + " стоит на клетке с предметом.");
        }
    }
}
