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
    private ScrollContainer _scroller = new ScrollContainer();
    private DirGodotScoreGrid _grid;
    private bool _dragging;

    public DirGodotScoreWindow()
        : base("Score")
    {
        Position = new Vector2(0, 30);
        _grid = new DirGodotScoreGrid();
        AddChild(_scroller);
        _scroller.AddChild( _grid );
        _scroller.Size = new Vector2(800, 600);
        _scroller.Position = new Vector2(0, 20);
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
internal partial class DirGodotScoreGrid : Control
{
    private LingoMovie? _movie;
    private const int ChannelHeight = 16;
    private const int FrameWidth = 9;
    private const int ChannelNumberWidth = 32; // includes visibility square


    public void SetMovie(LingoMovie? movie) => _movie = movie;

    public override void _Input(InputEvent @event)
    {
        if (!Visible || _movie == null) return;
        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            Vector2 pos = GetLocalMousePosition();
            int channel = (int)(pos.Y / ChannelHeight);
            if (channel >= 0 && _movie != null && channel < _movie.MaxSpriteChannelCount)
            {
                if (pos.X >= 0 && pos.X < ChannelHeight)
                {
                    var ch = _movie.Channel(channel);
                    ch.Visibility = !ch.Visibility;
                    QueueRedraw();
                }
            }
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
        Size = new Vector2(channelCount* FrameWidth + 80, frameCount* ChannelHeight);

        for (int f = 0; f < frameCount; f++)
        {
            float x = ChannelNumberWidth + f * FrameWidth;
            // Frames per 5
            if (f % 5 == 0)
                DrawRect(new Rect2(x, 0, FrameWidth, channelCount * ChannelHeight), new Color(0.3f, 0.3f, 0.3f, 0.2f));
        }

        for (int f = 0; f <= frameCount; f++)
        {
            float x = ChannelNumberWidth + f * FrameWidth;
            DrawLine(new Vector2(x, 0), new Vector2(x, channelCount * ChannelHeight), Colors.DarkGray);
            // Frames per 5
            if (f % 5 == 0)
            {
                DrawRect(new Rect2(0, 0, FrameWidth, channelCount* ChannelHeight), Colors.Green);
                DrawString(font, new Vector2(x -(f.ToString().Length*8), -12 + font.GetHeight()), f.ToString(), HorizontalAlignment.Center, -1, 10, Colors.White);
            }
        }

        for (int c = 0; c <= channelCount; c++)
        {
            float y = c * ChannelHeight;
            DrawLine(new Vector2(0, y), new Vector2(ChannelNumberWidth + frameCount * FrameWidth, y), Colors.DarkGray);
            if (c < channelCount)
            {
                var ch = _movie.Channel(c);
                Color vis = ch.Visibility ? Colors.LightGray : new Color(0.2f, 0.2f, 0.2f);
                DrawRect(new Rect2(0, y, ChannelHeight, ChannelHeight), vis);
                DrawString(font, new Vector2(ChannelHeight + 2, y + font.GetAscent()- 6), (c + 1).ToString(), HorizontalAlignment.Left, -1,11, Colors.White);
            }
        }

        int cur = _movie.CurrentFrame - 1;
        if (cur < 0) cur = 0;
        float barX = ChannelNumberWidth + cur * FrameWidth + FrameWidth / 2f;
        DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * ChannelHeight), Colors.Red, 2);
    }
}
