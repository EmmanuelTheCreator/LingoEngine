using Director.IO.Data;
using Director.Primitives;
using Director.Tools;
using Director.Fonts;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.IO.Compression;
using static Director.IO.Data.FileVWSCData;

namespace Director.IO
{
    // https://github.com/tomysshadow/director-files-extract/blob/master/shock.py
    // https://github.com/tomysshadow/Format-Documentation/blob/master/structure/core/formatNotes_DCR.txt

    public partial class ArchiveFileLoader
    {
        public static byte foreColor { get; private set; }

        public DirectorFileData ReadFile(string fileName)
        {
            var archive = new Archive(isBigEndian: true);
            LoadInto(archive, fileName);
            LogHelper.DebugLog(2, DebugChannel.Loading, $"Tags: {string.Join(',',archive.ListTagStringsWithId())}");

            var dirData = new DirectorFileData();
            dirData.Archive = archive;
            dirData.IMap = ReadIMap(archive.GetResource(ResourceTags.Imap, 0));
            dirData.MMap = ReadMMap(archive.TryGetResource(ResourceTags.Mmap, 0));
            dirData.KeyStar = ReadKeyStar(archive.GetResource(ResourceTags.KEYStar, 0));
            dirData.DRCF = ReadDRCF(archive.GetResource(ResourceTags.DRCF, 0));
            dirData.VWSC = ReadVWSC(archive.TryGetResource(ResourceTags.VWSC, 0));
            dirData.VWLB = ReadVWLB(archive.TryGetResource(ResourceTags.VWLB, 0));
            dirData.CASt = ReadCASt(archive.TryGetResource(ResourceTags.CASt, 0));
            return dirData;

            
        }

       

        public void LoadInto(Archive archive, string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fs);

            fs.Seek(0, SeekOrigin.Begin);
            uint metaFourCC = reader.ReadUInt32BE();
            bool littleEndian = metaFourCC == ResourceTags.XFIR;

            Func<uint> read32 = littleEndian ? reader.ReadUInt32 : reader.ReadUInt32BE;
            Func<ushort> read16 = littleEndian ? reader.ReadUInt16 : reader.ReadUInt16BE;

            uint metaLength = read32();
            uint codec = read32();

            if (codec == ResourceTags.MV93)
            {
                LogHelper.DebugLog(1, DebugChannel.Loading, "Detected MV93-format Director file (Director 9+). Using MV93 loader.");
                fs.Seek(0, SeekOrigin.Begin);
                LoadMv93Format(archive, fs, reader);
                return;
            }
            if (codec == ResourceTags.MKTAG('F','G','D','M') || codec == ResourceTags.MKTAG('F','G','D','C'))
            {
                LogHelper.DebugLog(1, DebugChannel.Loading, "Detected Afterburner Director file");
                LoadAfterburnerFormat(archive, fs, reader, littleEndian);
                return;
            }
            if (metaFourCC == ResourceTags.XFIR)
            {
                LogHelper.DebugLog(1, DebugChannel.Loading, "Detected XFIR-format Director file");
                fs.Seek(0, SeekOrigin.Begin);
                ParseXfir(fs, archive);
                return;
            }

            fs.Seek(0, SeekOrigin.Begin);

            var directory = new List<(uint tag, int id, int offset)>();

            int endHeader = FindEndHeader(path, fs, reader);
            LogHelper.DebugLog(2, DebugChannel.Loading, $"Resource directory starts at 0x{endHeader:X}");
            fs.Seek(endHeader, SeekOrigin.Begin);

            // Phase 1: Parse resource map
            ParseResourceMap(fs, reader, directory);

            // Phase 2: Register unique resource chunks
            RegisterUniqueResourceChunks(archive, path, fs, reader, directory);
        }

