using LingoEngine.Director.Core.Windows;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Casts
{
    public class DirectorCastWindow : DirectorWindow<IDirFrameworkCastWindow>
    {
        public void LoadMovie(ILingoMovie lingoMovie) => Framework.SetActiveMovie(lingoMovie);
    }
}
