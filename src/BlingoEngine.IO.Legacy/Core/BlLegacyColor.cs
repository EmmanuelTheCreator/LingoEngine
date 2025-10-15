namespace BlingoEngine.IO.Legacy.Core;

public struct BlLegacyColor
{
    /// <summary>Red component (0–255)</summary>
    public byte R { get; }

    /// <summary>Green component (0–255)</summary>
    public byte G { get; }

    /// <summary>Blue component (0–255)</summary>
    public byte B { get; }
    public bool IsIndex{ get; }

    public BlLegacyColor(byte indexColor)
    {
        IsIndex = true;
        R = indexColor;
        G = indexColor;
        B = indexColor;
    }
  
    public BlLegacyColor(byte r, byte g, byte b)
    {
        IsIndex = false;
        R = r;
        G = g;
        B = b;
    }
  

    /// <summary>
    /// Converts the color to a 24-bit packed RGB value (0xRRGGBB).
    /// </summary>
    public uint ToRgb24()
    {
        return (uint)(R << 16 | G << 8 | B);
    }

    /// <summary>
    /// Constructs a BlingoColor from a 16-bit RGB 555 value (0RRRRRGGGGGBBBBB).
    /// </summary>
    /// <param name="rgb555">The 16-bit color value.</param>
    public BlLegacyColor(ushort rgb555)
    {
        R = (byte)((rgb555 >> 10 & 0x1F) * 255 / 31);
        G = (byte)((rgb555 >> 5 & 0x1F) * 255 / 31);
        B = (byte)((rgb555 & 0x1F) * 255 / 31);
    }


    /// <summary>
    /// Converts the color to a packed 16-bit RGB 555 format.
    /// </summary>
    public ushort ToRgb555()
    {
        return (ushort)(
            R * 31 / 255 << 10 |
            G * 31 / 255 << 5 |
            B * 31 / 255);
    }

    /// <summary>
    /// Returns a string representation of the color in RGB format.
    /// </summary>
    public override string ToString() => $"RGB({R}, {G}, {B})";



    /// <summary>
    /// Converts the RGB color to a hex string, e.g., "#FF0000".
    /// </summary>
    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";




    /// <summary>
    /// Creates a BlingoColor from an RGB tuple.
    /// </summary>
    public static BlLegacyColor FromRGB(byte r, byte g, byte b)
        => new BlLegacyColor(r, g, b);
}

