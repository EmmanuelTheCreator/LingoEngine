using Godot;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Members;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotSoundBar : Control
{
    private LingoMovie? _movie;
    private readonly List<DirGodotScoreAudioClip> _clips = new();
    private readonly DirGodotScoreGfxValues _gfxValues;
    private bool _dirty = true;
    private bool _collapsed;

    public DirGodotSoundBar(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
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
        if (data.Obj is not GodotObject obj) return false;
        if (obj is not LingoMemberSound) return false;
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (_movie == null || _collapsed) return;
        if (data.Obj is not GodotObject obj) return;
        if (obj is not LingoMemberSound sound) return;
        int channel = (int)(atPosition.Y / _gfxValues.ChannelHeight);
        int frame = Mathf.Clamp(Mathf.RoundToInt((atPosition.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1, 1, _movie.FrameCount);
        _movie.AddAudioClip(channel, frame, sound);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Position.Y < 10 && mb.Position.X < 10)
            {
                _collapsed = !_collapsed;
                QueueRedraw();
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
        DrawString(font, new Vector2(2, font.GetAscent()-2), (_collapsed ? "▶" : "▼"));
        if (_collapsed) return;

        for (int c = 0; c < channels; c++)
        {
            float y = c * _gfxValues.ChannelHeight + 10;
            DrawLine(new Vector2(0,y), new Vector2(Size.X,y), Colors.DarkGray);
            DrawString(font, new Vector2(12, y + font.GetAscent()-4), $"🔊{c+1}");
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
}