        private static void ParseResourceMap(FileStream fs, BinaryReader reader, List<(uint tag, int id, int offset)> directory)
        {
            const int MAX_ENTRIES = 256;
            for (int i = 0; i < MAX_ENTRIES && fs.Position + 12 <= fs.Length; i++)
            {
                long entryPos = fs.Position;
                uint tag = reader.ReadUInt32BE();
                int id = reader.ReadInt32BE();
                int offset = reader.ReadInt32BE();

                LogHelper.DebugLog(2, DebugChannel.Loading, $"Parsed raw at 0x{entryPos:X}: tag=0x{tag:X8}, id={id}, offset=0x{offset:X}");

                if (tag == 0 || offset <= 0 || offset >= fs.Length)
                {
                    LogHelper.DebugLog(2, DebugChannel.Loading,
                        $"Stopping directory parse at 0x{entryPos:X}: tag=0 or bad offset");
                    break;
                }

                bool isAscii = Enumerable.Range(0, 4).All(n =>
                {
                    byte ch = (byte)(tag >> (8 * (3 - n)));
                    return ch >= 32 && ch <= 126;
                });

                if (!isAscii)
                {
                    LogHelper.DebugWarning($"Non-ASCII tag at 0x{entryPos:X}: {tag:X8}");
                    break;
                }

                LogHelper.DebugLog(2, DebugChannel.Loading,
                    $"Entry[0x{entryPos:X}]: {ResourceTags.FromTag(tag)}#{id} -> 0x{offset:X}");
                directory.Add((tag, id, offset));
            }
        }

