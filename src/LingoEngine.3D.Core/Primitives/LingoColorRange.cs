using LingoEngine.Primitives;

namespace LingoEngine._3D.Primitives;

/// <summary>
/// Represents the start and end colors for particle systems.
/// </summary>
public struct LingoColorRange
{
    public LingoColor Start { get; set; }
    public LingoColor End { get; set; }

    public LingoColorRange(LingoColor start, LingoColor end)
    {
        Start = start;
        End = end;
    }
}
