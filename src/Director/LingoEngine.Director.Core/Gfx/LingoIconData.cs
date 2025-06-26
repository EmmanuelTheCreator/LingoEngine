using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Gfx;

/// <summary>
/// Raw icon pixel data used for rendering editor icons.
/// </summary>
public readonly record struct LingoIconData(byte[] Data, int Width, int Height, LingoPixelFormat Format);
