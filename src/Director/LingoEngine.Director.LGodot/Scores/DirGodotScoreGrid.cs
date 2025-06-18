using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Events;
using System.Linq;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreGrid : Control, IHasSpriteSelectedEvent
{
    private LingoMovie? _movie;
   
    private ILingoSprite? _dragSprite;
    private bool _dragBegin;
    private bool _dragEnd;
    private readonly List<DirGodotScoreSprite> _sprites = new();
    private DirGodotScoreSprite? _selected;
    private readonly IDirectorEventMediator _mediator;
    private readonly PopupMenu _contextMenu = new();
    private DirGodotScoreSprite? _contextSprite;
    private readonly DirGodotScoreGfxValues _gfxValues;
    public DirGodotScoreGrid(IDirectorEventMediator mediator, DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        _mediator = mediator;
        AddChild(_contextMenu);
        _contextMenu.IdPressed += OnContextMenuItem;
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        _sprites.Clear();
        if (_movie == null) return;
        int idx = 1;
        while (_movie.TryGetAllTimeSprite(idx, out var sp))
        {
            _sprites.Add(new DirGodotScoreSprite((LingoSprite)sp));
            idx++;
        }
    }

    private void SelectSprite(DirGodotScoreSprite? sprite, bool raiseEvent = true)
    {
        if (_selected == sprite) return;
        if (_selected != null) _selected.Selected = false;
        _selected = sprite;
        if (_selected != null)
        {
            _selected.Selected = true;
            if (raiseEvent)
                _mediator.RaiseSpriteSelected(_selected.Sprite);
        }
        QueueRedraw();
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible || _movie == null) return;
        if (@event is InputEventMouseButton mb)
        {
            Vector2 pos = GetLocalMousePosition();
            int totalChannels = _movie.MaxSpriteChannelCount;
            int channel = (int)(pos.Y / _gfxValues.ChannelHeight);
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    if (channel >= 0 && channel < totalChannels)
                    {
                        foreach (var sp in _sprites)
                        {
                            int sc = sp.Sprite.SpriteNum - 1;
                            if (sc == channel)
                            {
                                float sx = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
                                float ex = _gfxValues.LeftMargin + sp.Sprite.EndFrame * _gfxValues.FrameWidth;
                                if (pos.X >= sx && pos.X <= ex)
                                {
                                    if (Math.Abs(pos.X - sx) < 3)
                                    {
                                        _dragSprite = sp.Sprite;
                                        _dragBegin = true;
                                        _dragEnd = false;
                                    }
                                    else if (Math.Abs(pos.X - ex) < 3)
                                    {
                                        _dragSprite = sp.Sprite;
                                        _dragBegin = false;
                                        _dragEnd = true;
                                    }
                                    else
                                    {
                                        SelectSprite(sp);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _dragSprite = null;
                    _dragBegin = _dragEnd = false;
                }
            }
            else if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
            {
                if (channel >= 0 && channel < totalChannels)
                {
                    var sp = GetSpriteAt(pos, channel);
                    if (sp != null && sp.Sprite.Member != null)
                    {
                        _contextSprite = sp;
                        _contextMenu.Clear();
                        _contextMenu.AddItem("Find Cast Member", 1);
                        var gp = GetGlobalMousePosition();
                        _contextMenu.Popup(new Rect2I((int)gp.X, (int)gp.Y, 0, 0));
                    }
                }
            }
        }
        else if (@event is InputEventMouseMotion motion && _dragSprite != null)
        {
            float frame = (GetLocalMousePosition().X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
            int newFrame = Math.Max(1, Mathf.RoundToInt(frame) + 1);
            if (_dragBegin)
                _dragSprite.BeginFrame = Math.Min(newFrame, _dragSprite.EndFrame);
            else if (_dragEnd)
                _dragSprite.EndFrame = Math.Max(newFrame, _dragSprite.BeginFrame);
            QueueRedraw();
        }
    }

    public override void _Process(double delta)
    {
        if (Visible)
            QueueRedraw();
    }

    public void SpriteSelected(ILingoSprite sprite)
    {
        var match = _sprites.FirstOrDefault(x => x.Sprite == sprite);
        SelectSprite(match, false);
    }

    private DirGodotScoreSprite? GetSpriteAt(Vector2 pos, int channel)
    {
        foreach (var sp in _sprites)
        {
            int sc = sp.Sprite.SpriteNum - 1;
            if (sc == channel)
            {
                float sx = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
                float ex = _gfxValues.LeftMargin + sp.Sprite.EndFrame * _gfxValues.FrameWidth;
                if (pos.X >= sx && pos.X <= ex)
                    return sp;
            }
        }
        return null;
    }

    private void OnContextMenuItem(long id)
    {
        if (id == 1 && _contextSprite?.Sprite.Member != null)
        {
            _mediator.RaiseFindMember(_contextSprite.Sprite.Member);
        }
        _contextSprite = null;
    }
    

    public override void _Draw()
    {
        if (!Visible || _movie == null) return;
        int channelCount = _movie.MaxSpriteChannelCount;
        int frameCount = _movie.FrameCount;
        var font = ThemeDB.FallbackFont;

        const int ExtraMargin = 20;

        Size = new Vector2(_gfxValues.LeftMargin + frameCount * _gfxValues.FrameWidth + ExtraMargin,
            channelCount * _gfxValues.ChannelHeight + ExtraMargin);
        CustomMinimumSize = Size;

        DrawRect(new Rect2(0, 0, Size.X, Size.Y), Colors.White);

        for (int f = 0; f < frameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if (f % 5 == 0)
                DrawRect(new Rect2(x, 0, _gfxValues.FrameWidth, channelCount * _gfxValues.ChannelHeight), Colors.DarkGray);
        }

        for (int f = 0; f <= frameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            DrawLine(new Vector2(x, 0), new Vector2(x, channelCount * _gfxValues.ChannelHeight), Colors.DarkGray);
        }

        // Draw sprites
        foreach (var sp in _sprites)
        {
            int ch = sp.Sprite.SpriteNum - 1;
            if (ch < 0 || ch >= channelCount) continue;
            float x = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
            float width = (sp.Sprite.EndFrame - sp.Sprite.BeginFrame + 1) * _gfxValues.FrameWidth;
            float y = ch * _gfxValues.ChannelHeight;
            sp.Draw(this, new Vector2(x, y), width, _gfxValues.ChannelHeight, font);
        }

        int cur = _movie.CurrentFrame - 1;
        if (cur < 0) cur = 0;
        float barX = _gfxValues.LeftMargin + cur * _gfxValues.FrameWidth + _gfxValues.FrameWidth / 2f;
        DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * _gfxValues.ChannelHeight), Colors.Red, 2);
    }
}
