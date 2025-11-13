# Director Pre-10 Score and Frame Layout

[\u2190 Back to documentation home](README.md)

Classic Director movies drive their stage from a collection of timeline resources. Each movie ships a
`VWSC` score stream that holds the frame records, optional `VWLB` label tables, and `VWAC` action
blocks. This page distils the byte layouts into versioned tables so a new loader can rebuild the
Director runtime without consulting external tools. Director 10 and newer projectors introduced
additional structures that are not yet documented here; see the [Director 10+ score template](Director10PlusScoreTemplate.md)
for the placeholders that still need samples.

## Resource bundle overview

A complete legacy score implementation needs to handle three container types:

- **`VWSC` score stream:** frame descriptors, channel records, and Afterburner detail tables.
- **`VWLB` label stream:** packed frame labels and comments.
- **`VWAC` action stream:** packed frame-action scripts.

Each stream begins with a byte-count guard and uses big-endian encoding regardless of platform. On
Afterburner builds (Director 6 and later) the `VWSC` body is sometimes compressed; inflate it before
parsing the headers below.

## Score stream headers

### Director 2–3 minimal header (version < `0x400`)

Early Mac and Windows movies announce only the overall byte count before the first frame payload.
Stage geometry defaults to 30 sprite channels.

| Bytes | Length | Description |
| --- | --- | --- |
| `0x0000`..`0x0003` | 4 bytes | Total score size stored as an unsigned 32-bit integer. The parser copies this into its frame-stream guard. |

### Afterburner preamble (Director 6 and later, `0x600 ≤ version < 0x1100`)

Afterburner-enabled scores reserve twelve bytes ahead of the standard header. The detail table
provides sprite metadata, behaviour lists, and names.

| Bytes | Length | Description |
| --- | --- | --- |
| `0x0000`..`0x0003` | 4 bytes | Repeated frame-stream size used to guard Afterburner lookups. |
| `0x0004`..`0x0007` | 4 bytes | Detail-table format version. Values observed in historic projectors include `0x00000001` and `0x00000002`. |
| `0x0008`..`0x000B` | 4 bytes | Offset from the beginning of the score stream to the detail list. Readers seek here after the main header is parsed. |

### Detail list header (Director 6 and later)

The list header sits at the offset announced above. Offsets in this table are absolute positions
inside the score stream and are relative to the start of the detail data block.

| Bytes | Length | Description |
| --- | --- | --- |
| `list+0x00`..`list+0x03` | 4 bytes | Entry count covering sprite info, behaviour arrays, and sprite names. |
| `list+0x04`..`list+0x07` | 4 bytes | Count of 32-bit offsets in the pointer table. Multiply by four and add `list+0x08` to locate the payload area. |
| `list+0x08`..`list+0x0B` | 4 bytes | Maximum payload length for any detail record. Useful when preallocating buffers. |
| `list+0x0C`..`list+0x0C+4*count-1` | `4 * count` bytes | Offset table. Each entry is added to the base address announced above to reach the corresponding detail record. |

### Director 4–10 score header (`0x400 ≤ version < 0x1100`)

Director 4 expanded the header so that loaders know where the first frame begins, how many channels
are encoded, and the advertised frame count. Later versions reuse the same structure.

| Bytes | Length | Description |
| --- | --- | --- |
| `0x0000`..`0x0003` | 4 bytes | Frame-stream size in bytes. Duplicate of the Director 2 guard when no Afterburner preamble is present. |
| `0x0004`..`0x0007` | 4 bytes | Offset to the first frame payload relative to the start of the score stream. |
| `0x0008`..`0x000B` | 4 bytes | Claimed frame count. Some projectors exaggerate this value, so implementations should recompute the true count while decoding. |
| `0x000C`..`0x000D` | 2 bytes | Frame-format version. Values `0`–`7` map to Director 4–5, `8`–`13` to Director 6, and higher numbers to Director 7–9. |
| `0x000E`..`0x000F` | 2 bytes | Sprite-channel record size in bytes. Used to step through sprite data within each frame. |
| `0x0010`..`0x0011` | 2 bytes | Total sprite channels allocated in the score. |
| `0x0012`..`0x0013` | 2 bytes | Displayed channel count. Director 5 defaults to 48 when zero, Director 6 to 120. Later versions provide an explicit value. |
| `0x0014`..`0x0015` | 2 bytes | Reserved padding. Retained for compatibility; usually zero. |

