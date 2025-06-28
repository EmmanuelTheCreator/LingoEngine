using LingoEngine.L3D.Core.Primitives;

namespace LingoEngine.L3D.Core.Members;

/// <summary>
/// A basic node in a 3D world used to group objects.
/// </summary>
public class LingoGroup
{
    public string Name { get; set; } = string.Empty;
    public LingoGroup? Parent { get; set; }
    public LingoVector3 WorldPosition { get; set; } = new(0, 0, 0);
}
