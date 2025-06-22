using Godot;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotGridPainter : Control
{
    private readonly DirGodotScoreGfxValues _gfxValues;
    public int FrameCount { get; set; }
    public int ChannelCount { get; set; }
    public bool DrawBackground { get; set; } = true;

    public DirGodotGridPainter(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
    }

    public override void _Draw()
    {
        float width = _gfxValues.LeftMargin + FrameCount * _gfxValues.FrameWidth;
        float height = ChannelCount * _gfxValues.ChannelHeight;
        Size = new Vector2(width, height);
        if (DrawBackground)
            DrawRect(new Rect2(0, 0, width, height), Colors.White);
        for (int f = 0; f < FrameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if (f % 5 == 0)
                DrawRect(new Rect2(x, 0, _gfxValues.FrameWidth, height), Colors.DarkGray);
        }
        for (int f = 0; f <= FrameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            DrawLine(new Vector2(x, 0), new Vector2(x, height), Colors.DarkGray);
        }
        DrawLine(new Vector2(0, 0), new Vector2(width, 0), Colors.DarkGray);
        DrawLine(new Vector2(0, height), new Vector2(width, height), Colors.DarkGray);
    }
}
