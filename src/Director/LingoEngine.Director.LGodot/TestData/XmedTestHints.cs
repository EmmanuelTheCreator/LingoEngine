using System.Collections.Generic;
using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.TestData
{
    internal class XmedFileHints
    {
        public List<XmedBlock> Blocks = new();
        public int? StartOffset { get; set; }

        public void AddBlock(int start, int length, string desc, bool style = false)
        {
            int adjusted = StartOffset.HasValue ? start - StartOffset.Value : start;
            Blocks.Add(new SimpleBlock(adjusted, length, desc, style));
        }

        public void AddStyleBlock(int start, int length, string desc)
        {
            AddBlock(start, length, desc, true);
        }
    }

    internal static class XmedTestHints
    {
        public static readonly HalloTempOffsets HalloDefault;
        public static readonly MultiMultiTempOffsets MultiStyleMultiLine;
        public static readonly MultiSingleTempOffsets MultiStyleSingleLine;
        public static readonly XmedFileHints WiderWidth4;

        static XmedTestHints()
        {
            HalloDefault = new HalloTempOffsets();
            MultiStyleMultiLine = new MultiMultiTempOffsets();
            MultiStyleSingleLine = new MultiSingleTempOffsets();
            WiderWidth4 = new XmedFileHints();
            WiderWidth4.AddBlock(0x0018, 1, "width 4in");
        }
    }
}
