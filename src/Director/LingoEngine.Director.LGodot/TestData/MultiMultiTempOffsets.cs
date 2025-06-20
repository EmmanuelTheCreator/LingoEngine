using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.TestData;

internal sealed class MultiMultiTempOffsets : XmedFileHints
{
    public MultiMultiTempOffsets()
    {
        StartOffset = 0x20E4;
        // highlight major blocks described in Text_Multi_Line_Multi_Style.md
        // default block and colour table
        AddBlock(0x20EC, 32, "color table");
        // style map table
        AddBlock(0x230C, 120, "style map table");

        // style descriptor blocks (second copy)
        AddStyleBlock(0x2680, 48, "style 0008");
        AddStyleBlock(0x289C, 48, "style 0006");
        AddStyleBlock(0x2946, 48, "style 000B");
        AddStyleBlock(0x1A30, 48, "style 0003"); // first copy
        AddStyleBlock(0x26A8, 48, "style 0005");

        // font strings after descriptors
        AddBlock(0x269C, 5, "font Arial");
        AddBlock(0x274A, 8, "font Arcade *");
        AddBlock(0x27F8, 5, "font arial");
        AddBlock(0x28A6, 6, "font Tahoma");
        AddBlock(0x2954, 8, "font Terminal");

        // trailing numeric tables
        AddBlock(0x29D0, 16, "numeric table");
        AddBlock(0x29E0, 16, "numeric table");
        AddBlock(0x2A20, 32, "numeric table");
    }
}
