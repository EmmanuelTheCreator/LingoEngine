using LingoEngine.Primitives;

namespace LingoEngine.Primitives3D;

/// <summary>
/// Defines how a model surface is shaded.
/// </summary>
public class LingoShader
{
    public string Name { get; set; } = string.Empty;
    public LingoColor DiffuseColor { get; set; } = new();
    public LingoColor SpecularColor { get; set; } = new();
    public float Smoothness { get; set; } = 0f;
}
