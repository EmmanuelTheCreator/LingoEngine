# Director 10+ Score Stream (VWSC)

[← Back to documentation home](README.md)

Director MX 2004 and newer store the movie timeline inside the `VWSC` chunk. The format is
self-contained: a header announces the payload organisation, descriptors describe every sprite
interval, and a packed keyframe stream updates the sprite channels frame-by-frame. This document
summarises the binary protocol so an implementation can reconstruct the score without relying on the
original tooling.

All multi-byte integers are big-endian. Byte offsets below are relative to the start of the structure
being described.

## Chunk layout

```
[VWSC header]
[Entry offset table]
[Entry payloads]
```

### Outer header (24 bytes)

| Offset | Type | Description |
| ------ | ---- | ----------- |
| `0x00` | `u32` | Total payload length covered by this table. |
| `0x04` | `s32` | Header signature. Director 10+ exports `-3` (`0xFFFFFFFD`). |
| `0x08` | `u32` | Offset (from the start of the header) to the entry offset table. |
| `0x0C` | `u32` | Number of entries described by the table. |
| `0x10` | `u32` | Notation base. Equals `entryCount + 1` in known samples. |
| `0x14` | `u32` | Sum of all entry sizes. Matches the last offset in the table. |

Immediately after the header comes an array of `entryCount + 1` 32-bit offsets. Each offset points to
an entry payload relative to the start of the array. The final offset equals the value stored at
`entrySizeSum` and therefore marks the end of the `VWSC` payload.

### Entry directory

Entry `0` always contains the frame data block described later. Entry `1` stores the sprite ordering
list: a 32-bit count followed by 32-bit indices that point at descriptor entries. Each sprite is
described by a primary entry (interval data) and an optional secondary entry (behaviour list). The
remaining entries contain zero-length payloads or additional descriptor data when the ordering table
references them.

## Frame data entry

Entry `0` begins with a secondary header followed by the keyframe stream.

### Frame data header (20 bytes)

| Offset | Type | Description |
| ------ | ---- | ----------- |
| `0x00` | `u32` | `actualSize` – number of bytes that follow the header. |
| `0x04` | `u8[3]` | Reserved header bytes (all observed exports use zero). |
| `0x07` | `u8` | Header length. Director emits `0x14` (20 bytes). |
| `0x08` | `u32` | Highest frame index referenced by the stream (inclusive). |
| `0x0C` | `u8` | Reserved, currently zero. |
| `0x0D` | `u8` | Channel group count used by Director’s editor (e.g. `13`). |
| `0x0E` | `u16` | Size of each sprite record (`0x0030`). |
| `0x10` | `u8` | Timeline version (`0x03` for Director 10+). |
| `0x11` | `s8` | Timeline feature flags (`0xEE` in samples). |
| `0x12` | `u16` | Maximum sprite channel count described by the timeline. |

The parser must preserve these fields verbatim so tooling can surface the same values exported by the
Director authoring environment.

### Keyframe stream

After the header, the payload becomes a stream of frame records. Each record starts with a 16-bit
length. If the length is zero, the stream terminates. Otherwise the decoder reads `length - 2` bytes
containing a sequence of tagged payloads:

```
[frameLength]
  [payloadLength][addressOffset][payload bytes]
  [payloadLength][addressOffset][payload bytes]
  …
```

The address offset determines which channel the payload belongs to. Sprite channels emit 48-byte
records that use the layout documented in the next section. Control lanes use shorter payloads to
announce tween flags, frame advances, colour updates, and similar metadata.

### Channel map

Sprite channels follow a fixed 0x30-byte stride after the control lanes. The mapping below has held
across Director 10–11 exports:

| Channel | Address range | Purpose |
| ------- | ------------- | ------- |
| 0 | `0x0000–0x002F` | Behaviour / script lane |
| 1 | `0x0030–0x005F` | Tempo lane |
| 2 | `0x0060–0x008F` | Transition lane |
| 3 | `0x0090–0x00BF` | Sound lane #1 |
| 4 | `0x00C0–0x00EF` | Sound lane #2 |
| 5 | `0x00F0–0x011F` | Palette lane |
| 6+ | `0x0120 + n·0x30` | Sprite channels |

Older files occasionally pack several sprite channels into a single payload by concatenating multiple
48-byte sprite records. When the payload length is a multiple of `0x30`, split it into individual
blocks and treat each block as a separate channel update. This ensures every sprite receives its own
`BlScoreToken` even if the source timeline combined them.

## Sprite record layout (48 bytes)

Sprite defaults and per-keyframe updates use a fixed 48-byte record. The table below lists every
byte:

