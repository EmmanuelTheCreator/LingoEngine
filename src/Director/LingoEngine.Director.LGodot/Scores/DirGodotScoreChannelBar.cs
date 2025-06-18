using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreChannelBar : Control
{

    DirGodotScoreGfxValues _gfxValues;

    private LingoMovie? _movie;

    public DirGodotScoreChannelBar(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        ClipContents = true;
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_movie == null) return;

        var font = ThemeDB.FallbackFont;
        int channelCount = _movie.MaxSpriteChannelCount;

        for (int c = 0; c < channelCount; c++)
        {
            float y = c * _gfxValues.ChannelHeight;
            var ch = _movie.Channel(c);
            Color vis = ch.Visibility ? Colors.LightGray : new Color(0.2f, 0.2f, 0.2f);

            DrawRect(new Rect2(0, y, _gfxValues.ChannelInfoWidth, _gfxValues.ChannelHeight), new Color("#f0f0f0"));
            DrawRect(new Rect2(0, y, _gfxValues.ChannelHeight, _gfxValues.ChannelHeight), vis);
            DrawString(font, new Vector2(_gfxValues.ChannelHeight + 2, y + font.GetAscent() - 6),
                (c + 1).ToString(), HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
        }
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (_movie == null || !Visible) return;

        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            Vector2 pos = GetLocalMousePosition();
            int channel = (int)(pos.Y / _gfxValues.ChannelHeight);
            if (channel >= 0 && channel < _movie.MaxSpriteChannelCount && pos.X >= 0 && pos.X < _gfxValues.ChannelHeight)
            {
                var ch = _movie.Channel(channel);
                ch.Visibility = !ch.Visibility;
                QueueRedraw();
            }
        }
    }
}