### Frame index (Director 6 and later)

When the frame-format version is Director 6 or newer, the score keeps a frame index immediately
before the frame payloads. Each entry contributes a start offset and the next frame’s start so the
loader can compute the compressed size.

| Bytes | Length | Description |
| --- | --- | --- |
| `entry*8+0x00`..`+0x03` | 4 bytes | Relative frame start. Add the base frame-data offset to reach the payload. |
| `entry*8+0x04`..`+0x07` | 4 bytes | Next frame start. Subtract the previous start to obtain the compressed size for logging or validation. |

## Frame payloads

Each frame begins with a length word followed by channel descriptors. The descriptors identify which
channels are present and how many bytes belong to each block.

| Bytes | Length | Description |
| --- | --- | --- |
| `frame+0x0000`..`frame+0x0001` | 2 bytes | Frame payload size excluding the length word. The reader subtracts two and loops until all channel descriptors are consumed. |
| `frame+0x0002`..`frame+0x0003` | 2 bytes | Channel count for the frame (main + sprite channels). |
| `frame+0x0004`..`frame+0x0005` | 2 bytes | Offset of the first channel block relative to `frame+0x0002`. |
| `frame+0x0006`.. | Variable | For each channel: 2-byte offset followed by a 2-byte size. Director 2–3 pack offsets and sizes into single bytes; later versions promote them to 16-bit values. |

After the descriptor table, channel payloads are stored back-to-back. The loader slices the data into
a main channel block and `numChannels` sprite blocks using the sizes announced above.

## Channel dispatch by version

Main-channel and sprite-channel sizes depend on the frame-format version stored in the header. The
loader maps the version to the appropriate decoder using the table below.

| Version range | Main channel bytes | Sprite channel bytes | Decoder hints |
| --- | --- | --- | --- |
| `< 0x400` | `0x20` (`kMainChannelSizeD2`) | `0x10` (`kSprChannelSizeD2`) | Director 2–3 records with signed colour bytes. |
| `0x400–0x4FF` | `0x28` (`kMainChannelSizeD4`) | `0x14` (`kSprChannelSizeD4`) | Director 4 layout with colour chips and blend fields. |
| `0x500–0x5FF` | `0x30` (`kMainChannelSizeD5`) | `0x18` (`kSprChannelSizeD5`) | Director 5 adds cast-library identifiers. |
| `0x600–0x6FF` | `0x90` (`kMainChannelSizeD6`) | `0x18` (`kSprChannelSizeD6`) | Director 6 Afterburner records with sprite-detail pointers. |
| `0x700–0x10FF` | `0x120` (`kMainChannelSizeD7`) | `0x30` (`kSprChannelSizeD7`) | Director 7–9 extend the Afterburner format with RGB triples and rotation/skew fields. |

## Main channel layouts

The main channel controls score-wide state: tempo, transitions, palette animation, and the IDs used
for script or sound cast members. Tables below record every byte so the parser can update the runtime
state precisely.

### Director 2 main channel (`0x20` bytes)

