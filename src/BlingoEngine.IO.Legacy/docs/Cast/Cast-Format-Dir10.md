# Cast for Director 10+ (LsCM, CAS* , CASt and Cinf)

[← Back to the format overview](./README.md)

## CAS* — Cast Member Table

### Each Cast Entry
| Relative Offset | Example Bytes             | Description                                                              |
|----------------:|---------------------------|--------------------------------------------------------------------------|
| 0x00            | `08`                      | Pascal length of cast name (8).                                          |
| 0x01            | `49 6E 74 65 72 6E 61 6C` | “Internal” (cast name).                                                  |
| —               | `00`                      | Null terminator.                                                         |
| —               | optional Pascal string (starts with length, e.g. `5E`) | External cast file path (only if external). |
| —               | `00 00`                   | Preload flag.                                                            |
| —               | `00 01`                   | Unknown constant.                                                        |
| —               | `00 01`                   | Secondary flag.                                                          |
| —               | `00 03`                   | **Internal Cast Index (1-based)**; `0` if external.                      |
| —               | `04 00`                   | Constant footer.                                                         |

Each entry ends aligned to the next 4-byte boundary.  

| Name       | index   | Used for                    | 
|------------|---------|-----------------------------|
| ScriptText | 0       | for Scripts                 |
| Name       | 1       | All                         |
| Video      | 2       | for Video                   |
| ScriptLink | 3       | for Scripts                 |
| ScriptA    | 5       | for Scripts                 |
|            | 9       | for GIF /Flash /Video       |
| MemberType | 10      | for text/Flash              |
| ScriptB    | 11      | for Scripts                 |
|            | 12      | for text/Flash              |
| BitmapType | 16      | for Bitmaps                 |
| created    | 17      | All                         |
| modified   | 18      | All                         |
| Username   | 19      | All                         |
| Comment    | 20*     | All                         |
|            | 21*     |                             |
* = only some types of members have 22 values other only 20, like custom painted bitmap
0-index table



External casts have both **name** and **path** strings; internal ones only the name.

| Offset  | Size  | Example Bytes                | Meaning                                             |
|--------:|-------|------------------------------|-----------------------------------------------------|
| 0x00    | 4 × N | `00 00 00 00`, `00 00 01 23` | Resource ID of each cast member (`0` = empty slot). |
| —       | —     | —                            | Table length = number of member slots × 4 bytes.    |



---

## Cinf — Cast Info Block
| Offset | Size | Example Bytes | Description                           |
|-------:|------|---------------|---------------------------------------|
| 0x00   | 4    | `00 00 00 04` | Version or constant header.           |
| 0x04   | 2    | `00 05`       | Entry count or tag ID.                |
| 0x06   | 24   | `00 00 00 00 00 00 00 00 00 00 00 12 00 00 00 1A 00 00 00 65 00 00 00 67` | Six 32-bit offsets (relative positions of subfields). |
| 0x1E   | 2    | `00 01`       | Possibly display flag.                |
| 0x20   | 2    | `00 01`       | Unknown flag.                         |
| 0x22   | 2    | `00 03`       | Row width index (e.g. 0–3 = 8–Fit).   |
| 0x24   | 2    | `00 01`       | Visible-count index (e.g. 512–32000). |
| 0x26   | 4    | `00 00 00 01` | Unknown / flag bits.                  |
| 0x2A   | 4    | `00 00 00 00` | Reserved.                             |
| 0x2E   | 2    | `04 9F`       | Possibly flags or timestamp high.     |
| 0x30   | 4    | `00 00 00 00` | Reserved.                             |
| 0x34   | 2    | `01 1D`       | Unknown.                              |
| 0x36   | 2    | `01 D1`       | Unknown.                              |
| 0x38   | 1    | `49`          | Pascal string length (73).            |
| 0x39   | 73   | ASCII text    | `D:\...\Casts` — folder path of external cast. |
| ...    | padding | `00 00 00` | Aligns to 4-byte boundary.            |


#### Enum  row Width Type
|    | Amount        |
| -- | ------------- |
| 00 | 8 Thumbnails  |
| 01 | 10 Thumbnails |
| 02 | 20 Thumbnails |
| 03 | Fit to window |

#### Number of Visible Member Types in director
|    | Amount        |
| -- | ------------- |
| 00 | 512           |
| 01 | 1000          | 
| 02 | 2000          | 
| 03 | 5000          | 
| 04 | 10000         |
| 05 | 32000         |
       
*To find flags :*
Number, Created, Modified, Modified Date, Script, Modified By, Type, Filename, Size, Comments
00 03 = Number
00 11 = Type
00 45 = Modified + Created
04 9F = 
00 1B = Number Scripts and Types
07 FF = All columns visible
TODO: Parse flags








---

## LsCM (a.k.a. MCsL) — Cast Library List
| Offset  | Size | Example Bytes | Meaning                                  |
|--------:|------|---------------|------------------------------------------|
| 0x00    | 4    | `00 00 00 0C` | Header length (constant).                |
| 0x04    | 4    | `00 00 00 04` | Number of cast libraries.                |
| 0x08    | 2    | `00 04`       | Constant marker.                         |
| 0x0A    | 4    | `00 00 00 11` | Offset-count = (castCount×4)+1.          |
| 0x0E    | *    | `...`         | Table of (offsetCount) × 4-byte offsets. |


[← Back to the format overview](./README.md)