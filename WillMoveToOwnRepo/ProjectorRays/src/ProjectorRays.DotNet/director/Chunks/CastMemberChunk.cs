using System.Text;
using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class CastMemberChunk : Chunk
{
    public MemberType Type;
    public uint InfoLen;
    public uint SpecificDataLen;
    public CastInfoChunk? Info;
    public BufferView SpecificData;
    public CastMember? Member;
    public bool HasFlags1;
    public byte Flags1;
    public ushort Id;
    public ScriptChunk? Script;

    public CastMemberTextRead DecodedText { get; internal set; }

    public CastMemberChunk(DirectorFile? dir) : base(dir, ChunkType.CastMemberChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        Dir.Logger.LogInformation($"Reading CastMemberChunk at stream.Pos={stream.Pos}");

        var raw = stream.ReadByteView(12);
        Dir.Logger.LogInformation("Raw CastMember header bytes: " + BitConverter.ToString(raw.Data, raw.Offset, raw.Size));

        stream.Seek(stream.Pos - 12); // rewind
        // Respect the movie's byte order for configuration data as well.
        //stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        Type = (MemberType)stream.ReadUint32();
        InfoLen = stream.ReadUint32();
        SpecificDataLen = stream.ReadUint32();
        Dir.Logger.LogTrace($"CastMember InfoLen={InfoLen}, SpecificDataLen={SpecificDataLen}, Stream.Pos={stream.Pos}, Stream.Size={stream.Size}");
        var infoView = stream.ReadByteView((int)InfoLen);
        Dir.Logger.LogTrace($"infoView.Size={infoView.Size}");

        if (InfoLen > 0)
        {
            var infoStream = new ReadStream(stream.ReadByteView((int)InfoLen), stream.Endianness);
            Info = new CastInfoChunk(Dir);
            Info.Read(infoStream);
        }
        HasFlags1 = false;
        SpecificData = stream.ReadByteView((int)SpecificDataLen);
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("type", Type.ToString());
        json.WriteField("id", Id);
        if (Info != null)
        {
            json.WriteKey("info");
            Info.WriteJSON(json);
        }
        json.EndObject();
    }

    public uint GetScriptID() => Info?.ScriptId ?? 0;
    public string GetScriptText() => Info?.ScriptSrcText ?? string.Empty;
    public void SetScriptText(string val) { if (Info != null) Info.ScriptSrcText = val; }
    public string GetName() => Info?.Name ?? string.Empty;
    public static string ExtractTextFromMember(CastMemberChunk member)
    {
        var bytes = member.SpecificData.Data.AsSpan(member.SpecificData.Offset, member.SpecificData.Size);
        int nullPos = bytes.IndexOf((byte)0);
        if (nullPos >= 0)
            bytes = bytes.Slice(0, nullPos);

        return Encoding.UTF8.GetString(bytes);
    }
    public string GetText()
    {
        if (Type == MemberType.TextMember || Type == MemberType.FieldMember)
        {
            var span = SpecificData.Data.AsSpan(SpecificData.Offset, SpecificData.Size);
            // If Pascal-style string (length byte + text)
            if (span.Length > 1 && span[0] <= span.Length - 1)
                return Encoding.UTF8.GetString(span.Slice(1, span[0]));

            // If null-terminated string
            int len = span.IndexOf((byte)0);
            if (len < 0) len = span.Length;

            var text = Encoding.UTF8.GetString(span.Slice(0, len));
            return text;
        }
        return string.Empty;
    }
}
