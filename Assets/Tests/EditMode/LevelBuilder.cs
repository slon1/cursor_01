using Game.Core;

namespace Game.Tests.EditMode
{
    // Ряд с y = 0 идёт первым. . открытая пустая, # закрытая пустая.
    // Заглавная буква — открытый предмет, строчная — закрытый: T ловушка, R решётка, L рубильник, G золото, C сундук, E выход.
    static class LevelBuilder
    {
        public static GameState Play(
            string map,
            int ax,
            int ay,
            int bx,
            int by,
            int alarmThreshold = 20,
            int jumpCost = 2,
            int gateBreakCost = 3,
            int trapCost = 4)
        {
            return GameState.FromLevel(Data(map, ax, ay, bx, by, alarmThreshold, jumpCost, gateBreakCost, trapCost));
        }

        public static LevelData Data(
            string map,
            int ax,
            int ay,
            int bx,
            int by,
            int alarmThreshold = 20,
            int jumpCost = 2,
            int gateBreakCost = 3,
            int trapCost = 4)
        {
            var rows = map.Split('/');
            var height = rows.Length;
            var width = rows[0].Length;
            var cells = new Cell[width, height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                    cells[x, y] = Parse(rows[y][x]);
            }

            return new LevelData(
                width,
                height,
                cells,
                new CellPos(ax, ay),
                new CellPos(bx, by),
                alarmThreshold,
                jumpCost,
                gateBreakCost,
                trapCost);
        }

        static Cell Parse(char symbol)
        {
            switch (symbol)
            {
                case '.': return new Cell(true, ItemType.None);
                case '#': return new Cell(false, ItemType.None);
                case 'T': return new Cell(true, ItemType.Trap);
                case 't': return new Cell(false, ItemType.Trap);
                case 'R': return new Cell(true, ItemType.Gate);
                case 'r': return new Cell(false, ItemType.Gate);
                case 'L': return new Cell(true, ItemType.Lever);
                case 'l': return new Cell(false, ItemType.Lever);
                case 'G': return new Cell(true, ItemType.Gold);
                case 'g': return new Cell(false, ItemType.Gold);
                case 'C': return new Cell(true, ItemType.Chest);
                case 'c': return new Cell(false, ItemType.Chest);
                case 'E': return new Cell(true, ItemType.Exit);
                case 'e': return new Cell(false, ItemType.Exit);
                default: throw new System.ArgumentException("Неизвестный символ карты: " + symbol);
            }
        }
    }
}
