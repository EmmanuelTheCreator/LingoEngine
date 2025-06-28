using Godot;
using LingoEngine.Director.Core.Gfx;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Godot wrapper for <see cref="DirectorMainMenu"/>.
/// </summary>
internal partial class DirGodotMainMenu : Control, IDirFrameworkMainMenuWindow
{
    private readonly LingoGodotWrapPanel _menuBar;
    private readonly LingoGodotWrapPanel _iconBar;
    private readonly LingoGodotButton _fileButton;
    private readonly LingoGodotButton _editButton;
    private readonly LingoGodotButton _windowButton;

    public DirGodotMainMenu(IDirectorWindowManager windowManager,
        DirectorProjectManager projectManager,
        LingoPlayer player,
        IDirectorShortCutManager shortCutManager,
        IHistoryManager historyManager,
        DirectorMainMenu directorMainMenu)
    {
        directorMainMenu.Init(this);

        _menuBar = directorMainMenu.MenuBar.Framework<LingoGodotWrapPanel>();
        _iconBar = directorMainMenu.IconBar.Framework<LingoGodotWrapPanel>();

        AddChild(_menuBar);
        _menuBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _fileButton = directorMainMenu.FileButton.Framework<LingoGodotButton>();
        _editButton = directorMainMenu.EditButton.Framework<LingoGodotButton>();
        _windowButton = directorMainMenu.WindowButton.Framework<LingoGodotButton>();

        AddChild(_iconBar);
        _iconBar.Position = new Vector2(300, 0);
        _iconBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _iconBar.AddChild(directorMainMenu.RewindButton.Framework<LingoGodotButton>());
        _iconBar.AddChild(directorMainMenu.PlayButton.Framework<LingoGodotButton>());

        AddChild(directorMainMenu.FileMenu.Framework<LingoGodotMenu>());
        AddChild(directorMainMenu.EditMenu.Framework<LingoGodotMenu>());
        AddChild(directorMainMenu.WindowMenu.Framework<LingoGodotMenu>());

        // button events handled in core
    }


    public void OpenWindow()
    {
        // not allowed
    }
    public void CloseWindow()
    {
        // not allowed
    }
    public void MoveWindow(int x, int y)
    {
        // not allowed
    }
    public bool IsOpen => true;
}
