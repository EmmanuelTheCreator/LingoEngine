
namespace LingoEngine.Director.Core.Icons;

/// <summary>
/// Provides raw pixel data for editor icons so they can be drawn on a cross-platform canvas.
/// </summary>
public interface IDirectorIconManager
{
    /// <summary>Returns pixel data for the specified icon.</summary>
    LingoIconData GetData(DirectorIcon icon);
}
