namespace BlingoEngine.IO.Legacy.Texts.Data
{
    public sealed class XmedRect
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }
        public int Right { get; set; }

        public XmedRect()
        {
            
        }
        public XmedRect(int left, int top, int right, int bottom)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }
    }
}
