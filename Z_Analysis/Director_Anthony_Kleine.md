---
generator: Aspose.Words for Java 23.4.0;
---

More Director Movie File Unofficial Documentation

*Anthony Kleine (Team Earthquake)*

Documentation on the files with DIR, DXR, CST, and CXT types used by
Macromedia and later Adobe Director, with a brief look into the files
with DCR/CCT types as well.

**Note:** This is for Director 8.5, which is my main version of
interest. Director 12 is largely the same. Older versions not so much.

Object Types

VList (Vector List)

Defined in Director API, as they are the in-memory representation of
Lingo Lists. This structure comes in both 16-bit and 32-bit forms. They
always begin with a MotorollaINT32 specifying the size of the numbers
buffer of the VList, followed by either a series of MotorollaINT16 or
MotorollaINT32 numbers depending on the type of VList. Following that is
a MotorollaINT16 specifying the number of items in the list, and then
the end offsets of each list item. Each list item takes the form of a
buffer which can have a string or any arbitrary bytes.

Rect (Rectangle)

Coordinates for a 2D Rect expressed as four INT16s in a row.

Point (Point)

Coordinates for a 2D Point expressed as two INT16s in a row.

Vector (Vector)

Coordinates for a 3D Vector expressed as three INT16s in a row.

Palette (Palette)

The number for a palette in the movie. Negative numbers correspond to
default palettes.

Symbol (Symbol)

Akin to an enum in C++, they appear to be strings on the face of it but
are actually covering for an integer.

Chunks

Input Map (imap)

The Input Map must be the first chunk in the Director Movie File. It has
the absolute address of the Memory Map within the current file. The
Input Map is read directly from the file rather than read into memory.
It is changed when a new Memory Map is created, in order to avoid
overwriting the old one, which would take more time. The Save and
Compact feature removes old Memory Maps, updating this chunk. The Input
Map also has the version of Memory Map to use.

INT32 memoryMapCount = initialMapChunkBuffer.readInt32();

INT32 memoryMapOffset = initialMapChunkBuffer.readInt32();

INT32 memoryMapFileVersion = initialMapChunkBuffer.readInt32();

INT16 reserved = initialMapChunkBuffer.readInt16();

INT16 unknown = initialMapChunkBuffer.readInt16();

INT32 reserved2 = initialMapChunkBuffer.readInt32();

Memory Map (mmap)

The Memory Map has an array of resources within the Director Movie File.
Resources are Director\'s interpretation of chunks in the file. The
index of these resources correspond to what is called a ResourceID
(however, not all resources are in the Memory Map.) ResourceIDs are
INT32s. For example, a ResourceID of 4 can refer to the fifth resource
in the Memory Map (because they are zero indexed.) Negative one is the
null ResourceID, and is invalid. The Memory Map is read into memory, and
is not read directly from the file. It may be named after the Unix mmap
function.

One may wonder why the Memory Map needs to exist in a RIFX Container
where each chunk is one after the other. The reason is that not all of
these chunks are used, because when saving over an old file, deleting
one of these chunks would mean moving everything after it up in the
file, taking a significant amount of time. Instead, the Memory Map
resources point to the chunks being used, and deleted chunks are simply
not pointed to anymore by their resources instead of being outright
deleted.

Properties

The first property in the Memory Map is the Properties Size, which seems
to be ignored by recent Director versions. The second value is the
Resource Size, which is not ignored, and is the size of an individual
resource as the name would suggest. The next two values are the Max
Resource Count and the Used Resource Count. These two values describe
the maximum amount of resources allowed in this Memory Map before a new
one must be created and the Input Map be updated. The second value
describes the number of resources that currently exist in this Memory
Map.

The Memory Map has extra bytes on the end, past the number of used
resources, in order to allow easy adding of resources to the Memory Map
and ensure a new Memory Map doesn't have to be created often.

The next three properties are all ResourceIDs. It is not currently
entirely for certain what they do, but they do exhibit common patterns.
Remember that ResourceIDs will be -1 if null (to avoid using 0 because
they are zero-indexed) which is often the case with these ResourceIDs.

The first is the ResourceID of the last junk resource in the Memory Map.
Resources are considered junk if they exist but are empty - for example,
a Labels resource if there are no labels in the movie. Furthermore, each
junk chunk entry's lastResourceID points to the junk chunk before it.
This allows Director to quickly loop through all junk chunks to see how
many chunks in the Memory Map are not in use.

The second appears to be the ResourceID of a resource that corresponds
to the previous Memory Map before this one was created.

