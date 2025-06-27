using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

/// <summary>
/// Represents a Director XMED chunk (extended media info for styled text or field members).
/// This implementation captures raw data and optionally parses styled text.
/// </summary>
public class RaysXmedChunk : RaysChunk
{
    public RaysXmedChunk(RaysDirectorFile? dir, ChunkType type) : base(dir, type)
    {
    }

    /// <summary>
    /// Raw contents of the chunk, excluding header.
    /// </summary>
    public BufferView Data { get; private set; } = BufferView.Empty;

    /// <summary>
    /// Parsed rich/styled text info (optional).
    /// </summary>
    public RaysCastMemberTextRead? Parsed { get; private set; }

    public override void Read(ReadStream stream)
    {
        //Dir?.Logger.LogInformation($"XMED Begin Parse at stream.Pos={stream.Pos}, Size={stream.Size}");
        //Dir?.Logger.LogInformation("XMED Header Preview: " + BitConverter.ToString(stream.Data, stream.Pos, Math.Min(64, stream.Size - stream.Pos)));
        //byte[] preview = stream.PeekBytes(16);
        //Dir?.Logger?.LogInformation("XMED next 16 bytes: " + BitConverter.ToString(preview));

        Data = stream.ReadByteView(stream.Size - stream.Pos);

        // Try parsing styled text if possible
        try
        {
            Parsed = RaysCastMemberTextRead.FromXmedChunk(Data, Dir);
        }
        catch (Exception ex)
        {
            Dir?.Logger.LogWarning($"XMED parse failed: {ex.Message}");
        }
    }
}