using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Windows;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotToolsWindow : BaseGodotWindow, IDirFrameworkToolsWindow
{
    private readonly GridContainer _grid = new GridContainer();
    private readonly DirGodotIconManager _iconManager = new DirGodotIconManager();

    public event Action<int>? IconPressed;

    public DirGodotToolsWindow() : base("Tools")
    {
        Size = new Vector2(80, 200);
        CustomMinimumSize = Size;

        _grid.Columns = 2;
        _grid.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(_grid);

        var image = new Image();
        var err = image.Load("res://Medias/Data/tool_icons.png");
        if (err == Error.Ok)
        {
            int iconSize = image.GetHeight();
            int count = image.GetWidth() / iconSize;
            _iconManager.AddBitmap(image, iconSize, iconSize);
            for (int i = 0; i < count; i++)
            {
                var tex = _iconManager.GetIcon(i);
                var btn = new TextureButton { TextureNormal = tex };
                int index = i;
                btn.Pressed += () => IconPressed?.Invoke(index);
                _grid.AddChild(btn);
            }
        }
    }

    public bool IsOpen => Visible;
    public void OpenWindow() => Visible = true;
    public void CloseWindow() => Visible = false;
    public void MoveWindow(int x, int y) => Position = new Vector2(x, y);
}
