using LingoEngine.Director.Core.Windows;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Casts
{
    public interface IDirFrameworkCastWindow : IDirFrameworkWindow
    {
        void SetActiveMovie(ILingoMovie lingoMovie);
    }
}
