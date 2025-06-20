using Godot;

namespace LingoEngine.Director.LGodot;

internal class GodotWindowManager
{
    public BaseGodotWindow? ActiveWindow { get; private set; }

    public void SetActiveWindow(BaseGodotWindow window)
    {
        if (ActiveWindow == window)
            return;

        ActiveWindow = window;
        var parent = window.GetParent();
        if (parent != null)
        {
            parent.MoveChild(window, parent.GetChildCount() - 1);
        }
    }
}