| Offset | Length | Description |
| --- | --- | --- |
| `0x00` | 1 byte | Frame-action cast member id. |
| `0x01` | 1 byte | Sound type for `sound1` (0x17 = sampled, 0x16 = MIDI). |
| `0x02` | 1 byte | Transition flags and duration. Bit 7 toggles whole-stage fades; low bits encode duration in quarter-seconds. |
| `0x03` | 1 byte | Transition chunk size for wiped regions. |
| `0x04` | 1 byte | Frame tempo. Cache values between 1 and 120 bpm. |
| `0x05` | 1 byte | Transition type enumeration. |
| `0x06`..`0x07` | 2 bytes | `sound1` cast member id. |
| `0x08`..`0x09` | 2 bytes | `sound2` cast member id. |
| `0x0A` | 1 byte | Sound type for `sound2`. |
| `0x0B` | 1 byte | Skip-frame flag that forces tempo-based frame drops. |
| `0x0C` | 1 byte | Transition blend amount. |
| `0x0D` | 1 byte | Reserved byte logged when non-zero. |
| `0x0E`..`0x0F` | 2 bytes | Reserved words. |
| `0x10`..`0x11` | 2 bytes | Palette cast member id (signed; negative values reference external casts). |
| `0x12` | 1 byte | Palette first colour index (signed QuickDraw byte converted with `(value + 128) & 0xFF`). |
| `0x13` | 1 byte | Palette last colour index. |
| `0x14` | 1 byte | Palette flags: cycling, auto-reverse, fade, overtime bits. |
| `0x15` | 1 byte | Palette speed. |
| `0x16`..`0x17` | 2 bytes | Palette frame count. |
| `0x18`..`0x19` | 2 bytes | Palette cycle count. |
| `0x1A`..`0x1F` | 6 bytes | Reserved palette bytes. |

### Director 4 main channel (`0x28` bytes)

| Offset | Length | Description |
| --- | --- | --- |
| `0x00` | 1 byte | Reserved byte; log unexpected values for diagnostics. |
| `0x01` | 1 byte | Sound type for `sound1`. |
| `0x02` | 1 byte | Transition flags and duration. |
| `0x03` | 1 byte | Transition chunk size. |
| `0x04` | 1 byte | Frame tempo (1–120 bpm cached). |
| `0x05` | 1 byte | Transition type. |
| `0x06`..`0x07` | 2 bytes | `sound1` cast member id. |
| `0x08`..`0x09` | 2 bytes | `sound2` cast member id. |
| `0x0A` | 1 byte | Sound type for `sound2`. |
| `0x0B` | 1 byte | Skip-frame flag. |
| `0x0C` | 1 byte | Transition blend. |
| `0x0D` | 1 byte | Tempo channel colour chip. |
| `0x0E` | 1 byte | Sound1 channel colour chip. |
| `0x0F` | 1 byte | Sound2 channel colour chip. |
| `0x10`..`0x11` | 2 bytes | Script action cast member id. |
| `0x12` | 1 byte | Script channel colour chip. |
| `0x13` | 1 byte | Transition channel colour chip. |
| `0x14`..`0x25` | 18 bytes | Palette control block (cast id, cycling range, flags, timing, style, colour code). |
| `0x26` | 1 byte | Reserved byte. |
| `0x27`..`0x29` | 3 bytes | Reserved words/dword. |
| `0x2A` | 1 byte | Palette colour-code (stage swatch index). |
| `0x2B` | 1 byte | Reserved byte. |

### Director 5 main channel (`0x30` bytes)

| Offset | Length | Description |
| --- | --- | --- |
| `0x00`..`0x01` | 2 bytes | Script action cast-library id (signed). |
| `0x02`..`0x03` | 2 bytes | Script action cast-member id. |
| `0x04`..`0x05` | 2 bytes | Sound1 cast-library id. |
| `0x06`..`0x07` | 2 bytes | Sound1 cast-member id. |
| `0x08`..`0x09` | 2 bytes | Sound2 cast-library id. |
| `0x0A`..`0x0B` | 2 bytes | Sound2 cast-member id. |
| `0x0C`..`0x0D` | 2 bytes | Transition cast-library id. |
| `0x0E`..`0x0F` | 2 bytes | Transition cast-member id. |
| `0x10` | 1 byte | Tempo channel colour chip. |
| `0x11` | 1 byte | Sound1 channel colour chip. |
| `0x12` | 1 byte | Sound2 channel colour chip. |
| `0x13` | 1 byte | Script channel colour chip. |
| `0x14` | 1 byte | Transition channel colour chip. |
| `0x15` | 1 byte | Frame tempo (cached 1–120 bpm). |
| `0x16`..`0x17` | 2 bytes | Alignment padding; warn if non-zero. |
| `0x18`..`0x19` | 2 bytes | Palette cast-library id (signed). |
| `0x1A`..`0x1B` | 2 bytes | Palette cast-member id (signed). |
| `0x1C` | 1 byte | Palette speed. |
| `0x1D` | 1 byte | Palette flags (cycling, overtime, fade). |
| `0x1E` | 1 byte | Palette first colour (Mac signed byte). |
| `0x1F` | 1 byte | Palette last colour. |
| `0x20`..`0x21` | 2 bytes | Palette frame count. |
| `0x22`..`0x23` | 2 bytes | Palette cycle count. |
| `0x24` | 1 byte | Palette fade target. |
| `0x25` | 1 byte | Palette delay. |
| `0x26` | 1 byte | Palette style. |
| `0x27` | 1 byte | Palette colour-code. |
| `0x28`..`0x2F` | 8 bytes | Reserved padding. |

