using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreLabelsBar : Control
{
    private LingoMovie? _movie;

    private readonly DirGodotScoreGfxValues _gfxValues;
    public DirGodotScoreLabelsBar(DirGodotScoreGfxValues gfxValues)
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
        Size = new Vector2(_gfxValues.LeftMargin + (frameCount) * _gfxValues.FrameWidth, 20);
        DrawRect(new Rect2(0, 0, Size.X, 20), Colors.White);
        foreach (var kv in _movie.GetScoreLabels())
        {
            float x = _gfxValues.LeftMargin + (kv.Value - 1) * _gfxValues.FrameWidth;
            Vector2[] pts = { new Vector2(x, 5), new Vector2(x + 10, 5), new Vector2(x + 5, 15) };
            DrawPolygon(pts, new[] { Colors.Black });
            DrawString(font, new Vector2(x + 12, font.GetAscent() - 5), kv.Key,
                HorizontalAlignment.Left, -1, 10, Colors.Black);
        }
    }
}
