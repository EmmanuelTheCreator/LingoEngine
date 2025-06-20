namespace LingoEngine.Primitives3D;

/// <summary>
/// Describes how particles are emitted in a particle system.
/// Properties mirror the Lingo emitter object (angle, direction, mode, etc.).
/// </summary>
public class LingoEmitter
{
    public float Angle { get; set; } = 180.0f;
    public LingoVector3 Direction { get; set; } = new(0, 0, -1);
    public string Mode { get; set; } = "#stream"; // e.g., #stream or #burst
    public int Loop { get; set; } = 1;
    public float MinSpeed { get; set; } = 0.0f;
    public float MaxSpeed { get; set; } = 0.0f;
}
