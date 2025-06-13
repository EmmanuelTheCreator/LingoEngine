namespace Director.Primitives
{
    using System;
    using System.Globalization;

    public struct LingoRect : IEquatable<LingoRect>
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public LingoRect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public float Width => Right - Left;
        public float Height => Bottom - Top;
        public LingoPoint TopLeft => new(Left, Top);
        public LingoPoint BottomRight => new(Right, Bottom);
        public LingoPoint Center => new((Left + Right) / 2, (Top + Bottom) / 2);

        public void Offset(float dx, float dy)
        {
            Left += dx;
            Right += dx;
            Top += dy;
            Bottom += dy;
        }

        public bool Contains(LingoPoint point) =>
            point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

        public bool Intersects(LingoRect other) =>
            !(Right < other.Left || Left > other.Right || Bottom < other.Top || Top > other.Bottom);

        public LingoRect Intersect(LingoRect other)
        {
            if (!Intersects(other)) return default;
            return new LingoRect(
                Math.Max(Left, other.Left),
                Math.Max(Top, other.Top),
                Math.Min(Right, other.Right),
                Math.Min(Bottom, other.Bottom)
            );
        }

        public LingoRect Union(LingoRect other)
        {
            return new LingoRect(
                Math.Min(Left, other.Left),
                Math.Min(Top, other.Top),
                Math.Max(Right, other.Right),
                Math.Max(Bottom, other.Bottom)
            );
        }

        public LingoRect Inset(float dx, float dy)
        {
            return new LingoRect(Left + dx, Top + dy, Right - dx, Bottom - dy);
        }

        public override string ToString() =>
            $"Rect({Left}, {Top}, {Right}, {Bottom})";

        public string ToCsv() =>
            $"{Left.ToString(CultureInfo.InvariantCulture)}," +
            $"{Top.ToString(CultureInfo.InvariantCulture)}," +
            $"{Right.ToString(CultureInfo.InvariantCulture)}," +
            $"{Bottom.ToString(CultureInfo.InvariantCulture)}";

        public static LingoRect Parse(string csv)
        {
            var parts = csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(p => p.Trim())
               .ToArray();
            if (parts.Length != 4)
                throw new FormatException("LingoRect.Parse expects 4 comma-separated values.");

            return new LingoRect(
                float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture),
                float.Parse(parts[3], CultureInfo.InvariantCulture)
            );
        }

        public override bool Equals(object? obj) => obj is LingoRect rect && Equals(rect);

        public bool Equals(LingoRect other) =>
            Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

        public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

        public static bool operator ==(LingoRect left, LingoRect right) => left.Equals(right);
        public static bool operator !=(LingoRect left, LingoRect right) => !(left == right);
    }

}