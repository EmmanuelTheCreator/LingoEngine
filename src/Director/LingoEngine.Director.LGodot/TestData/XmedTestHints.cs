using System.Collections.Generic;

namespace LingoEngine.Director.LGodot.TestData
{
    internal class XmedFileHints
    {
        public Dictionary<int,string> Offsets = new();
        public HashSet<int> StyleBlocks = new();

        public void AddStyleBlock(int start, int length)
        {
            for (int i = 0; i < length; i++)
                StyleBlocks.Add(start + i);
        }
    }

    internal static class XmedTestHints
    {
        public static readonly XmedFileHints HalloDefault;
        public static readonly XmedFileHints MultiStyleMultiLine;
        public static readonly XmedFileHints MultiStyleSingleLine;
        public static readonly XmedFileHints WiderWidth4;

        static XmedTestHints()
        {
            HalloDefault = new XmedFileHints();
            HalloDefault.Offsets[0x0622] = "color table";
            HalloDefault.Offsets[0x0983] = "font name";
            HalloDefault.Offsets[0x0CAE] = "spacing before";
            HalloDefault.Offsets[0x0EF7] = "member name";
            HalloDefault.Offsets[0x1970] = "spacing after";

            MultiStyleMultiLine = new XmedFileHints();
            MultiStyleMultiLine.Offsets[0x16A8] = "style 0008";
            MultiStyleMultiLine.AddStyleBlock(0x16A8, 32);
            MultiStyleMultiLine.Offsets[0x18C4] = "style 0006";
            MultiStyleMultiLine.AddStyleBlock(0x18C4, 32);
            MultiStyleMultiLine.Offsets[0x196E] = "style 000B";
            MultiStyleMultiLine.AddStyleBlock(0x196E, 32);
            MultiStyleMultiLine.Offsets[0x1A30] = "style 0003";
            MultiStyleMultiLine.AddStyleBlock(0x1A30, 32);
            MultiStyleMultiLine.Offsets[0x26A8] = "style 0005";
            MultiStyleMultiLine.AddStyleBlock(0x26A8, 32);
            MultiStyleMultiLine.Offsets[0x1354] = "color table";

            MultiStyleSingleLine = new XmedFileHints();
            MultiStyleSingleLine.Offsets[0x1354] = "color table";
            WiderWidth4 = new XmedFileHints();
            WiderWidth4.Offsets[0x0018] = "width 4in";
        }
    }
}
