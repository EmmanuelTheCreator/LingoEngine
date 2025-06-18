using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotFrameScriptsBar : Control
{
    private LingoMovie? _movie;
    private readonly List<DirGodotScoreSprite> _sprites = new();
    private readonly DirGodotScoreGfxValues _gfxValues;
    public DirGodotFrameScriptsBar(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
    }

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
        Size = new Vector2(_gfxValues.LeftMargin + frameCount * _gfxValues.FrameWidth, 20);
        DrawRect(new Rect2(0, 0, Size.X, 20), new Color("#f0f0f0"));
        for (int f = 0; f < frameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if (f % 5 == 0)
                DrawRect(new Rect2(x, 0, _gfxValues.FrameWidth, _gfxValues.ChannelHeight), Colors.DarkGray);
        }
        for (int f = 0; f <= frameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            DrawLine(new Vector2(x, 0), new Vector2(x, _gfxValues.ChannelHeight), Colors.DarkGray);
        }
        DrawLine(new Vector2(0, 0), new Vector2(_gfxValues.LeftMargin + frameCount * _gfxValues.FrameWidth, 0), Colors.DarkGray);
        DrawLine(new Vector2(0, _gfxValues.ChannelHeight), new Vector2(_gfxValues.LeftMargin + frameCount * _gfxValues.FrameWidth, _gfxValues.ChannelHeight), Colors.DarkGray);

        foreach (var sp in _sprites)
        {
            float x = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
            float width = (sp.Sprite.EndFrame - sp.Sprite.BeginFrame + 1) * _gfxValues.FrameWidth;
            sp.Draw(this, new Vector2(x, 0), width, _gfxValues.ChannelHeight, font);
        }

        int cur = _movie.CurrentFrame - 1;
        if (cur < 0) cur = 0;
        float barX = _gfxValues.LeftMargin + cur * _gfxValues.FrameWidth + _gfxValues.FrameWidth / 2f;
        DrawLine(new Vector2(barX, 0), new Vector2(barX, _gfxValues.ChannelHeight), Colors.Red, 2);
    }
}
