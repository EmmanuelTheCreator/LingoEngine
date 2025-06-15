using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Events;
using System;
using System.Collections.Generic;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Simple timeline overlay showing the Score channels and frames.
/// Toggled with F2.
/// </summary>
public partial class DirGodotScoreWindow : BaseGodotWindow
{
    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _hScroller = new ScrollContainer();
    private readonly ScrollContainer _vScroller = new ScrollContainer();
    private readonly Control _scrollContent = new Control();
    private readonly DirGodotScoreGrid _grid;
    private readonly DirGodotFrameHeader _header;
    private readonly DirGodotFrameScriptsBar _frameScripts;
    private readonly DirGodotScoreLabelsBar _labelBar;

    private readonly Button _playButton = new Button();
    private readonly IDirectorEventMediator _directorMediator;


    public DirGodotScoreWindow(IDirectorEventMediator directorMediator)
        : base("Score")
    {
        _directorMediator = directorMediator;
        Position = new Vector2(0, 60);
        Size = new Vector2(800, 600);
        CustomMinimumSize = Size;
        _grid = new DirGodotScoreGrid(directorMediator);
        _header = new DirGodotFrameHeader();
        _frameScripts = new DirGodotFrameScriptsBar();
        _labelBar = new DirGodotScoreLabelsBar();

        AddChild(_hScroller);
        _hScroller.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _hScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _hScroller.Size = new Vector2(800, 580);
        _hScroller.Position = new Vector2(0, 20);
        _hScroller.AddChild(_scrollContent);

        _scrollContent.AddChild(_labelBar);
        _scrollContent.AddChild(_frameScripts);
        _scrollContent.AddChild(_header);
        _scrollContent.AddChild(_vScroller);


        _vScroller.VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _vScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;

        _vScroller.AddChild(_grid);
        _vScroller.Size = new Vector2(800, 520);
        _vScroller.Position = new Vector2(0, 60);

        _labelBar.Position = new Vector2(0, 0);
        _frameScripts.Position = new Vector2(0, 20);
        _header.Position = new Vector2(0, 40);
    }

    public void Toggle()
    {
        Visible = !Visible;
        _grid.Visible = Visible;
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        _grid.SetMovie(movie);
        _header.SetMovie(movie);
        _frameScripts.SetMovie(movie);
        _labelBar.SetMovie(movie);
    }
    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
    }
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.F2) && !wasToggleKey)
            Toggle();
        wasToggleKey = Input.IsKeyPressed(Key.F2);
    }

    protected override void Dispose(bool disposing)
    {
        _grid.Dispose();
        _labelBar.Dispose();
        _frameScripts.Dispose();
        _vScroller.Dispose();
        _hScroller.Dispose();
        base.Dispose(disposing);
    }
}

internal partial class DirGodotFrameHeader : Control
{
    private LingoMovie? _movie;
    private const int ChannelHeight = 16;
    private const int FrameWidth = 9;
    private const int ChannelLabelWidth = 54;
    private const int ChannelInfoWidth = ChannelHeight + ChannelLabelWidth;

    public void SetMovie(LingoMovie? movie) => _movie = movie;

public override void _Draw()
    {
        if (_movie == null) return;
        int frameCount = _movie.FrameCount;
        var font = ThemeDB.FallbackFont;
        Size = new Vector2(ChannelInfoWidth + frameCount * FrameWidth, 20);
        DrawRect(new Rect2(0, 0, Size.X, 20), new Color("#f0f0f0"));
        for (int f = 0; f <= frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
            if (f % 5 == 0)
                DrawString(font, new Vector2(x + 1, font.GetAscent()), f.ToString(),
                    HorizontalAlignment.Left, -1, 10, new Color("#a0a0a0"));
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            if (_movie == null) return;
            Vector2 pos = GetLocalMousePosition();
            int frame = Mathf.RoundToInt((pos.X - ChannelInfoWidth) / FrameWidth) + 1;
            if (frame >= 1 && frame <= _movie.FrameCount)
                _movie.GoTo(frame);
        }
    }
}