### Director 6 main channel (`0x90` bytes)

Director 6 splits the header into six 24-byte blocks (script, tempo, transition, sound2, sound1,
palette). Each block stores cast-library ids, sprite-detail pointers, colour chips, and alignment
padding. Low-word shadows exist for delta updates that only modify the low 16 bits of a pointer.

| Offset | Length | Description |
| --- | --- | --- |
| `0x00`..`0x01` | 2 bytes | Script action cast-library id. |
| `0x02`..`0x03` | 2 bytes | Script action cast-member id. |
| `0x04`..`0x07` | 4 bytes | Script sprite-detail index (32-bit). |
| `0x06`..`0x07` | 2 bytes | Low-word shadow for the script detail index. |
| `0x08` | 1 byte | Script channel colour chip. |
| `0x09`..`0x17` | 15 bytes | Alignment padding for the script block. |
| `0x18`..`0x19` | 2 bytes | Tempo cast-library id. |
| `0x1A`..`0x1B` | 2 bytes | Tempo cast-member id. |
| `0x1C`..`0x1F` | 4 bytes | Tempo sprite-detail index. |
| `0x1E`..`0x1F` | 2 bytes | Low-word shadow for the tempo detail index. |
| `0x20` | 1 byte | Tempo channel flags (`tempoD6Flags`). |
| `0x21` | 1 byte | Tempo value cached for puppet tempo (1–120 bpm). |
| `0x22` | 1 byte | Tempo channel colour chip. |
| `0x23`..`0x2F` | 13 bytes | Alignment padding. |
| `0x30`..`0x31` | 2 bytes | Transition cast-library id. |
| `0x32`..`0x33` | 2 bytes | Transition cast-member id. |
| `0x34`..`0x37` | 4 bytes | Transition sprite-detail index. |
| `0x36`..`0x37` | 2 bytes | Low-word shadow for the transition detail index. |
| `0x38` | 1 byte | Transition channel colour chip. |
| `0x39`..`0x47` | 15 bytes | Alignment padding. |
| `0x48`..`0x49` | 2 bytes | Sound2 cast-library id. |
| `0x4A`..`0x4B` | 2 bytes | Sound2 cast-member id. |
| `0x4C`..`0x4F` | 4 bytes | Sound2 sprite-detail index. |
| `0x4E`..`0x4F` | 2 bytes | Low-word shadow for the sound2 detail index. |
| `0x50` | 1 byte | Sound2 channel colour chip. |
| `0x51`..`0x5F` | 15 bytes | Alignment padding. |
| `0x60`..`0x61` | 2 bytes | Sound1 cast-library id. |
| `0x62`..`0x63` | 2 bytes | Sound1 cast-member id. |
| `0x64`..`0x67` | 4 bytes | Sound1 sprite-detail index. |
| `0x66`..`0x67` | 2 bytes | Low-word shadow for the sound1 detail index. |
| `0x68` | 1 byte | Sound1 channel colour chip. |
| `0x69`..`0x77` | 15 bytes | Alignment padding. |
| `0x78`..`0x79` | 2 bytes | Palette cast-library id (signed). |
| `0x7A`..`0x7B` | 2 bytes | Palette cast-member id (signed). |
| `0x7C` | 1 byte | Palette speed. |
| `0x7D` | 1 byte | Palette flags (cycling/fade/overtime bits). |
| `0x7E` | 1 byte | Palette first colour (Mac signed byte). |
| `0x7F` | 1 byte | Palette last colour. |
| `0x80`..`0x81` | 2 bytes | Palette frame count. |
| `0x82`..`0x83` | 2 bytes | Palette cycle count. |
| `0x84` | 1 byte | Palette fade value. |
| `0x85` | 1 byte | Palette delay. |
| `0x86` | 1 byte | Palette style selector. |
| `0x87` | 1 byte | Palette colour code. |
| `0x88`..`0x8B` | 4 bytes | Palette sprite-detail index. |
| `0x8A`..`0x8B` | 2 bytes | Low-word shadow for the palette detail index. |
| `0x8C`..`0x8F` | 4 bytes | Alignment padding. |

