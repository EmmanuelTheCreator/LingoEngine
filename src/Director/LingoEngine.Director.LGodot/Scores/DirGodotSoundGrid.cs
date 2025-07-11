using Godot;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Members;
using LingoEngine.Sounds;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Draws the scrollable area with audio clips and grid lines.
/// </summary>
internal partial class DirGodotSoundGrid : Control
{
    private LingoMovie? _movie;
    private readonly List<DirGodotScoreAudioClip> _clips = new();
    private readonly DirScoreGfxValues _gfxValues;
    private bool _collapsed;
    private bool _clipDirty = true;
    private float _scrollX;

    private readonly DirScoreGridPainter _gridCanvas;
    private readonly ILingoFrameworkFactory _factory;
    private readonly ClipCanvas _canvas;

    public DirGodotSoundGrid(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory)
    {
        _gfxValues = gfxValues;
        _factory = factory;
        _gridCanvas = new DirScoreGridPainter(factory, gfxValues);
        _canvas = new ClipCanvas(this);
        AddChild(_gridCanvas.Canvas.Framework<LingoGodotGfxCanvas>());
        AddChild(_canvas);
    }

    public bool Collapsed
    {
        get => _collapsed;
        set
        {
            _collapsed = value;
            _clipDirty = true;
            UpdateSize();
            QueueRedraw();
        }
    }

    public float ScrollX
    {
        get => _scrollX;
        set
        {
            _scrollX = value;
            _canvas.QueueRedraw();
            _gridCanvas.ScrollX = _scrollX;
            //_gridCanvas.Draw();
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
            _gridCanvas.FrameCount = _movie.FrameCount;
            _gridCanvas.ChannelCount = 4;
            _gridCanvas.Draw();
        }
        UpdateSize();
        _clipDirty = true;
        QueueRedraw();
    }

    private void OnClipsChanged()
    {
        _clips.Clear();
        if (_movie != null)
            foreach (var clip in _movie.GetAudioClips())
                _clips.Add(new DirGodotScoreAudioClip(clip));
        _clipDirty = true;
        QueueRedraw();
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (_movie == null || _collapsed) return false;

        var obj = data.Obj as LingoMemberSound;
        if (obj == null) return false;

        int channel = (int)(atPosition.Y / _gfxValues.ChannelHeight);
        if (channel < 0 || channel >= 4) return false;

        float frameX = atPosition.X + _scrollX;
        if (frameX < 0) return false;

        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (_movie == null || _collapsed) return;

        var sound = data.Obj as LingoMemberSound;
        if (sound == null) return;

        int channel = (int)(atPosition.Y / _gfxValues.ChannelHeight);
        float frameX = atPosition.X + _scrollX;
        int frame = Mathf.Clamp(Mathf.RoundToInt(frameX / _gfxValues.FrameWidth) + 1, 1, _movie.FrameCount);
        _movie.AddAudioClip(channel, frame, sound);
    }

    public override void _Process(double delta)
    {
        if (_clipDirty)
        {
            _clipDirty = false;
            _canvas.QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_movie == null || _collapsed) return;
        Size = CustomMinimumSize;
    }

    private void UpdateSize()
    {
        if (_movie == null) return;
        float width = _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float height = (_collapsed ? 0 : 4 * _gfxValues.ChannelHeight);
        CustomMinimumSize = new Vector2(width, height);
        _canvas.CustomMinimumSize = CustomMinimumSize;
        _gridCanvas.Canvas.Width = width;
        _gridCanvas.Canvas.Height = height;
        _gridCanvas.FrameCount = _movie.FrameCount;
        _gridCanvas.ChannelCount = 4;
        _gridCanvas.Draw();
    }

    private partial class ClipCanvas : Control
    {
        private readonly DirGodotSoundGrid _owner;
        public ClipCanvas(DirGodotSoundGrid owner) => _owner = owner;
        public override void _Draw()
        {
            var movie = _owner._movie;
            if (movie == null || _owner._collapsed) return;
            var font = ThemeDB.FallbackFont;
            foreach (var clip in _owner._clips)
            {
                int ch = clip.Clip.Channel;
                float x = -_owner._scrollX + (clip.Clip.BeginFrame - 1) * _owner._gfxValues.FrameWidth;
                float width = (clip.Clip.EndFrame - clip.Clip.BeginFrame + 1) * _owner._gfxValues.FrameWidth;
                float y = ch * _owner._gfxValues.ChannelHeight;
                clip.Draw(this, new Vector2(x, y), width, _owner._gfxValues.ChannelHeight, font);
            }
        }
    }
}
