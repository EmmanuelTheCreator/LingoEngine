using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

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
