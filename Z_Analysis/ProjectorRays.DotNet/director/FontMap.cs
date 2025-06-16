using ProjectorRays.Common;

namespace ProjectorRays.Director;

public static class FontMap
{
    public static BufferView GetFontMap(int version)
    {
        // This is a placeholder implementation that selects a bundled font map
        // based on the Director version. The original C++ version uses static
        // byte arrays defined in separate header files.
        // TODO: load actual font map data.
        return new BufferView(Array.Empty<byte>(), 0);
    }
}