internal partial class DirGodotFrameScriptsBar : Control
{
    private LingoMovie? _movie;
    private const int ChannelHeight = 16;
    private const int FrameWidth = 9;
    private const int ChannelLabelWidth = 54;
    private const int ChannelInfoWidth = ChannelHeight + ChannelLabelWidth;
    private readonly List<DirGodotScoreSprite> _sprites = new();

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        _sprites.Clear();
        if (_movie == null) return;
        foreach (var kv in _movie.GetFrameSpriteBehaviors())
            _sprites.Add(new DirGodotScoreSprite(kv.Value, false, true));
    }

    public override void _Draw()
    {
        if (_movie == null) return;
        int frameCount = _movie.FrameCount;
        var font = ThemeDB.FallbackFont;
        Size = new Vector2(ChannelInfoWidth + frameCount * FrameWidth, 20);
        DrawRect(new Rect2(0, 0, Size.X, 20), new Color("#f0f0f0"));
        for (int f = 0; f < frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
            if (f % 5 == 0)
                DrawRect(new Rect2(x, 0, FrameWidth, ChannelHeight), Colors.DarkGray);
        }
        for (int f = 0; f <= frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
            DrawLine(new Vector2(x, 0), new Vector2(x, ChannelHeight), Colors.DarkGray);
        }
        DrawLine(new Vector2(0, 0), new Vector2(ChannelInfoWidth + frameCount * FrameWidth, 0), Colors.DarkGray);
        DrawLine(new Vector2(0, ChannelHeight), new Vector2(ChannelInfoWidth + frameCount * FrameWidth, ChannelHeight), Colors.DarkGray);

        foreach (var sp in _sprites)
        {
            float x = ChannelInfoWidth + (sp.Sprite.BeginFrame - 1) * FrameWidth;
            float width = (sp.Sprite.EndFrame - sp.Sprite.BeginFrame + 1) * FrameWidth;
            sp.Draw(this, new Vector2(x, 0), width, ChannelHeight, font);
        }

        int cur = _movie.CurrentFrame - 1;
        if (cur < 0) cur = 0;
        float barX = ChannelInfoWidth + cur * FrameWidth + FrameWidth / 2f;
        DrawLine(new Vector2(barX, 0), new Vector2(barX, ChannelHeight), Colors.Red, 2);
    }
}
internal partial class DirGodotScoreGrid : Control
{
    private LingoMovie? _movie;
    private const int ChannelHeight = 16;
    private const int FrameWidth = 9;
    private const int ChannelLabelWidth = 54;
    private const int ChannelInfoWidth = ChannelHeight + ChannelLabelWidth;
    private ILingoSprite? _dragSprite;
    private bool _dragBegin;
    private bool _dragEnd;
    private readonly List<DirGodotScoreSprite> _sprites = new();
    private DirGodotScoreSprite? _selected;
    private readonly IDirectorEventMediator _mediator;