The third is the ResourceID of the last free resource in the Memory Map.
This is a similar scenario to junk resources. Free resources correspond
to deleted chunks or chunks that may eventually exist.

INT16 propertiesSize = memoryMapChunkBuffer.readInt16();

INT16 resourceSize = memoryMapChunkBuffer.readInt16(); // have confirmed
this in IDA to be true

INT32 maxResourceCount = memoryMapChunkBuffer.readInt32();

INT32 usedResourceCount = memoryMapChunkBuffer.readInt32();

ResourceID firstJunkResourceID = memoryMapChunkBuffer.readResourceID();

ResourceID oldMemoryMapResourceID =
memoryMapChunkBuffer.readResourceID();

ResourceID firstFreeResourceID = memoryMapChunkBuffer.readResourceID();

Resources

After these properties are the resources. Each resource is a memory
mapping which has a ChunkID, size of memory to allocate, the absolute
position of the chunk in the file, flags, a seemingly unused value, and
the number of cast member that has locked the chunk if any (or zero if
none, since cast member numbers are not zero indexed and start at one.)
The size and offset can be negative one if the resource has not been
properly initialized.

Array resources \[

ChunkID id = resourceBuffer.readChunkID();

INT32 size = resourceBuffer.readInt32();

INT32 offset = resourceBuffer.readInt32();

UINT32 flags = resourceBuffer.readUInt16();

resourceBuffer.readInt16(); // UNUSED

INT32 nextResourceID = resourceBuffer.readInt32();

\]

Key Table (KEY\*)

The Key Table must be the third chunk in the file. It allows for an
"owned by"relationship to exist between resources. For example, Sound
Headers and Sound Samples can be "owned by"a Cast Member. A Cast
Member's Thumbnail can also be "owned by"a Cast Member. Crucially, the
Key Table is context specific. This is because ResourceIDs do not
uniquely identify resources. You may only reliably use it if you already
know which resource is the owner and the ChunkID of the chunk you want
to find.

This is best illustrated by the fact that all of the resources owned by
the movie itself are "owned by"a resource with ResourceID 1024. This is
always true - it is a constant within Director itself, and it is
hardwired to be this number. However, it is entirely possible for the
Memory Map to also have a 1024th resource - say, a bitmap Cast Member.
In that scenario, the movie and the bitmap Cast Member resource will
share ResourceID 1024, and all of the resources they both own - such as
the movie-owned Score, and the bitmap Cast Member-owned Bitmap Data -
will be owned by ResourceID 1024. However, since Director only cares
about the Bitmap Data for bitmap Cast Members, it ignores the Score
which is owned by the movie, and since Director only cares about the
Score for the movie, it ignores the Bitmap Data.

The same is true for the ResourceIDs of the Cast resources. Cast Info,
Cast Tables, Lingo Contexts and Score References may all belong to a
Cast. The ResourceIDs of the Casts are defined in the Cast Properties
and may be any arbitrary number.

Properties

Much like the Memory Map, the Key Table has a maximum number of allowed
entries. However, it is for a different reason: Director doesn't use the
Key Table right away. It only uses the Key Table when it needs to find a
resource owned by a different resource, and when it does, it loops
through every entry in the Key Table. It applies an algorithm which skip
counts over entries until it has passed the one it is looking for, and
then it heads back. The reason for the extra unused memory on the end of
the Key Table is to allow for easy addition of resources to the table.
This is also why the entries must be in numerical order by Owner ID.

INT16 propertiesSize = keyTableChunkBuffer.readInt16();

INT16 keySize = keyTableChunkBuffer.readInt16();

INT32 maxKeyCount = keyTableChunkBuffer.readInt32();

INT32 usedKeyCount = keyTableChunkBuffer.readInt32();

Table Array

Array keys \[

ResourceID ownedResourceID = keyTableChunkBuffer.readResourceID();

ResourceID ownerResourceID = keyTableChunkBuffer.readResourceID();

ChunkID ownedChunkID = keyTableChunkBuffer.readChunkID();

\]

Config (VWCF/DRCF)

In older versions of Director, the ChunkID for the Config was VWCF, but
sometime after Director 6 it was replaced with DRCF. They are both
exactly identical, but DRCF cannot be loaded in older Director versions.
The VW stands for VideoWorks and the DR stands for Director.

MotorollaINT16 size = configChunkBuffer.readMotorollaInt16();

MotorollaUINT16 fileVersion = configChunkBuffer.readMotorollaUInt16();
// 0x163C if protected

