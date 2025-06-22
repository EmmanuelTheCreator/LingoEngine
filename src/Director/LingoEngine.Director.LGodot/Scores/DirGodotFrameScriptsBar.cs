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
    private readonly DirGodotGridPainter _gridCanvas;
    private readonly SpriteCanvas _spriteCanvas;
    private bool _spriteDirty = true;
    private int _lastFrame = -1;
    private LingoSprite? _dragSprite;
    private int _dragFrame;

    public DirGodotFrameScriptsBar(DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;

        _gridViewport.SetDisable3D(true);
        _gridViewport.TransparentBg = true;
        _gridViewport.SetUpdateMode(SubViewport.UpdateMode.Always);
        _gridCanvas = new DirGodotGridPainter(_gfxValues);
        _gridViewport.AddChild(_gridCanvas);

        _spriteViewport.SetDisable3D(true);
        _spriteViewport.TransparentBg = true;
        _spriteViewport.SetUpdateMode(SubViewport.UpdateMode.Always);
        _spriteCanvas = new SpriteCanvas(this);
        _spriteViewport.AddChild(_spriteCanvas);

        _gridTexture.Texture = _gridViewport.GetTexture();
        _gridTexture.Position = Vector2.Zero;
        _gridTexture.MouseFilter = MouseFilterEnum.Ignore;

        _spriteTexture.Texture = _spriteViewport.GetTexture();
        _spriteTexture.Position = Vector2.Zero;
        _spriteTexture.MouseFilter = MouseFilterEnum.Ignore;

        AddChild(_gridViewport);
        AddChild(_spriteViewport);
        AddChild(_gridTexture);
        AddChild(_spriteTexture);
    }

    public void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.SpriteListChanged -= OnSpritesChanged;

        _movie = movie;
        _sprites.Clear();
        if (_movie != null)
        {
            foreach (var kv in _movie.GetFrameSpriteBehaviors())
                _sprites.Add(new DirGodotScoreSprite(kv.Value, false, true));
            _movie.SpriteListChanged += OnSpritesChanged;
            _gridCanvas.FrameCount = _movie.FrameCount;
            _gridCanvas.ChannelCount = 1;
        }
        UpdateViewportSize();
        _spriteDirty = true;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (_movie == null) return;

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                Vector2 pos = GetLocalMousePosition();
                foreach (var sp in _sprites)
                {
                    float sx = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
                    float ex = sx + _gfxValues.FrameWidth;
                    if (pos.X >= sx && pos.X <= ex)
                    {
                        _dragSprite = sp.Sprite;
                        _dragFrame = sp.Sprite.BeginFrame;
                        break;
                    }
                }
            }
            else if (_dragSprite != null)
            {
                _dragSprite = null;
            }
        }
        else if (@event is InputEventMouseMotion && _dragSprite != null)
        {
            float frameF = (GetLocalMousePosition().X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
            int newFrame = Math.Clamp(Mathf.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
            if (newFrame != _dragFrame)
            {
                _movie.MoveFrameBehavior(_dragFrame, newFrame);
                _dragFrame = newFrame;
                _spriteDirty = true;
            }
        }
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
        _gridCanvas.FrameCount = _movie.FrameCount;
        _gridCanvas.ChannelCount = 1;
        _gridCanvas.QueueRedraw();
        _spriteCanvas.QueueRedraw();
    }

    private void OnSpritesChanged()
    {
        _sprites.Clear();
        if (_movie != null)
        {
            foreach (var kv in _movie.GetFrameSpriteBehaviors())
                _sprites.Add(new DirGodotScoreSprite(kv.Value, false, true));
            UpdateViewportSize();
        }
        _spriteDirty = true;
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

    protected override void Dispose(bool disposing)
    {
        if (_movie != null)
            _movie.SpriteListChanged -= OnSpritesChanged;
        base.Dispose(disposing);
    }
}
