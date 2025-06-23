using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Texts;
using LingoEngine.Members;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Core;
using LingoEngine.Movies;
using System.Linq;

namespace LingoEngine.Director.LGodot.Casts;

internal partial class DirGodotTextableMemberWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkTextEditWindow
{
    private const int NavigationBarHeight = 20;
    private const int ActionBarHeight = 20;
    private readonly TextEdit _textEdit = new TextEdit();
    private readonly MemberNavigationBar<ILingoMemberTextBase> _navBar;
    private readonly Button _alignLeft = new Button();
    private readonly Button _alignCenter = new Button();
    private readonly Button _alignRight = new Button();
    private readonly SpinBox _fontSize = new SpinBox();
    private readonly HBoxContainer _topBar = new HBoxContainer();

    private readonly ILingoPlayer _player;
    private readonly IDirGodotIconManager _iconManager;
    private ILingoMemberTextBase? _member;

    public DirGodotTextableMemberWindow(IDirectorEventMediator mediator, ILingoPlayer player, DirectorTextEditWindow directorTextEditWindow, IDirGodotWindowManager windowManager, IDirGodotIconManager iconManager)
        : base(DirectorMenuCodes.TextEditWindow, "Edit Text", windowManager)
    {
        _player = player;
        _iconManager = iconManager;
        mediator.Subscribe(this);
        directorTextEditWindow.Init(this);

        Size = new Vector2(450, 200);
        CustomMinimumSize = Size;

        _navBar = new MemberNavigationBar<ILingoMemberTextBase>(mediator, player, _iconManager, NavigationBarHeight);
        AddChild(_navBar);
        _navBar.Position = new Vector2(0, TitleBarHeight);
        _navBar.CustomMinimumSize = new Vector2(Size.X, NavigationBarHeight);

        var bar = new HBoxContainer();
        bar.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight);
        AddChild(bar);

        _alignLeft.Text = "L";
        _alignLeft.CustomMinimumSize = new Vector2(20, 16);
        _alignLeft.Pressed += () => SetAlignment(LingoTextAlignment.Left);
        _topBar.AddChild(_alignLeft);

        _alignCenter.Text = "C";
        _alignCenter.CustomMinimumSize = new Vector2(20, 16);
        _alignCenter.Pressed += () => SetAlignment(LingoTextAlignment.Center);
        _topBar.AddChild(_alignCenter);

        _alignRight.Text = "R";
        _alignRight.CustomMinimumSize = new Vector2(20, 16);
        _alignRight.Pressed += () => SetAlignment(LingoTextAlignment.Right);
        _topBar.AddChild(_alignRight);

        _fontSize.MinValue = 1;
        _fontSize.MaxValue = 200;
        _fontSize.CustomMinimumSize = new Vector2(50, 16);
        _fontSize.ValueChanged += v => { if (_member != null) _member.FontSize = (int)v; };
        _topBar.AddChild(_fontSize);

        _textEdit.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + ActionBarHeight);
        _textEdit.Size = new Vector2(Size.X - 10, Size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5));
        _textEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _textEdit.SizeFlagsVertical = SizeFlags.ExpandFill;
        _textEdit.TextChanged += () =>
        {
            if (_member != null)
                _member.Text = _textEdit.Text;
        };
        AddChild(_textEdit);
    }

    public void MemberSelected(ILingoMember member)
    {
        if (member is ILingoMemberTextBase text)
        {
            _member = text;
            _textEdit.Text = text.Text;
            _fontSize.Value = text.FontSize;
            _navBar.SetMember(text);
        }
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _navBar.CustomMinimumSize = new Vector2(size.X, NavigationBarHeight);
        _textEdit.Size = new Vector2(size.X - 10, size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5));
    }

    private void SetAlignment(LingoTextAlignment alignment)
    {
        if (_member != null)
            _member.Alignment = alignment;
    }


   
}
