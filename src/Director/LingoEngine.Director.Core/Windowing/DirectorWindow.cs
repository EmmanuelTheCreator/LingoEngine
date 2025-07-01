namespace LingoEngine.Director.Core.Windowing
{
    public class DirectorWindow<TFrameworkWindow> : IDirectorWindow, IDisposable
        where TFrameworkWindow : IDirFrameworkWindow
    {
#pragma warning disable CS8618 
        private TFrameworkWindow _Framework;
        public TFrameworkWindow Framework => _Framework;
#pragma warning restore CS8618 

        public virtual void Init(IDirFrameworkWindow frameworkWindow)
        {
            _Framework = (TFrameworkWindow)frameworkWindow;
        }

        public bool IsOpen => _Framework.IsOpen;
        public virtual void OpenWindow() => _Framework.OpenWindow();
        public virtual void CloseWindow() => _Framework.CloseWindow();
        public virtual void MoveWindow(int x, int y) => _Framework.MoveWindow(x, y);

        public virtual void Dispose()
        {

        }

        public IDirFrameworkWindow FrameworkObj => _Framework;



    }
}