    public DirGodotScoreGrid(IDirectorEventMediator mediator)
    {
        _mediator = mediator;
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

    private void SelectSprite(DirGodotScoreSprite? sprite)
    {
        if (_selected == sprite) return;
        if (_selected != null) _selected.Selected = false;
        _selected = sprite;
        if (_selected != null)
        {
            _selected.Selected = true;
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
            int channel = (int)(pos.Y / ChannelHeight);
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    if (channel >= 0 && channel < totalChannels)
                    {
                        if (channel < _movie.MaxSpriteChannelCount && pos.X >= 0 && pos.X < ChannelHeight)
                        {
                            var ch = _movie.Channel(channel);
                            ch.Visibility = !ch.Visibility;
                            QueueRedraw();
                            return;
                        }

                        foreach (var sp in _sprites)
                        {
                            int sc = sp.Sprite.SpriteNum - 1;
                            if (sc == channel)
                            {
                                float sx = ChannelInfoWidth + (sp.Sprite.BeginFrame - 1) * FrameWidth;
                                float ex = ChannelInfoWidth + sp.Sprite.EndFrame * FrameWidth;
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
        }
        else if (@event is InputEventMouseMotion motion && _dragSprite != null)
        {
            float frame = (GetLocalMousePosition().X - ChannelInfoWidth) / FrameWidth;
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
    

    public override void _Draw()
    {
        if (!Visible || _movie == null) return;
        int channelCount = _movie.MaxSpriteChannelCount;
        int frameCount = _movie.FrameCount;
        var font = ThemeDB.FallbackFont;
        const int ExtraMargin = 10;
        Size = new Vector2(ChannelInfoWidth + frameCount * FrameWidth + ExtraMargin,
            channelCount * ChannelHeight + ExtraMargin);
        CustomMinimumSize = Size;

        DrawRect(new Rect2(0, 0, Size.X, Size.Y), Colors.White);

        for (int f = 0; f < frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
            if (f % 5 == 0)
                DrawRect(new Rect2(x, 0, FrameWidth, channelCount * ChannelHeight), Colors.DarkGray);
        }

        for (int f = 0; f <= frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
            DrawLine(new Vector2(x, 0), new Vector2(x, channelCount * ChannelHeight), Colors.DarkGray);
        }

        for (int c = 0; c < channelCount; c++)
        {
            float y = c * ChannelHeight;
            DrawLine(new Vector2(0, y), new Vector2(ChannelInfoWidth + frameCount * FrameWidth, y), Colors.DarkGray);
            var ch = _movie.Channel(c);
            Color vis = ch.Visibility ? Colors.LightGray : new Color(0.2f, 0.2f, 0.2f);
            DrawRect(new Rect2(0, y, ChannelInfoWidth, ChannelHeight), new Color("#f0f0f0"));
            DrawRect(new Rect2(0, y, ChannelHeight, ChannelHeight), vis);
            DrawString(font, new Vector2(ChannelHeight + 2, y + font.GetAscent() - 6), (c + 1).ToString(),
                HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
        }

        // Draw sprites
        foreach (var sp in _sprites)
        {
            int ch = sp.Sprite.SpriteNum - 1;
            if (ch < 0 || ch >= channelCount) continue;
            float x = ChannelInfoWidth + (sp.Sprite.BeginFrame - 1) * FrameWidth;
            float width = (sp.Sprite.EndFrame - sp.Sprite.BeginFrame + 1) * FrameWidth;
            float y = ch * ChannelHeight;
            sp.Draw(this, new Vector2(x, y), width, ChannelHeight, font);
        }

        int cur = _movie.CurrentFrame - 1;
        if (cur < 0) cur = 0;
        float barX = ChannelInfoWidth + cur * FrameWidth + FrameWidth / 2f;
        DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * ChannelHeight), Colors.Red, 2);
    }
}

internal partial class DirGodotScoreLabelsBar : Control
{
    private LingoMovie? _movie;
    private const int ChannelHeight = 16;
    private const int FrameWidth = 9;
    private const int ChannelLabelWidth = 54;
    private const int ChannelInfoWidth = ChannelHeight + ChannelLabelWidth;
    public void SetMovie(LingoMovie? movie) => _movie = movie;
    public override void _Draw()
    {
        if (_movie == null) return;
        int frameCount = _movie.FrameCount;
        var font = ThemeDB.FallbackFont;
        Size = new Vector2(ChannelInfoWidth + frameCount * FrameWidth, 20);
        DrawRect(new Rect2(0, 0, Size.X, 20), Colors.White);
        foreach (var kv in _movie.GetScoreLabels())
        {
            float x = ChannelInfoWidth + (kv.Value - 1) * FrameWidth;
            Vector2[] pts = { new Vector2(x, 5), new Vector2(x + 10, 5), new Vector2(x + 5, 15) };
            DrawPolygon(pts, new[] { Colors.Black });
            DrawString(font, new Vector2(x + 12, font.GetAscent() + 5), kv.Key,
                HorizontalAlignment.Left, -1, 10, Colors.Black);
        }
    }
}
