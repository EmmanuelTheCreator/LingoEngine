using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.TestData;

internal sealed class MultiMultiTempOffsets : XmedFileHints
{
    public MultiMultiTempOffsets()
    {
        StartOffset = 0x20E4;
        // style descriptor blocks after the text
        // offsets from docs/XMED_FileComparisons.md
        AddBlock(0x120A, 0x12A, "text copy 1");
        AddBlock(0x1334, 120, "style map table");
        AddStyleBlock(0x16A8, 48, "style 0008");
        AddStyleBlock(0x18C4, 48, "style 0006");
        AddStyleBlock(0x196E, 48, "style 000B");
        AddStyleBlock(0x1A30, 48, "style 0003");
        AddBlock(0x21E1, 0x12A, "text copy 2");
        AddBlock(0x230C, 120, "style map table");
        AddStyleBlock(0x26A8, 48, "style 0005");
    }
}
