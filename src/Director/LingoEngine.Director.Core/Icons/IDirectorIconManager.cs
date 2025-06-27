
using LingoEngine.Bitmaps;

namespace LingoEngine.Director.Core.Icons;

public interface IDirectorIconManager
{

    ILingoImageTexture Get(DirectorIcon icon);
}
