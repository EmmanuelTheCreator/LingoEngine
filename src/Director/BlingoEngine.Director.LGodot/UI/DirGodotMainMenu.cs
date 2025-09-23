using AbstUI.Commands;
using AbstUI.Components;
using AbstUI.Components.Menus;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Primitives;
using AbstUI.LGodot.Windowing;
using AbstUI.Primitives;
using AbstUI.Tools;
using AbstUI.Windowing;
using Godot;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Casts.Commands;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Texts;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.LGodot.Casts;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.LGodot.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;

namespace BlingoEngine.Director.LGodot;

/// <summary>
/// Godot wrapper for <see cref="DirectorMainMenu"/>.
/// </summary>
internal partial class DirGodotMainMenu : Control , IDirFrameworkMainMenuWindow, IFrameworkFor<DirectorMainMenu>
{
    private readonly AbstGodotWrapPanel _menuBar;
    private readonly AbstGodotWrapPanel _iconBar;
    private Panel _bgColorPanel;
    private readonly DirectorMainMenu _directorMainMenu;
    private readonly IAbstGodotWindowManager _windowManager;
    private readonly IAbstCommandManager _commandManager;
    private readonly BlingoPlayer _player;

    protected StyleBoxFlat Style = new StyleBoxFlat();

    public bool IsOpen => true;
    public bool IsActiveWindow => true;
    public string Title { get; set; }
    public AColor BackgroundColor { get; set; }
    public AColor BackgroundTitleColor { get; set; }

    IAbstMouse IAbstFrameworkWindow.Mouse => _directorMainMenu.Mouse;

    public IAbstKey AbstKey => _directorMainMenu.Key;

    string IAbstFrameworkNode.Name { get => Name; set => Name = value; }
    public bool Visibility { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public AMargin Margin { get; set; } = new AMargin();

    public object FrameworkNode => throw new NotImplementedException();

    public IAbstFrameworkNode? Content { get; set ; }

    public DirGodotMainMenu(
        DirectorProjectManager projectManager,
        BlingoPlayer player,
        IAbstShortCutManager shortCutManager,
        IHistoryManager historyManager,
        IAbstGodotWindowManager windowManager,
        IAbstCommandManager commandManager,
        DirectorMainMenu directorMainMenu, IBlingoFrameworkFactory factory)
    {
        _directorMainMenu = directorMainMenu;
        _windowManager = windowManager;
        _commandManager = commandManager;
        _player = player;
        _directorMainMenu.Init(this);

        _menuBar = directorMainMenu.MenuBar.Framework<AbstGodotWrapPanel>();
        _iconBar = directorMainMenu.IconBar.Framework<AbstGodotWrapPanel>();
        CreateBgColor();

        AddChild(_bgColorPanel);
        AddChild(_menuBar);
        AddChild(_iconBar);
        _menuBar.Position = new Vector2(10, 1);
        _iconBar.Position = new Vector2(400, 1);

        StyleTopMenu(directorMainMenu);
        foreach (var childItem in _iconBar.GetChild(0).GetChildren())
        {
            if (childItem is Button btn)
            {
                StyleIconButton(btn);
            }
        }
       
    }
    public void Init(IAbstWindow instance)
    {

    }

    public void RegisterTopMenu(AbstMenu menu)
    {
        AddChild(menu.Framework<AbstGodotMenu>());
    }

    private static void StyleIconButton(Button btn)
    {
        var topMenuBtnStyle = new StyleBoxFlat
        {
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            BorderWidthTop = 0,
            BorderWidthBottom = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
            ContentMarginLeft = 2,
            ContentMarginRight = 2,
        };
        btn.AddThemeStyleboxOverride("normal", topMenuBtnStyle);
        btn.Size = new Vector2(18, 18);
    }
    private static void StyleTopMenu(DirectorMainMenu directorMainMenu)
    {
        var topMenuBtnStyle = new StyleBoxFlat
        {
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            BorderWidthTop = 0,
            BorderWidthBottom = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
        };
        var topMenuBtnStyle_hover = new StyleBoxFlat
        {
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            BorderWidthBottom = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
        };
        
        directorMainMenu.CallOnAllTopMenuButtons(btn =>
        {
            var btnG = (Button)btn.Framework<AbstGodotButton>().FrameworkNode;
            btnG.AddThemeStyleboxOverride("normal", topMenuBtnStyle);
            btnG.AddThemeStyleboxOverride("hover", topMenuBtnStyle_hover);
            btnG.CustomMinimumSize = new Vector2(30, 18);
        });
    }

    private void CreateBgColor()
    {
        StyleBoxFlat panelStyle = new StyleBoxFlat();
        _bgColorPanel = new Panel();
        _bgColorPanel.Size = new Vector2(3000, 20);
        //_bgColorPanel.CustomMinimumSize = new Vector2(3000, 20);
        _bgColorPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _bgColorPanel.GrowHorizontal = GrowDirection.End;
        _bgColorPanel.Name = "MainMenuBackgroundColorPanel";
        panelStyle.BgColor = DirectorColors.BG_TopMenu.ToGodotColor(); ;
        _bgColorPanel.AddThemeStyleboxOverride("panel", panelStyle);
    }


    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Delete)
        {
            var active = _windowManager.ActiveWindow;
            if (active is DirGodotCastWindow castWin && _player.ActiveMovie is BlingoMovie movie2)
            {
                var member = castWin.SelectedMember as BlingoMember;
                if (member != null)
                {
                    var cast = (BlingoCast)movie2.CastLib.GetCast(member.CastLibNum);
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

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        // not allowed
    }
    public void SetSize(int width, int height)
    {
        // not allowed
    }

    public new APoint GetPosition() => Position.ToAbstPoint();

    public new APoint GetSize() => Size.ToAbstPoint();

  
}

