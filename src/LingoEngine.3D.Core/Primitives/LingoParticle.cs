using LingoEngine.Primitives;

namespace LingoEngine.L3D.Core.Primitives;

/// <summary>
/// Model resource primitive of type #particle.
/// Contains an emitter and appearance properties.
/// </summary>
public class LingoParticle
{
    public LingoEmitter Emitter { get; set; } = new();
    public LingoColorRange ColorRange { get; set; } = new(new LingoColor(), new LingoColor());
    public int Lifetime { get; set; } = 0; // in milliseconds
}
