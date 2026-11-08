using Game.Core;

namespace Game.Tests.EditMode
{
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
                    cells[x, y] = LevelMapParser.ParseCell(rows[y][x]);
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

    }
}
