using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Tools;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Commands;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.Casts.Commands;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.Core.UI;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Godot wrapper for <see cref="DirectorMainMenu"/>.
/// </summary>
internal partial class DirGodotMainMenu : Control, IDirFrameworkMainMenuWindow
{
    private readonly LingoGodotWrapPanel _menuBar;
    private readonly LingoGodotWrapPanel _iconBar;
    private readonly IDirGodotWindowManager _windowManager;
    private readonly ILingoCommandManager _commandManager;
    private readonly LingoPlayer _player;
    public bool IsOpen => true;

    public DirGodotMainMenu(
        DirectorProjectManager projectManager,
        LingoPlayer player,
        IDirectorShortCutManager shortCutManager,
        IHistoryManager historyManager,
        IDirGodotWindowManager windowManager,
        ILingoCommandManager commandManager,
        DirectorMainMenu directorMainMenu)
    {
        _windowManager = windowManager;
        _commandManager = commandManager;
        _player = player;
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

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Delete)
        {
            var active = _windowManager.ActiveWindow;
            if (active is DirGodotScoreWindow scoreWin && _player.ActiveMovie is LingoMovie movie)
            {
                var sprite = scoreWin.SelectedSprite;
                if (sprite != null)
                    _commandManager.Handle(new RemoveSpriteCommand(movie, sprite));
            }
            else if (active is DirGodotCastWindow castWin && _player.ActiveMovie is LingoMovie movie2)
            {
                var member = castWin.SelectedMember as LingoMember;
                if (member != null)
                {
                    var cast = (LingoCast)movie2.CastLib.GetCast(member.CastLibNum);
                    _commandManager.Handle(new RemoveMemberCommand(cast, member));
                }
            }
        }
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
