using System;
using System.IO;
using ProjectorRays.Common;

namespace ProjectorRays.Director;

public static class RaysFontMap
{
    public static BufferView GetFontMap(int version)
    {
        string file = version switch
        {
            >= 1150 => "fontmap_D11_5.txt",
            >= 1100 => "fontmap_D11.txt",
            >= 1000 => "fontmap_D10.txt",
            >= 900  => "fontmap_D9.txt",
            >= 850  => "fontmap_D8_5.txt",
            >= 800  => "fontmap_D8.txt",
            >= 700  => "fontmap_D7.txt",
            _ => "fontmap_D6.txt",
        };

        // Load the font mapping shipped with the library. The files live next
        // to the assembly inside the `fontmaps` directory so the package does
        // not depend on the original ProjectorRays sources.
        var baseDir = Path.GetDirectoryName(typeof(RaysFontMap).Assembly.Location) ??
                     AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "fontmaps", file);
        if (!File.Exists(path))
            return new BufferView(Array.Empty<byte>(), 0);
        var bytes = File.ReadAllBytes(path);
        return new BufferView(bytes, bytes.Length);
    }
}