### Director 7 main channel (`0x120` bytes)

Director 7 doubles each block to 48 bytes, keeping the same order while expanding the padding. The
field meanings mirror Director 6; only the offsets change. Each block still exposes cast ids,
sprite-detail pointers, colour chips, and low-word shadows.

| Offset | Length | Description |
| --- | --- | --- |
| `0x00`..`0x01` | 2 bytes | Script action cast-library id. |
| `0x02`..`0x03` | 2 bytes | Script action cast-member id. |
| `0x04`..`0x07` | 4 bytes | Script sprite-detail index. |
| `0x06`..`0x07` | 2 bytes | Low-word shadow for the script detail index. |
| `0x08` | 1 byte | Script channel colour chip. |
| `0x09`..`0x2F` | 39 bytes | Alignment padding. |
| `0x30`..`0x33` | 4 bytes | Tempo sprite-detail index. |
| `0x32`..`0x33` | 2 bytes | Low-word shadow for the tempo detail index. |
| `0x34`..`0x35` | 2 bytes | Tempo flags. |
| `0x36` | 1 byte | Tempo value (1–120 bpm cached). |
| `0x37` | 1 byte | Tempo channel colour chip. |
| `0x38`..`0x5F` | 40 bytes | Alignment padding. |
| `0x60`..`0x61` | 2 bytes | Transition cast-library id. |
| `0x62`..`0x63` | 2 bytes | Transition cast-member id. |
| `0x64`..`0x67` | 4 bytes | Transition sprite-detail index. |
| `0x66`..`0x67` | 2 bytes | Low-word shadow for the transition detail index. |
| `0x68` | 1 byte | Transition channel colour chip. |
| `0x69`..`0x8F` | 39 bytes | Alignment padding. |
| `0x90`..`0x91` | 2 bytes | Sound2 cast-library id. |
| `0x92`..`0x93` | 2 bytes | Sound2 cast-member id. |
| `0x94`..`0x97` | 4 bytes | Sound2 sprite-detail index. |
| `0x96`..`0x97` | 2 bytes | Low-word shadow for the sound2 detail index. |
| `0x98` | 1 byte | Sound2 channel colour chip. |
| `0x99`..`0xBF` | 39 bytes | Alignment padding. |
| `0xC0`..`0xC1` | 2 bytes | Sound1 cast-library id. |
| `0xC2`..`0xC3` | 2 bytes | Sound1 cast-member id. |
| `0xC4`..`0xC7` | 4 bytes | Sound1 sprite-detail index. |
| `0xC6`..`0xC7` | 2 bytes | Low-word shadow for the sound1 detail index. |
| `0xC8` | 1 byte | Sound1 channel colour chip. |
| `0xC9`..`0xEF` | 39 bytes | Alignment padding. |
| `0xF0`..`0xF1` | 2 bytes | Palette cast-library id (signed). |
| `0xF2`..`0xF3` | 2 bytes | Palette cast-member id (signed). |
| `0xF4` | 1 byte | Palette speed. |
| `0xF5` | 1 byte | Palette flags. |
| `0xF6` | 1 byte | Palette first colour. |
| `0xF7` | 1 byte | Palette last colour. |
| `0xF8`..`0xF9` | 2 bytes | Palette frame count. |
| `0xFA`..`0xFB` | 2 bytes | Palette cycle count. |
| `0xFC` | 1 byte | Palette fade value. |
| `0xFD` | 1 byte | Palette delay. |
| `0xFE` | 1 byte | Palette style selector. |
| `0xFF` | 1 byte | Palette colour code. |
| `0x100`..`0x103` | 4 bytes | Palette sprite-detail index. |
| `0x102`..`0x103` | 2 bytes | Low-word shadow for the palette detail index. |
| `0x104`..`0x11F` | 28 bytes | Alignment padding. |

