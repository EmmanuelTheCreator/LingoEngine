using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotFrameHeader : Control
{
    private LingoMovie? _movie;

    private readonly DirScoreGfxValues _gfxValues;
    public DirGodotFrameHeader(DirScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
    }
    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        Size = new Vector2(_gfxValues.LeftMargin + (_movie?.FrameCount ?? 0) * _gfxValues.FrameWidth, 20);
    }

    public override void _Draw()
    {
        if (_movie == null) return;
        int frameCount = _movie.FrameCount;
        var font = ThemeDB.FallbackFont;
        Size = new Vector2(_gfxValues.LeftMargin + (frameCount ) * _gfxValues.FrameWidth, 20);
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color("#f0f0f0"));
        for (int f = 0; f <= frameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if (f % 5 == 0)
                DrawString(font, new Vector2(x + 1, font.GetAscent()-6), f.ToString(),
                    HorizontalAlignment.Left, -1, 10, new Color("#a0a0a0"));
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            if (_movie == null) return;
            Vector2 pos = GetLocalMousePosition();
            int frame = Mathf.RoundToInt((pos.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1;
            if (frame >= 1 && frame <= _movie.FrameCount)
            {
                if (_movie.IsPlaying)
                    _movie.GoTo(frame);
                else
                    _movie.GoToAndStop(frame);
            }
        }
    }
}
