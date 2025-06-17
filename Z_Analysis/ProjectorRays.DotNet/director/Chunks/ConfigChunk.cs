using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class ConfigChunk : Chunk
{
    public short FileVersion;
    public short DirectorVersion;
    public short MinMember;

    public ConfigChunk(DirectorFile? dir) : base(dir, ChunkType.ConfigChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        // Use the director file's byte order so configuration values are
        // interpreted correctly on both Windows (little endian) and Mac
        // (big endian) authored movies.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        short len = stream.ReadInt16();
        FileVersion = stream.ReadInt16();
        stream.Skip(8); // movie rect
        MinMember = stream.ReadInt16();
        stream.Skip(2); // maxMember
        stream.Seek(36);
        DirectorVersion = stream.ReadInt16();
        stream.Seek(len); // skip the remainder
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("fileVersion", FileVersion);
        json.WriteField("minMember", MinMember);
        json.WriteField("directorVersion", DirectorVersion);
        json.EndObject();
    }
}
