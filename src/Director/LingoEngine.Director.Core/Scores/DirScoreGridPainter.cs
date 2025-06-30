using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Scores;

/// <summary>
/// Framework independent painter for the Score grid.
/// Uses a <see cref="LingoGfxCanvas"/> to draw the grid graphics.
/// </summary>
public class DirScoreGridPainter
{
    private readonly DirScoreGfxValues _gfxValues;
    public LingoGfxCanvas Canvas { get; }

    public int FrameCount { get; set; }
    public int ChannelCount { get; set; }
    public bool DrawBackground { get; set; } = true;
    public float ScrollX { get; set; }

    public DirScoreGridPainter(ILingoFrameworkFactory factory, DirScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        Canvas = factory.CreateGfxCanvas("ScoreGridCanvas", 0, 0);
    }

    /// <summary>Redraws the grid to the canvas.</summary>
    public void Draw()
    {
        float width = _gfxValues.LeftMargin + FrameCount * _gfxValues.FrameWidth;
        float height = ChannelCount * _gfxValues.ChannelHeight;
        Canvas.Width = width;
        Canvas.Height = height;

        if (DrawBackground)
            Canvas.DrawRect(LingoRect.New(-ScrollX, 0, width, height), LingoColorList.White, true);

        for (int c = 0; c <= ChannelCount; c++)
        {
            float y = c * _gfxValues.ChannelHeight;
            Canvas.DrawLine(new LingoPoint(-ScrollX, y), new LingoPoint(width - ScrollX, y), _gfxValues.ColLineLight);
        }

        for (int f = 0; f < FrameCount; f++)
        {
            float x = -ScrollX + _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if (f % 5 == 0)
                Canvas.DrawRect(LingoRect.New(x, 0, _gfxValues.FrameWidth, height), LingoColorList.DarkGray, true);
        }

        for (int f = 0; f <= FrameCount; f++)
        {
            float x = -ScrollX + _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            Canvas.DrawLine(new LingoPoint(x, 0), new LingoPoint(x, height), LingoColorList.DarkGray);
        }

        Canvas.DrawLine(new LingoPoint(-ScrollX, 0), new LingoPoint(width - ScrollX, 0), LingoColorList.DarkGray);
        Canvas.DrawLine(new LingoPoint(-ScrollX, height), new LingoPoint(width - ScrollX, height), LingoColorList.DarkGray);
    }
}