Rect sourceRect = configChunkBuffer.readRect();

MotorollaINT16 minMember = configChunkBuffer.readMotorollaInt16(); //
obsolete, see Cast Properties

MotorollaINT16 maxMember = configChunkBuffer.readMotorollaInt16(); //
obsolete, see Cast Properties

INT8 tempo = configChunkBuffer.readInt8();

configChunkBuffer.readInt8();

Array bgColor = \[\]

INT8 bgColor\[1\] = configChunkBuffer.readInt8();

if (fileVersion \<= 0x4C7) {

bgColor\[1\] = 255

}

INT8 bgColor\[2\] = configChunkBuffer.readInt8();

if (fileVersion \<= 0x4C7) {

bgColor\[2\] = 255

}

configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

// these unknown values found via reverse analysis

MotorollaINT16 unknown = configChunkBuffer.readMotorollaInt16();

if (fileVersion \<= 0x4C6) {

unknown = 0;

}

INT8 bgColor\[0\] = configChunkBuffer.readInt8();

if (fileVersion \<= 0x4C7) {

bgColor\[0\] = 255

}

configChunkBuffer.readInt8();

configChunkBuffer.readMotorollaInt16();

UINT8 unknown2 = configChunkBuffer.readInt8();

if (fileVersion \<= 0x551) {

unknown2 = -2;

}

configChunkBuffer.readInt8();

configChunkBuffer.readMotorollaInt32();

MotorollaINT16 movieFileVersion =
configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

MotorollaINT32 unknown3 = configChunkBuffer.readMotorollaInt32();

if (fileVersion \<= 0x6A3) {

unknown3 = 0;

}

MotorollaINT32 unknown4 = configChunkBuffer.readMotorollaInt32();

if (fileVersion \<= 0x73A) {

unknown4 = 1;

}

MotorollaINT32 unknown5 = configChunkBuffer.readMotorollaInt32();

if (fileVersion \<= 0x4C7) {

unknown5 = 0;

}

INT8 trial = configChunkBuffer.readInt8();

if (fileVersion \<= 0x742) {

unknown6 = 0;

}

INT8 unknown7 = configChunkBuffer.readInt8();

if (fileVersion \<= 0x4C7) {

unknown7 = 80;

}

configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

INT16 random = configChunkBuffer.readMotorollaInt16(); // if fileVersion
\> 0x459 and this is divisible by 0x17, movie is protected

configChunkBuffer.readMotorollaInt32();

configChunkBuffer.readMotorollaInt32();

MotorollaINT16 oldDefaultPalette =
configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

MotorollaINT32 unknown8 = configChunkBuffer.readMotorollaInt32();

if (fileVersion \<= 0x4C0) {

unknown8 = 1024;

}

// in Director this is read as two MotorollaINT16s?

Palette defaultPalette = configChunkBuffer.readMotorollaInt32();

if (fileVersion \<= 0x578) {

MotorollaINT16 unknown11 = configChunkBuffer.readMotorollaInt16();

if (unknown11 == 1) {

INT8 unknown9 = 0

INT8 unknown10 = 0

} else {

if (unknown11 == 2) {

INT8 unknown9 = 0

INT8 unknown10 = 1

} else {

INT8 unknown9 = 1

INT8 unknown10 = 0

}

}

} else {

INT8 unknown9 = configChunkBuffer.readMotorollaInt8();

INT8 unknown10 = configChunkBuffer.readMotorollaInt8();

}

configChunkBuffer.readMotorollaInt16();

if (fileVersion \>= 0x73A) {

MotorollaINT32 downloadFramesBeforePlaying =
configChunkBuffer.readMotorollaInt32();

} else {

MotorollaINT32 downloadFramesBeforePlaying = 90

configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

configChunkBuffer.readMotorollaInt16();

}

if (fileVersion \<= 0x45D) {

defaultPalette = oldDefaultPalette;

}

Score (VWSC)

The Score consists of two main parts: notation and sprite properties.
The notation has instructions on where to put certain buffers in the
memory for them to be interpreted properly by Director.

Basically, there were so many null bytes between actual useful data in
these buffers that the engineers at Macromedia thought it'd be a good
idea to describe the offset and size of everything that isn't a null
byte instead of saving the whole buffer to a file.

This is what is saved to the file, and needs to be decompressed.

MotorollaINT32 memHandleSize = scoreChunkBuffer.readMotorollaInt32(); //
this is interpreted as the beginning of a memHandle, the size of it

