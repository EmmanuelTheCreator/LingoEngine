using Godot;
using LingoEngine.Movies;
using System;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Simple timeline overlay showing the Score channels and frames.
/// Toggled with F2.
/// </summary>
public partial class DirGodotScoreWindow : BaseGodotWindow
{
    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _scroller = new ScrollContainer();
    private readonly DirGodotScoreGrid _grid;
    private readonly DirGodotFrameHeader _header;
    private bool _dragging;

    public DirGodotScoreWindow()
        : base("Score")
    {
        Position = new Vector2(0, 30);
        _grid = new DirGodotScoreGrid();
        _header = new DirGodotFrameHeader();
        AddChild(_header);
        AddChild(_scroller);
        _scroller.AddChild(_grid);
        _scroller.Size = new Vector2(800, 580);
        _scroller.Position = new Vector2(0, 40);
        _header.Position = new Vector2(0, 20);
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
    }
    public override void _GuiInput(InputEvent @event)
    {

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
        _scroller.Dispose();
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


    public void SetMovie(LingoMovie? movie) => _movie = movie;

    public override void _Input(InputEvent @event)
    {
        if (!Visible || _movie == null) return;
        if (@event is InputEventMouseButton mb)
        {
            Vector2 pos = GetLocalMousePosition();
            int channel = (int)(pos.Y / ChannelHeight);
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    if (channel >= 0 && channel < _movie.MaxSpriteChannelCount)
                    {
                        if (pos.X >= 0 && pos.X < ChannelHeight)
                        {
                            var ch = _movie.Channel(channel);
                            ch.Visibility = !ch.Visibility;
                            QueueRedraw();
                            return;
                        }

                        int idx = 1;
                        while (_movie.TryGetAllTimeSprite(idx, out var sp))
                        {
                            int sc = sp.SpriteNum - 1;
                            if (sc == channel)
                            {
                                float sx = ChannelInfoWidth + (sp.BeginFrame - 1) * FrameWidth;
                                float ex = ChannelInfoWidth + sp.EndFrame * FrameWidth;
                                if (pos.X >= sx && pos.X <= ex)
                                {
                                    if (Math.Abs(pos.X - sx) < 3)
                                    {
                                        _dragSprite = sp;
                                        _dragBegin = true;
                                        _dragEnd = false;
                                    }
                                    else if (Math.Abs(pos.X - ex) < 3)
                                    {
                                        _dragSprite = sp;
                                        _dragBegin = false;
                                        _dragEnd = true;
                                    }
                                    break;
                                }
                            }
                            idx++;
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
        Size = new Vector2(ChannelInfoWidth + frameCount * FrameWidth, channelCount * ChannelHeight);

        for (int f = 0; f < frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
            if (f % 5 == 0)
                DrawRect(new Rect2(x, 0, FrameWidth, channelCount * ChannelHeight), new Color(0.3f, 0.3f, 0.3f, 0.2f));
        }

        for (int f = 0; f <= frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
            DrawLine(new Vector2(x, 0), new Vector2(x, channelCount * ChannelHeight), Colors.DarkGray);
        }

        for (int c = 0; c <= channelCount; c++)
        {
            float y = c * ChannelHeight;
            DrawLine(new Vector2(0, y), new Vector2(ChannelInfoWidth + frameCount * FrameWidth, y), Colors.DarkGray);
            if (c < channelCount)
            {
                var ch = _movie.Channel(c);
                Color vis = ch.Visibility ? Colors.LightGray : new Color(0.2f, 0.2f, 0.2f);
                DrawRect(new Rect2(0, y, ChannelInfoWidth, ChannelHeight), new Color("#f0f0f0"));
                DrawRect(new Rect2(0, y, ChannelHeight, ChannelHeight), vis);
                DrawString(font, new Vector2(ChannelHeight + 2, y + font.GetAscent() - 6), (c + 1).ToString(),
                    HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
            }
        }

        // Draw sprites
        int index = 1;
        while (_movie.TryGetAllTimeSprite(index, out var sprite))
        {
            int ch = sprite.SpriteNum - 1;
            if (ch < 0 || ch >= channelCount) { index++; continue; }
            float x = ChannelInfoWidth + (sprite.BeginFrame - 1) * FrameWidth;
            float width = (sprite.EndFrame - sprite.BeginFrame + 1) * FrameWidth;
            float y = ch * ChannelHeight;
            DrawRect(new Rect2(x, y, width, ChannelHeight), new Color("#ccccff"));
            if (sprite.Member != null)
                DrawString(font, new Vector2(x + 2, y + font.GetAscent() - 2), sprite.Member.Name ?? string.Empty,
                    HorizontalAlignment.Left, width - 4, 11, Colors.Black);
            index++;
        }

        int cur = _movie.CurrentFrame - 1;
        if (cur < 0) cur = 0;
        float barX = ChannelInfoWidth + cur * FrameWidth + FrameWidth / 2f;
        DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * ChannelHeight), Colors.Red, 2);
    }
}
