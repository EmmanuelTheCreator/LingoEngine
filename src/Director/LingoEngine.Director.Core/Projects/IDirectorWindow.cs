namespace LingoEngine.Director.Core.Projects
{
    using LingoEngine.Director.Core.Windows;

    public interface IDirectorWindow
    {
        bool IsOpen { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);

        IDirFrameworkWindow FrameworkObj { get; }
    }
}
