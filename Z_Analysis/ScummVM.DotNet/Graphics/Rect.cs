using Director.IO;

namespace Director.Graphics
{
    public class Rect
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }
        public int Right { get; set; }

        public Rect()
        {
            Top = 0;
            Left = 0;
            Bottom = 0;
            Right = 0;
        }

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public bool IsEmpty => Width <= 0 || Height <= 0;

        public override string ToString() => $"Rect({Left}, {Top}, {Right}, {Bottom})";

        public static Rect ReadFrom(BinaryReader reader)
        {
            int top = reader.ReadInt16BE();
            int left = reader.ReadInt16BE();
            int bottom = reader.ReadInt16BE();
            int right = reader.ReadInt16BE();
            return new Rect(left, top, right, bottom);
        }
    }

}

