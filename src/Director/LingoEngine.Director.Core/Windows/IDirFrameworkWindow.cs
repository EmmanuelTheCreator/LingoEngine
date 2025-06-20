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
        void SetActiveMovie(ILingoMovie lingoMovie);
    }
    public interface IDirFrameworkMainMenuWindow : IDirFrameworkWindow { }

    public interface IDirFrameworkScoreWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkStageWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkToolsWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkBinaryViewerWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkProjectSettingsWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkPropertyInspectorWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkTextEditWindow : IDirFrameworkWindow { }
    public interface IDirFrameworkPictureEditWindow : IDirFrameworkWindow { }
}
