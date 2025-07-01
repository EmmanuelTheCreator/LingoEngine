using LingoEngine.Director.Core.Windowing;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Casts
{
    public interface IDirFrameworkCastWindow : IDirFrameworkWindow
    {
        void SetActiveMovie(ILingoMovie lingoMovie);
    }
}
