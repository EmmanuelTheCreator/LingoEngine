using LingoEngine.Director.Core.Windows;

namespace LingoEngine.Director.Core.Casts
{
    public class DirectorTextEditWindow : DirectorWindow<IDirFrameworkTextEditWindow>
    {
        public DirectorTextEditWindow(IDirFrameworkTextEditWindow framework) : base(framework) { }
    }
}
