namespace LingoEngine.Director.Core.Projects
{
    public interface IDirectorWindow
    {
        bool IsOpen { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);
    }
}