## Sprite channel layouts

Sprite channels describe on-stage cast members. Puppet control suppresses updates to individual
fields; when a flag is set, the loader keeps the previous value instead of applying the new bytes.

### Director 2 sprite channel (`0x10` bytes)

| Offset | Length | Description |
| --- | --- | --- |
| `0x00` | 1 byte | Script cast-member id. |
| `0x01` | 1 byte | Sprite type (0 = inactive). Puppet sprites skip the update. |
| `0x02` | 1 byte | Foreground colour (signed byte converted with `(value + 128) & 0xFF`). Puppet sprites keep the prior colour. |
| `0x03` | 1 byte | Background colour (signed). |
| `0x04` | 1 byte | Line thickness (upper bit cleared). |
| `0x05` | 1 byte | Ink flags: low six bits ink mode, bit 6 trails, bit 7 stretch. |
| `0x06`..`0x07` | 2 bytes | Cast member id or QuickDraw pattern id for shapes. Puppet sprites skip the update. |
| `0x08`..`0x09` | 2 bytes | Top coordinate. |
| `0x0A`..`0x0B` | 2 bytes | Left coordinate. |
| `0x0C`..`0x0D` | 2 bytes | Height in pixels. |
| `0x0E`..`0x0F` | 2 bytes | Width in pixels. Negative or zero values collapse the sprite. |

### Director 4 sprite channel (`0x14` bytes)

| Offset | Length | Description |
| --- | --- | --- |
| `0x00` | 1 byte | Script cast-member id. |
| `0x01` | 1 byte | Sprite type (puppet-aware). |
| `0x02` | 1 byte | Foreground colour (0–255). Puppet sprites keep previous colour. |
| `0x03` | 1 byte | Background colour. |
| `0x04` | 1 byte | Thickness. |
| `0x05` | 1 byte | Ink flags (mode/trails/stretch). |
| `0x06`..`0x07` | 2 bytes | Cast member id or QuickDraw pattern. |
| `0x08`..`0x09` | 2 bytes | Top coordinate. |
| `0x0A`..`0x0B` | 2 bytes | Left coordinate. |
| `0x0C`..`0x0D` | 2 bytes | Height. |
| `0x0E`..`0x0F` | 2 bytes | Width. |
| `0x10`..`0x11` | 2 bytes | Script id (16-bit) reused for behaviour dispatch. |
| `0x12` | 1 byte | Colour-code flags: low nibble stage colour, bit 6 editable, bit 7 moveable. |
| `0x13` | 1 byte | Blend amount for inks with opacity. |

### Director 5 sprite channel (`0x18` bytes)