MotorollaINT32 headerType = scoreChunkBuffer.readMotorollaInt32(); //
always -3?

MotorollaINT32 spritePropertiesOffsetsCountOffset =
scoreChunkBuffer.readMotorollaInt32(); // offset of
spritePropertiesOffsetsCount from beginning of chunk, must be twelve

MotorollaINT32 spritePropertiesOffsetsCount =
scoreChunkBuffer.readMotorollaInt32();

MotorollaINT32 notationOffset = scoreChunkBufferScoreBuffer.readInt32()
\* 4 + 12 + scoreSpritePropertiesOffsetsCountOffset; // this has been
confirmed in IDA to be true\
MotorollaINT32 notationAndSpritePropertiesSize =
scoreChunkBuffer.readMotorollaInt32();

Array spritePropertiesOffsets \[

INT16 spritePropertiesOffset = scoreChunkBuffer.readInt16();

\] // the first one is unused

// notation

MotorollaINT32 framesEndOffset = scoreChunkBuffer.readMotorollaInt32();

skip(4)

INT32 framesSize = scoreChunkBuffer.readInt32();

INT16 framesType = scoreChunkBuffer.readInt16();

INT16 channelSize = scoreChunkBuffer.readInt16();

INT16 lastChannelMax = scoreChunkBuffer.readInt16();

INT16 lastChannel = scoreChunkBuffer.readInt16();

