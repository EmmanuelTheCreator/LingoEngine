using LingoEngine.Director.Core.Pictures;

namespace LingoEngine.Director.Core.Windows
{
    public interface IDirFrameworkWindow
    {
        bool IsOpen { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);
     
    }
}
