using Godot;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Primitives;

/// <summary>
/// Extensions for converting <see cref="LingoPixelFormat"/> to and from Godot formats.
/// </summary>
public static class LingoPixelFormatExtensions
{
    public static Image.Format ToGodotFormat(this LingoPixelFormat format) => format switch
    {
        LingoPixelFormat.Rgba8888 => Image.Format.Rgba8,
        LingoPixelFormat.Rgb888 => Image.Format.Rgb8,
        LingoPixelFormat.Rgb5650 => Image.Format.Rgb565,
        LingoPixelFormat.Rgb5550 => Image.Format.Rgb565,
        LingoPixelFormat.Rgba5551 => Image.Format.Rgba4444,
        LingoPixelFormat.Rgba4444 => Image.Format.Rgba4444,
        _ => Image.Format.Rgb8,
    };

    public static LingoPixelFormat ToLingoFormat(this Image.Format format) => format switch
    {
        Image.Format.Rgba8 => LingoPixelFormat.Rgba8888,
        Image.Format.Rgb8 => LingoPixelFormat.Rgb888,
        Image.Format.Rgb565 => LingoPixelFormat.Rgb5650,
        Image.Format.Rgba4444 => LingoPixelFormat.Rgba4444,
        _ => LingoPixelFormat.Rgb888,
    };
}
