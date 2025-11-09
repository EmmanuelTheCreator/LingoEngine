# Member Bitmap Format: Director Pre Version 10

[← Documentation Overview](README.md)

Legacy Director movies store bitmap members in a mix of QuickDraw BITD chunks, Windows DIB streams, and the authoring metadata blocks that Director MX introduced. This note stitches together the behaviours implemented by `BlLegacyBitmapReader` and `BlLegacyBitmapFormat` so the loader remains compatible with every Director generation. The byte layouts were transcribed from the archived interpreter notes derived from ScummVM commit `4eb06084a8449d8c8e5060d8611bd101c6b39cee`, rewritten here without any external links.

## Resource tags and container detection

| Tag    | Purpose | Detection outcome |
| ------ | ------- | ----------------- |
| `ediM` | Authoring metadata that often embeds PNG, JPEG, or DIB streams. If the payload cannot be classified it is ignored so the reader can fall back to the classic bitmap chunk. | Signature inspection on the payload bytes (PNG, JPEG, GIF, BMP, or TIFF magic). |
| `BITD` | Classic Macintosh bitmap data with RLE segments. | Classified as `Bitd` immediately from the tag. |
| `DIB ` | Windows device-independent bitmap stored alongside the original palette. | Classified as `Dib` from the tag, while the byte layout mirrors the BITMAPINFOHEADER table below. |
| `PICT` | QuickDraw PICT drawing. | Classified as `Pict` from the tag; payload bytes are forwarded as-is. |
| `ALFA` | Standalone alpha mask chunk. | Classified as `AlphaMask`. |
| `Thum` | Thumbnail image saved by the authoring tool. | Classified as `Thumbnail`. |
| Other tags beginning with `PNG`, `JPG`, `JPEG`, `JFIF`, `GIF`, `BMP`, or `TIF` | Director and Shockwave occasionally emit chunks that use the container name directly. | Classified via signature inspection of the payload bytes. |

## Director cast-member metadata

The following tables capture the bytes Director stores in bitmap cast-member records. Each section corresponds to the version gates used by the classic Director executables.

### Director 3 and earlier (version < 0x400)

`BitmapCastMember` reads a fixed sequence of 16-bit values. When the high bit of the size word is set the record appends palette metadata so the bitmap can bind to a CLUT.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<cast data size>` | 2 bytes | Total size of the bitmap record; bit `0x8000` flags an inline palette reference. |
| `<rect.top>` | 2 bytes | Signed top coordinate read through the standard rectangle helper. |
| `<rect.left>` | 2 bytes | Signed left coordinate for `_initialRect`. |
| `<rect.bottom>` | 2 bytes | Signed bottom coordinate for `_initialRect`. |
| `<rect.right>` | 2 bytes | Signed right coordinate for `_initialRect`. |
| `<bounds.top>` | 2 bytes | Top of `_boundingRect`. |
| `<bounds.left>` | 2 bytes | Left of `_boundingRect`. |
| `<bounds.bottom>` | 2 bytes | Bottom of `_boundingRect`. |
| `<bounds.right>` | 2 bytes | Right of `_boundingRect`. |
| `<regY>` | 2 bytes | Signed registration offset on the Y axis. |
| `<regX>` | 2 bytes | Signed registration offset on the X axis. |
| `<bits per pixel>` | 2 bytes (optional) | Present when `cast data size` has bit `0x8000` set; records the pixel depth. |
| `<CLUT id>` | 2 bytes (optional) | Signed palette resource number; negative or zero references the built-in system palettes. |

### Director 4–5 (0x400 ≤ version < 0x600)

Director 4 switches to a longer record that starts with a packed pitch word. Optional bytes attach palette metadata and Director 5 adds a cast-library selector alongside reserved fields.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<pitch word>` | 2 bytes | Upper nibble carries the color-flag bit; lower 12 bits store the pixel pitch. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values for `_initialRect`. |
| `<bounds.top>`..`<bounds.right>` | 8 bytes | Bounding rectangle stored in the same layout. |
| `<regY>` | 2 bytes | Registration offset on Y. |
| `<regX>` | 2 bytes | Registration offset on X. |
| `<unknown padding>` | 1 byte (optional) | Present when the record exceeds 22 bytes; historically used as a flag byte. |
| `<bits per pixel>` | 1 byte (optional) | Pixel depth stored after the padding byte. |
| `<CLUT cast library>` | 2 bytes (optional, Director 5+) | Library ID for the palette when version ≥ 0x500. |
| `<CLUT id>`   | 2 bytes (optional) | Palette resource number; non-positive values select built-ins. |
| `<unknown16>` | 2 bytes (optional) | Additional metadata observed in longer Director 4/5 records. |
| `<unknown16>` | 2 bytes (optional) | Second 16-bit value retained for completeness. |
| `<unknown16>` | 2 bytes (optional) | Third 16-bit value preceding the 32-bit fields. |
| `<unknown32>` | 4 bytes (optional) | Reserved field emitted by Director in longer records. |
| `<unknown32>` | 4 bytes (optional) | Second reserved dword preserved for parity. |
| `<flags2>` | 2 bytes (optional) | Extra bitmap flags captured when the extended block is present. |

