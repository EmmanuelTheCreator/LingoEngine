using LingoEngine.Movies;
using LingoEngine._3D.Members;
using LingoEngine.Events;

namespace LingoEngine._3D.Movies;

/// <summary>
/// Sprite type that exposes basic 3D functionality.
/// </summary>
public class Lingo3DSprite : LingoSprite
{
    private readonly List<LingoCamera> _cameras = new();

    public Lingo3DSprite(ILingoMovieEnvironment environment) : base(environment)
    {
    }

    /// <summary>Active camera for this sprite.</summary>
    public LingoCamera? Camera { get; set; }

    /// <summary>Adds a camera to the sprite.</summary>
    public void AddCamera(LingoCamera camera) => _cameras.Add(camera);

    /// <summary>Deletes a camera from the sprite.</summary>
    public void DeleteCamera(LingoCamera camera) => _cameras.Remove(camera);

    /// <summary>Number of cameras associated with the sprite.</summary>
    public int CameraCount => _cameras.Count;
}