| Offset | Size | Description |
| ------ | ---- | ----------- |
| `0x00` | `u8` | Control flags and tween mask bits. Bit meanings follow the table in [Tween flag mask](#tween-flag-mask). |
| `0x01` | `u8` | Ink mode (lower 7 bits). |
| `0x02` | `u8` | Foreground colour – red component. |
| `0x03` | `u8` | Background colour – red component. |
| `0x04` | `u16` | Cast library identifier. |
| `0x06` | `u16` | Cast member identifier. |
| `0x08` | `u16` | Reserved (historically zero). |
| `0x0A` | `u16` | Sprite-properties table offset used for behaviour lookup. |
| `0x0C` | `s16` | Vertical position (`locV`). |
| `0x0E` | `s16` | Horizontal position (`locH`). |
| `0x10` | `s16` | Height in pixels. |
| `0x12` | `s16` | Width in pixels. |
| `0x14` | `u8` | Score chip colour and edit flag (`0x40` marks editable sprites). |
| `0x15` | `u8` | Blend value (0–255). Convert to opacity with `100 - value / 255 * 100`. |
| `0x16` | `u8` | Flip flags (`0x02` = horizontal, `0x04` = vertical). |
| `0x17` | `u8` | Reserved padding byte. |
| `0x18` | `u8` | Foreground colour – green component. |
| `0x19` | `u8` | Background colour – green component. |
| `0x1A` | `u8` | Foreground colour – blue component. |
| `0x1B` | `u8` | Background colour – blue component. |
| `0x1C` | `u16` | Reserved word (zero in known files). |
| `0x1E` | `s32` | Rotation angle in hundredths of a degree. |
| `0x22` | `s32` | Skew angle in hundredths of a degree. |
| `0x26` | `u8[10]` | Reserved padding to reach 48 bytes. |

The first non-control record encountered for a sprite provides its default state. Subsequent updates
contain only the fields that change on that frame. Properties not mentioned in the payload retain
their previous values.

### Tween flag mask

Byte `0x00` inside the sprite record exposes the tween configuration. Director stores the same bits in
control tags `0x01F6` and `0x1CF6` when keyframes change tween flags at runtime.

| Bit | Mask | Meaning |
| --- | ---- | ------- |
| 0 | `0x01` | Tween data present for this sprite. |
| 1 | `0x02` | Path (position) tweening enabled. |
| 2 | `0x04` | Size tweening enabled. |
| 3 | `0x08` | Rotation tweening enabled. |
| 4 | `0x10` | Skew tweening enabled. |
| 5 | `0x20` | Blend tweening enabled. |
| 6 | `0x40` | Foreground colour tweening enabled. |
| 7 | `0x80` | Background colour tweening enabled. |

### Advance-frame control flags

Channel-specific advance tags (`0x0136`, `0x0166`, `0x0196`, …) carry a 16-bit payload. The high byte
stores timing information while the low byte contains additional sprite flags.

| Bits | Mask | Description |
| ---- | ---- | ----------- |
| 15 | `0x8000` | Create a keyframe when set. Otherwise continue tweening. |
| 8–14 | `0x7F00` | Frame delta (zero means advance by one frame). |
| 7 | `0x0080` | Tween continuation flag. |
| 6 | `0x0040` | Flip vertical. |
| 5 | `0x0020` | Flip horizontal. |
| 0–4 | `0x001F` | Reserved / currently unknown. |

### Behaviour descriptors

Descriptor entries contain interval-level metadata:

| Offset | Type | Description |
| ------ | ---- | ----------- |
| `0x00` | `s32` | Start frame (inclusive). |
| `0x04` | `s32` | End frame (inclusive). |
| `0x08` | `s32` | Reserved. |
| `0x0C` | `s16` | Reserved. |
| `0x0E` | `u16` | Sprite flags (lock, trails, editable, etc.). |
| `0x10` | `s32` | Channel number (zero-based). |
| `0x14` | `s16` | Constant `1` in observed exports. |
| `0x16` | `s16` | Reserved. |
| `0x18` | `s16` | Reserved (often `15`). |
| `0x1A` | `u8` | Constant `0xE1`. |
| `0x1B` | `u8` | Constant `0xFD`. |
| `0x1C` | `s16` | Reserved. |
| `0x1E` | `s32` | Reserved. |
| `0x22` | `s32[]` | Optional trailing integers (zero or descriptor size). |

A secondary entry may follow containing a list of behaviour script references. Each tuple stores a
cast library ID, a cast member ID, and a reserved `u32` field.

## Implementation notes

1. Read the outer header and offset table to locate each entry.
2. Parse entry `1` to learn the order of sprite descriptors.
3. Parse each descriptor and optional behaviour list.
4. Tokenise entry `0` into per-frame payloads, splitting multi-channel sprite blocks when the payload
   length is a multiple of 48 bytes.
5. Use the first full sprite record for each channel to populate default properties; subsequent
   updates only change fields that appear in the payload.
6. Apply tween flags, ease values, curvature, and blend conversions exactly as stored so downstream
   tools can reproduce Director’s interpolation.
