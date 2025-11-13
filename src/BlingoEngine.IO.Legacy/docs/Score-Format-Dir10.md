# Director 10+ VWSC Score Stream Specification

[← Back to documentation home](README.md)

Director 10, Director MX 2004, and Director 11 store their stage timeline inside
an extended `VWSC` chunk. The structure differs from the classic Director 2–9
format and introduces indirection tables that let the authoring tool shuffle
sprite descriptors without rewriting the frame data. This guide captures the
binary layout byte-for-byte so that a fresh implementation can rebuild the score
stream from scratch.

All multi-byte integers are big-endian. Offsets in the tables below are relative
to the start of the structure being described. Padding bytes must be preserved
unchanged; Director checks them when validating edited movies.

## 1. Chunk outline

```
VWSC
├─ Header (24 bytes)
├─ Entry offset table (entryCount + 1 dwords)
└─ Entry payloads
```

### 1.1 Header (24 bytes)

| Offset | Type  | Description                                                                     |
| -----: | :---- | :------------------------------------------------------------------------------ |
| `0x00` | `u32` | `payloadLength`. Total size of the data covered by the offset table.            |
| `0x04` | `s32` | `signature`. Director 10+ writes `-3` (`0xFFFFFFFD`). Reject other values.      |
| `0x08` | `u32` | `tableOffset`. Distance from the start of the header to the first entry offset. |
| `0x0C` | `u32` | `entryCount`. Number of payload entries described by the table.                 |
| `0x10` | `u32` | `notationBase`. Normally `entryCount + 1`. Reserve the value as-is.             |
| `0x14` | `u32` | `entrySpan`. Sum of all entry sizes. Matches the last offset in the table.      |

Immediately after the header sits an array of `entryCount + 1` 32-bit offsets.
Each offset points to a payload relative to the start of the offset array. The
final offset equals `entrySpan` and therefore marks the end of the `VWSC`
payload.

### 1.2 Entry directory

Director 10+ exports use the entry indices below. Empty slots are kept as zero
length payloads so indices remain stable when the authoring tool adds features.

| Entry | Purpose            | Notes                                                                    |
| ----: | :----------------- | :----------------------------------------------------------------------- |
|   `0` | Frame stream       | Keyframes and channel payloads (see section 2).                          |
|   `1` | Sprite order       | `u32 count` followed by `count` 32-bit indices into the descriptor area. |
|  `≥2` | Sprite descriptors | Interval descriptors and behaviour lists (section 3).                    |

## 2. Frame stream (entry 0)

The frame stream begins with its own header and then encodes a variable-length
sequence of per-frame records.

### 2.1 Frame-stream header (20 bytes)

| Offset | Type    | Description                                                                   |
| -----: | :------ | :---------------------------------------------------------------------------- |
| `0x00` | `u32`   | `actualSize` — number of bytes that follow the header.                        |
| `0x04` | `u8[3]` | Reserved header bytes. Director writes zeros. Preserve verbatim.              |
| `0x07` | `u8`    | `headerLength`. Director 10+ sets `0x14` (20 bytes).                          |
| `0x08` | `u32`   | `highestFrameIndex`. Inclusive maximum frame number referenced by the stream. |
| `0x0C` | `u8`    | Reserved. Currently zero.                                                     |
| `0x0D` | `u8`    | `channelGroupCount`. Number of score lanes visible in the editor (e.g. `13`). |
| `0x0E` | `u16`   | `spriteRecordSize`. Always `0x0030` (48 bytes) in Director 10+.               |
| `0x10` | `u8`    | `timelineVersion`. Director 10+ writes `0x03`.                                |
| `0x11` | `s8`    | `featureFlags`. Signed mask of timeline options. Observed value `0xEE`.       |
| `0x12` | `u16`   | `maxSpriteChannels`. Maximum sprite channel number referenced in the score.   |

### 2.2 Frame record encoding

Each frame record starts with a 16-bit length word. When the length word is
zero, the frame stream terminates. Otherwise the decoder consumes `frameLength`
bytes starting immediately after the length field.

Within a frame payload the data is organised as a list of channel updates. Every
update contains a two-byte payload length, a two-byte channel address, and the
payload bytes for that channel.

```
frameRecord
├─ u16 frameLength (size of the remaining bytes; zero terminates stream)
└─ channelPayload*
     ├─ u16 payloadLength (including the 2-byte address)
     ├─ u16 channelAddress
     └─ u8[payloadLength - 2] payloadBytes
```

A decoder subtracts `2` from `payloadLength` to learn how many bytes belong to
the channel payload. After consuming the payload, processing continues with the
next channel entry until the `frameLength` budget reaches zero.

