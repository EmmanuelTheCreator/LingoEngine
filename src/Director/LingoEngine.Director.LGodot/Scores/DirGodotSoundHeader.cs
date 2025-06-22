using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Fixed left area showing sound channel icons and numbers.
/// </summary>
internal partial class DirGodotSoundHeader : Control
{
    private LingoMovie? _movie;
    private readonly DirGodotScoreGfxValues _gfxValues;
    private readonly bool[] _muted = new bool[4];
    private bool _collapsed;

    public DirGodotSoundHeader(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        Size = new Vector2(_gfxValues.ChannelInfoWidth, 0);
        CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, 0);
    }

    public bool Collapsed
    {
        get => _collapsed;
        set { _collapsed = value; QueueRedraw(); }
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        QueueRedraw();
    }

    public bool IsMuted(int channel) => _muted[channel];

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            if (!_collapsed)
            {
                int ch = (int)(mb.Position.Y / _gfxValues.ChannelHeight);
                if (ch >= 0 && ch < 4 && mb.Position.X >= 12 && mb.Position.X <= 28)
                    ToggleMute(ch);
            }
        }
    }

    public override void _Draw()
    {
        if (_movie == null || _collapsed) return;
        int channels = 4;
        float height = channels * _gfxValues.ChannelHeight;
        Size = new Vector2(_gfxValues.ChannelInfoWidth, height);
        DrawRect(new Rect2(0,0,Size.X,Size.Y), new Color("#f0f0f0"));
        var font = ThemeDB.FallbackFont;
        for (int c = 0; c < channels; c++)
        {
            float y = c * _gfxValues.ChannelHeight;
            string icon = _muted[c] ? "🔇" : "🔊";
            DrawString(font, new Vector2(4, y + font.GetAscent() - 6), icon, HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
            DrawString(font, new Vector2(_gfxValues.ChannelHeight + 2, y + font.GetAscent() - 6), $"{c + 1}", HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
        }
    }

    private void ToggleMute(int channel)
    {
        if (_movie == null) return;
        _muted[channel] = !_muted[channel];
        var chObj = _movie.GetEnvironment().Sound.Channel(channel + 1);
        chObj.Volume = _muted[channel] ? 0 : 255;
        QueueRedraw();
    }
}