| Offset | Length | Description |
| --- | --- | --- |
| `0x00` | 1 byte | Sprite type (puppet-aware). |
| `0x01` | 1 byte | Ink flags. |
| `0x02`..`0x03` | 2 bytes | Cast-library id (signed). Puppet sprites keep previous value. |
| `0x04`..`0x05` | 2 bytes | Cast-member id. |
| `0x06`..`0x07` | 2 bytes | Script cast-library id. |
| `0x08`..`0x09` | 2 bytes | Script cast-member id. |
| `0x0A` | 1 byte | Foreground colour. |
| `0x0B` | 1 byte | Background colour. |
| `0x0C`..`0x0D` | 2 bytes | Top coordinate. |
| `0x0E`..`0x0F` | 2 bytes | Left coordinate. |
| `0x10`..`0x11` | 2 bytes | Height. |
| `0x12`..`0x13` | 2 bytes | Width. |
| `0x14` | 1 byte | Colour-code flags (editable, moveable, RGB hints). |
| `0x15` | 1 byte | Blend amount. |
| `0x16` | 1 byte | Thickness. |
| `0x17` | 1 byte | Reserved padding. |

### Director 6 sprite channel (`0x18` bytes)

Afterburner sprite records reuse the 24-byte footprint but replace the early words with sprite-detail
pointers. Auto-puppet flags suppress updates for individual fields.

| Offset | Length | Description |
| --- | --- | --- |
| `0x00` | 1 byte | Sprite type (puppet-aware). |
| `0x01` | 1 byte | Ink flags (auto-puppet aware). |
| `0x02` | 1 byte | Foreground colour (skipped when auto-puppet fore-colour is set). |
| `0x03` | 1 byte | Background colour (auto-puppet aware). |
| `0x04`..`0x05` | 2 bytes | Cast-library id (skipped when puppet or auto-puppet-cast is active). |
| `0x06`..`0x07` | 2 bytes | Cast-member id. |
| `0x08`..`0x0B` | 4 bytes | Primary sprite-detail index. |
| `0x0C`..`0x0D` | 2 bytes | Low-word shadow for the detail index. |
| `0x0E`..`0x0F` | 2 bytes | Top coordinate (auto-puppet location aware). |
| `0x10`..`0x11` | 2 bytes | Left coordinate. |
| `0x12`..`0x13` | 2 bytes | Height (auto-puppet aware). |
| `0x14`..`0x15` | 2 bytes | Width (auto-puppet aware). |
| `0x16` | 1 byte | Colour-code flags (skipped when auto-puppet moveable is set). |
| `0x17` | 1 byte | Blend amount (skipped for puppet sprites). |
| `0x18` | 1 byte | Thickness (skipped for puppet sprites). |
| `0x19` | 1 byte | Reserved padding. |

### Director 7 sprite channel (`0x30` bytes)

Director 7 doubles the record and introduces per-channel RGB components, rotation, skew, and a sprite
flag byte. Auto-puppet suppression still applies to the same fields as Director 6.

| Offset | Length | Description |
| --- | --- | --- |
| `0x00` | 1 byte | Sprite type (auto-puppet aware). |
| `0x01` | 1 byte | Ink flags. |
| `0x02` | 1 byte | Foreground colour (auto-puppet aware). |
| `0x03` | 1 byte | Background colour. |
| `0x04`..`0x05` | 2 bytes | Cast-library id (auto-puppet aware). |
| `0x06`..`0x07` | 2 bytes | Cast-member id. |
| `0x08`..`0x0B` | 4 bytes | Sprite-detail index. |
| `0x0C`..`0x0D` | 2 bytes | Low-word shadow for the detail index. |
| `0x0E`..`0x0F` | 2 bytes | Top coordinate (auto-puppet aware). |
| `0x10`..`0x11` | 2 bytes | Left coordinate. |
| `0x12`..`0x13` | 2 bytes | Height. |
| `0x14`..`0x15` | 2 bytes | Width. |
| `0x16` | 1 byte | Colour-code flags. |
| `0x17` | 1 byte | Blend amount. |
| `0x18` | 1 byte | Thickness. |
| `0x19` | 1 byte | Sprite flags (Director 7+ specific). |
| `0x1A` | 1 byte | Foreground green component (auto-puppet aware). |
| `0x1B` | 1 byte | Background green component. |
| `0x1C` | 1 byte | Foreground blue component. |
| `0x1D` | 1 byte | Background blue component. |
| `0x1E`..`0x21` | 4 bytes | Rotation angle in fixed-point units. |
| `0x22`..`0x23` | 2 bytes | Lower half of the rotation word (used by some projectors). |
| `0x24`..`0x27` | 4 bytes | Skew angle. |
| `0x28`..`0x2F` | 8 bytes | Reserved padding. |

