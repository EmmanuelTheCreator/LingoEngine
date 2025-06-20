using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Texts;
using LingoEngine.Members;

namespace LingoEngine.Director.LGodot.Casts;

internal partial class TextableMemberWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkTextEditWindow
{
    private readonly TextEdit _textEdit = new TextEdit();
    private readonly Button _alignLeft = new Button();
    private readonly Button _alignCenter = new Button();
    private readonly Button _alignRight = new Button();
    private readonly SpinBox _fontSize = new SpinBox();

    private ILingoMemberTextBase? _member;

    public TextableMemberWindow(IDirectorEventMediator mediator) : base("Edit Text")
    {
        mediator.Subscribe(this);

        Size = new Vector2(300, 200);
        CustomMinimumSize = Size;

        var bar = new HBoxContainer();
        bar.Position = new Vector2(0, TitleBarHeight);
        AddChild(bar);

        _alignLeft.Text = "L";
        _alignLeft.CustomMinimumSize = new Vector2(20, 16);
        _alignLeft.Pressed += () => SetAlignment(LingoTextAlignment.Left);
        bar.AddChild(_alignLeft);

        _alignCenter.Text = "C";
        _alignCenter.CustomMinimumSize = new Vector2(20, 16);
        _alignCenter.Pressed += () => SetAlignment(LingoTextAlignment.Center);
        bar.AddChild(_alignCenter);

        _alignRight.Text = "R";
        _alignRight.CustomMinimumSize = new Vector2(20, 16);
        _alignRight.Pressed += () => SetAlignment(LingoTextAlignment.Right);
        bar.AddChild(_alignRight);

        _fontSize.MinValue = 1;
        _fontSize.MaxValue = 200;
        _fontSize.CustomMinimumSize = new Vector2(50, 16);
        _fontSize.ValueChanged += v => { if (_member != null) _member.FontSize = (int)v; };
        bar.AddChild(_fontSize);

        _textEdit.Position = new Vector2(0, TitleBarHeight + 20);
        _textEdit.Size = new Vector2(Size.X - 10, Size.Y - (TitleBarHeight + 25));
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
        }
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _textEdit.Size = new Vector2(size.X - 10, size.Y - (TitleBarHeight + 25));
    }

    private void SetAlignment(LingoTextAlignment alignment)
    {
        if (_member != null)
            _member.Alignment = alignment;
    }

    public bool IsOpen => Visible;
    public void OpenWindow() => Visible = true;
    public void CloseWindow() => Visible = false;
    public void MoveWindow(int x, int y) => Position = new Vector2(x, y);
}
