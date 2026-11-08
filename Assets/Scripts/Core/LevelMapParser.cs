using System;
using System.Collections.Generic;

namespace Game.Core
{
    public static class LevelMapParser
    {
        public static Cell ParseCell(char symbol)
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
                default:
                    throw new FormatException("Неизвестный символ '" + symbol + "'.");
            }
        }

        public static LevelData Parse(string map, int alarmThreshold, int jumpCost, int gateBreakCost, int trapCost, int minCoins = 0)
        {
            var rows = SplitRows(map);
            if (rows.Count == 0)
                throw new FormatException("Карта пустая.");

            var width = rows[0].Length;
            if (width == 0)
                throw new FormatException("Карта пустая.");

            var height = rows.Count;
            var cells = new Cell[width, height];
            var lionA = new CellPos(-1, -1);
            var lionB = new CellPos(-1, -1);
            var hasA = false;
            var hasB = false;

            for (var y = 0; y < height; y++)
            {
                var row = rows[y];
                if (row.Length != width)
                    throw new FormatException("Ряд " + (y + 1) + " имеет длину " + row.Length + ", ожидалась " + width + ".");

                for (var x = 0; x < width; x++)
                {
                    var symbol = row[x];
                    if (symbol == 'A' || symbol == 'B')
                    {
                        if (symbol == 'A' && hasA)
                            throw new FormatException("Повторная стартовая клетка льва A в ряду " + (y + 1) + ", колонке " + (x + 1) + ".");
                        if (symbol == 'B' && hasB)
                            throw new FormatException("Повторная стартовая клетка льва B в ряду " + (y + 1) + ", колонке " + (x + 1) + ".");

                        if (symbol == 'A')
                        {
                            hasA = true;
                            lionA = new CellPos(x, y);
                        }
                        else
                        {
                            hasB = true;
                            lionB = new CellPos(x, y);
                        }

                        cells[x, y] = new Cell(true, ItemType.None);
                        continue;
                    }

                    try
                    {
                        cells[x, y] = ParseCell(symbol);
                    }
                    catch (FormatException)
                    {
                        throw new FormatException("Неизвестный символ '" + symbol + "' в ряду " + (y + 1) + ", колонке " + (x + 1) + ".");
                    }
                }
            }

            if (!hasA)
                throw new FormatException("В карте нет стартовой клетки льва A.");
            if (!hasB)
                throw new FormatException("В карте нет стартовой клетки льва B.");

            return new LevelData(width, height, cells, lionA, lionB, alarmThreshold, jumpCost, gateBreakCost, trapCost, minCoins);
        }

        static List<string> SplitRows(string map)
        {
            var rows = new List<string>();
            if (string.IsNullOrWhiteSpace(map))
                return rows;

            var normalized = map.Replace("\r\n", "\n").Replace('\r', '\n');
            var parts = normalized.Split(new[] { '/', '\n' }, StringSplitOptions.None);
            for (var i = 0; i < parts.Length; i++)
            {
                var row = parts[i].Trim();
                if (row.Length == 0)
                    continue;
                rows.Add(row);
            }

            return rows;
        }
    }
}
