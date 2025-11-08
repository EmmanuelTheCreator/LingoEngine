using System.Diagnostics;

namespace BlingoEngine.IO.Data.DTO;

[DebuggerDisplay("PointDto: {X} x {Y}")]
public struct BlingoPointDTO
{
    public float X { get; set; }
    public float Y { get; set; }

    public BlingoPointDTO(float x, float y)
    {
        X = x;
        Y = y;
    }
    public BlingoPointDTO()
    {
        
    }
}

