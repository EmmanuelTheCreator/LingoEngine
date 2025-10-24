using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlingoEngine.IO.Legacy.Texts.Data.Txc;

/// <summary>
/// Represents the decoded contents of a legacy <c>TXc</c> resource that stores a
/// pre-rendered text bitmap. The image keeps track of both the compressed payload and
/// the expanded pixel indices so higher layers can experiment with alternate decoders
/// without touching the original buffer again.
/// </summary>
public sealed class BlLegacyTxcImage
{
    public BlLegacyTxcImage(
        char variant,
        ushort width,
        ushort height,
        byte bitsPerPixel,
        BlLegacyTxcCompressionKind compression,
        int paletteOffset,
        int pixelDataOffset,
        ReadOnlyCollection<BlLegacyTxcPaletteEntry> palette,
        byte[] encodedPixels,
        byte[] decodedPixels,
        byte[] remainingData)
    {
        Variant = variant;
        Width = width;
        Height = height;
        BitsPerPixel = bitsPerPixel;
        Compression = compression;
        PaletteOffset = paletteOffset;
        PixelDataOffset = pixelDataOffset;
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
        EncodedPixels = encodedPixels ?? Array.Empty<byte>();
        Pixels = decodedPixels ?? Array.Empty<byte>();
        RemainingData = remainingData ?? Array.Empty<byte>();
    }

    /// <summary>Gets the character that follows the <c>TXc</c> tag (for example <c>'p'</c>).</summary>
    public char Variant { get; }

    /// <summary>Gets the bitmap width in pixels.</summary>
    public ushort Width { get; }

    /// <summary>Gets the bitmap height in pixels.</summary>
    public ushort Height { get; }

    /// <summary>Gets the number of bits per pixel after decoding. TXc resources that carry
    /// an indexed color table typically report eight bits per pixel.</summary>
    public byte BitsPerPixel { get; }

    /// <summary>Gets the compression scheme used to encode <see cref="EncodedPixels"/>.</summary>
    public BlLegacyTxcCompressionKind Compression { get; }

    /// <summary>Gets the offset of the QuickDraw color table within the original buffer.</summary>
    public int PaletteOffset { get; }

    /// <summary>Gets the offset of the encoded pixel payload within the original buffer.</summary>
    public int PixelDataOffset { get; }

    /// <summary>Gets the palette entries defined by the color table.</summary>
    public IReadOnlyList<BlLegacyTxcPaletteEntry> Palette { get; }

    /// <summary>Gets the compressed pixel payload as stored inside the resource.</summary>
    public byte[] EncodedPixels { get; }

    /// <summary>Gets the expanded pixel indices after decoding the payload.</summary>
    public byte[] Pixels { get; }

    /// <summary>Gets the trailing bytes that were not consumed by the decoder. Director
    /// often stores additional masks after the main image; the remainder keeps that data
    /// available for future decoders.</summary>
    public byte[] RemainingData { get; }
}
