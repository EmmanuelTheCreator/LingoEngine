using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Movies;
using System;
using System.Linq;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class MemberNavigationBar<T> : HBoxContainer where T : class, ILingoMember
{
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;

    private readonly Button _prevButton = new Button();
    private readonly Button _nextButton = new Button();
    private readonly Label _typeLabel = new Label();
    private readonly LineEdit _nameEdit = new LineEdit();
    private readonly Label _numberLabel = new Label();
    private readonly Button _infoButton = new Button();
    private readonly Label _castLibLabel = new Label();

    private T? _member;

    public MemberNavigationBar(IDirectorEventMediator mediator, ILingoPlayer player, int barHeight = 20)
    {
        _mediator = mediator;
        _player = player;

        CustomMinimumSize = new Vector2(0, barHeight);

        _prevButton.Text = "<";
        _prevButton.CustomMinimumSize = new Vector2(20, barHeight);
        _prevButton.Pressed += () => Navigate(-1);
        AddChild(_prevButton);

        _nextButton.Text = ">";
        _nextButton.CustomMinimumSize = new Vector2(20, barHeight);
        _nextButton.Pressed += () => Navigate(1);
        AddChild(_nextButton);

        _typeLabel.CustomMinimumSize = new Vector2(20, barHeight);
        AddChild(_typeLabel);

        _nameEdit.CustomMinimumSize = new Vector2(100, barHeight);
        _nameEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _nameEdit.TextChanged += t => { if (_member != null) _member.Name = t; };
        AddChild(_nameEdit);

        AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

        _numberLabel.CustomMinimumSize = new Vector2(40, barHeight);
        AddChild(_numberLabel);

        _infoButton.Text = "I";
        _infoButton.Modulate = Colors.Blue;
        _infoButton.CustomMinimumSize = new Vector2(20, barHeight);
        _infoButton.Pressed += OnInfo;
        AddChild(_infoButton);

        _castLibLabel.CustomMinimumSize = new Vector2(80, barHeight);
        AddChild(_castLibLabel);
    }

    public void SetMember(T member)
    {
        _member = member;
        _nameEdit.Text = member.Name;
        _numberLabel.Text = member.NumberInCast.ToString();
        _castLibLabel.Text = GetCastName(member);
        _typeLabel.Text = member.Type.ToString();
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
