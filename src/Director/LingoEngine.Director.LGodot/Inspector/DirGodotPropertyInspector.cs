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
        _header.Width = size.X - 10;
        _header.Height = HeaderHeight;
        var godotTabs = _tabs.Framework<LingoGodotTabContainer>();
        godotTabs.Position = new Vector2(0, TitleBarHeight + HeaderHeight);
        if (_behaviorPanel.Visible)
        {
            var half = (Size.Y - 30 - HeaderHeight) / 2f;
            godotTabs.Size = new Vector2(Size.X - 10, half);
            _behaviorPanel.Position = new Vector2(0, TitleBarHeight + HeaderHeight + half);
            _behaviorPanel.Size = new Vector2(Size.X - 10, half);
        }
        else
        {
            godotTabs.Size = new Vector2(Size.X - 10, Size.Y - 30 - HeaderHeight);
        }
    }
    public void SpriteSelected(ILingoSprite sprite) => ShowObject(sprite);
    public void MemberSelected(ILingoMember member) => ShowObject(member);

    public void ShowObject(object obj)
    {
        _tabs.ClearTabs();
        ILingoMember? member = null;
        if (obj is LingoSprite sp)
        {
            member = sp.Member;
            if (member != null)
            {
                _thumb.SetMember(member);
                _inspectorWindow.SpriteText = $"Sprite : {sp.SpriteNum}: {member.Type}";
            }
        }
        else if (obj is ILingoMember m)
        {
            member = m;
            _thumb.SetMember(member);
            _inspectorWindow.SpriteText = member.Type.ToString();
        }
        if (member != null)
        {
            _inspectorWindow.MemberText = member.Name;
            _inspectorWindow.CastText = GetCastName(member);
        }
        switch (obj)
        {
            case LingoSprite sp2:
                AddTab("Sprite", sp2);
                if (sp2.Member != null)
                    AddMemberTabs(sp2.Member);
                break;
            case ILingoMember member2:
                AddMemberTabs(member2);
                break;
            default:
                AddTab(obj.GetType().Name, obj);
                break;
        }
    }

    private void AddMemberTabs(ILingoMember member)
    {
        AddTab("Member", member);
        switch (member)
        {
            case Texts.LingoMemberText text:
                AddTab("Text", text);
                break;
            case LingoMemberBitmap pic:
                AddTab("Picture", pic);
                break;
            case Sounds.LingoMemberSound sound:
                AddTab("Sound", sound);
                break;
            case LingoMemberFilmLoop film:
                AddTab("FilmLoop", film);
                break;
        }
    }

    private void AddTab(string name, object obj)
    {
        var vScroller = new ScrollContainer();
        vScroller.Name = name;
        _tabs.Framework<LingoGodotTabContainer>().AddTab(name, vScroller);
        var container = new VBoxContainer();

        if (obj is LingoMemberBitmap || obj is ILingoMemberTextBase)
        {
            var editBtn = new Button { Text = "Edit" };
            editBtn.Pressed += () =>
            {
                string code = obj switch
                {
                    LingoMemberBitmap => DirectorMenuCodes.PictureEditWindow,
                    ILingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                    _ => string.Empty
                };
                if (!string.IsNullOrEmpty(code))
                    _commandManager.Handle(new OpenWindowCommand(code));
            };
            container.AddChild(editBtn);
        }

        if (obj is LingoSprite sprite)
        {
            var behLabel = new Label { Text = "Behaviors" };
            container.AddChild(behLabel);
            var list = new ItemList
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 80)
            };
            foreach (var b in sprite.Behaviors)
                list.AddItem(b.GetType().Name);
            list.ItemActivated += idx => ShowBehavior(sprite.Behaviors[(int)idx]);
            container.AddChild(list);
        }

        var props = _inspectorWindow.BuildProperties(_player.Factory, obj);
        container.AddChild(props.Framework<LingoGodotWrapPanel>());

        vScroller.AddChild(container);
    }

    }

    private string GetCastName(ILingoMember m)
    {
        if (_player.ActiveMovie is ILingoMovie movie)
        {
            return movie.CastLib.GetCast(m.CastLibNum).Name;
        }
        return string.Empty;
    }

    private void ShowBehavior(LingoSpriteBehavior behavior)
    {
        foreach (var child in _behaviorBox.GetChildren().ToArray())
        {
            if (child != _behaviorBox.GetChild(0))
                _behaviorBox.RemoveChild(child);
        }
        var container = new VBoxContainer();
        var props = _inspectorWindow.BuildProperties(_player.Factory, behavior);
        container.AddChild(props.Framework<LingoGodotWrapPanel>());
        if (behavior is ILingoPropertyDescriptionList descProvider)
        {
            string? desc = descProvider.GetBehaviorDescription();
            if (!string.IsNullOrEmpty(desc))
                container.AddChild(new Label { Text = desc });

            var props = behavior.UserProperties;
            if (props.Count > 0)
            {
                container.AddChild(new Label { Text = "Properties" });
                foreach (var item in props)
                {
                    string labelText = item.Key.ToString();
                    if (props.DescriptionList != null &&
                        props.DescriptionList.TryGetValue(item.Key, out var desc2) &&
                        !string.IsNullOrEmpty(desc2.Comment))
                    {
                        labelText = desc2.Comment!;
                    }

                    var h = new HBoxContainer();
                    h.AddChild(new Label { Text = labelText, CustomMinimumSize = new Vector2(80, 16) });
                    h.AddChild(new Label { Text = item.Value?.ToString() ?? string.Empty });
                    container.AddChild(h);
                }
            }
        }
        _behaviorBox.AddChild(container);
        _behaviorPanel.Visible = true;
