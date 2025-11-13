# Legacy Sound Loading Notes

[← Back to Docs Home](README.md)

The legacy IO layer now exposes a `BlLegacySoundReader` that resolves sound payloads across
all Director releases. Classic movies expose Mac `SND ` chunks, mid-era files list `snd `
children directly, and Director 6+ stores the edited bytes inside `ediM` resources. The reader
mirrors those layouts: it checks the `KEY*` relationships, chooses the most complete child
(`ediM` > `sndS` > `SND ` > `snd `), inflates Afterburner payloads when needed, and returns the
raw bytes alongside a lightweight `BlLegacySoundFormatKind` classification.

## Format classification

`BlLegacySoundFormat.Detect` performs a lightweight header inspection so higher layers can decide
how to replay or transcode the bytes. The classifier recognises:

- ID3 tags and MP3 frame sync words for MPEG Layer III streams.
- Big-endian RIFF variants (RIFF/FFIR/RIFX) for waveform data.
- FORM chunks that expand into AIFF or AIFC subtypes.
- Ogg, FLAC, AU, CAF, and MIDI signatures via their magic numbers.
- MPEG-4 audio when an `ftyp` box appears at the beginning of the buffer.

If no signature is found the loader reports the payload as `Unknown` so callers can fall back to
external probing tools.

## Director-era resource layouts

Reverse engineering of classic projectors shows a clear progression for how Macromedia packaged
sound members. Keeping these byte-level layouts documented ensures we preserve the same behaviour
when recreating the loader.

### Director 2–3 – implicit SND lookups

Sound members default to Macintosh `SND ` resources. The runtime derives the resource ID from the
cast member index and consults the platform sound manager for playback.

| Hex Bytes | Length | Purpose |
| --- | --- | --- |
| `<'S','N','D',' '>` | 4 bytes | Resource tag pointing to the legacy Mac sound chunk. |
| `<castId + libraryOffset>` | 2 bytes | Resource identifier derived from the cast member number. |

### Director 4–5 – explicit child resources

With the introduction of the `KEY*` table, sound members list their children directly. The loader
prefers the lowercase `snd ` entry but still honours uppercase `SND ` resources and falls back to
the derived Director 3 ID when no child exists.

| Hex Bytes | Length | Purpose |
| --- | --- | --- |
| `<'s','n','d',' '>` | 4 bytes | Preferred child tag that points to the stored sound bytes. |
| `<'S','N','D',' '>` | 4 bytes | Uppercase variant seen in older exports; treated the same as `snd `. |
| `<fallback castId>` | 2 bytes | Derived identifier used when no child resource is present. |

### Director 6 – MOA sound bundles

Director 6 migrated to MOA assets that split metadata, samples, and editor bytes across multiple
children. The modern loader mirrors that order when walking the resource table.

| Hex Bytes | Length | Purpose |
| --- | --- | --- |
| `<'s','n','d',' '>` | 4 bytes | Directory entry whose child list references the MOA pieces. |
| `<'s','n','d','H'>` | 4 bytes | Optional header describing compression, rate, and loop flags. |
| `<'s','n','d','S'>` | 4 bytes | Raw sample data used when no `ediM` stream is present. |
| `<'e','d','i','M'>` | 4 bytes | Edited sound bytes embedded by Director’s mixer. |

### Runtime fallback and linked files

When the archive lacks embedded data, movies may reference external assets. The loader mimics that
behaviour by returning an empty payload so higher layers can request the linked file. Loop metadata
remains tied to the decoded stream so the caller can decide whether to respect or ignore it.

| Bytes | Length | Purpose |
| --- | --- | --- |
| `<resource payload>` | variable | Raw bytes streamed from the archive or a linked file. |

[← Back to Docs Home](README.md)