Treat the address as an in-memory pointer into a packed lane table. Director
lays the behaviour lane at offset `0x0000` and then places every additional lane
directly after it. Sprite lanes follow the control lanes and are stored in
chunks of 0x30 bytes. A keyframe therefore updates a lane by pointing at the
first byte of the relevant slot and then streaming the modified bytes. When the
payload covers several consecutive sprites, Director simply keeps writing past
the initial address without re-emitting a new header.

The channel address determines the lane being updated. Sprite channels start at
`0x0120`; lower addresses are reserved for control lanes. To calculate the sprite
channel index, subtract `0x0120` and divide by the fixed `0x30` stride.

| Channel | Address range     | Description                                                         |
| ------: | :---------------- | :------------------------------------------------------------------ |
|     `0` | `0x0000–0x002F`   | Behaviour/script lane. Encodes frame actions and sprite behaviours. |
|     `1` | `0x0030–0x005F`   | Tempo lane. Holds tempo words and skip-frame instructions.          |
|     `2` | `0x0060–0x008F`   | Transition lane. Announces transitions and stage wipes.             |
|     `3` | `0x0090–0x00BF`   | Sound lane #1. Selects and tweaks the first sound channel.          |
|     `4` | `0x00C0–0x00EF`   | Sound lane #2. Same layout as sound lane #1.                        |
|     `5` | `0x00F0–0x011F`   | Palette lane. Controls palette cycling and colour effects.          |
| `6 + n` | `0x0120 + n·0x30` | Sprite channel `n`. Each update spans 48 bytes (see section 2.3).   |

When a sprite payload exceeds 48 bytes, split the payload into consecutive
48-byte records. Director occasionally concatenates several sprite updates in a
single block when multiple channels change during the same frame.

Control lanes follow the same memory overlay rule even though their internal
fields differ. Each 0x30-byte slot matches the in-memory `ScoreData` structure
used by Director, so a decoder only needs to copy the bytes into its own lane
buffer. No tag headers or opcode bytes are present inside the payload: the
channel address alone tells you which structure you are mirroring.

### 2.3 Sprite record layout (48 bytes)

Sprite channels share a fixed 48-byte structure. Bytes omitted by an update keep
the previous value from the last non-empty payload.

