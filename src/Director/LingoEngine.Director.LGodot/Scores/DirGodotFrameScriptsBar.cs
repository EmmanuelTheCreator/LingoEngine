using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotFrameScriptsBar : Control
{
    private LingoMovie? _movie;
    private readonly List<DirGodotScoreSprite> _sprites = new();
    private readonly DirGodotScoreGfxValues _gfxValues;

    private readonly SubViewport _gridViewport = new();
    private readonly SubViewport _spriteViewport = new();
    private readonly TextureRect _gridTexture = new();
    private readonly TextureRect _spriteTexture = new();
    private readonly GridCanvas _gridCanvas;
    private readonly SpriteCanvas _spriteCanvas;
    private bool _spriteDirty = true;
    private int _lastFrame = -1;

    public DirGodotFrameScriptsBar(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;

        _gridViewport.SetDisable3D(true);
        _gridViewport.TransparentBg = true;
        _gridViewport.SetUpdateMode(SubViewport.UpdateMode.Always);
        _gridCanvas = new GridCanvas(this);
        _gridViewport.AddChild(_gridCanvas);

        _spriteViewport.SetDisable3D(true);
        _spriteViewport.TransparentBg = true;
        _spriteViewport.SetUpdateMode(SubViewport.UpdateMode.Always);
        _spriteCanvas = new SpriteCanvas(this);
        _spriteViewport.AddChild(_spriteCanvas);

        _gridTexture.Texture = _gridViewport.GetTexture();
        _gridTexture.MouseFilter = MouseFilterEnum.Ignore;

        _spriteTexture.Texture = _spriteViewport.GetTexture();
        _spriteTexture.MouseFilter = MouseFilterEnum.Ignore;

        AddChild(_gridViewport);
        AddChild(_spriteViewport);
        AddChild(_gridTexture);
        AddChild(_spriteTexture);
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        _sprites.Clear();
        if (_movie == null) return;
        foreach (var kv in _movie.GetFrameSpriteBehaviors())
            _sprites.Add(new DirGodotScoreSprite(kv.Value, false, true));
        UpdateViewportSize();
        _spriteDirty = true;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        int cur = _movie?.CurrentFrame ?? -1;
        if (_spriteDirty || cur != _lastFrame)
        {
            _spriteDirty = false;
            _lastFrame = cur;
            _spriteCanvas.QueueRedraw();
        }
    }

    private void UpdateViewportSize()
    {
        if (_movie == null) return;
        float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
        Size = new Vector2(width, 20);
        CustomMinimumSize = Size;
        _gridViewport.SetSize(new Vector2I((int)width, 20));
        _spriteViewport.SetSize(new Vector2I((int)width, 20));
        _gridTexture.CustomMinimumSize = new Vector2(width, 20);
        _spriteTexture.CustomMinimumSize = new Vector2(width, 20);
        _gridCanvas.QueueRedraw();
        _spriteCanvas.QueueRedraw();
    }

    private partial class GridCanvas : Control
    {
        private readonly DirGodotFrameScriptsBar _owner;
        public GridCanvas(DirGodotFrameScriptsBar owner) => _owner = owner;
        public override void _Draw()
        {
            var movie = _owner._movie;
            if (movie == null) return;
            int frameCount = movie.FrameCount;
            DrawRect(new Rect2(0, 0, _owner.Size.X, 20), new Color("#f0f0f0"));
            for (int f = 0; f < frameCount; f++)
            {
                float x = _owner._gfxValues.LeftMargin + f * _owner._gfxValues.FrameWidth;
                if (f % 5 == 0)
                    DrawRect(new Rect2(x, 0, _owner._gfxValues.FrameWidth, _owner._gfxValues.ChannelHeight), Colors.DarkGray);
            }
            for (int f = 0; f <= frameCount; f++)
            {
                float x = _owner._gfxValues.LeftMargin + f * _owner._gfxValues.FrameWidth;
                DrawLine(new Vector2(x, 0), new Vector2(x, _owner._gfxValues.ChannelHeight), Colors.DarkGray);
            }
            DrawLine(new Vector2(0, 0), new Vector2(_owner._gfxValues.LeftMargin + frameCount * _owner._gfxValues.FrameWidth, 0), Colors.DarkGray);
            DrawLine(new Vector2(0, _owner._gfxValues.ChannelHeight), new Vector2(_owner._gfxValues.LeftMargin + frameCount * _owner._gfxValues.FrameWidth, _owner._gfxValues.ChannelHeight), Colors.DarkGray);
        }
    }

    private partial class SpriteCanvas : Control
    {
        private readonly DirGodotFrameScriptsBar _owner;
        public SpriteCanvas(DirGodotFrameScriptsBar owner) => _owner = owner;
        public override void _Draw()
        {
            var movie = _owner._movie;
            if (movie == null) return;
            var font = ThemeDB.FallbackFont;
            foreach (var sp in _owner._sprites)
            {
                float x = _owner._gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _owner._gfxValues.FrameWidth;
                float width = (sp.Sprite.EndFrame - sp.Sprite.BeginFrame + 1) * _owner._gfxValues.FrameWidth;
                sp.Draw(this, new Vector2(x, 0), width, _owner._gfxValues.ChannelHeight, font);
            }

            int cur = movie.CurrentFrame - 1;
            if (cur < 0) cur = 0;
            float barX = _owner._gfxValues.LeftMargin + cur * _owner._gfxValues.FrameWidth + _owner._gfxValues.FrameWidth / 2f;
            DrawLine(new Vector2(barX, 0), new Vector2(barX, _owner._gfxValues.ChannelHeight), Colors.Red, 2);
        }
    }
}
