using Godot;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Members;
using LingoEngine.Sounds;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotSoundBar : Control
{
    private LingoMovie? _movie;
    private readonly List<DirGodotScoreAudioClip> _clips = new();
    private readonly DirGodotScoreGfxValues _gfxValues;
    private bool _dirty = true;
    private bool _collapsed;
    private readonly bool[] _muted = new bool[4];

    public DirGodotSoundBar(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
    }

    public bool IsMuted(int channel) => _muted[channel];

    public bool Collapsed
    {
        get => _collapsed;
        set
        {
            _collapsed = value;
            QueueRedraw();
        }
    }

    public void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.AudioClipListChanged -= OnClipsChanged;
        _movie = movie;
        _clips.Clear();
        if (_movie != null)
        {
            foreach (var clip in _movie.GetAudioClips())
                _clips.Add(new DirGodotScoreAudioClip(clip));
            _movie.AudioClipListChanged += OnClipsChanged;
        }
        QueueRedraw();
    }

    private void OnClipsChanged()
    {
        _clips.Clear();
        if (_movie != null)
            foreach (var clip in _movie.GetAudioClips())
                _clips.Add(new DirGodotScoreAudioClip(clip));
        QueueRedraw();
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (_movie == null || _collapsed) return false;

        var obj = data.Obj as LingoMemberSound;
        return obj != null;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (_movie == null || _collapsed) return;

        var sound = data.Obj as LingoMemberSound;
        if (sound == null) return;

        int channel = (int)(atPosition.Y / _gfxValues.ChannelHeight);
        int frame = Mathf.Clamp(Mathf.RoundToInt((atPosition.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1, 1, _movie.FrameCount);
        _movie.AddAudioClip(channel, frame, sound);
    }


    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            if (!_collapsed && mb.Position.Y >= 10)
            {
                int ch = (int)((mb.Position.Y - 10) / _gfxValues.ChannelHeight);
                if (ch >= 0 && ch < 4 && mb.Position.X >= 12 && mb.Position.X <= 28)
                {
                    ToggleMute(ch);
                }
            }
        }
    }

    public override void _Draw()
    {
        if (_movie == null) return;
        int channels = _collapsed ? 0 : 4;
        float height = channels * _gfxValues.ChannelHeight + (_collapsed ? 0 : 0);
        Size = new Vector2(_gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth, height + 10);
        DrawRect(new Rect2(0,0,Size.X,Size.Y), new Color("#f0f0f0"));

        var font = ThemeDB.FallbackFont;
        if (_collapsed) return;

        for (int c = 0; c < channels; c++)
        {
            float y = c * _gfxValues.ChannelHeight + 10;
            DrawLine(new Vector2(0, y), new Vector2(Size.X, y), Colors.DarkGray);
            DrawRect(new Rect2(0, y, _gfxValues.ChannelInfoWidth, _gfxValues.ChannelHeight), new Color("#f0f0f0"));
            string icon = _muted[c] ? "🔇" : "🔊";
            DrawString(font, new Vector2(4, y + font.GetAscent() - 6), icon, HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
            DrawString(font, new Vector2(_gfxValues.ChannelHeight + 2, y + font.GetAscent() - 6), $"{c + 1}", HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
        }

        foreach (var clip in _clips)
        {
            int ch = clip.Clip.Channel;
            if (ch < 0 || ch >= channels) continue;
            float x = _gfxValues.LeftMargin + (clip.Clip.BeginFrame -1) * _gfxValues.FrameWidth;
            float width = (clip.Clip.EndFrame - clip.Clip.BeginFrame +1) * _gfxValues.FrameWidth;
            float y = ch * _gfxValues.ChannelHeight + 10;
            clip.Draw(this, new Vector2(x,y), width, _gfxValues.ChannelHeight, font);
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
