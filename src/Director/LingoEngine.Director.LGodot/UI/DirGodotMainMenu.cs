using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Tools;
using LingoEngine.LGodot.Gfx;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Godot wrapper for <see cref="DirectorMainMenu"/>.
/// </summary>
internal partial class DirGodotMainMenu : Control, IDirFrameworkMainMenuWindow
{
    private readonly LingoGodotWrapPanel _menuBar;
    private readonly LingoGodotWrapPanel _iconBar;
    public bool IsOpen => true;

    public DirGodotMainMenu(
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


        AddChild(_iconBar);
        _iconBar.Position = new Vector2(300, 0);
        _iconBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _iconBar.AddItem(directorMainMenu.RewindButton.Framework<LingoGodotButton>());
        _iconBar.AddItem(directorMainMenu.PlayButton.Framework<LingoGodotButton>());

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
    
}
