using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;

namespace LingoEngine.L3D.Core.Members;

/// <summary>
/// Represents a Shockwave 3D cast member containing a complete 3D world.
/// Mirrors the Lingo 3D Member object.
/// </summary>
public class LingoMember3D : LingoMember
{
    public List<LingoCamera> Cameras { get; } = new();
    public List<LingoGroup> Groups { get; } = new();
    public List<LingoLight> Lights { get; } = new();
    public List<LingoModel> Models { get; } = new();

    public LingoMember3D(LingoCast cast, ILingoFrameworkMember3D frameworkMember, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
        : base(frameworkMember, LingoMemberType.Shockwave3D, cast, numberInCast, name, fileName, regPoint)
    {
    }
}
