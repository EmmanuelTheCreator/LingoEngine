using LingoEngine.Director.Core.Windows;

namespace LingoEngine.Director.Core.Casts
{
    public class DirectorCastWindow : DirectorWindow<IDirFrameworkCastWindow>
    {
        public DirectorCastWindow(IDirFrameworkCastWindow framework) : base(framework) { }
    }
}
