using Godot;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Movies;
using System;
using System.Linq;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Tools;
using LingoEngine.LGodot.Bitmaps;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class MemberNavigationBar<T> : HBoxContainer where T : class, ILingoMember
{
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly int _barHeight;
    private readonly Button _prevButton = new Button();
    private readonly Button _nextButton = new Button();
    private readonly PanelContainer _typePanel = new();
    private readonly TextureRect _typeIcon = new();
    private readonly LineEdit _nameEdit = new LineEdit();
    private readonly Label _numberLabel = new Label();
    private readonly Button _infoButton = new Button();
    private readonly Label _castLibLabel = new Label();

    private T? _member;

    public MemberNavigationBar(IDirectorEventMediator mediator, ILingoPlayer player, IDirectorIconManager iconManager, int barHeight = 20)
    {
        _mediator = mediator;
        _player = player;
        _iconManager = iconManager;
        _barHeight = barHeight;

        CustomMinimumSize = new Vector2(0, barHeight);

        StyleIconButton(_prevButton, DirectorIcon.Previous);
        _prevButton.Pressed += () => Navigate(-1);
        AddChild(_prevButton);

        StyleIconButton(_nextButton, DirectorIcon.Next);
        _nextButton.Pressed += () => Navigate(1);
        AddChild(_nextButton);

        _typePanel.CustomMinimumSize = new Vector2(20, barHeight);
        var typeStyle = new StyleBoxFlat
        {
            BgColor = Colors.White,
            BorderColor = Colors.Black,
            BorderWidthBottom = 1,
            BorderWidthTop = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1
        };
        _typePanel.AddThemeStyleboxOverride("panel", typeStyle);
        _typeIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _typeIcon.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _typeIcon.SizeFlagsVertical = SizeFlags.ExpandFill;
        _typePanel.AddChild(_typeIcon);
        AddChild(_typePanel);

        _nameEdit.CustomMinimumSize = new Vector2(100, barHeight);
        _nameEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _nameEdit.TextChanged += t => { if (_member != null) _member.Name = t; };
        AddChild(_nameEdit);

        AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

        _numberLabel.CustomMinimumSize = new Vector2(40, barHeight);
        AddChild(_numberLabel);

        StyleIconButton(_infoButton, DirectorIcon.Info);
        _infoButton.Pressed += OnInfo;
        AddChild(_infoButton);

        _castLibLabel.CustomMinimumSize = new Vector2(80, barHeight);
        AddChild(_castLibLabel);
    }
    private void StyleIconButton(Button button, DirectorIcon icon)
    {
        button.Icon = ((LingoGodotImageTexture)_iconManager.Get(icon)).Texture;
        button.CustomMinimumSize = new Vector2(20, _barHeight);
        button.AddThemeStyleboxOverride("normal", new StyleBoxFlat
        {
            BgColor = Colors.Transparent,
            BorderWidthBottom = 0,
            BorderWidthTop = 0,
            BorderWidthLeft = 0,
            BorderWidthRight = 0
        });
    }
    public void SetMember(T member)
    {
        _member = member;
        _nameEdit.Text = member.Name;
        _numberLabel.Text = member.NumberInCast.ToString();
        _castLibLabel.Text = GetCastName(member);
        var icon = LingoMemberTypeIcons.GetIconType(member);
        _typeIcon.Texture = icon.HasValue ? ((LingoGodotImageTexture)_iconManager.Get(icon.Value)).Texture : null;
    }

    private string GetCastName(ILingoMember m)
    {
        if (_player.ActiveMovie is ILingoMovie movie)
        {
            return movie.CastLib.GetCast(m.CastLibNum).Name;
        }
        return string.Empty;
    }

    private void OnInfo()
    {
        if (_member == null) return;
        _mediator.RaiseFindMember(_member);
        _mediator.RaiseMemberSelected(_member);
    }

    private void Navigate(int offset)
    {
        if (_member == null) return;
        if (_player.ActiveMovie is not ILingoMovie movie) return;
        var cast = movie.CastLib.GetCast(_member.CastLibNum);
        var items = cast.GetAll().OfType<T>().OrderBy(m => m.NumberInCast).ToList();
        int index = items.FindIndex(m => m == _member);
        if (index < 0) return;
        int target = index + offset;
        if (target < 0 || target >= items.Count) return;
        var next = items[target];
        _mediator.RaiseFindMember(next);
        _mediator.RaiseMemberSelected(next);
        SetMember(next);
    }
}
