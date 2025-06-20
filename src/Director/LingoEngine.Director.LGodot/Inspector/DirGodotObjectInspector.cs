using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Movies;
using System;
using System.Reflection;
using LingoEngine.Pictures;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Members;

namespace LingoEngine.Director.LGodot.Inspector;

public partial class DirGodotObjectInspector : BaseGodotWindow, IHasSpriteSelectedEvent, IHasMemberSelectedEvent 
{
    private readonly IDirectorEventMediator _mediator;
    private readonly ScrollContainer _vScroller = new ScrollContainer();
    private readonly TabContainer _tabs = new TabContainer();

    public DirGodotObjectInspector(IDirectorEventMediator mediator) : base("Inspector")
    {
        _mediator = mediator;
        _mediator.SubscribeToMenu(DirectorMenuCodes.ObjectInspector, () => Visible = !Visible);
        
        //Position = new Vector2(500, 20);
        Size = new Vector2(260, 400);
        CustomMinimumSize = Size;
        _tabs.Position = new Vector2(0, 20);
        _tabs.Size = new Vector2(Size.X - 10, Size.Y - 30);
        
        AddChild(_tabs);

        //_vScroller.AddChild(_tabs);
        //_vScroller.Size = new Vector2(Size.X - 10, Size.Y - 30);
        //_vScroller.Position = new Vector2(0, 60);
        _tabs.AddChild(_vScroller);

        _mediator.Subscribe(this);
    }
    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _tabs.Size = new Vector2(Size.X - 10, Size.Y - 30);
        //_vScroller.Size = new Vector2(Size.X - 10, Size.Y - 30);
    }
    public void SpriteSelected(ILingoSprite sprite) => ShowObject(sprite);
    public void MemberSelected(ILingoMember member) => ShowObject(member);

    public void ShowObject(object obj)
    {
        foreach (Node child in _tabs.GetChildren())
            _tabs.RemoveChild(child);
        switch (obj)
        {
            case LingoSprite sp:
                AddTab("Sprite", sp);
                if (sp.Member != null)
                    AddMemberTabs(sp.Member);
                break;
            case ILingoMember member:
                AddMemberTabs(member);
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
        BuildProperties(container, obj);
        vScroller.AddChild(container);
    }

    private void BuildProperties(VBoxContainer root, object obj)
    {
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead)
                continue;
            if (!IsSimpleType(prop.PropertyType))
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
