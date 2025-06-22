using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Movies;
using System.Reflection;
using LingoEngine.Pictures;
using LingoEngine.Members;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Core;
using LingoEngine.Commands;
using LingoEngine.Texts;
using System.Linq;
using LingoEngine.Director.Core.Commands;
using System.Reflection.PortableExecutable;
using System.Drawing;
using LingoEngine.Primitives;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.Core.Gfx;

namespace LingoEngine.Director.LGodot.Inspector;

public partial class DirGodotPropertyInspector : BaseGodotWindow, IHasSpriteSelectedEvent, IHasMemberSelectedEvent, IDirFrameworkPropertyInspectorWindow
{
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoCommandManager _commandManager;
    private readonly ScrollContainer _vScroller = new ScrollContainer();
    private readonly TabContainer _tabs = new TabContainer();
    private readonly PanelContainer _behaviorPanel = new PanelContainer();
    private readonly VBoxContainer _behaviorBox = new VBoxContainer();
    private readonly Button _behaviorClose = new Button();
    private readonly ILingoPlayer _player;
    private readonly HBoxContainer _header = new HBoxContainer();
    private readonly DirGodotMemberThumbnail _thumb = new DirGodotMemberThumbnail(36,36);
    private readonly VBoxContainer _headerText = new VBoxContainer();
    private readonly Label _spriteInfo = new Label();
    private readonly Label _memberInfo = new Label();
    private readonly Label _castInfo = new Label();
    private const int HeaderHeight = 44;

    public DirGodotPropertyInspector(IDirectorEventMediator mediator, ILingoCommandManager commandManager, DirectorPropertyInspectorWindow inspectorWindow, ILingoPlayer player, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.PropertyInspector, "Property Inspector", windowManager)
    {
        _mediator = mediator;
        _commandManager = commandManager;
        _player = player;
        inspectorWindow.Init(this);

        //Position = new Vector2(500, 20);
        Size = new Vector2(260, 400);
        CustomMinimumSize = Size;

        CreateHeader();

        _tabs.Position = new Vector2(0, TitleBarHeight + HeaderHeight);
        _tabs.Size = new Vector2(Size.X - 10, Size.Y - 30 - HeaderHeight);

        AddChild(_tabs);

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

        //_vScroller.AddChild(_tabs);
        //_vScroller.Size = new Vector2(Size.X - 10, Size.Y - 30);
        //_vScroller.Position = new Vector2(0, 60);
        _tabs.AddChild(_vScroller);

        _mediator.Subscribe(this);
    }