Object frames {

INT16 frameEnd = scoreChunkBuffer.readInt16();

Array channels \[

INT16 size = scoreChunkBuffer.readInt16();

INT16 offset = scoreChunkBuffer.readInt16();

Buffer buffer = scoreChunkBuffer.readInt16();

\]

MotorollaINT32 spritePropertiesOffsetElementCount =
scoreChunkBuffer.readMotorollaInt32();

Array spritePropertiesOffsetElements \[

INT32 element = scoreChunkBuffer.readInt32();

} // indices to load for tracks in spritePropertiesOffsets

Array spriteProperties \[

MotorollaINT32 startFrame = scoreChunkBuffer.readMotorollaInt32();

MotorollaINT32 endFrame = scoreChunkBuffer.readMotorollaInt32();

skip(4)

MotorollaINT32 channel = scoreChunkBuffer.readMotorollaInt32(); // these
start at 5 because of legacy reasons since Director 5 had five reserved
tracks for specific purposes

MotorollaINT32 number = scoreChunkBuffer.readMotorollaInt32();

skip(28)

\] // each of these are 0x2C long and are loaded immediately, contain
the start and end frame and channel number - the channels start at 6 for
bitmaps because the five others from D5 are still there but hidden

Channels

If read correctly, the resulting buffer looks like this.

Array channels \[

// only investigated for bitmaps, may be different for sounds

// where is ink mask and behaviors?

UINT8 flags = scoreChunkBuffer.readUInt8();

Boolean multipleMembers = !(flags & 0x10 \>\> 4)

UINT8 temp = scoreChunkBuffer.readUInt8();

Boolean inkFlag = temp & 0x80 \>\> 7

UINT8 spriteProperties.ink = temp & 0x7F

UINT8 spriteProperties.foreColor = scoreChunkBuffer.readUInt8();

UINT8 spriteProperties.backColor = scoreChunkBuffer.readUInt8();

MotorollaUINT32 display.member = scoreChunkBuffer.readMotorollaUInt32();

skip(2)

MotorollaINT16 spritePropertiesOffset =
scoreChunkBuffer.readMotorollaUInt16();

MotorollaINT16 geometry.locV = scoreChunkBuffer.readMotorollaUInt16();

MotorollaINT16 geometry.locH = scoreChunkBuffer.readMotorollaUInt16();

MotorollaINT16 geometry.height = scoreChunkBuffer.readMotorollaUInt16();

MotorollaINT16 geometry.width = scoreChunkBuffer.readMotorollaUInt16();

UINT8 flags = scoreChunkBuffer.readUInt8();

Boolean spriteProperties.editable = flags & 0x40 \>\> 6

UINT8 scoreColor = flags & 0x0F = scoreChunkBuffer.readUInt8();

UINT8 display.blend = scoreChunkBuffer.readUInt8();

UINT8 flags = scoreChunkBuffer.readUInt8(); // is 0x81 one frame after
sprite start, why?

Boolean geometry.flipV = flags & 0x04 \>\> 2

Boolean geometry.flipH = flags & 0x02 \>\> 1

skip(5)

Float32 geometry.rotation = scoreChunkBuffer.readFloat32();

Float32 geometry.skew = scoreChunkBuffer.readFloat32();

skip(12)

\]

Labels (VWLB)

This begins with the size of an FArray, followed by the FArray of all
the labels for frames. Each array item is a UINT32 with the higher two
bytes being the offset in the labelList of the label for a frame and the
lower two bytes being its corresponding frame number. The labelList may
contain carriage returns, delimiting labels.

INT16 fArraySize = labelsChunkBuffer.readInt16();

FArray fArray = labelsChunkBuffer.readFArray(fArraySize, 4);

INT32 labelListLength = labelsChunkBuffer.readInt32();

String labelList = labelsChunkBuffer.readString(labelListLength);

FileInfo (VWFI)

This has the Global Movie Properties. These properties aren\'t part of
the FileInfo because they are global.

VList32 vList = fileInfoChunkBuffer.readVList32();

String createName = vList\[2\];

String modifyName = vList\[3\];

String pathName = vList\[4\];

CastInfo (VWCI/Cinf)

Not Yet Investigated

Movie Xtra List (XTRl, Director 5.0+)

This is a series of buffers with a single Vector List each, describing
the different Xtras for the movie. This draws some information from
xtrainfo.txt but additionally has the GUID of each required component.
Corresponds to Lingo\'s the movieXtraList property.

skip(4)

INT32 vListsCount = xtraListChunkBuffer.readInt32();

for(var i=0;i\<vListsCount;i++) {

INT32 vListCount = xtraListChunkBuffer.readInt32();

VList16 vList = xtraListChunkBuffer.readVList16();

Buffer theGUID = vList.numbers\[2\] + vList.numbers\[3\] +
vList.numbers\[4\] + vList.numbers\[5\] + vList.numbers\[6\] +
vList.numbers\[7\] + vList.numbers\[8\] + vList.numbers\[9\] +
vList.numbers\[10\]

for(var j=0;j\<vList.length;j++) {

vList\[j\].skip(2)

UINT8 xtraInfoNameLength

String xtraInfoName = vList\[j\].readString(xtraInfoNameLength)

}\
}

Font Map (FXmp)

Contents of fontmap.txt from the creator's Adobe Director install
directory. The ChunkID of FXmp is short for FontXtra Map. TODO

Fields Map (Fmap)

Various info pertaining to text fields. TODO

Cast Properties (MCsL, Director 5.0+)

This has a single VList with properties of every Cast. This chunk is
crucial since it has the ResourceIDs that own Cast Properties in the Key
Table. Added in Director 5 to support multiple Casts per movie. Thanks
to MrBrax for his early preliminary research.

VList16 vList = castPropertiesChunk.readVList16();

Array casts = \[\];

for(var i=0;i\<vList\[1\];i++) {

String casts\[i\].name = vList\[(i \* 4) + 2\].readString(vList\[(i \*
4) + 2\].readUInt8());

String casts\[i\].fileName = vList\[(i \* 4) + 3\].readString(vList\[(i
\* 4) + 3\].readUInt8());

UINT8 casts\[i\].minMember = vList\[(i \* 4) + 5\].readUInt8();

UINT8 casts\[i\].maxMember = vList\[(i \* 4) + 5\].readUInt8();

INT16 casts\[i\].memberCount = vList\[(i \* 4) + 5\].readInt16BE();

ResourceID casts\[i\].ownerResourceID = vList\[(i \* 4) +
5\].readResourceID();

}

Cast Table (CAS\*)

Array resourceIDs \[

ResourceID resourceID

\]

CastMember Properties (VWCR/CASt)

TODO

MotorollaINT32 member.commonMemberProperties.type =
castMemberPropertiesChunk.readMotorollaInt32();

MotorollaINT32 memberPropertiesBufferSize =
castMemberPropertiesChunk.readMotorollaInt32();

MotorollaINT32 typePropertiesBufferSize =
castMemberPropertiesChunk.readMotorollaInt32();

Member Properties

VList32 vList = castMemberPropertiesChunk.readVList32();

UINT8 this.member.commonMemberProperties.purgePriority =
vList.numbers\[2\] \>\> 4;

UINT8 this.type.playbackProperties.autohilite = vList.numbers\[2\] \>\>
1 & 1;

String member.commonMemberProperties.scriptText =
trimNull(vList\[0\].toString())

String member.commonMemberProperties.name = vList\[1\]

String member.commonMemberProperties.filePath = vList\[2\]

String member.commonMemberProperties.fileName = vList\[3\]

String member.commonMemberProperties.fileType = vList\[4\]

GUID member.commonMemberProperties.requiredComponentGUID = vList\[9\]

String member.commonMemberProperties.requiredComponentName = vList\[10\]

FArray member.graphicProperties.\_regPoints = vList\[12\]

String member.commonMemberProperties.clipboardFormat = vList\[16\]

Date member.commonMemberProperties.creationDate = vList\[17\]

Date member.commonMemberProperties.modifiedDate = vList\[18\]

String member.commonMemberProperties.modifiedBy = vList\[19\]

String member.commonMemberProperties.comments = vList\[20\]

Boolean type.playbackProperties.imageCompression = vList\[21\]\[0\] &
0x04

Boolean type.playbackProperties.imageQuality = vList\[21\]\[1\]

Type Properties

Bitmap or OLE

Float16 member.mediaProperties.fileSize =
castMemberPropertiesChunk.readFloat16(); // not for certain, but it does
change with file size

Rect member.graphicProperties.rect =
castMemberPropertiesChunk.readRect();

UINT8 type.playbackProperties.alphaThreshold =
castMemberPropertiesChunk.readUInt8();

skip(7)

Point member.graphicProperties.regPoint =
castMemberPropertiesChunk.readPoint();

UINT8 flags = castMemberPropertiesChunk.readUInt8();

Boolean type.playbackProperties.trimWhiteSpace = flags & 0x80 \>\> 7

Boolean type.mediaProperties.centerRegPoint = flags & 0x20 \>\> 5

Boolean type.playbackProperties.dither = flags & 0x01

INT8 type.playbackProperties.depth =
castMemberPropertiesChunk.readInt8();

INT32 type.mediaProperties.palette =
castMemberPropertiesChunk.readInt32();

Film Loop, Movie, Digital Video, or Xtra

// where is scriptsEnabled?

Symbol member.commonMemberProperties.type =
symbol(castMemberPropertiesChunk.readString(castMemberPropertiesChunk.readUInt8()))

skip(10)

UINT8 flags = castMemberPropertiesChunk.readUInt8();

Boolean type.playbackProperties.streaming = flags & 0x01

Boolean type.displayProperties.streaming =
type.playbackProperties.streaming

UINT8 flags

Boolean type.playbackProperties.sound = flags & 0x02 \>\> 1

Boolean type.playbackProperties.pausedAtStart = flags & 0x01

UINT8 flags = castMemberPropertiesChunk.readUInt8();

Boolean type.playbackProperties.loop = flags & 0x40 \>\> 6

Boolean type.displayProperties.invertMask = flags & 0x20 \>\> 5

Boolean type.displayProperties.directToStage = flags & 0x10 \>\> 4

Boolean type.displayProperties.video = flags & 0x08 \>\> 3

Boolean type.displayProperties.center = flags & 0x04 \>\> 2

Boolean type.displayProperties.crop = flags & 0x02 \>\> 1

Boolean type.displayProperties.controller = flags & 0x01

skip(3)

INT8 type.playbackProperties.frameRate =
castMemberPropertiesChunk.readInt8();

skip(32)

Rect member.graphicProperties.rect =
castMemberPropertiesChunk.readRect();

Text or Button

castMemberPropertiesChunk.readInt32();

switch(Math.sign(castMemberPropertiesChunk.readInt16())) {

case 0:

String type.textProperties.alignment = \"center\";

break;

case 1:

String type.textProperties.alignment = \"left\";

break;

case -1:

String type.textProperties.alignment = \"right\";

}

Array type.chunkProperties.bgColor

UINT8 type.chunkProperties.bgColor\[0\] =
castMemberPropertiesChunk.readUInt8();

castMemberPropertiesChunk.readUInt8();

UINT8 type.chunkProperties.bgColor\[1\] =
castMemberPropertiesChunk.readUInt8();

castMemberPropertiesChunk.readUInt8();

UINT8 type.chunkProperties.bgColor\[2\] =
castMemberPropertiesChunk.readUInt8();

castMemberPropertiesChunk.readUInt8();

skip(2)

Rect member.graphicProperties.rect =
castMemberPropertiesChunk.readRect();

UINT16 type.chunkProperties.lineHeight =
castMemberPropertiesChunk.readUInt16();

castMemberPropertiesChunk.readUInt32();

INT16 type.buttonProperties.buttonType =
castMemberPropertiesChunk.readInt16();

Palette

None

Picture

Unknown

Shape

INT16 type.displayProperties.shapeType =
castMemberPropertiesChunk.readInt16();

Rect member.graphicProperties.rect =
castMemberPropertiesChunk.readRect();

INT16 type.displayProperties.pattern =
castMemberPropertiesChunk.readInt16();

skip(2)

UINT8 flags = castMemberPropertiesChunk.readUInt8();

Boolean type.displayProperties.filled = flags & 0x01

INT8 type.displayProperties.lineSize =
castMemberPropertiesChunk.readInt8() - 1;

INT8 type.displayProperties.lineDirection =
castMemberPropertiesChunk.readInt8() - 5;

Script

INT16 type.scriptProperties.scriptType =
castMemberPropertiesChunk.readInt16() / 9;

Rich Text

TODO

Transition

TODO

Lingo Context (Lctx)

There exists one Lingo Context per Cast. They are the context within
which many scripts can exist. These are integral to the movie, and the
scripts are not owned by their individual Cast Members in the Key Table
as you may expect. Instead, each Lingo Script has the number of its
corresponding Cast Member.

// update movie properties

skip(20)

lingoContextChunkBuffer.readMotorollaINT32();

lingoContextChunkBuffer.readMotorollaINT32();

lingoContextChunkBuffer.readMotorollaINT32();

ResourceID lingoNamesResourceID =
lingoContextChunkBuffer.readResourceID();

MotorollaINT16 size = lingoContextChunkBuffer.readMotorollaINT16();

MotorollaINT16 flags = lingoContextChunkBuffer.readMotorollaINT16();

ResourceID freeResourceIDlingoContextChunkBuffer.readResourceID();

Lingo Names (Lnam)

There exist Lingo Names for every Lingo Context. This chunk has six
properties ignored by Director, likely for the Update Movies feature.
The rest of the chunk is just an array of Strings.

// TODO

Lingo Script (Lscr)

This is a compiled script, equivalent to a Cast Member's scriptText
property but as bytecode. Check the repo for more info on the bytecode.
This is mostly copied from Brian's research.

MotorollaINT32 size = lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT32 size2 = lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT16 headerSize = lingoScriptChunkBuffer.readMotorollaInt16();

skip(36)

MotorollaINT16 number = lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 scriptType = lingoScriptChunkBuffer.readMotorollaInt32();

skip(8)

MotorollaINT16 handlerVectorSize =
lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 handlerVectorOffset =
lingoScriptChunkBuffer.readMotorollaInt32();

UINT32 handlerVectorFlags = lingoScriptChunkBuffer.readUInt32();

MotorollaINT16 propertiesCount =
lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 propertiesNameOffset =
lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT16 globalCount =
lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 globalNameOffset =
lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT16 handlerCount =
lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 handlerOffset =
lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT16 literalsCount =
lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 literalsOffset =
lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT32 literalValuesSize =
lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT32 literalValuesOffset =
lingoScriptChunkBuffer.readMotorollaInt32();

Array globals \[

MotorollaINT16 lingoNameElement =
lingoScriptChunkBuffer.readMotorollaInt16();

\]

Array properties \[

MotorollaINT16 lingoNameElement =
lingoScriptChunkBuffer.readMotorollaInt16();

\]

Array handlers \[

MotorollaUInt16 name = lingoScriptChunkBuffer.readMotorollaUInt16(); //
use names table

INT16 handler = lingoScriptChunkBuffer.readInt16(); // offset in handler
vector, or -1 if not standard

MotorollaINT32 size = lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT32 offset = lingoScriptChunkBuffer.readMotorollaInt32(); //
relative to entire section

MotorollaINT16 argumentsCount =
lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 argumentsOffset =
lingoScriptChunkBuffer.readMotorollaInt32(); // relative to entire
section

MotorollaINT16 localsCount =
lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 localNamesOffset =
lingoScriptChunkBuffer.readMotorollaInt32(); // relative to entire
section

MotorollaINT16 xCount = lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 xOffset = lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT32 unknown1 = lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT16 unknown2 = lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT16 lineCount = lingoScriptChunkBuffer.readMotorollaInt16();

MotorollaINT32 lineOffset = lingoScriptChunkBuffer.readMotorollaInt32();
// relative to entire section

MotorollaINT32 stackHeight =
lingoScriptChunkBuffer.readMotorollaInt32();

\]

// handler bytecode seems to appear in this area

Array handlers {

Buffer bytecode

Array parameters {

MotorollaINT16 lingoNameElement =
lingoScriptChunkBuffer.readMotorollaInt16();

}

Array lineSizes {

INT8 size = lingoScriptChunkBuffer.readInt8(); // the numbers of bytes
in the bytecode that corresponds to one line of text for error reporting

}

}

Array literals \[

MotorollaINT32 type = lingoScriptChunkBuffer.readMotorollaInt32();

MotorollaINT32 offset = lingoScriptChunkBuffer.readMotorollaInt32(); //
relative to literalValuesPos

\]

Array literalValues \[

MotorollaINT32 length = lingoScriptChunkBuffer.readMotorollaInt32();

Buffer value = lingoScriptChunkBuffer.read(length);

\]

Array handlerVector {

MotorollaINT16 handlerNumber =
lingoScriptChunkBuffer.readMotorollaInt16(); // -1, or handler number of
handler in script

}

Score References (SCRF)

This chunk exists in case there are collaborators working together on a
movie. It keeps track of the cast libraries and members from external
casts being used in the score so if an external cast is modified and a
cast library or member is deleted from it, Director can ask if you'd
like to update the movie to match the external cast or leave the movie
alone so the issue can hopefully be resolved. It corresponds to the Cast
Table of the external cast.

Cast Clipboard (ccl )

Not Yet Investigated

Score Order (Sord)

This chunk has six properties ignored by Director, likely for the Update
Movies feature, followed by an FArray for the remainder of the chunk,
with the Cast Member numbers. It\'s likely to do with how moving Cast
Members around but keeping their numbers in the Score correct is handled
but more precise details are unknown.

Version (VERS)

More detailed version information such as locale. TODO

Favorite Colors (FCOL)

Sixteen colors that appear at the top of the color picker. By default,
sixteen shades of gray from black to white. If one of the colors is not
in the palette used, the next closest color in the palette will be
picked instead.

Publish Settings (PUBL)

Strings for the inputs in the Publish Settings window. Also each
character of the string is 32-bits...wait, what? 32-bits long!? Also
note that the width is 32-bit and the height is 16-bit. I just thought
I'd point that out. I can't make this garbage up. TODO

Guides and Grid (GRID)

Some of the Guides and Grid properties as they appear in the Property
Inspector.

skip(4)

INT16 grid.width = guidesAndGridChunkBuffer.readInt16();

INT16 grid.height = guidesAndGridChunkBuffer.readInt16();

INT16 grid.display = guidesAndGridChunkBuffer.readInt16();

switch (grid.display) {

case 2:

grid.display = "Dots"

break;

default:

grid.display = "Lines"\
}

INT16 grid.color = guidesAndGridChunkBuffer.readInt16();

INT16 guides.count = guidesAndGridChunkBuffer.readInt16();

INT16 guides.color = guidesAndGridChunkBuffer.readInt16();

Array guides \[

INT16 axis = guidesAndGridChunkBuffer.readInt16();

INT16 position = guidesAndGridChunkBuffer.readInt16();

\]

File Version (Fver)

A file version string. Empty in old versions.

File Compression Types (Fcdr)

This has the names and GUIDs of the MOA interfaces to use for
decompression. Examples include Ziplib compression (often referred to as
Standard compression within the Director interface,) Fontmap
compression, Shockwave Audio compression and JPEG compression.

TODO

AfterBurner Map (ABMP)

Shockwave Movie equivalent of the Memory Map. Ziplib compressed.
Resources with a pos of -1 are the next in the Initial Load Segment.

skip(8)

INT32 resourcesCount = afterBurnerMapChunkBuffer.readVarint();

Array resources \[

INT32 mappingID = afterBurnerMapChunkBuffer.readVarint();

INT32 offset = afterBurnerMapChunkBuffer.readVarint();

INT32 sizeCompressed = afterBurnerMapChunkBuffer.readVarint();

INT32 sizeDecompressed = afterBurnerMapChunkBuffer.readVarint();

INT32 compressionTypeID = afterBurnerMapChunkBuffer.readVarint();

ChunkID id = afterBurnerMapChunkBuffer.readChunkID();

\]

File Ending (FGEI)

Marks ends of RIFX Container in a Shockwave Movie. When \`the
traceLoad\` is two in a Shockwave Movie offsets are relative to this
chunk\'s offset.

Initial Load Segment (ILS )

Has chunks that must be loaded first before anything else in a Shockwave
Movie. The data in this chunk is completely variable since it has the
MappingIDs and data for other chunks. The name of this chunk is known
thanks to an Autodesk document on Shockwave 3D.