### Director 6–10 (0x600 ≤ version < 0x1100)

Later versions keep the general layout but add UI metadata such as the edit version, scroll offsets, and update flags. The pitch word still uses the high bit to mark colour bitmaps.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<pitch word>` | 2 bytes | Includes the color-image bit; the lower 12 bits store the pitch after masking. |
| `<rect.top>`..`<rect.right>` | 8 bytes | `_initialRect` coordinates stored as signed 16-bit values. |
| `<alpha threshold>` | 1 byte (version ≥ 0x700) | Alpha cutoff for transparent pixels; followed by 1 byte of padding. |
| `<padding>` | 2 bytes (version < 0x700) | Alignment field for earlier Director 6 builds. |
| `<edit version>` | 2 bytes | Authoring-version stamp stored in the resource. |
| `<scrollY>` | 2 bytes | Signed scroll offset on the Y axis. |
| `<scrollX>` | 2 bytes | Signed scroll offset on the X axis. |
| `<regY>` | 2 bytes | Registration offset on Y. |
| `<regX>` | 2 bytes | Registration offset on X. |
| `<update flags>` | 1 byte | Bitfield describing runtime behaviours (center registration, matte usage, etc.). |
| `<bits per pixel>` | 1 byte (optional) | Present when the pitch word’s high bit marked the bitmap as colour. |
| `<CLUT cast library>` | 2 bytes (optional) | Palette library ID reused from the Director 5 layout. |
| `<CLUT id>` | 2 bytes (optional) | Palette resource identifier; negative or zero selects built-ins. |

## Windows DIB palettes and headers

When a bitmap references a Windows DIB the loader forwards the data to the DIB decoder. Director prepends palette records (six bytes per colour) and the standard 40-byte BITMAPINFOHEADER structure.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<palette red>` | 1 byte | High-byte red component for a palette entry. |
| `<palette pad>` | 1 byte | Unused byte between colour components. |
| `<palette green>` | 1 byte | High-byte green component. |
| `<palette pad>` | 1 byte | Unused byte between components. |
| `<palette blue>` | 1 byte | High-byte blue component stored by Director. |
| `<palette pad>` | 1 byte | Final unused byte completing the 6-byte colour record. |
| `28 00 00 00` | 4 bytes | Little-endian BITMAPINFOHEADER size (0x28 == 40). |
| `<width>` | 4 bytes | Signed little-endian pixel width. |
| `<height>` | 4 bytes | Signed little-endian pixel height. |
| `<planes>` | 2 bytes | Always `0x0001`, read and ignored. |
| `<bits per pixel>` | 2 bytes | Colour depth of the DIB image. |
| `<compression>` | 4 bytes | Compression identifier forwarded to the codec. |
| `<image size>` | 4 bytes | Declared bitmap data size; kept for diagnostics. |
| `<pixels/meter X>` | 4 bytes | Horizontal resolution metric. |
| `<pixels/meter Y>` | 4 bytes | Vertical resolution metric. |
| `<palette color count>` | 4 bytes | Overrides the palette length when non-zero. |
| `<important colors>` | 4 bytes | Unused field copied for completeness. |

## BITD RLE control bytes

Classic Mac bitmaps rely on the BITD RLE scheme. The control bytes below match the routines used by Director to reconstruct pixel rows.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<control byte>` | 1 byte | Values `< 0x80` represent literal runs of `len = byte + 1`; values ≥ `0x80` encode repeats with `len = (0xFF ^ byte) + 2`. |
| `<literal data>` | `len` bytes | Raw pixel bytes copied when the control byte `< 0x80`. |
| `<repeat value>` | 1 byte | Single byte repeated `len` times when the control byte ≥ `0x80`. |
| `<zlib segment>` | `res.size` bytes | Compressed payload fed to `readZlibData()` when Afterburner metadata marks the bitmap as compressed. |

## Compatibility notes

- `BlLegacyBitmapReader` prefers `ediM` children when the payload advertises a modern container, but it automatically falls back to the classic `BITD` or `DIB ` chunk when the metadata stream is unclassified.
- Alpha masks and thumbnails are returned alongside the main bitmap so editors can reproduce the authoring environment used by Director 5–MX.
- Signature checks allow the reader to pick up stand-alone PNG, JPEG, GIF, BMP, and TIFF resources that newer Shockwave builds emit without the legacy tags.
- The byte layouts above match the data consumed by the classic Director executables, so the same codepath can decode movies built for Director 2 all the way through Director MX 2004.

[← Back to the format overview](./README.md)