    private void CreateHeader()
    {
        _header.CustomMinimumSize = new Vector2(Size.X - 10, HeaderHeight);
        _header.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 4);
        margin.AddThemeConstantOverride("margin_right", 4);
        margin.AddThemeConstantOverride("margin_top", 2);
        margin.AddThemeConstantOverride("margin_bottom", 2);
        var thumbContainer = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = Colors.White,
            BorderColor = Colors.DarkGray
        };
        thumbContainer.AddThemeStyleboxOverride("panel", style);
        style.BorderWidthTop = 1;
        style.BorderWidthBottom = 1;
        style.BorderWidthLeft = 1;
        style.BorderWidthRight = 1;
        thumbContainer.AddChild(_thumb);
        margin.AddChild(thumbContainer);
        _header.AddChild(margin);

        _spriteInfo.LabelSettings = new LabelSettings { FontSize = 10, FontColor = Colors.Black };
        _memberInfo.LabelSettings = new LabelSettings { FontSize = 10, FontColor = Colors.Black };
        _castInfo.LabelSettings = new LabelSettings { FontSize = 10, FontColor = Colors.Black };
        _headerText.AddChild(_spriteInfo);
        _headerText.AddChild(_memberInfo);
        _headerText.AddChild(_castInfo);
        _headerText.AddThemeConstantOverride("separation", 1);
        _header.AddChild(_headerText);

        var headerPanel = new PanelContainer();
        headerPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor() });
        headerPanel.AddChild(_header);
        headerPanel.Position = new Vector2(0, TitleBarHeight);

        AddChild(headerPanel);
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _header.CustomMinimumSize = new Vector2(size.X - 10, HeaderHeight);
        _tabs.Position = new Vector2(0, TitleBarHeight + HeaderHeight);
        if (_behaviorPanel.Visible)
        {
            var half = (Size.Y - 30 - HeaderHeight) / 2f;
            _tabs.Size = new Vector2(Size.X - 10, half);
            _behaviorPanel.Position = new Vector2(0, TitleBarHeight + HeaderHeight + half);
            _behaviorPanel.Size = new Vector2(Size.X - 10, half);
        }
        else
        {
            _tabs.Size = new Vector2(Size.X - 10, Size.Y - 30 - HeaderHeight);
        }
        //_vScroller.Size = new Vector2(Size.X - 10, Size.Y - 30);
    }
    public void SpriteSelected(ILingoSprite sprite) => ShowObject(sprite);
    public void MemberSelected(ILingoMember member) => ShowObject(member);

    public void ShowObject(object obj)
    {
        foreach (Node child in _tabs.GetChildren())
            _tabs.RemoveChild(child);
        ILingoMember? member = null;
        if (obj is LingoSprite sp)
        {
            member = sp.Member;
            if (member != null)
            {
                _thumb.SetMember(member);
                _spriteInfo.Text = $"Sprite : {sp.SpriteNum}: {member.Type}";
            }
        }
        else if (obj is ILingoMember m)
        {
            member = m;
            _thumb.SetMember(member);
            _spriteInfo.Text = member.Type.ToString();
        }
        if (member != null)
        {
            _memberInfo.Text = member.Name;
            _castInfo.Text = GetCastName(member);
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
            case LingoMemberPicture pic:
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
        //_vScroller.Size = new Vector2(Size.X - 10, Size.Y - 30);
        //_vScroller.Position = new Vector2(0, 60);
        var vScroller = new ScrollContainer();
        vScroller.Name = name;
        _tabs.AddChild(vScroller);
        var container = new VBoxContainer();

        if (obj is LingoMemberPicture || obj is ILingoMemberTextBase)
        {
            var editBtn = new Button { Text = "Edit" };
            editBtn.Pressed += () =>
            {
                string code = obj switch
                {
                    LingoMemberPicture => DirectorMenuCodes.PictureEditWindow,
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

        BuildProperties(container, obj);

        vScroller.AddChild(container);
    }

    private void BuildProperties(VBoxContainer root, object obj)
    {
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead)
                continue;
            if (!IsSimpleType(prop.PropertyType) && prop.PropertyType != typeof(LingoEngine.Primitives.LingoPoint))
                continue;
            var h = new HBoxContainer();
            var label = new Label { Text = prop.Name, CustomMinimumSize = new Vector2(80, 16) };
            h.AddChild(label);
            Control editor;
            object? val = prop.GetValue(obj);
            if (obj is ILingoSprite && prop.Name == "Lock" && prop.PropertyType == typeof(bool))
            {
                var btn = new Button { Text = val is bool b && b ? "🔒" : "🔓" };
                if (prop.CanWrite)
                    btn.Pressed += () =>
                    {
                        bool current = prop.GetValue(obj) is bool b && b;
                        bool newVal = !current;
                        prop.SetValue(obj, newVal);
                        btn.Text = newVal ? "🔒" : "🔓";
                    };
                else
                    btn.Disabled = true;
                editor = btn;
            }
            else if (prop.PropertyType == typeof(bool))
            {
                var cb = new CheckBox { ButtonPressed = val is bool b && b };
                if (prop.CanWrite)
                    cb.Toggled += v => prop.SetValue(obj, v);
                else
                    cb.Disabled = true;
                editor = cb;
            }
            else if (prop.PropertyType == typeof(LingoEngine.Primitives.LingoPoint))
            {
                var point = val is LingoEngine.Primitives.LingoPoint p ? p : new LingoEngine.Primitives.LingoPoint();
                var xSpin = new SpinBox { Value = point.X, CustomMinimumSize = new Vector2(50, 16) };
                var ySpin = new SpinBox { Value = point.Y, CustomMinimumSize = new Vector2(50, 16) };
                if (prop.CanWrite)
                {
                    xSpin.ValueChanged += v =>
                    {
                        var pVal = (LingoEngine.Primitives.LingoPoint)prop.GetValue(obj);
                        pVal.X = (float)v;
                        prop.SetValue(obj, pVal);
                    };
                    ySpin.ValueChanged += v =>
                    {
                        var pVal = (LingoEngine.Primitives.LingoPoint)prop.GetValue(obj);
                        pVal.Y = (float)v;
                        prop.SetValue(obj, pVal);
                    };
                }
                else
                {
                    xSpin.Editable = false;
                    ySpin.Editable = false;
                }
                h.AddChild(xSpin);
                h.AddChild(ySpin);
                root.AddChild(h);
                continue;
            }
            else
            {
                var line = new LineEdit { Text = val?.ToString() ?? string.Empty };
                if (prop.CanWrite)
                    line.TextSubmitted += t =>
                    {
                        try
                        {
                            prop.SetValue(obj, ConvertTo(t, prop.PropertyType));
                        }
                        catch { }
                    };
                else
                    line.Editable = false;
                editor = line;
            }
            h.AddChild(editor);
            root.AddChild(h);
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
        BuildProperties(container, behavior);
        _behaviorBox.AddChild(container);
        _behaviorPanel.Visible = true;
        OnResizing(Size);
    }

    private static bool IsSimpleType(Type t)
    {
        return t.IsPrimitive || t == typeof(string) || t.IsEnum || t == typeof(float) || t == typeof(double) || t == typeof(decimal);
    }

    private static object ConvertTo(string text, Type t)
    {
        if (t == typeof(string)) return text;
        if (t.IsEnum) return Enum.Parse(t, text);
        return Convert.ChangeType(text, t);
    }

 


   
}
