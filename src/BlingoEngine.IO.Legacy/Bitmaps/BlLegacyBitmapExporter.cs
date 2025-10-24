using System;

using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO.Legacy.Bitmaps;

/// <summary>
/// Converts legacy bitmap payloads into resource DTOs that the tooling layer can persist alongside
/// the movie data. The exporter maps the detected format into a representative file extension and
/// preserves the raw bytes streamed from the archive.
/// </summary>
public sealed class BlLegacyBitmapExporter
{
    /// <summary>
    /// Creates a <see cref="DirFileResourceDTO"/> that wraps the supplied bitmap payload.
    /// </summary>
    /// <param name="bitmap">Bitmap payload returned by the legacy reader.</param>
    /// <param name="castName">Logical name of the cast library that owns the member.</param>
    /// <param name="baseFileName">Base file name that will be combined with the detected extension.</param>
    /// <returns>A resource DTO ready to be added to a <see cref="DirFilesContainerDTO"/>.</returns>
    public DirFileResourceDTO CreateResource(BlLegacyBitmap bitmap, string castName, string baseFileName)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        ArgumentNullException.ThrowIfNull(castName);
        ArgumentNullException.ThrowIfNull(baseFileName);

        var extension = GetExtension(bitmap.Format);
        return new DirFileResourceDTO
        {
            CastName = castName,
            FileName = baseFileName + extension,
            Bytes = bitmap.Bytes,
            Kind = DirFileResourceKind.Bitmap
        };
    }

    /// <summary>
    /// Resolves the preferred file extension for the supplied bitmap format.
    /// </summary>
    /// <param name="format">Bitmap format detected by the legacy loader.</param>
    /// <returns>The file extension (including the dot) that matches the payload.</returns>
    public string GetExtension(BlLegacyBitmapFormatKind format)
    {
        return format switch
        {
            BlLegacyBitmapFormatKind.Bitd => ".bitd",
            BlLegacyBitmapFormatKind.Dib => ".bmp",
            BlLegacyBitmapFormatKind.Pict => ".pict",
            BlLegacyBitmapFormatKind.Png => ".png",
            BlLegacyBitmapFormatKind.Jpeg => ".jpg",
            BlLegacyBitmapFormatKind.Gif => ".gif",
            BlLegacyBitmapFormatKind.Bmp => ".bmp",
            BlLegacyBitmapFormatKind.Tiff => ".tiff",
            _ => ".bin"
        };
    }
}
