using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Reads <c>CAS*</c> resources and exposes the cast-member tables they contain. Each table stores a
/// packed list of 32-bit identifiers that point to individual <c>CASt</c> resources. The loader keeps
/// the slot index for every populated entry so higher layers can reconstruct cast numbering.
/// </summary>
internal sealed class BlLegacyCastReader
{
    private readonly ReaderContext _context;


    public BlLegacyCastReader(ReaderContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Iterates over all registered <c>CAS*</c> resources, loading their payload bytes and decoding the
    /// cast-member tables contained within. The reader respects both classic chunk storage and
    /// Afterburner inline segments, inflating compressed data when necessary.
    /// </summary>
    public IReadOnlyList<BlLegacyCastLibrary> Read()
    {
        var castVisibleInfo = new List<CinfData>();
        List<LsCmEntry> castNameInfos = new List<LsCmEntry>();
        var libraries = new List<BlLegacyCastLibrary>();
        var parsingLibraries = new List<BlLegacyCastLibrary>();
        if (_context.Resources.Entries.Count == 0)
            return libraries;

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
        }

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != BlTag.CasStar && entry.Tag != BlTag.Cinf && entry.Tag != BlTag.MCsL)
                continue;

            // Always the first of an internal cast library
            if (entry.Tag == BlTag.CasStar)
            {
                var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
                var library = ParseLibrary(entry, payload, classicLoader, afterburnerLoader);
                parsingLibraries.Add(library);
                continue;
            }
            // Follows immediately after a Cas* entry
            if (entry.Tag == BlTag.Cinf)
            {
                var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
                var visibleInfo = ReadCastInfoCinf(payload);
                castVisibleInfo.Add(visibleInfo);
                var lastLib = parsingLibraries.Last();
                lastLib.CastPath = visibleInfo.CastPath;
                lastLib.RowWidth = visibleInfo.RowWidth;
                lastLib.VisibleColumnsFlags = visibleInfo.VisibleColumnsFlags;
                lastLib.NumberOfVisibleMembers = visibleInfo.NumberOfVisibleMembers;
                lastLib.ShowAsThumbList = visibleInfo.ShowAsThumbList;
                continue;
            }
            // Is always the last entry
            if (entry.Tag == BlTag.MCsL)
            {
                var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
                castNameInfos = ReadCastsMCsL(payload);
                var index = 1;
                foreach (var info in castNameInfos)
                {
                    if (info.IsInternal)
                    {
                        var lib = parsingLibraries[info.InternalNumber - 1];
                        if (lib is not null)
                        {
                            lib.Name = info.Name;
                            lib.CastPath = info.Path;
                            lib.Preload = info.Preload;
                            lib.IsInternal = true;
                            libraries.Add(lib);
                        }
                    }
                    else
                    {
                        var lib = new BlLegacyCastLibrary(-1, index, 0)
                        {
                            Name = info.Name,
                            CastPath = info.Path,
                            Preload = info.Preload,
                            IsInternal = false,
                        };
                        libraries.Add(lib);
                    }
                    index++;
                }
                continue;
            }
        }
        // When we read a cast file .cst , there is no MCsL, so we need to return the parsing library
        if (libraries.Count == 0 && parsingLibraries.Count > 0)
        {
            parsingLibraries[0].Name = _context.FileName != null ? System.IO.Path.GetFileNameWithoutExtension(_context.FileName) : "Unnamed Cast";
            return parsingLibraries;
        }
        return libraries;
    }

    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private BlLegacyCastLibrary ParseLibrary(BlLegacyResourceEntry entry, byte[] payload, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        int? parentId = null;
        if (_context.Resources.ParentByChild.TryGetValue(entry.Id, out var link))
            parentId = link.ParentId;

        var entryCount = payload.Length / 4;
        var library = new BlLegacyCastLibrary(entry.Id, parentId, entryCount);
        if (entryCount == 0)
        {
            return library;
        }

        var reader = _context.CreateMemoryReader(payload, BlEndianness.BigEndian);
        for (var slot = 0; slot < entryCount; slot++)
        {
            var castResourceId = unchecked((int)reader.ReadUInt32());
            if (castResourceId == 0)
                continue;

            library.MemberSlots.Add(CreateMember(slot, castResourceId, classicLoader, afterburnerLoader));
        }

        return library;
    }

    private BlLegacyCastMemberSlot CreateMember(int slot, int resourceId, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        BlCastMemberItem memberInfo = new();
        if (_context.Resources.TryGetEntry(resourceId, out var memberEntry))
        {
            var memberPayload = LoadPayload(memberEntry, classicLoader, afterburnerLoader);
            if (memberPayload.Length > 0 && TryParseMemberChunk(memberPayload, out var memberInfo1))
                memberInfo = memberInfo1!;
        }

        return new BlLegacyCastMemberSlot(slot, resourceId, memberInfo);
    }

    private static bool TryParseMemberChunk(byte[] payload, out BlCastMemberItem? castMemberInfo)
    {
        var memberData = new BlLegacyCastItemReader().ReadItem(string.Empty, payload);
        castMemberInfo = memberData.MemberItem;

        return true;
    }

    #region Cinf: Read internal Cast Visible data 

    private class CinfData
    {
        public string? CastPath { get; set; }
        public string? RowWidth { get; set; }
        public ushort VisibleColumnsFlags { get; internal set; }
        public int NumberOfVisibleMembers { get; internal set; }
        public bool ShowAsThumbList { get; internal set; }
    }
    private CinfData ReadCastInfoCinf(byte[] payload)
    {
        var returnData = new CinfData();
        // Cinf
        var reader = new BlStreamReader(new MemoryStream(payload));
        var hexString = reader.ReadBytesAsHexString(payload.Length);
        reader.BaseStream.Dispose();
        reader = new BlStreamReader(new MemoryStream(payload));
        

        var something = reader.ReadUInt32();    // 00 00 00 04 
        var something2 = reader.ReadUInt16();   // 00 05 
        var someInts = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            // 00 00 00 00
            // 00 00 00 00
            // 00 00 00 12
            // 00 00 00 1A
            // 00 00 00 65
            // 00 00 00 67
            someInts.Add(reader.ReadInt32());
        }
        var something9 = reader.ReadUInt16();       // 00 01 
        var something10 = reader.ReadUInt16();      // 00 01 
        var rowWidthType = reader.ReadUInt16();     // 00 03                // Row width
        returnData.RowWidth = "8 Thumbnails";
        switch (rowWidthType)
        {
            case 00: returnData.RowWidth = "8 Thumbnails"; break;
            case 01: returnData.RowWidth = "10 Thumbnails"; break;
            case 02: returnData.RowWidth = "20 Thumbnails"; break;
            case 03: returnData.RowWidth = "Fit to window"; break;
        }
        var numberOfVisibleMembersType = reader.ReadUInt16();  // 00 01     // Number of visible members
        returnData.NumberOfVisibleMembers = 1000;
        switch (numberOfVisibleMembersType)
        {
            case 00: returnData.NumberOfVisibleMembers = 512; break;
            case 01: returnData.NumberOfVisibleMembers = 1000; break;
            case 02: returnData.NumberOfVisibleMembers = 2000; break;
            case 03: returnData.NumberOfVisibleMembers = 5000; break;
            case 04: returnData.NumberOfVisibleMembers = 10000; break;
            case 05: returnData.NumberOfVisibleMembers = 32000; break;
        }
        var something13 = reader.ReadUInt32();                  // 00 00 00 01 
        returnData.ShowAsThumbList = reader.ReadUInt16() > 0;   // 00 00
        var something14 = reader.ReadUInt16();                  // 00 00 
        returnData.VisibleColumnsFlags = reader.ReadUInt16();   // 04 9F // visible Columns as Flags
        // To find flags :
        // Number, Created, Modified, Modified Date, Script, Modified By, Type, Filename, Size, Comments
        // 00 03 = Number
        // 00 11 = Type
        // 00 45 = Modified + Created
        // 04 9F = 
        // 00 1B = Number Scripts and Types
        // 07 FF = All columns visible
        // TODO: Parse flags
        var something16 = reader.ReadUInt32();  // 00 00 00 00 
        var something17 = reader.ReadUInt16();  // 01 1D
        var something18 = reader.ReadUInt16();  // 01 D1 
        var stringLength = reader.ReadByte();   // 0x49 (73 bytes)
        if (stringLength > 0)
            returnData.CastPath = reader.ReadAsciiString(stringLength);
        reader.BaseStream.Dispose();
        return returnData;
    }
    /*
    Cast 1 :
    00 00 00 04   00 05  
    00 00 00 00   00 00 00 00   
    00 00 00 12 
    00 00 00 1A 
    00 00 00 1C 
    00 00 00 1E 
    00 01   00 01   00 03   00 01   00 00 00 01   00 00 00 00 
    04 9F
    00 00 00 00   
    01 32                   // Different
    01 D1   00 00 00 00

    Cast 2 :
    00 00 00 04   00 05 
    00 00 00 00   00 00 00 00 
    00 00 00 12 
    00 00 00 1A 
    00 00 00 1C 
    00 00 00 1E 
    00 01   00 01   00 03   00 01   00 00 00 01   00 00 00 00 
    04 9F
    00 00 00 00   
    01 1D                   // Different
    01 D1   00 00 00 00
    */

    #endregion


    #region MCsL: Read All Casts data 


    public sealed class LsCmEntry
    {
        public string Name { get; set; } = "";
        public string? Path { get; set; }
        public bool IsInternal => InternalNumber > 0;
        public int InternalNumber { get; set; }
        public int Data1 { get; set; }
        public int Data2 { get; set; }
        public int Data3 { get; set; }
        public BlLegacyCastLibrary.CastPreload Preload { get; internal set; }
    }

    private List<LsCmEntry> ReadCastsMCsL(byte[] payload)
    {
        var results = new List<LsCmEntry>();
        var reader = new BlStreamReader(new MemoryStream(payload));
        var hexString = reader.ReadBytesAsHexString(payload.Length);
        reader.BaseStream.Dispose();
        reader = new BlStreamReader(new MemoryStream(payload));
        var headerLen = reader.ReadInt32();            // 00 00 00 0C 
        var numberOfCasts = reader.ReadInt32();        // 00 00 00 01
        var castDataItemLength = reader.ReadUInt16();   // 00 04 
        var offsetCount = reader.ReadInt32();          // 05 or 0D or 11   (Number of casts * 4) + 1
        //var something5 = reader.ReadUInt32();           // always 0 it seems
        //var something6 = reader.ReadUInt32();           // always 0 it seems
        var offs = new int[offsetCount+1];
        for (int i = 0; i < offs.Length; i++) offs[i] = reader.ReadInt32();
        var datas = new List<byte[]>();
        var startData = (int)reader.Position; // + 4; // 18 + (offsetCount * 4);
        var end = 0;
        for (int i = 0; i < offs.Length - 1; i++)
        {
            int start = startData + offs[i];
            end = startData + offs[i + 1];
            var data = payload[start..end];
            datas.Add(data);
        }
        var list = new List<LsCmEntry>(numberOfCasts);

        for (int i = 0; i < numberOfCasts; i++)
        {
            var offset = i * 4 + 1;
            var entry = new LsCmEntry();
            entry.Name = datas[offset].ReadStringWithFirstByteLength();
            entry.Path = datas[offset + 1].ReadStringWithFirstByteLength();
            entry.Preload = (BlLegacyCastLibrary.CastPreload)datas[offset + 2].ReadInt16(0);
            var castData2 = datas[offset + 3];
            entry.Data1 = castData2.ReadInt16(0);
            entry.Data2 = castData2.ReadInt16(2);
            entry.InternalNumber = castData2.ReadInt16(4);
            entry.Data3 = castData2.ReadInt16(6);
            results.Add(entry);
        }
        reader.BaseStream.Dispose();
        return results;
        /*
        
        // With 1 cast:
        00 00 00 0C 
        00 00 00 01                         // Number of Casts
        00 04 
        00 00 00 05                         // (Number of casts * 4) + 1
        00 00 00 00   00 00 00 00 

        // For every cast, 4 offsets
        00 00 00 0A   00 00 00 0A 
        00 00 00 0C   00 00 00 14 

        08   49 6E 74 65 72 6E 61 6C 00
        00 00   // Preload mode
        00 01   
        00 01   
        00 01   // Cast number or 0 for external
        04 00


        // With 3 casts
        00 00 00 0C 
        00 00 00 03                          // number of Casts
        00 04 
        00 00 00 0D   00 00 00 00   00 00 00 00  
        // For every cast, 4 offsets
        00 00 00 0A   00 00 00 0A 
        00 00 00 0C   00 00 00 14 

        00 00 00 2D   00 00 00 2D 
        00 00 00 2F   00 00 00 37 

        00 00 00 49   00 00 00 A8 
        00 00 00 AA   00 00 00 B2           // offset index = 70

        08    49 6E 74 65 72 6E 61 6C    00                                                 // Cast lib name: Internal
        00 00   00 01   00 01   00 01   04 00  

        17   4D 79 20 53 65 63 6F 6E 64 20 49 6E 74 65 72 6E 61 6C 20 43 61 73 74   00      // Cast lib name: My Second Internal Cast
        00 00   00 01   00 00   00 02   04 00 

        10   4D 79 20 45 78 74 65 72 6E 61 6C 20 43 61 73 74    00                          // Cast lib name: My External Cast
        5E   44 3A 5C ....... 4D 79 20 45 78 74 65 72 6E 61 6C 20 43 61 73 74 2E 63 73 74   // File path
        00 00   00 01   00 00   00 00   04 00



        // with 4 casts
        00 00 00 0C 
        00 00 00 04                                                 // Number of casts
        00 04 
        00 00 00 11   00 00 00 00   00 00 00 00 
        // For every cast, 4 offsets
        00 00 00 0A   00 00 00 0A 
        00 00 00 0C   00 00 00 14 

        00 00 00 1F   00 00 00 1F 
        00 00 00 21   00 00 00 29 

        00 00 00 3B   00 00 00 9A 
        00 00 00 9C   00 00 00 A4 

        00 00 00 AF   00 00 00 AF 
        00 00 00 B1   00 00 00 B9 

        08   49 6E 74 65 72 6E 61 6C   00                           // Internal 
        00 00   00 01   00 04   00 01   04 00 
        
        09   49 6E 74 65 72 6E 61 6C 32   00                        // Internal
        00 00   00 01   00 01   00 02   04 00 

        10   4D 79 20 45 78 74 65 72 6E 61 6C 20 43 61 73 74   00   // External
        5E   44 3A 5C ...... 45 78 74 65 72 6E 61 6C 20 43 61 73 74 2E 63 73 74 
        00 00   00 01   00 01   00 00   04 00 

        09   49 6E 74 65 72 6E 61 6C 33   00                        // Internal
        00 00   00 01   00 01   00 03   04 00 
        00
            */
    } 
    #endregion


}
/// <summary>
/// Extension helpers that expose the <see cref="BlLegacyCastReader"/> through the shared
/// <see cref="ReaderContext"/> type used by the legacy pipeline.
/// </summary>
internal static class BlLegacyCastReaderExtensions
{
    public static IReadOnlyList<BlLegacyCastLibrary> ReadCastLibraries(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyCastReader(context);
        return reader.Read();
    }
}
