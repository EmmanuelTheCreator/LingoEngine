namespace Director.Primitives
{
    public struct LingoPoint : IEquatable<LingoPoint>
    {
        public float X { get; set; }
        public float Y { get; set; }

        public LingoPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Offset(float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public static LingoPoint operator +(LingoPoint a, LingoPoint b) =>
            new(a.X + b.X, a.Y + b.Y);

        public static LingoPoint operator -(LingoPoint a, LingoPoint b) =>
            new(a.X - b.X, a.Y - b.Y);

        public static LingoPoint operator *(LingoPoint p, float scalar) =>
            new(p.X * scalar, p.Y * scalar);

        public static LingoPoint operator /(LingoPoint p, float scalar) =>
            new(p.X / scalar, p.Y / scalar);

        public override string ToString() => $"({X}, {Y})";

        public override bool Equals(object? obj) => obj is LingoPoint point && Equals(point);

        public bool Equals(LingoPoint other) => X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(LingoPoint left, LingoPoint right) => left.Equals(right);

        public static bool operator !=(LingoPoint left, LingoPoint right) => !(left == right);

        public static implicit operator LingoPoint((float x, float y) tuple)
       => new LingoPoint(tuple.x, tuple.y);
    }

}

