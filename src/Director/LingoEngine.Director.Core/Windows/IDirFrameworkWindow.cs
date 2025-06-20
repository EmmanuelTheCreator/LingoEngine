using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Windows
{
    public interface IDirFrameworkWindow
    {
        bool IsOpen { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);
    }

    public interface IDirFrameworkCastWindow : IDirFrameworkWindow
    {
        void LoadMovie(ILingoMovie lingoMovie);
    }
    public interface IDirFrameworkScoreWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkStageWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkToolsWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkBinaryViewerWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkProjectSettingsWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkPropertyInspectorWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkTextEditWindow : IDirFrameworkWindow { }
}
