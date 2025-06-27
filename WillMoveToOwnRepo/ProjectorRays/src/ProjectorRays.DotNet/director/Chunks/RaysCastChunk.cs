using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class RaysCastChunk : RaysChunk
{
    public List<int> MemberIDs = new();
    public string Name = string.Empty;
    public Dictionary<ushort, RaysCastMemberChunk> Members = new();
    public RaysScriptContextChunk? Lctx;

    public RaysCastChunk(RaysDirectorFile? dir) : base(dir, ChunkType.CastChunk) { }

    public override void Read(ReadStream stream)
    {
        // The memory map entries follow the same byte order as the parent file.
        // Use the endianness of the owning DirectorFile instead of forcing
        // big endian. Movies created on Windows use little endian here.
        //stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        while (!stream.Eof)
        {
            //Dir.Logger.LogInformation($"Stream.Pos={stream.Pos}, Size={stream.Size}");
            //var view = stream.ReadByteView(4);
            //Dir.Logger.LogInformation("Raw bytes: " + BitConverter.ToString(view.Data, view.Offset, view.Size));
            //stream.Seek(stream.Pos - 4); // rewind
            //var _offset = 902;
            //var p = 0;
            //var test = BinaryPrimitives.ReadUInt32LittleEndian(stream.Data.AsSpan(_offset + p, 4));
            //var test2 = BinaryPrimitives.ReadUInt32BigEndian(stream.Data.AsSpan(_offset + p, 4));
            //Dir.Logger.LogInformation("CastChunk:memberId test var=" + test+ ":test2=" + test2);
            // BinaryPrimitives.ReadUInt32BigEndian(_data.AsSpan(_offset + p, 4));
            // memberId = 67108864
            var memberId = stream.ReadUint32();
            Dir.Logger.LogTrace("CastChunk:memberId=" + memberId);
            MemberIDs.Add((int)memberId);
        }
    }

    /// <summary>
    /// Serialize this cast chunk to JSON. The member list is written as an
    /// array of integers since <see cref="RaysJSONWriter"/> only handles
    /// primitives directly.
    /// </summary>
    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteKey("memberIDs");
        json.StartArray();
        foreach (var id in MemberIDs)
            json.WriteVal(id);
        json.EndArray();
        json.EndObject();
    }

    /// <summary>
    /// Populate this cast chunk by loading all referenced members and linking
    /// script resources. This mirrors the original C++ populate() method and is
    /// required so that script restoration knows which member owns which script.
    /// </summary>
    public void Populate(string castName, int id, ushort minMember)
    {
        Name = castName;
        foreach (var entry in Dir!.KeyTable!.Entries)
        {
            var fourCC = Common.RaysUtil.FourCCToString(entry.FourCC);
            Dir.Logger.LogInformation($"KEY* Entry: FourCC={fourCC}, ResourceID={entry.ResourceID}, SectionID={entry.SectionID}, CastID={entry.CastID}");
        }
        foreach (var key in Dir.KeyTable!.Entries.Where(k => k.FourCC == RaysDirectorFile.FOURCC('X', 'M', 'E', 'D')))
        {
            Dir.Logger.LogInformation($"XMED key: CastID={key.CastID}, ResourceID={key.ResourceID}, SectionID={key.SectionID}");
        }
        // Look up the script context for this cast using the key table.
        foreach (var entry in Dir!.KeyTable!.Entries)
        {
            if (entry.CastID == id &&
               (entry.FourCC == RaysDirectorFile.FOURCC('L', 'c', 't', 'x') || // Lctx' compiled script context, // Native readable
                entry.FourCC == RaysDirectorFile.FOURCC('L', 'c', 't', 'X')) &&
               Dir.ChunkExists(entry.FourCC, entry.SectionID))
            {
                Lctx = (RaysScriptContextChunk)Dir.GetChunk(entry.FourCC, entry.SectionID);
                break;
            }
        }
        Dir.Logger.LogInformation("MemberIDs: " + string.Join(", ", MemberIDs));
        // Load each member chunk referenced by this cast and assign an ID.
        for (int i = 0; i < MemberIDs.Count; i++)
        {
            int sectionID = MemberIDs[i];
            if (sectionID <= 0)
            {
                Dir.Logger.LogInformation($"Skipping invalid section ID {sectionID}");
                continue;
            }

            int chunkID = -1;
            foreach (var entry in Dir!.KeyTable!.Entries)
            {
                Dir.Logger.LogInformation($"Entry ResourceID={entry.ResourceID};SectionID={entry.SectionID};CastID={entry.CastID};FourCC={entry.FourCC}");

                if (entry.FourCC == RaysDirectorFile.FOURCC('C', 'A', 'S', 't') &&
                    entry.CastID == id &&
                    entry.ResourceID == sectionID)
                {
                    chunkID = entry.SectionID;
                    break;
                }
            }
            if (chunkID < 0)
            {
                Dir.Logger.LogInformation($"No KEY* match for ResourceID={sectionID}, trying direct chunk ID fallback");
                chunkID = sectionID;  // fallback: assume it's the same as the chunk ID
            }


            if (!Dir.ChunkExists(RaysDirectorFile.FOURCC('C', 'A', 'S', 't'), sectionID))
            {
                Dir.Logger.LogInformation($"Missing cast member chunk: {sectionID}");
                continue;
            }

            var member = (RaysCastMemberChunk)Dir.GetChunk(RaysDirectorFile.FOURCC('C', 'A', 'S', 't'), chunkID);
            member.Id = (ushort)(i + minMember);
            // Look for XMED chunk for styled text/field members
            if (member.Type == RaysMemberType.TextMember || member.Type == RaysMemberType.FieldMember)
            {
                Dir.Logger.LogInformation($"Checking for XMED: CastID={id}, ResourceID={sectionID}");

                var xmedKey = Dir.KeyTable!.Entries.FirstOrDefault(e =>
                    e.FourCC == RaysDirectorFile.FOURCC('X', 'M', 'E', 'D') && e.ResourceID == sectionID);

                ChunkInfo? xmedInfo = null;

                if (xmedKey != null && Dir.ChunkExists(xmedKey.FourCC, xmedKey.SectionID))
                {
                    xmedInfo = Dir.ChunkInfoMap.GetValueOrDefault(xmedKey.SectionID);
                    Dir.Logger.LogInformation($"XMED found via KEY* for ResourceID={sectionID}, SectionID={xmedKey.SectionID}");
                }
                else
                {
                    xmedInfo = Dir.ChunkInfoMap.Values.FirstOrDefault(c =>
                        c.FourCC == RaysDirectorFile.FOURCC('X', 'M', 'E', 'D') &&
                        Dir.ChunkExists(c.FourCC, c.Id));

                    if (xmedInfo != null)
                        Dir.Logger.LogInformation($"XMED fallback found: FourCC=XMED, SectionID={xmedInfo.Id}");
                }

                if (xmedInfo != null)
                {
                    var xmedChunk = (RaysXmedChunk)Dir.GetChunk(xmedInfo.FourCC, xmedInfo.Id);
                    var xmedText = RaysCastMemberTextRead.FromXmedChunk(xmedChunk.Data, Dir);
                    member.DecodedText = xmedText;
                }
            }

            Members[member.Id] = member;

            uint scriptId = member.GetScriptID();
            if (scriptId != 0 && Dir.ChunkExists(RaysDirectorFile.FOURCC('L', 's', 'c', 'r'), (int)scriptId))
            {
                var scriptChunk = (RaysScriptChunk)Dir.GetChunk(RaysDirectorFile.FOURCC('L', 's', 'c', 'r'), (int)scriptId);
                member.Script = scriptChunk;
                scriptChunk.Member = member;
            }
        }
    }
}