        private static void RegisterUniqueResourceChunks(Archive archive, string path, FileStream fs, BinaryReader reader, List<(uint tag, int id, int offset)> directory)
        {
            var grouped = directory.GroupBy(e => e.offset).Select(g => g.First()).OrderBy(e => e.offset).ToList();
            for (int i = 0; i < grouped.Count; i++)
            {
                var (tag, id, offset) = grouped[i];
                int nextOffset = (i + 1 < grouped.Count) ? grouped[i + 1].offset : (int)fs.Length;
                int size = nextOffset - offset;

                if (size <= 0 || offset + size > fs.Length)
                {
                    LogHelper.DebugWarning($"Invalid size for {ResourceTags.FromTag(tag)}#{id}: {size}");
                    continue;
                }
                var encryptedExts = new[] { ".cxt", ".cct", ".dcr", ".dxr" };
                archive.RegisterResource(tag, id, offset, size, () =>
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    byte[] raw = reader.ReadBytes(size);
                    Stream dataStream = new MemoryStream(raw, writable: false);

                    // Detect encrypted formats

                    if (encryptedExts.Contains(Path.GetExtension(path).ToLowerInvariant()))
                        dataStream = DirectorDecryptor.WrapWithDecryption(dataStream);

                    // Handle possible compression (e.g. .dcr/.dxr)
                    dataStream = DirectorDecompressor.WrapIfNeeded(dataStream);

                    return new SeekableReadStreamEndian(dataStream, isBigEndian: true);
                });

                LogHelper.DebugLog(2, DebugChannel.Loading,
                    $"Registered {ResourceTags.FromTag(tag)}#{id} @0x{offset:X}, sz={size}");
            }
        }

        private static int FindEndHeader(string path, Stream fs, BinaryReader reader)
        {
            fs.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[0x2000];
            fs.Read(buffer, 0, buffer.Length);

            // Look for '*YEK' (0x2A59454B) marker which always follows the directory offset
            for (int i = 0; i < buffer.Length - 8; i += 4)
            {
                uint tag = BitConverter.ToUInt32(buffer, i);
                if (tag == 0x2A59454B) // '*YEK'
                {
                    // Usually directory starts 4 bytes before this tag
                    int dirOffset = i - 4;
                    if (dirOffset >= 0)
                    {
                        int offset = BitConverter.ToInt32(buffer, dirOffset);
                        if (offset > 0 && offset < fs.Length)
                        {
                            LogHelper.DebugLog(2, DebugChannel.Loading, $"Detected *YEK at 0x{i:X}, directory offset={offset}");
                            return offset;
                        }
                    }
                }
            }

            // Fallback: look for first ASCII tag/ID/offset triplet
            fs.Seek(0, SeekOrigin.Begin);
            for (long pos = 0; pos < 0x1000; pos += 4)
            {
                fs.Seek(pos, SeekOrigin.Begin);
                uint possibleTag = reader.ReadUInt32BE();
                bool isAscii = Enumerable.Range(0, 4).All(i =>
                {
                    byte ch = (byte)(possibleTag >> (8 * (3 - i)));
                    return ch >= 32 && ch <= 126;
                });

                if (isAscii)
                {
                    int id = reader.ReadInt32();
                    int offset = reader.ReadInt32();

                    if (offset > 0 && offset < fs.Length)
                    {
                        LogHelper.DebugLog(2, DebugChannel.Loading, $"Fallback detected ASCII tag at 0x{pos:X}");
                        return (int)pos;
                    }
                }
            }

            LogHelper.DebugWarning("Failed to locate resource directory offset. Defaulting to 0.");
            return 0;
        }
        public void LoadInto(Archive archive, Stream stream, BinaryReader reader, string context = "stream")
        {
            // Reset position
            stream.Seek(0, SeekOrigin.Begin);
            uint header = reader.ReadUInt32BE();
            if (header == ResourceTags.MV93)
            {
                LogHelper.DebugLog(1, DebugChannel.Loading, "Detected MV93-format Director file (Director 9+). Using MV93 loader.");
                LoadMv93Format(archive, stream, reader);
                return;
            }

            // ✅ Only do this for classic formats (non-MV93)
            stream.Seek(0, SeekOrigin.Begin);

            int dirOffset = FindEndHeader(context, stream, reader);
            stream.Seek(dirOffset, SeekOrigin.Begin);

            var directory = new List<(uint tag, int id, int offset)>();

            for (int i = 0; i < 256 && stream.Position + 12 <= stream.Length; i++)
            {
                long entryPos = stream.Position;
                uint tag = reader.ReadUInt32BE();
                int id = reader.ReadInt32();
                int offset = reader.ReadInt32();

                if (tag == 0 && id == 0 && offset == 0)
                {
                    LogHelper.DebugLog(2, DebugChannel.Loading, $"[{context}] End of resource directory at 0x{entryPos:X}");
                    break;
                }

                if (offset <= 0 || offset >= stream.Length)
                {
                    LogHelper.DebugWarning($"[{context}] Skipping invalid resource: {ResourceTags.FromTag(tag)}#{id} offset={offset}");
                    break;
                }

                directory.Add((tag, id, offset));
            }

            var sorted = directory.OrderBy(d => d.offset).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var (tag, id, offset) = sorted[i];
                int nextOffset = (i + 1 < sorted.Count) ? sorted[i + 1].offset : (int)stream.Length;
                int size = nextOffset - offset;

                if (size <= 0 || offset + size > stream.Length)
                {
                    LogHelper.DebugWarning($"[{context}] Invalid resource size: {ResourceTags.FromTag(tag)}#{id} size={size}");
                    continue;
                }

                archive.RegisterResource(tag, id, offset, size, () =>
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                    byte[] data = reader.ReadBytes(size);
                    return new SeekableReadStreamEndian(new MemoryStream(data, writable: false), isBigEndian: true);
                });

                LogHelper.DebugLog(2, DebugChannel.Loading,
                    $"[{context}] Registered CST resource {ResourceTags.FromTag(tag)}#{id} at offset=0x{offset:X}, size={size}");
            }
        }
        private void LoadMv93Format(Archive archive, Stream fs, BinaryReader reader)
        {
            fs.Seek(0, SeekOrigin.Begin);
            string signature = Encoding.ASCII.GetString(reader.ReadBytes(4)); // 'MV93'
            uint movieSize = reader.ReadUInt32(); // total size of movie
            ushort version = reader.ReadUInt16(); // Director version
            ushort flags = reader.ReadUInt16();   // maybe compression/encryption flags

            LogHelper.DebugLog(1, DebugChannel.Loading, $"MV93 Header: sig={signature}, size={movieSize}, version={version}, flags={flags}");

            // Read block offsets
            uint block1Offset = reader.ReadUInt32();
            uint block2Offset = reader.ReadUInt32();
            uint block3Offset = reader.ReadUInt32();

            LogHelper.DebugLog(2, DebugChannel.Loading,
                $"MV93 Block offsets: 1=0x{block1Offset:X}, 2=0x{block2Offset:X}, 3=0x{block3Offset:X}");

            // Register entire MV93 resource under tag MV93#0
            fs.Seek(0, SeekOrigin.Begin);
            byte[] fullData = reader.ReadBytes((int)Math.Min(movieSize, fs.Length));
            archive.RegisterResource(ResourceTags.MV93, 0, 0, fullData.Length, () =>
                new SeekableReadStreamEndian(new MemoryStream(fullData, writable: false), isBigEndian: true));

            LogHelper.DebugLog(2, DebugChannel.Loading, $"Registered MV93 resource @0x0, size={fullData.Length}");

            // NOTE: Actual parsing of blocks inside MV93 must be implemented later.
            LogHelper.DebugWarning("MV93 parsing not yet implemented — only registered whole block.");
        }

        public void ParseXfir(Stream stream, Archive archive)
        {
            var reader = new BinaryReader(stream);
            uint magic = reader.ReadUInt32(); // Should be 'XFIR'
            uint totalSize = reader.ReadUInt32(); // Total file size minus 8 bytes

            var tagIdCounter = new Dictionary<uint, int>();

            while (stream.Position + 8 <= stream.Length)
            {
                long pos = stream.Position;
                uint tagBE = reader.ReadUInt32BE();
                uint tag = BinaryPrimitives.ReverseEndianness(tagBE);
                uint size = reader.ReadUInt32();

                if (size == 0)
                {
                    LogHelper.DebugWarning($"Chunk at 0x{pos:X} has size 0 — skipping");
                    continue;
                }

                if (stream.Position + size > stream.Length)
                {
                    LogHelper.DebugWarning($"Chunk at 0x{pos:X} claims invalid size {size} — skipping");
                    break;
                }

                byte[] data = reader.ReadBytes((int)size);

                int id = tagIdCounter.TryGetValue(tag, out var count) ? count : 0;
                tagIdCounter[tag] = id + 1;

                archive.AddResource(tag, id, data);
                LogHelper.DebugLog(2, DebugChannel.Loading, $"Read XFIR chunk {ResourceTags.FromTag(tag)}#{id} size={size}");
            }
        }

        private class ChunkInfo
        {
            public int Id { get; set; }
            public uint Tag { get; set; }
            public int Offset { get; set; }
            public int Size { get; set; }
            public int UncompressedSize { get; set; }
            public int CompressionIndex { get; set; }
        }

        private static byte[] DecompressZlib(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var z = new System.IO.Compression.ZLibStream(input, System.IO.Compression.CompressionMode.Decompress);
            using var ms = new MemoryStream();
            z.CopyTo(ms);
            return ms.ToArray();
        }

        private void LoadAfterburnerFormat(Archive archive, Stream fs, BinaryReader reader, bool littleEndian)
        {
            Func<uint> read32 = littleEndian ? reader.ReadUInt32 : reader.ReadUInt32BE;

            // Fver block
            uint tag = read32();
            if (tag != ResourceTags.MKTAG('F','v','e','r'))
                return;

            uint fverLength = reader.ReadVarInt();
            long blockStart = fs.Position;
            uint fverVersion = reader.ReadVarInt();
            int directorVersion = 0;
            if (fverVersion >= 0x401)
            {
                reader.ReadVarInt(); // imapVersion
                directorVersion = (int)reader.ReadVarInt(); // directorVersion
            }
            if (fverVersion >= 0x501)
            {
                int len = reader.ReadByte();
                reader.ReadBytes(len); // version string
            }
            int humanVersion = VersionHelper.HumanVersion(directorVersion);
            fs.Seek(blockStart + fverLength, SeekOrigin.Begin);

            // Fcdr compression table
            tag = read32();
            if (tag != ResourceTags.MKTAG('F','c','d','r'))
                return;

            uint fcdrLength = reader.ReadVarInt();
            byte[] fcdrComp = reader.ReadBytes((int)fcdrLength);
            byte[] fcdrBuf = DecompressZlib(fcdrComp);
            var fcdrReader = new BinaryReader(new MemoryStream(fcdrBuf));
            ushort compTypeCount = littleEndian ? fcdrReader.ReadUInt16() : fcdrReader.ReadUInt16BE();
            var compressionIDs = new List<Guid>();
            for (int i = 0; i < compTypeCount; i++)
                compressionIDs.Add(fcdrReader.ReadGuidBE());
            var compressionDescs = new List<string>();
            for (int i = 0; i < compTypeCount; i++)
                compressionDescs.Add(fcdrReader.ReadCString());

            // ABMP map
            tag = read32();
            if (tag != ResourceTags.MKTAG('A','B','M','P'))
                return;

            uint abmpLength = reader.ReadVarInt();
            long abmpEnd = fs.Position + abmpLength;
            reader.ReadVarInt(); // compression type
            reader.ReadVarInt(); // uncompressed length
            byte[] abmpComp = reader.ReadBytes((int)(abmpEnd - fs.Position));
            byte[] abmpBuf = DecompressZlib(abmpComp);
            var abmpReader = new BinaryReader(new MemoryStream(abmpBuf));

            abmpReader.ReadVarInt(); // unk1
            abmpReader.ReadVarInt(); // unk2
            uint resCount = abmpReader.ReadVarInt();

            var infos = new Dictionary<int, ChunkInfo>();
            for (int i = 0; i < resCount; i++)
            {
                int resId = (int)abmpReader.ReadVarInt();
                int offset = (int)abmpReader.ReadVarInt();
                uint compSize = abmpReader.ReadVarInt();
                uint uncompSize = abmpReader.ReadVarInt();
                int compType = (int)abmpReader.ReadVarInt();
                uint resTag = littleEndian ? abmpReader.ReadUInt32() : abmpReader.ReadUInt32BE();

                infos[resId] = new ChunkInfo
                {
                    Id = resId,
                    Tag = resTag,
                    Offset = offset,
                    Size = (int)compSize,
                    UncompressedSize = (int)uncompSize,
                    CompressionIndex = compType
                };

                archive.RegisterResource(resTag, resId, offset, (int)compSize, () =>
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    byte[] raw = reader.ReadBytes((int)compSize);
                    if (compType < compressionIDs.Count)
                    {
                        var guid = compressionIDs[compType];
                        if (guid == CompressionGuids.FontMap)
                        {
                            string text = FontMapDefaults.GetFontMapText(humanVersion);
                            raw = Encoding.ASCII.GetBytes(text);
                        }
                        else if (compressionDescs[compType].Contains("zlib", StringComparison.OrdinalIgnoreCase))
                        {
                            raw = DecompressZlib(raw);
                        }
                    }
                    return new SeekableReadStreamEndian(new MemoryStream(raw, false), isBigEndian: true);
                });
            }

            // Initial load segment
            if (!infos.TryGetValue(2, out var ilsInfo))
                return;

            tag = read32();
            if (tag != ResourceTags.MKTAG('F','G','E','I'))
                return;

            reader.ReadVarInt(); // unk
            byte[] ilsComp = reader.ReadBytes(ilsInfo.Size);
            byte[] ilsBuf = DecompressZlib(ilsComp);
            var ilsReader = new BinaryReader(new MemoryStream(ilsBuf));
            while (ilsReader.BaseStream.Position < ilsReader.BaseStream.Length)
            {
                int resId = (int)ilsReader.ReadVarInt();
                if (!infos.TryGetValue(resId, out var info))
                    break;
                byte[] chunk = ilsReader.ReadBytes(info.Size);
                archive.AddResource(info.Tag, resId, chunk);
            }
        }


        private static FileIMapData ReadIMap(SeekableReadStreamEndian mmapStream)
        {
            // 01-00-00-00-98-CE-00-00-44-07-00-00-00-00-00-00-00-00-00-00-00-00-00-00
            var reader = new BinaryReader(mmapStream.BaseStream);
            var result = new FileIMapData();
            result.MemoryMapCount = reader.ReadInt32();
            result.MemoryMapOffset = reader.ReadInt32();
            result.MemoryMapFileVersion = reader.ReadInt32();
            result.Reserved = reader.ReadInt16();
            result.Unknown = reader.ReadInt16();
            result.Reserved2 = reader.ReadInt32();
            return result;
        }
        public static FileMMapData ReadMMap(SeekableReadStreamEndian? mmapStream)
        {
            if (mmapStream == null) return new FileMMapData();
            var mmap = new FileMMapData();
            var reader = new BinaryReader(mmapStream.BaseStream);
            //var entryCount = reader.ReadUInt16();
            mmap.PropertySize = reader.ReadInt16();
            mmap.ResourceSize = reader.ReadInt16();
            mmap.MaxResourceCount = reader.ReadUInt32();
            mmap.UsedResourceCount = reader.ReadUInt32();
            mmap.FirstJunkResourceId = reader.ReadInt32();
            mmap.OldMapResourceId = reader.ReadInt32();
            mmap.FirstFreeResourceId = reader.ReadInt32();

            for (int i = 0; i < mmap.UsedResourceCount; i++)
            {
                string tagStr = reader.ReadChunkID();
                var size = reader.ReadInt32();
                var offset = reader.ReadInt32();
                var flags = reader.ReadUInt16();
                var unused = reader.ReadInt16();// skip unused
                var nextResourceId = reader.ReadInt32();
                var resourceId = nextResourceId;
                var entry = new FileMMapData.MMapEntry
                {
                    Tag = tagStr,
                    Size = (uint)size,
                    Offset = (uint)offset,
                    Unused = unused,
                    ResourceId = resourceId,
                };
                LogHelper.DebugLog(2, DebugChannel.Loading, $"{i}) MMapEntry: {tagStr} :resourceId={resourceId}:Offset={offset}:Size={size}");

                mmap.Entries.Add(entry);
            }

            return mmap;
        }
        private static FileKeyStarData ReadKeyStar(SeekableReadStreamEndian mmapStream)
        {
            // 01-00-00-00-98-CE-00-00-44-07-00-00-00-00-00-00-00-00-00-00-00-00-00-00
            var reader = new BinaryReader(mmapStream.BaseStream);
            var result = new FileKeyStarData();
            result.PropertiesSize = reader.ReadInt16();
            result.KeySize = reader.ReadInt16();
            result.MaxKeyCount = reader.ReadInt32();
            result.UsedKeyCount = reader.ReadInt32();
            for (int i = 0; i < result.UsedKeyCount; i++)
            {
                var keyy = new FileKeyStarData.KeyEntryData
                {
                    OwnedResourceID = reader.ReadInt32(),
                    OwnerResourceID = reader.ReadInt32(),
                    OwnedChunkID = reader.ReadChunkID()
                };
                result.Keys.Add(keyy);
            }
            return result;
        }
        private static FileDRCFData ReadDRCF(SeekableReadStreamEndian? stream)
        {
            if (stream == null) return new FileDRCFData();
            var reader = new BinaryReader(stream.BaseStream);
            var result = new FileDRCFData();
            result.Size = reader.ReadMotorolaInt16();
            result.FileVersion = reader.ReadMotorolaUInt16(); // 0x163C if protected
            result.SourceRect = reader.ReadRect();
            var minMember = reader.ReadMotorolaInt16(); // obsolete, see Cast Properties
            var maxMember = reader.ReadMotorolaInt16(); // obsolete, see Cast Properties
            result.Tempo = reader.ReadInt8();

            result.BgColor = reader.ReadColor(result.FileVersion);
            reader.ReadMotorolaInt16();
            reader.ReadMotorolaInt16();
            // these unknown values found via reverse analysis
            var unknown = reader.ReadMotorolaInt16();
            if (result.FileVersion <= 0x4C6)
            {
                unknown = 0;
            }
            var alphaBgColor = reader.ReadByte();
            if (result.FileVersion <= 0x4C7)
                alphaBgColor = 255;
            result.BgColor.Insert(0, alphaBgColor);
            reader.ReadInt8();
            reader.ReadMotorolaInt16();
            var unknown2 = reader.ReadInt8();
            if (result.FileVersion <= 0x551)
            {
                unknown2 = -2;
            }
            reader.ReadInt8();
            reader.ReadMotorolaInt32();
            result.MovieFileVersion = reader.ReadMotorolaInt16();
            reader.ReadMotorolaInt16();
            var unknown3 = reader.ReadMotorolaInt32();
            if (result.FileVersion <= 0x6A3)
            {
                unknown3 = 0;
            }
            var unknown4 = reader.ReadMotorolaInt32();
            if (result.FileVersion <= 0x73A)
            {
                unknown4 = 1;
            }
            var unknown5 = reader.ReadMotorolaInt32();
            if (result.FileVersion <= 0x4C7)
            {
                //var unknown5 = 0;
            }
            var trial = reader.ReadInt8();
            if (result.FileVersion <= 0x742)
            {
                // unknown6 = 0;
            }
            var unknown7 = reader.ReadInt8();
            if (result.FileVersion <= 0x4C7)
            {
                unknown7 = 80;
            }
            reader.ReadMotorolaInt16();
            reader.ReadMotorolaInt16();
            result.ProtectedMovie = false;
            short random = reader.ReadMotorolaInt16();

            // If file version > 0x459 and random is divisible by 0x17 (23 decimal), it's protected
            if (result.FileVersion > 0x0459 && random % 0x17 == 0)
            {
                result.ProtectedMovie = true;
            }
            reader.ReadMotorolaInt32();
            reader.ReadMotorolaInt32();
            result.OldDefaultPalette = reader.ReadMotorolaInt16();
            reader.ReadMotorolaInt16();
            var unknown8 = reader.ReadMotorolaInt32();
            if (result.FileVersion <= 0x4C0)
            {
                unknown8 = 1024;
            }
            // in Director this is read as two vars?
            result.DefaultPalette = reader.ReadMotorolaInt32();
            if (result.FileVersion <= 0x578)
            {
                var unknown11 = reader.ReadMotorolaInt16();
                if (unknown11 == 1)
                {
                    var unknown9 = 0;
                    var unknown10 = 0;
                }
                else
                {
                    if (unknown11 == 2)
                    {
                        var unknown9 = 0;
                        var unknown10 = 1;
                    }
                    else
                    {
                        var unknown9 = 1;
                        var unknown10 = 0;
                    }
                }
            }
            else
            {
                var unknown9 = reader.ReadMotorolaInt8();
                var unknown10 = reader.ReadMotorolaInt8();
            }
            reader.ReadMotorolaInt16();
            if (reader.BaseStream.Position == reader.BaseStream.Length) return result;
            if (result.FileVersion >= 0x73A)
            {
                result.DownloadFramesBeforePlaying = reader.ReadMotorolaInt32();
            }
            else
            {
                result.DownloadFramesBeforePlaying = 90;
                reader.ReadMotorolaInt16();
                reader.ReadMotorolaInt16();
                reader.ReadMotorolaInt16();
                reader.ReadMotorolaInt16();
                reader.ReadMotorolaInt16();
                reader.ReadMotorolaInt16();
            }
            if (result.FileVersion <= 0x45D)
            {
                result.DefaultPalette = result.OldDefaultPalette;
            }

            return result;
        }

        private static FileVWSCData ReadVWSC(SeekableReadStreamEndian? streamm)
        {
            if (streamm == null) return new FileVWSCData { };
            //This is what is saved to the file, and needs to be decompressed.
            var reader = new BinaryReader(streamm.BaseStream);
            var result = new FileVWSCData();
            result.MemHandleSize = reader.ReadMotorolaInt32(); // this is interpreted as the beginning of a memHandle, the size of it
            result.HeaderType = reader.ReadMotorolaInt32(); // always -3?
            result.SpritePropertiesOffsetsCountOffset = reader.ReadMotorolaInt32(); // offset of spritePropertiesOffsetsCount from beginning of chunk, must be twelve
            result.SpritePropertiesOffsetsCount = reader.ReadMotorolaInt32();
            int notationBase = reader.ReadInt32();
            result.NotationOffset = notationBase * 4 + 12 + result.SpritePropertiesOffsetsCountOffset;
            result.NotationAndSpritePropertiesSize = reader.ReadMotorolaInt32();
            for (int i = 0; i < result.SpritePropertiesOffsetsCountOffset; i++)
                result.SpritePropertiesOffsets.Add(reader.ReadInt16());
            // the first one is unused
            // notation
            result.FramesEndOffset = reader.ReadMotorolaInt32();
            reader.ReadBytes(4);
            result.FramesSize = reader.ReadInt32();
            result.FramesType = reader.ReadInt16();
            result.ChannelSize = reader.ReadInt16();
            result.LastChannelMax = reader.ReadInt16();
            result.LastChannel = reader.ReadInt16();
            // Read frame definitions
            for (int i = 0; i < result.FramesSize; i++)
            {
                var frame = new FrameData();
                result.Frames.Add(frame);
                frame.FrameEnd = reader.ReadInt16();
                for (int f = 0; f < frame.FrameEnd; f++)
                {
                    var channel1 = new ChannelData();

                    channel1.Size = reader.ReadInt16();
                    channel1.Offset = reader.ReadInt16();
                    channel1.Buffer = reader.ReadInt16();
                    frame.Channels.Add(channel1);   
                }
                var spritePropertiesOffsetElementCount = reader.ReadMotorolaInt32();
                for (int s = 0; s < spritePropertiesOffsetElementCount; s++)
                    frame.PropertiesOffsets.Add(reader.ReadInt32());
            }

            // Read frame interval descriptors
            // each descriptor is 0x2C bytes long and contains the start and end frame
            // and channel information for a particular sprite
            for (int i = 0; i < result.FramesSize; i++)
            {
                var frame = result.Frames[i];
                frame.StartFrame = reader.ReadMotorolaInt32();
                frame.EndFrame = reader.ReadMotorolaInt32();
                reader.ReadBytes(4);
                reader.ReadBytes(4);

                frame.ChannelNum = reader.ReadMotorolaInt32(); // these start at 5 because of legacy reasons since Director 5 had five reserved tracks for specific purposes
                frame.Number = reader.ReadMotorolaInt32();
                reader.ReadBytes(28);
            }

            // Read channel default properties
            for (int i = 0; i < result.LastChannelMax; i++)
            {
                var channel = new ChannelData();
                var sprite = new SpriteData();
                byte flags = reader.ReadByte();
                channel.MultipleMembers = ((flags & 0x10) >> 4) == 0;
                byte temp = reader.ReadByte();
                channel.InkFlag = (temp & 0x80) >> 7;
                sprite.Ink = temp & 0x7F;
                sprite.ForeColor = reader.ReadByte();
                sprite.BackColor = reader.ReadByte();
                sprite.DisplayMember = reader.ReadMotorolaUInt32();
                reader.ReadBytes(2);
                channel.SpritePropertiesOffset = reader.ReadMotorolaUInt16();
                sprite.LocV = reader.ReadMotorolaUInt16();
                sprite.LocH = reader.ReadMotorolaUInt16();
                sprite.Height = reader.ReadMotorolaUInt16();
                sprite.Width = reader.ReadMotorolaUInt16();
                byte flags2 = reader.ReadByte();
                sprite.Editable = (flags2 & 0x40) >> 6;
                sprite.ScoreColor = flags2 & 0x0F;
                sprite.Blend = reader.ReadByte();
                byte flags3 = reader.ReadByte();
                sprite.FlipV = (flags3 & 0x04) >> 2;
                sprite.FlipH = (flags3 & 0x02) >> 1;
                reader.ReadBytes(5);
                sprite.Rotation = reader.ReadFloat32BE();
                sprite.Skew = reader.ReadFloat32BE();
                reader.ReadBytes(12);
                channel.Sprite = sprite;
                result.ChannelInfo.Add(channel);
            }

            // Link sprite data to channel via same index in Frames if needed
            for (int f = 0; f < result.Frames.Count; f++)
            {
                var frame = result.Frames[f];
                for (int i = 0; i < frame.Channels.Count && i < result.ChannelInfo.Count; i++)
                {
                    var src = result.ChannelInfo[i];
                    var dest = frame.Channels[i];
                    dest.Sprite = src.Sprite;
                    dest.InkFlag = src.InkFlag;
                    dest.MultipleMembers = src.MultipleMembers;
                    dest.SpritePropertiesOffset = src.SpritePropertiesOffset;
                }
            }



            return result;
        }
        private static FileVWLBData ReadVWLB(SeekableReadStreamEndian? streamm)
        {
            if (streamm == null) return new FileVWLBData { };
            // 01-00-00-00-98-CE-00-00-44-07-00-00-00-00-00-00-00-00-00-00-00-00-00-00
            var reader = new BinaryReader(streamm.BaseStream);
            var result = new FileVWLBData();
            var labelsChunkBuffer = reader.ReadInt16();
            var labelFrames = reader.ReadFArray(labelsChunkBuffer, 4);
            var labelListLength = reader.ReadInt32();
            var labelsOneString = reader.ReadPascalString(labelListLength);
            var labels = labelsOneString.Split('\r', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < labelFrames.Length; i++)
            {
                result.Labels.Add(new FileVWLBData.LabelTimeLine { Frame = labelFrames[i], Label = labels[i] });
            }
            return result;
        }

        private static FileCAStData ReadCASt(SeekableReadStreamEndian? stream)
        {
            var result = new FileCAStData();
            if (stream == null)
                return result;

            var reader = new BinaryReader(stream.BaseStream);
            ushort fieldCount = reader.ReadUInt16BE();
            ushort maxCastId = reader.ReadUInt16BE();
            ushort itemCount = reader.ReadUInt16BE();
            ushort fieldSize = reader.ReadUInt16BE();
            ushort _unused = reader.ReadUInt16BE();

            for (int i = 0; i < itemCount; i++)
            {
                ushort castId = reader.ReadUInt16BE();
                int offset = reader.ReadInt32BE();
                int length = reader.ReadInt32BE();
                byte flags = reader.ReadByte();
                reader.ReadByte(); // padding
                ushort type = reader.ReadUInt16BE();

                long save = stream.Position;
                if (offset > 0 && length > 0 && offset + length <= stream.Length)
                {
                    stream.Position = offset;
                    byte[] data = reader.ReadBytes(length);
                    result.MembersData.Add(new FileCAStData.MemberData
                    {
                        Id = castId,
                        Type = type,
                        Data = data
                    });
                }
                stream.Position = save;
            }

            return result;
        }

    }
}
