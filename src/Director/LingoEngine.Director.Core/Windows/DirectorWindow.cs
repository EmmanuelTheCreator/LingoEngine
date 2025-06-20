using LingoEngine.Director.Core.Projects;

namespace LingoEngine.Director.Core.Windows
{
    public class DirectorWindow<TFrameworkWindow> : IDirectorWindow
        where TFrameworkWindow : IDirFrameworkWindow
    {
        protected readonly TFrameworkWindow _frameworkWindow;
        public DirectorWindow(TFrameworkWindow frameworkWindow)
        {
            _frameworkWindow = frameworkWindow;
        }

        public bool IsOpen => _frameworkWindow.IsOpen;
        public void OpenWindow() => _frameworkWindow.OpenWindow();
        public void CloseWindow() => _frameworkWindow.CloseWindow();
        public void MoveWindow(int x, int y) => _frameworkWindow.MoveWindow(x, y);

        public IDirFrameworkWindow FrameworkObj => _frameworkWindow;
        public T Framework<T>() where T : class, IDirFrameworkWindow => (T)_frameworkWindow;
    }
}
