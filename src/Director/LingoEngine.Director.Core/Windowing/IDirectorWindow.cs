namespace LingoEngine.Director.Core.Windowing
{
    public interface IDirectorWindow
    {
        void Init(IDirFrameworkWindow frameworkWindow);
        bool IsOpen { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);

        IDirFrameworkWindow FrameworkObj { get; }
    }
}
