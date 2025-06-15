using Godot;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Top application menu bar with a follow-up icon bar.
/// </summary>
internal partial class DirGodotMainMenu : Control
{
    private readonly HBoxContainer _menuBar = new HBoxContainer();
    private readonly MenuButton _fileMenu = new MenuButton();
    private readonly HBoxContainer _iconBar = new HBoxContainer();

    public DirGodotMainMenu()
    {
        AddChild(_menuBar);
        _menuBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _fileMenu.Text = "File";
        var popup = _fileMenu.GetPopup();
        popup.AddItem("Quit", 1);
        popup.IdPressed += id => { if (id == 1) GetTree().Quit(); };
        _menuBar.AddChild(_fileMenu);

        AddChild(_iconBar);
        _iconBar.Position = new Vector2(0, 20);
        _iconBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

    public HBoxContainer IconBar => _iconBar;
}
