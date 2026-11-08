using System;

namespace Game.Core
{
    public readonly struct CellPos : IEquatable<CellPos>
    {
        public int X { get; }
        public int Y { get; }

        public CellPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(CellPos other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is CellPos other && Equals(other);

        public override int GetHashCode() => unchecked((X * 397) ^ Y);

        public static bool operator ==(CellPos left, CellPos right) => left.Equals(right);

        public static bool operator !=(CellPos left, CellPos right) => !left.Equals(right);
    }
}
