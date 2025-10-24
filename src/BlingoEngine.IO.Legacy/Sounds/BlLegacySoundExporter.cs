using System;

using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO.Legacy.Sounds;

/// <summary>
/// Converts legacy sound payloads into resource DTOs so tooling can persist the audio bytes next to
/// the imported movie. The exporter maps the detected format into a representative file extension.
/// </summary>
public sealed class BlLegacySoundExporter
{
    /// <summary>
    /// Creates a <see cref="DirFileResourceDTO"/> that wraps the supplied sound payload.
    /// </summary>
    /// <param name="sound">Sound payload returned by the legacy reader.</param>
    /// <param name="castName">Logical name of the cast library that owns the member.</param>
    /// <param name="baseFileName">Base file name that will be combined with the detected extension.</param>
    /// <returns>A resource DTO ready to be added to a <see cref="DirFilesContainerDTO"/>.</returns>
    public DirFileResourceDTO CreateResource(BlLegacySound sound, string castName, string baseFileName, int castLibNum, int numberInCast)
    {
        ArgumentNullException.ThrowIfNull(sound);
        ArgumentNullException.ThrowIfNull(castName);
        ArgumentNullException.ThrowIfNull(baseFileName);

        var extension = GetExtension(sound.Format);
        return new DirFileResourceDTO
        {
            CastName = castName,
            FileName = baseFileName + extension,
            Bytes = sound.Bytes,
            CastLibNum = castLibNum,
            NumberInCast = numberInCast,
            Kind = DirFileResourceKind.Sound
        };
    }

    /// <summary>
    /// Resolves the preferred file extension for the supplied sound format.
    /// </summary>
    /// <param name="format">Sound format detected by the legacy loader.</param>
    /// <returns>The file extension (including the dot) that matches the payload.</returns>
    public string GetExtension(BlLegacySoundFormatKind format)
    {
        return format switch
        {
            BlLegacySoundFormatKind.Mp3 => ".mp3",
            BlLegacySoundFormatKind.Wave => ".wav",
            BlLegacySoundFormatKind.Aiff => ".aiff",
            BlLegacySoundFormatKind.Aifc => ".aifc",
            BlLegacySoundFormatKind.Midi => ".mid",
            BlLegacySoundFormatKind.Caf => ".caf",
            BlLegacySoundFormatKind.Ogg => ".ogg",
            BlLegacySoundFormatKind.Flac => ".flac",
            BlLegacySoundFormatKind.Au => ".au",
            BlLegacySoundFormatKind.Mp4 => ".m4a",
            _ => ".snd"
        };
    }
}
