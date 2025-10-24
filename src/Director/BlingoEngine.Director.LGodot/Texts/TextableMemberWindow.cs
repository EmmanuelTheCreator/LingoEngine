using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using Godot;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Events;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.Texts;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Texts;

namespace BlingoEngine.Director.LGodot.Casts;

internal partial class DirGodotTextableMemberWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkTextEditWindow, IFrameworkFor<DirectorTextEditWindow>
{
    private const int NavigationBarHeight = 20;
    private const int ActionBarHeight = 22;
    private readonly TextEdit _textEdit = new TextEdit();
    private readonly MemberNavigationBar<IBlingoMemberTextBase> _navBar;
    private readonly TextEditIconBar _iconBar;
    private readonly AbstGodotPanel _topBar;

    private readonly IBlingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly IAbstFontManager _blingoFontManager;
    private IBlingoMemberTextBase? _member;
    private const int _topOffset = 4;
    public DirGodotTextableMemberWindow(IDirectorEventMediator mediator, IBlingoPlayer player, DirectorTextEditWindow directorTextEditWindow, IServiceProvider serviceProvider, IDirectorIconManager iconManager, IAbstFontManager blingoFontManager, IBlingoFrameworkFactory factory)
        : base( "Edit Text", serviceProvider)
    {
        _player = player;
        _iconManager = iconManager;
        _blingoFontManager = blingoFontManager;
        mediator.Subscribe(this);
        Init(directorTextEditWindow);

        Size = new Vector2(450, 200);
        CustomMinimumSize = Size;

        _navBar = new MemberNavigationBar<IBlingoMemberTextBase>(mediator, player, _iconManager, factory, NavigationBarHeight);
        AddChild(_navBar.Panel.Framework<AbstGodotWrapPanel>());
        _navBar.Panel.X = 0;
        _navBar.Panel.Y = TitleBarHeight;
        _navBar.Panel.Width = Size.X;
        _navBar.Panel.Height = NavigationBarHeight;

        _iconBar = directorTextEditWindow.IconBar;
        _iconBar.AlignmentChanged += a => SetAlignment(a);
        _iconBar.BoldChanged += v => ToggleStyle(BlingoTextStyle.Bold, v);
        _iconBar.ItalicChanged += v => ToggleStyle(BlingoTextStyle.Italic, v);
        _iconBar.UnderlineChanged += v => ToggleStyle(BlingoTextStyle.Underline, v);
        _iconBar.FontSizeChanged += v =>
        {
            if (_member != null)
                _member.FontSize = v;
            _textEdit.AddThemeConstantOverride("font_size", v);
            ApplyStyleToEditor();
        };
        _iconBar.FontChanged += fontName =>
        {
            if (_member != null)
                _member.Font = fontName;
            var f = _blingoFontManager.Get<Font>(fontName);
            if (f != null)
                _textEdit.AddThemeFontOverride("font", f);
            ApplyStyleToEditor();
        };
        _iconBar.ColorChanged += c =>
        {
            _textEdit.AddThemeColorOverride("font_color", c.ToGodotColor());
            if (_member != null)
                _member.Color = c;
        };

        _iconBar.SetFonts(_blingoFontManager.GetAllNames());

        _topBar = _iconBar.Panel.Framework<AbstGodotPanel>();
        _topBar.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + 2);
        AddChild(_topBar);

        _textEdit.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + ActionBarHeight + _topOffset);
        _textEdit.Size = new Vector2(Size.X - 10, Size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5));
        _textEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _textEdit.SizeFlagsVertical = SizeFlags.ExpandFill;
        _textEdit.TextChanged += () =>
        {
            if (_member != null)
                _member.Text = _textEdit.Text;
        };
        AddChild(_textEdit);
        ApplyStyleToEditor();
    }

    public void MemberSelected(IBlingoMember member)
    {
        if (member is IBlingoMemberTextBase textMember)
            SetMemberValues(textMember);
    }

    private void SetMemberValues(IBlingoMemberTextBase textMember)
    {
        _member = textMember;
        _textEdit.Text = textMember.Text.Replace("\r", "\r\n");
        _textEdit.AddThemeColorOverride("font_color", textMember.Color.ToGodotColor());
        _textEdit.AddThemeConstantOverride("font_size", textMember.FontSize);
        var font = _blingoFontManager.Get<Font>(textMember.Font, textMember.FontStyle.ToAbstUI());
        if (font != null)
            _textEdit.AddThemeFontOverride("font", font);

        _iconBar.SetMemberValues(textMember);
        _navBar.SetMember(textMember);
        ApplyStyleToEditor();
    }

    protected override void OnResizing(bool firstResize, Vector2 size)
    {
        base.OnResizing(firstResize, size);
        _iconBar.OnResizing(size.X, size.Y);
        _navBar.Panel.Width = size.X;
        _navBar.Panel.Height = NavigationBarHeight;
        _textEdit.Size = new Vector2(size.X - 10, size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5 + _topOffset));
    }

    private bool SetAlignment(AbstTextAlignment alignment)
    {
        if (_member != null)
            _member.Alignment = alignment;
        int val = alignment switch
        {
            AbstTextAlignment.Left => (int)HorizontalAlignment.Left,
            AbstTextAlignment.Center => (int)HorizontalAlignment.Center,
            AbstTextAlignment.Right => (int)HorizontalAlignment.Right,
            AbstTextAlignment.Justified => (int)HorizontalAlignment.Fill,
            _ => (int)HorizontalAlignment.Left
        };
        _textEdit.Set("alignment", val);
        return true;
    }

    private void ToggleStyle(BlingoTextStyle style, bool on)
    {
        if (_member == null) return;
        switch (style)
        {
            case BlingoTextStyle.Bold:
                _member.Bold = on; break;
            case BlingoTextStyle.Italic:
                _member.Italic = on; break;
            case BlingoTextStyle.Underline:
                _member.Underline = on; break;
        }
        ApplyStyleToEditor();
    }

    private void ApplyStyleToEditor()
    {
        var fontName = _iconBar.SelectedFont;
        //if (font != null)
        //{
        //    var variation = new FontVariation { BaseFont = font };
        AbstFontStyle fs = AbstFontStyle.Regular;
            if (_iconBar.IsBold)
                fs |= AbstFontStyle.Bold;
            if (_iconBar.IsItalic)
                fs |= AbstFontStyle.Italic;
            // variation.FontStyle = fs;
            //_textEdit.AddThemeFontOverride("font", variation);
        //}
        var font = _blingoFontManager.Get<Font>(fontName, fs);
        if (font != null)
            _textEdit.AddThemeFontOverride("font", font);
        //_textEdit.UnderlineMode = _iconBar.IsUnderline ? UnderlineMode.Always : UnderlineMode.Disabled;
    }



}

