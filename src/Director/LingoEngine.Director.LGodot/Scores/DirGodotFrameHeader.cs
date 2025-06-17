using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

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
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color("#f0f0f0"));
        for (int f = 0; f <= frameCount; f++)
        {
            float x = ChannelInfoWidth + f * FrameWidth;
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
            int frame = Mathf.RoundToInt((pos.X - ChannelInfoWidth) / FrameWidth) + 1;
            if (frame >= 1 && frame <= _movie.FrameCount)
                _movie.GoTo(frame);
        }
    }
}
