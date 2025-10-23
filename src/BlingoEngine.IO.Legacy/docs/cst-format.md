# Cast Library (`.cst`) Resources

## Overview

Cast libraries hold the assets that populate a Director movie: sprites, scripts, sounds, and other cast members. Exported movies keep this information in `CASt` resources that are referenced through `KEY*` and `CAS*` tables. Understanding these tables makes it easier to resolve each cast slot back to the chunk that supplies its data.

## `KEY*` – parent/child relationships

`KEY*` resources are always read before any cast data is parsed. Each row in the table links a parent resource (movie or cast library) to a child resource (such as `CASt`, `Lscr`, or media chunks).

| Field | Length | Notes |
| --- | --- | --- |
| Entry size | 2 bytes | Confirms the 12-byte row layout. |
| Reserved size | 2 bytes | Secondary size field stored alongside the entry size. |
| Total rows | 4 bytes | Number of allocated rows in the table. |
| Used rows | 4 bytes | Number of rows that contain actual data. |
| Child resource id | 4 bytes per row | Resource index of the child chunk. |
| Parent resource id | 4 bytes per row | Resource index of the owning movie or cast library. |
| Child tag | 4 bytes per row | Four-character code that identifies the child resource type. |

When a row references a `CAS*` resource, the loader records which library owns that table before continuing.

## `CAS*` – cast slot lookup

After the `KEY*` table is processed, each referenced `CAS*` chunk is opened. The chunk consists of a series of big-endian 32-bit cast indices:

| Value | Meaning |
| --- | --- |
| `00 00 00 00` | Empty cast slot. |
| Non-zero | Resource id of the `CASt` chunk that supplies the cast member. |

The slot position in the table becomes the cast member number, and the parent id recorded from `KEY*` identifies the cast library that owns the slot.

## `CASt` – cast member records

Every cast member ultimately lives in a `CASt` resource. The header layout changes across Director releases, but the goal is the same: isolate a small block of cast-specific bytes ("cast data") and optional metadata ("cast info") before passing the payload to the appropriate loader.

### Director 2–3 (`VWCR` entries)

Earlier movies use the `VWCR` block instead of standalone `CASt` resources. Each entry is parsed as:

| Field | Length | Notes |
| --- | --- | --- |
| Entry size | 1 byte | Total length of the entry, including the type byte. |
| Cast type | 1 byte | Enumerated cast type (bitmap, text, palette, etc.). |
| Flags1 | 1 byte (optional) | Present when `entry size` is greater than 1. |
| Cast payload | Remaining bytes | Forwarded directly to the cast-type loader. |

### Director 4–5 (`CASt` header)

Director 4 introduces explicit `CASt` chunks with a short header:

| Field | Length | Notes |
| --- | --- | --- |
| Cast data size | 2 bytes | Big-endian length of the cast-data section (includes the type byte). |
| Cast info size | 4 bytes | Big-endian length of the metadata block that follows the cast data. |
| Cast type | 1 byte | Identifies which cast subclass should parse the payload. |
| Flags1 | 1 byte (optional) | Present when the cast data size exceeds one byte. |
| Cast data payload | `cast data size` − consumed bytes | Bytes passed to the cast-specific parser. |
| Cast info payload | `cast info size` bytes | Optional metadata strings or timestamps. |

### Director 5– (`CASt` header)

Note: version 10.1 seem to behave differently

Later versions expand the header and reorder the fields. Director 10 and the final Director 10.1 maintenance release reuse this layout unchanged, so the same parsing rules cover all modern classic exports:

