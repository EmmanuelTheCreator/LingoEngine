using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Gfx;

/// <summary>
/// Provides raw pixel data for editor icons so they can be drawn on a cross-platform canvas.
/// </summary>
public interface IDirIconManager
{
    /// <summary>Returns pixel data for the specified icon.</summary>
    LingoIconData GetData(DirEditorIcon icon);
}
