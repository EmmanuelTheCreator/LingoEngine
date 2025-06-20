
using LingoEngine.Director.Core.Menus;

namespace LingoEngine.Director.LGodot;

public interface IDirGodotWindowManager : IDirFrameworkWindowManager
{
    void Register(BaseGodotWindow godotWindow);
    void SetActiveWindow(BaseGodotWindow window);
}
internal class DirGodotWindowManager : IDirGodotWindowManager
{
    private IDirectorWindowManager _directorWindowManager;
    private readonly Dictionary<string, BaseGodotWindow> _godotWindows = new();
    public BaseGodotWindow? ActiveWindow { get; private set; }

    public DirGodotWindowManager(IDirectorWindowManager directorWindowManager)
    {
        _directorWindowManager = directorWindowManager;
        directorWindowManager.Init(this);
    }
    public void Register(BaseGodotWindow godotWindow)
    {
        _godotWindows.Add(godotWindow.WindowCode, godotWindow);
    }

    public void SetActiveWindow(BaseGodotWindow window)
    {
        _directorWindowManager.SetActiveWindow(window.WindowCode);
        if (ActiveWindow == window)
            return;

       
    }

    public void SetActiveWindow(IDirectorWindowRegistration windowRegistration)
    {
        var window = _godotWindows[windowRegistration.WindowCode];
        ActiveWindow = window;
        var parent = window.GetParent();
        if (parent != null)
            parent.MoveChild(window, parent.GetChildCount() - 1);
    }
}
