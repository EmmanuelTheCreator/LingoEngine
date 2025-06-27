using LingoEngine.Primitives;
namespace LingoEngine.L3D.Core.Members;

/// <summary>
/// Texture applied to a model surface.
/// </summary>
public class LingoTexture
{
    public string FileName { get; set; } = string.Empty;
    public LingoColor? TransparentColor { get; set; }
}
