using LingoEngine.Primitives;

namespace LingoEngine.L3D.Core.Members;

/// <summary>
/// Camera used by a 3D sprite to view the world.
/// </summary>
public class LingoCamera
{
    /// <summary>backdrop color of the camera</summary>
    public LingoColor Backdrop { get; set; } = new();

    /// <summary>fieldOfView property</summary>
    public float FieldOfView { get; set; } = 60f;

    /// <summary>near clipping plane distance (hither)</summary>
    public float Hither { get; set; } = 1f;

    /// <summary>far clipping plane distance (yon)</summary>
    public float Yon { get; set; } = 1000f;
}
