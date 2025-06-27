using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Importer.TestData;

internal sealed class HalloTempOffsets : XmedFileHints
{
    public HalloTempOffsets()
    {
        StartOffset = 0x061A;
        AddBlock(0x0622, 1, "color table");
        AddBlock(0x0983, 1, "font name");
        AddBlock(0x0CAE, 1, "spacing before");
        AddBlock(0x0EF7, 1, "member name");
        AddBlock(0x1970, 1, "spacing after");
    }
}