| Field | Length | Notes |
| --- | --- | --- |
| Cast type | 4 bytes | Big-endian 32-bit member-type identifier. Values map to the classic type table summarised in [Cast member types](#cast-member-types). |
| Cast info size | 4 bytes | Big-endian length of the metadata block that immediately follows. |
| Cast data size | 4 bytes | Big-endian length of the cast-data section stored after the info block. |
| Cast info payload | `cast info size` bytes | Optional metadata strings or timestamps. |
| Cast data payload | `cast data size` bytes | Bytes forwarded to the cast-specific parser. |

Classic authoring tools usually store the cast-member name at the start of the info payload using a
Pascal-style string: the first byte encodes the number of UTF-8 characters that follow. Additional
metadata, such as authoring timestamps or flags, may appear after the name depending on the Director
release.

### Loading process

Regardless of the version, the loader wraps the cast-data payload in a memory stream, instantiates the matching cast member class, and attaches any linked resources recorded in the `KEY*` table. This allows higher-level systems to fetch sprites, scripts, and media by cast member number without re-reading the tables.

## Cast member types

Director stores the cast-member classification as a big-endian 32-bit word at the start of the modern `CASt` header. The values below match the enumeration in `BlLegacyCastMemberType`. Later Shockwave releases introduced additional codes; until those layouts are documented the loader reports them as `Unknown` while still exposing the raw payload bytes.

| Value | Type | Notes |
| --- | --- | --- |
| `0` | Null | Placeholder slot; older movies use this for empty cast entries. |
| `1` | Bitmap | Raster member backed by `BITD`, `DIB `, or authoring metadata. See [Legacy Bitmap Loading](./LegacyBitmapLoading.md). |
| `2` | Film loop | Timeline snippets that replay a sequence of sprites. |
| `3` | Text | Static text members documented in [Legacy Text and Field Members](./LegacyTextFieldMembers.md). |
| `4` | Palette | Colour table resources referenced by bitmap members. |
| `5` | Picture | QuickDraw `PICT` drawings or embedded images exposed as pictures. |
| `6` | Sound | Audio members that resolve to `ediM`, `sndS`, or classic `SND ` payloads. See [Legacy Sound Loading](./LegacySoundLoading.md). |
| `7` | Button | Interactive sprites that bind scripts and button states. |
| `8` | Shape | QuickDraw shape records described in [LegacyShapeRecords](./LegacyShapeRecords.md). |
| `9` | Movie | Linked movie assets (commonly QuickTime clips). |
| `10` | Digital video | Digital video members that wrap platform-specific codecs. |
| `11` | Script | Lingo scripts; see [Legacy Script Cast Members](./LegacyScriptMembers.md). |
| `12` | RTE field | Rich-text edit fields introduced in later Director releases. |
| `13` | Font | Embedded font resources registered in the cast. |
| `14` | Xtra | Third-party Xtras and internal extensions. |
| `15` | Field | Editable text fields documented in [Legacy Text and Field Members](./LegacyTextFieldMembers.md). |


### Bitmap cast members

Bitmap entries reserve their cast-data length for zero bytes because the actual raster payloads live
in sibling resources linked through the `KEY*` table. Director 5–10 exports commonly attach
combinations of `BITD`, `DIB `, `PICT`, `ALFA`, `Thum`, and modern `ediM` metadata streams to describe
the surface, colour depth, and optional thumbnails. The Pascal-style name described above remains in
the info block so cataloguing tools can display the member without reading any media chunks. See
[Legacy Bitmap Loading](./LegacyBitmapLoading.md) for byte-level layouts.

### Sound cast members

Sound members follow the same pattern: the info payload stores the Pascal-style name and optional
authoring metadata, while the audio bytes remain in dedicated media resources referenced from the
`KEY*` table. Classic movies point at `sndS` or `SND ` payloads, whereas Director 7+ authoring tools
prefer `ediM` containers for MP3 and streaming media. The cast-data length therefore stays at zero
because the loader pulls audio directly from the linked resource. Refer to [Legacy Sound
Loading](./LegacySoundLoading.md) for per-format parsing rules.

### Member type: Text 

#### extra bytes breakdown example
Typical bytes just before ending N/A: 
```
68 EF 75 86    68    EF 77 75
```
4 first are almost identical to next 4.

#### Specific bytes:

| RAW bytes   	| Property				| Value/Description					|
|---------------|-----------------------|-----------------------------------|
| 00 00 00 04 	| type length			| 4									|
| 74 65 78 74 	| type					| text								|
| 00 00 01 B0	| 						| 432								|
| 00 00	00 01	| Editable				| ON  								|
| 00 00 00 00 	| Framing				| 1=Scroling, 2=Fixed				|
| 00 00 00 00 	| Tab on/off			| 1=ON, 0=OFF						|
| 00 00 00 00 	| DTD on/off			| 1=ON, 0=OFF						|
| 00 00 00 01 	| Antialias on/off		| 1=ON, 0=OFF						|
| 00 00 00 0E 	| Antialias Mode		| 0=AllText, E0=Default? ,13=LargetThen		|
| 00 00 00 00 	| AntaAlias Larger Size	| 15								|
| 00 00 00 00 	| 						| 									|
| 00 00 00 0F 	| Kerning Larger Size	| 15								|
| 00 00 1F F4 	| 						| 81.80								|
| 00 00 00 01 	| Kerning On/Off		| 1=ON, 0=OFF						|
| 00 00 00 0E 	| Kerning Mode			| 0=AllText, E0=Default? ,13=LargetThen		|
| 00 00 00 01 	| UseHyperlinkStyles	| 1=ON, 0=OFF						|
| 00 00 00 00 	| 						| 									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 00 	| PreRender Ink			| 1=InkCopy, 2=InkOther				|
| 00 00 00 00 	| PreRender Save BMP	| 1=ON, 0=OFF						|
| 33 54 45 58 	| 						| 3TEX or XET3						|
| 00 00 01 64 	| 						| 356								|
| FF FF FF FF 	| 						| White								|
| 00 53 21 47 	| Tunnel Depth			| 50 or 83.13, 16.16 fixed-point, big-endian. 0x00532147 / 65536 = 83.13 |
| 00 00 00 01 	| 						| 1									|
| 00 02 CC CC	| Bevel Amount			| 2.80 = 16.16 fixed (big-endian).	0x0002CCCC / 65536 = 2.8027 ≈ 2.80 |
| 00 00 00 01 	| 						| 1									|
| 00 00 00 05 	| 						| 5									|
| 00 00 00 01 	| 						| 1									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 02 	| Perhaps light dir?	| 2									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 1E  	| Reflectivity     		| 30								|
| 99 66 33 00  	| Directional			| ≈ #A0522D	16-bit RGB				|
| 99 33 66 00  	| Ambient				| ≈ #DA70D6	16-bit RGB				|
| 00 99 66 00 	| Background			| ≈ #008080	16-bit RGB				|
| 41 40 00 00  	| Camera Pos X			| IEEE-754 32-bit floating-point	|
| 42 08 00 00  	| Camera Pos Y			| IEEE-754 32-bit floating-point	|
| 42 60 00 00 	| Camera Pos Z			| IEEE-754 32-bit floating-point	|
| 02 19 F6 10 	| Distance?				| 0x0219F610 / 65536 = 537.9612		|
| 42 9C 00 00  	| Camera Rot X			| IEEE-754 32-bit floating-point	|
| 42 C4 00 00  	| Camera Rot Y			| IEEE-754 32-bit floating-point	|
| 42 98 00 00 	| Camera Rot Z			| IEEE-754 32-bit floating-point	|
| 02 19 EF A8	| Focal?				| 0x0219EFA8 / 65536 = 537.9362		|
| 4E 6F 54 65   | TextureName			| NoTexture							|
| 78 74 75 72   | ...					|									| 
| 65            | ...					|									| 


#### Example values:
| Label            | Value                          |
|------------------|--------------------------------|
| Camera Pos (XYZ) | [12.00, 34.00, 56.00]          | 
| Rotation (XYZ)   | [78.00, 98.00, 76.00]          | 
| Face: Front      | ✗                             |
| Face: Back       | ✓                              |
| Face: Tunnel     | ✗                             |
| Smoothness       | 2                              |
| Tunnel Depth     | 83.13                          | 
| Bevel Amount     | 2.80                           | 
| Bevel Edge       | Miter                          |
| Light            | Middle Left                    |
| Directional      | #A0522D (brown)                | 
| Ambient          | #DA70D6 (orchid purple)        | 
| Background       | #008080 (teal)                 | 
| Shader Texture   | Default                        |
| Texture Name     | NoTexture (disabled)           |
| Diffuse          | #0000FF (blue)                 | 
| Specular         | #008000 (green)                | 
| Reflectivity     | 53                             | 