| Offset | Size     | Description                                                             |
| -----: | :------- | :---------------------------------------------------------------------- |
| `0x00` | `u8`     | Control/tween mask. See [Tween flags](#tween-flags).                    |
| `0x01` | `u8`     | Ink flags. Lower seven bits select the ink mode, bit 7 enables trails.  |
| `0x02` | `u8`     | Foreground colour — red component.                                      |
| `0x03` | `u8`     | Background colour — red component.                                      |
| `0x04` | `u16`    | Cast library identifier.                                                |
| `0x06` | `u16`    | Cast member identifier.                                                 |
| `0x08` | `u16`    | Reserved. Director writes zero; keep the stored value.                  |
| `0x0A` | `u16`    | Sprite-property table offset. Resolves behaviours and script instances. |
| `0x0C` | `s16`    | `locV` vertical position in pixels.                                     |
| `0x0E` | `s16`    | `locH` horizontal position in pixels.                                   |
| `0x10` | `s16`    | Height in pixels.                                                       |
| `0x12` | `s16`    | Width in pixels.                                                        |
| `0x14` | `u8`     | Chip colour and edit flag. Bit 6 (`0x40`) means editable in the score.  |
| `0x15` | `u8`     | Blend value (0 = opaque, 255 = transparent).                            |
| `0x16` | `u8`     | Flip flags (`0x02` horizontal, `0x04` vertical).                        |
| `0x17` | `u8`     | Reserved padding byte.                                                  |
| `0x18` | `u8`     | Foreground colour — green component.                                    |
| `0x19` | `u8`     | Background colour — green component.                                    |
| `0x1A` | `u8`     | Foreground colour — blue component.                                     |
| `0x1B` | `u8`     | Background colour — blue component.                                     |
| `0x1C` | `u16`    | Reserved. Observed value `0`.                                           |
| `0x1E` | `s32`    | Rotation angle in hundredths of a degree.                               |
| `0x22` | `s32`    | Skew angle in hundredths of a degree.                                   |
| `0x26` | `u8[10]` | Padding. Director keeps these bytes at zero.                            |

#### Ink mode enumeration

| Value | Name                   | Behaviour                                             |
| ----: | :--------------------- | :---------------------------------------------------- |
|   `0` | Copy                   | Normal compositing (opaque).                          |
|   `1` | Transparent            | Treat colour index zero as transparent.               |
|   `2` | Reverse                | Invert the destination pixels using the sprite.       |
|   `3` | Ghost                  | Alpha blend using palette highlight.                  |
|   `4` | Matte                  | Use sprite as matte against the background.           |
|   `5` | Mask                   | Use sprite as a binary mask.                          |
|   `6` | Add                    | Additive blending.                                    |
|   `7` | Subtract               | Subtractive blending.                                 |
|   `8` | Multiply               | Multiply destination by sprite.                       |
|   `9` | Lightest               | Keep the lighter pixel.                               |
|  `10` | Darkest                | Keep the darker pixel.                                |
|  `11` | Background Transparent | Similar to Transparent but honours background colour. |
|  `12` | Blend                  | Linear interpolation using the blend value.           |
|  `13` | Wipe                   | Directional wipe using the sprite bitmap.             |
|  `14` | Replace                | Replace palette indices without alpha.                |
|  `15` | Custom                 | Projectors expose the slot for extensibility.         |

#### Tween flags

Byte `0x00` inside the sprite record packs the tween configuration.

| Bit | Mask   | Meaning                             |
| --: | :----- | :---------------------------------- |
| `0` | `0x01` | Tween data present for this sprite. |
| `1` | `0x02` | Path tween enabled (position).      |
| `2` | `0x04` | Size tween enabled.                 |
| `3` | `0x08` | Rotation tween enabled.             |
| `4` | `0x10` | Skew tween enabled.                 |
| `5` | `0x20` | Blend tween enabled.                |
| `6` | `0x40` | Foreground colour tween enabled.    |
| `7` | `0x80` | Background colour tween enabled.    |

#### Sprite flag bits (property table)

Field `0x0A` points to a short property table. The following bit mask reproduces
the values used by Director 10+ when editing a sprite interval.

|  Bit | Mask     | Meaning                           |
| ---: | :------- | :-------------------------------- |
|  `0` | `0x0001` | Locked in the score.              |
|  `1` | `0x0002` | Editable in the score.            |
|  `2` | `0x0004` | Trails enabled.                   |
|  `3` | `0x0008` | Ink affects background.           |
|  `4` | `0x0010` | Matte is preserved when trimming. |
|  `5` | `0x0020` | Sprite casts drop shadow.         |
|  `6` | `0x0040` | Reserved. Maintain stored value.  |
|  `7` | `0x0080` | Reserved. Maintain stored value.  |
|  `8` | `0x0100` | Editable path.                    |
|  `9` | `0x0200` | Editable script list.             |
| `10` | `0x0400` | Editable behaviours.              |
| `11` | `0x0800` | Editable blend.                   |
| `12` | `0x1000` | Editable colour.                  |
| `13` | `0x2000` | Editable skew.                    |
| `14` | `0x4000` | Editable rotation.                |
| `15` | `0x8000` | Director-specific testing flag.   |

## 3. Sprite descriptors (entries ≥ 2)

Every sprite referenced by the ordering table contributes at least one descriptor
entry. The descriptor covers the interval on the timeline and links to optional
behaviour lists stored in the following entry slot.

### 3.1 Interval descriptor (32 bytes + trailing integers)

| Offset | Type    | Description                                                |
| -----: | :------ | :--------------------------------------------------------- |
| `0x00` | `s32`   | Start frame (inclusive).                                   |
| `0x04` | `s32`   | End frame (inclusive).                                     |
| `0x08` | `s32`   | Reserved. Director writes zero.                            |
| `0x0C` | `s16`   | Reserved. Director writes zero.                            |
| `0x0E` | `u16`   | Sprite flag mask (see table in section 2.3).               |
| `0x10` | `s32`   | Sprite channel number (zero-based).                        |
| `0x14` | `s16`   | Constant `1` in observed exports.                          |
| `0x16` | `s16`   | Reserved. Preserve the stored value.                       |
| `0x18` | `s16`   | Reserved (commonly `15`).                                  |
| `0x1A` | `u8`    | Constant `0xE1`.                                           |
| `0x1B` | `u8`    | Constant `0xFD`.                                           |
| `0x1C` | `s16`   | Reserved word.                                             |
| `0x1E` | `s32`   | Reserved dword.                                            |
| `0x22` | `s32[]` | Optional trailing integers. Count derived from entry size. |

### 3.2 Behaviour list (variable)

If a descriptor references behaviours, the next entry stores an array of tuples:
`u16 castLibId`, `u16 memberId`, `u32 reserved`. The list is empty when the
sprite has no attached behaviours.

## 4. Implementation checklist

1. Parse the 24-byte header and the offset table.
2. Use entry `1` to learn which descriptor slots correspond to sprite channels.
3. Decode the descriptor entries and their optional behaviour lists.
4. Process the frame stream: for each frame, iterate over channel payloads and
   dispatch them according to the channel address.
5. For sprite channels, apply the first full record as the default state and
   then patch subsequent updates onto it.
6. For control lanes, copy the patched bytes straight into the lane snapshot.
   Director stores these payloads as direct memory overlays, so reproducing the
   score only requires forwarding the bytes without inventing higher-level
   tags.

Preserving unspecified bytes and untouched lane snapshots guarantees that the
rebuilt score remains compatible with Director MX 2004 and Director 11
projectors.
