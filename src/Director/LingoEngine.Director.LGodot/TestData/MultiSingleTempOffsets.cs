using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.TestData;

internal sealed class MultiSingleTempOffsets : XmedFileHints
{
    public MultiSingleTempOffsets()
    {
        StartOffset = 0x110C;
        AddBlock(0x1354, 1, "color table");
    }
}