## Sprite detail lookups (Director 6+)

Any sprite channel with a non-zero detail index resolves three adjacent records in the Afterburner
detail list:

| Entry | Description |
| --- | --- |
| `index` | `SpriteInfo` structure with behaviours, initialisation flags, and bounding boxes. |
| `index + 1` | Behaviour list describing scripts attached to the sprite. |
| `index + 2` | Pascal-style sprite name string. |

Offsets are read from the detail pointer table described earlier. Each pointer is added to the base
detail offset to locate the record.

## Label stream (`VWLB`)

Labels use a packed table followed by CR-delimited UTF-8 strings. The first tuple stores the base
offset for the string block.

| Bytes | Length | Description |
| --- | --- | --- |
| `0x0000`..`0x0001` | 2 bytes | `countMinusOne`; add one to obtain the number of label entries. |
| `0x0002`..`0x0003` | 2 bytes | Base offset: `count * 4 + 2`. Subsequent offsets are relative to this value. |
| `0x0004`..`0x0005` | 2 bytes | Frame number of the first label. |
| `0x0006`..`0x0007` | 2 bytes | String offset of the first label. |
| `0x0008`.. | `count * 4` bytes | Frame number and string offset pairs for remaining labels. |
| `strings` | Variable | CR-terminated label text and optional comment text stored sequentially. |

## Action stream (`VWAC`)

Frame actions reuse the same packed-table scheme as labels but include id/sub-id pairs.

| Bytes | Length | Description |
| --- | --- | --- |
| `0x0000`..`0x0001` | 2 bytes | `countMinusOne`; plus one yields the number of action entries. |
| `0x0002`..`0x0003` | 2 bytes | Base offset: `count * 4 + 2`. |
| `0x0004` | 1 byte | Initial action id. |
| `0x0005` | 1 byte | Initial sub-id (for grouping related scripts). |
| `0x0006`..`0x0007` | 2 bytes | Offset of the first action script relative to the base. |
| `0x0008`.. | `count * 4` bytes | Repeated id, sub-id, and offset tuples for additional entries. |
| `strings` | Variable | Script text stored as length-prefixed strings that should be decoded using the movie’s encoding table. |

## Rendering workflow

A renderer that targets Director 2–9 can follow the sequence below:

1. **Inflate and parse headers:** Load the `VWSC` stream, decompress if necessary, and record the
   version, channel sizes, and frame index.
2. **Preload detail tables:** For Director 6+, capture the Afterburner detail pointers so sprite
   channels can resolve behaviours and names on demand.
3. **Iterate frames:** For each frame request, read the payload size, walk the channel descriptors,
   and slice the main/sprite blocks according to the version-specific sizes.
4. **Apply the main channel:** Update tempo, palette animation, transition state, and control-channel
   cast IDs directly from the main channel layout for the current version.
5. **Update sprite channels:** For each sprite block, honour puppet and auto-puppet flags, then apply
   geometry, ink, colour, and cast references. When a detail index is present, resolve sprite info,
   behaviours, and names through the detail list.
6. **Process labels and actions:** Load the optional `VWLB` and `VWAC` streams to attach frame labels
   and frame-action scripts. Both use packed offset tables guarded by the same `countMinusOne`
   convention.
7. **Render the stage:** Once the runtime state is updated, draw sprites in channel order using the
   ink mode, blend, colour, and geometry recorded above. Puppet sprites keep their prior values when
   the frame omits an update, so the renderer must cache previous state.

These rules preserve byte-for-byte compatibility with legacy Director projectors through Director 9.
Future work on Director 10+ should extend the [template](Director10PlusScoreTemplate.md) with the
same level of detail once sample files are available.
