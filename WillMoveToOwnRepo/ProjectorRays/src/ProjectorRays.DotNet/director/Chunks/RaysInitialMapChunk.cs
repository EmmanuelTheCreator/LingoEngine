using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class RaysInitialMapChunk : RaysChunk
{
    public uint Version;
    public uint MmapOffset;

    public uint DirectorVersion { get; private set; }

    public RaysInitialMapChunk(RaysDirectorFile? dir) : base(dir, ChunkType.InitialMapChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        Version = stream.ReadUint32();
        MmapOffset = stream.ReadUint32();
        DirectorVersion = stream.ReadUint32(); 

        stream.Skip(8); // Skip remaining unused bytes
        Dir!.Logger.LogInformation($"InitialMapChunk: Version={Version}, MmapOffset={MmapOffset}, DirectorVersion={DirectorVersion}");

    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("version", Version);
        json.WriteField("mmapOffset", MmapOffset);
        json.WriteField("directorVersion", DirectorVersion);
        json.EndObject();
    }
}
