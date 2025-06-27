using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Members;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Core;
using LingoEngine.Texts;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Gfx;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Windowing.Commands;
using LingoEngine.Sprites;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Icons;

namespace LingoEngine.Director.LGodot.Inspector;

public partial class DirGodotPropertyInspector : BaseGodotWindow, IHasSpriteSelectedEvent, IHasMemberSelectedEvent, IDirFrameworkPropertyInspectorWindow
{
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoCommandManager _commandManager;
    private readonly LingoGfxTabContainer _tabs;
    private readonly PanelContainer _behaviorPanel = new PanelContainer();
    private readonly VBoxContainer _behaviorBox = new VBoxContainer();
    private readonly Button _behaviorClose = new Button();
    private readonly LingoPlayer _player;
    private readonly LingoGfxPanel _headerPanel;
    private readonly LingoGfxWrapPanel _header;
    private readonly DirectorMemberThumbnail _thumb;
    private readonly DirectorPropertyInspectorWindow _inspectorWindow;
    private const int HeaderHeight = 44;

    public DirGodotPropertyInspector(IDirectorEventMediator mediator, ILingoCommandManager commandManager, DirectorPropertyInspectorWindow inspectorWindow, ILingoPlayer player, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager)
        : base(DirectorMenuCodes.PropertyInspector, "Property Inspector", windowManager)
    {
        _mediator = mediator;
        _commandManager = commandManager;
        _player = (LingoPlayer)player;
        _inspectorWindow = inspectorWindow;
        _inspectorWindow.Init(this);

        //Position = new Vector2(500, 20);
        Size = new Vector2(260, 400);
        CustomMinimumSize = Size;

        var headerElems = _inspectorWindow.CreateHeaderElements(_player.Factory, iconManager);
        _thumb = headerElems.Thumbnail;
        _header = headerElems.Header;
        _headerPanel = headerElems.Panel;
        _tabs = _player.Factory.CreateTabContainer("InspectorTabs");
        _inspectorWindow.Setup(_player, _commandManager, _tabs, _thumb, _header);
        CreateHeader();

        var godotTabs = _tabs.Framework<LingoGodotTabContainer>();
        godotTabs.Position = new Vector2(0, TitleBarHeight + HeaderHeight);
        godotTabs.Size = new Vector2(Size.X - 10, Size.Y - 30 - HeaderHeight);

        AddChild(godotTabs);

        _behaviorPanel.Visible = false;
        _behaviorPanel.Position = new Vector2(0, TitleBarHeight + HeaderHeight);
        _behaviorPanel.Size = new Vector2(Size.X - 10, 0);
        AddChild(_behaviorPanel);
        _behaviorPanel.AddChild(_behaviorBox);
        var closeRow = new HBoxContainer();
        closeRow.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
        _behaviorClose.Text = "X";
        _behaviorClose.Modulate = Colors.Red;
        _behaviorClose.CustomMinimumSize = new Vector2(12, 12);
        _behaviorClose.Pressed += () => { _behaviorPanel.Visible = false; OnResizing(Size); };
        closeRow.AddChild(_behaviorClose);
        _behaviorBox.AddChild(closeRow);

        //_tabs.Framework<LingoGodotTabContainer>().AddTab(name, vScroller);
        _mediator.Subscribe(this);
    }

    private void CreateHeader()
    {
        _header.Width = Size.X - 10;
        _header.Height = HeaderHeight;

        AddChild(_headerPanel.Framework<LingoGodotPanel>());
        _headerPanel.X = 0;
        _headerPanel.Y = TitleBarHeight;
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        var layout = _inspectorWindow.OnResizing(size.X, size.Y, TitleBarHeight, _behaviorPanel.Visible);
        if (layout is { } rect)
        {
            _behaviorPanel.Position = new Vector2(rect.X, rect.Y);
            _behaviorPanel.Size = new Vector2(rect.Width, rect.Height);
        }
    }
    public void SpriteSelected(ILingoSprite sprite) => _inspectorWindow.ShowObject(sprite);
    public void MemberSelected(ILingoMember member) => _inspectorWindow.ShowObject(member);

    private void ShowBehavior(LingoSpriteBehavior behavior)
    {
        foreach (var child in _behaviorBox.GetChildren().ToArray())
        {
            if (child != _behaviorBox.GetChild(0))
                _behaviorBox.RemoveChild(child);
        }
        var panel = _inspectorWindow.BuildBehaviorPanel(_player.Factory, behavior);
        _behaviorBox.AddChild(panel.Framework<LingoGodotWrapPanel>());
        _behaviorPanel.Visible = true;
        OnResizing(Size);
    }
}
