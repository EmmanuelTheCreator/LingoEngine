using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Events;
using LingoEngine.Members;
using LingoEngine.Director.Core.Inputs;
using LingoEngine.Primitives;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreGrid : Control, IHasSpriteSelectedEvent
{
    private LingoMovie? _movie;

    

    private readonly List<DirGodotScoreSprite> _sprites = new();
    private DirGodotScoreSprite? _selected;
    private readonly IDirectorEventMediator _mediator;
    private readonly PopupMenu _contextMenu = new();
    private DirGodotScoreSprite? _contextSprite;
    private readonly DirGodotScoreGfxValues _gfxValues;

    private readonly SubViewport _gridViewport = new();
    private readonly SubViewport _spriteViewport = new();
    private readonly TextureRect _gridTexture = new();
    private readonly TextureRect _spriteTexture = new();
    private readonly DirGodotGridPainter _gridCanvas;
    private readonly SpriteCanvas _spriteCanvas;
    private readonly DirGodotScoreDragHandler _dragHandler;
    private bool _spriteDirty = true;
    private bool _spriteListDirty;
    private int _lastFrame = -1;
    internal bool SpriteListDirty { get  => _spriteListDirty; set => _spriteListDirty = value; }
    internal bool SpriteDirty { get => _spriteDirty; set => _spriteDirty = value; }
    internal Rect2? SpritePreviewRect => _dragHandler.SpritePreviewRect;
    internal bool ShowPreview => _dragHandler.ShowPreview;
    internal int PreviewChannel => _dragHandler.PreviewChannel;

    internal int PreviewBegin => _dragHandler.PreviewBegin;
    internal int PreviewEnd => _dragHandler.PreviewEnd;

    private readonly HashSet<Vector2I> _selectedCells = new();
    private Vector2I? _lastSelectedCell = null;


    public DirGodotScoreGrid(IDirectorEventMediator mediator, DirGodotScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        _mediator = mediator;
        AddChild(_contextMenu);
        _contextMenu.IdPressed += OnContextMenuItem;

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
        // Ensure textures draw above the window background
        //_gridTexture.ZIndex = 0;
        _gridTexture.MouseFilter = MouseFilterEnum.Ignore;
        
        MouseFilter = MouseFilterEnum.Stop;

        _spriteTexture.Texture = _spriteViewport.GetTexture();
        _spriteTexture.Position = Vector2.Zero;
        //_spriteTexture.ZIndex = 1;
        _spriteTexture.MouseFilter = MouseFilterEnum.Ignore;

        AddChild(_gridViewport);
        AddChild(_spriteViewport);
        AddChild(_gridTexture);
        AddChild(_spriteTexture);
        _dragHandler = new DirGodotScoreDragHandler(this,_movie, _gfxValues,_sprites);
    }

    public void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.SpriteListChanged -= OnSpritesChanged;

        _movie = movie;
        BuildSpriteList();
        _spriteListDirty = false;
        if (_movie != null)
        {
            _movie.SpriteListChanged += OnSpritesChanged;
            _gridCanvas.FrameCount = _movie.FrameCount;
            _gridCanvas.ChannelCount = _movie.MaxSpriteChannelCount;
            _dragHandler.SetMovie(_movie);
        }

        UpdateViewportSize();
        _spriteDirty = true;
    }

    private void OnSpritesChanged()
    {
        _spriteListDirty = true;
        _spriteDirty = true;
        RefreshSprites();
    }

    private void BuildSpriteList()
    {
        _sprites.Clear();
        if (_movie != null)
        {
            int idx = 1;
            while (_movie.TryGetAllTimeSprite(idx, out var sp))
            {
                _sprites.Add(new DirGodotScoreSprite((LingoSprite)sp));
                idx++;
            }
        }
    }

    private void RefreshSprites()
    {
        if (_spriteListDirty)
        {
            BuildSpriteList();
            UpdateViewportSize();
            _spriteListDirty = false;
            _spriteDirty = true;
        }
    }



    internal void SelectSprite(DirGodotScoreSprite? sprite, bool raiseEvent = true)
    {
        if (_selected == sprite) return;
        if (_selected != null) _selected.Selected = false;
        _selected = sprite;
        if (_selected != null)
        {
            _selected.Selected = true;
            if (raiseEvent)
                _mediator.RaiseSpriteSelected(_selected.Sprite);
        }
        _spriteDirty = true;
    }


    public override void _Input(InputEvent @event)
    {
        if (!Visible || _movie == null) return;

        if (@event is InputEventMouseButton mb)
            _dragHandler.HandleMouseButton(mb);
        else if (@event is InputEventMouseMotion)
            _dragHandler.HandleMouseMotion();
    }
    internal void SpriteCanvasQueueRedraw() => _spriteCanvas.QueueRedraw();


    internal void TryOpenContextMenu(Vector2 pos, int channel)
    {
        var sp = GetSpriteAt(pos, channel);
        if (sp == null || sp.Sprite.Member == null) return;

        _contextSprite = sp;
        _contextMenu.Clear();
        _contextMenu.AddItem("Find Cast Member", 1);
        var gp = GetGlobalMousePosition();
        _contextMenu.Popup(new Rect2I((int)gp.X, (int)gp.Y, 0, 0));
    }
    public void HandleSelection(Vector2I cell, bool ctrl, bool shift)
    {
        if (shift && _lastSelectedCell.HasValue)
        {
            SelectRange(_lastSelectedCell.Value, cell);
        }
        else if (ctrl)
        {
            if (!_selectedCells.Add(cell))
                _selectedCells.Remove(cell);
            _lastSelectedCell = cell;
        }
        else
        {
            _selectedCells.Clear();
            _selectedCells.Add(cell);
            _lastSelectedCell = cell;
        }

        _spriteCanvas.QueueRedraw();
    }
    private void SelectRange(Vector2I from, Vector2I to)
    {
        _selectedCells.Clear();
        int minX = Mathf.Min(from.X, to.X);
        int maxX = Mathf.Max(from.X, to.X);
        int minY = Mathf.Min(from.Y, to.Y);
        int maxY = Mathf.Max(from.Y, to.Y);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                _selectedCells.Add(new Vector2I(x, y));
    }
    public bool IsCellSelected(Vector2I cell) => _selectedCells.Contains(cell);

    public void ClearSelection()
    {
        _selectedCells.Clear();
        _lastSelectedCell = null;
        _spriteCanvas.QueueRedraw();
    }
    public Vector2I? GetCellFromPosition(Vector2 position)
    {
        if (_movie == null) return null;
        // Replace with your actual cell size logic
        const int CellWidth = 32;
        const int CellHeight = 32;

        int column = (int)(position.X / CellWidth);
        int row = (int)(position.Y / CellHeight);

        if (column < 0 || column >= _movie.MaxSpriteChannelCount || row < 0 || row >= _movie.FrameCount)
            return null;

        return new Vector2I(column, row);
    }


    #region Old

    //public override bool _CanDropData(Vector2 atPosition, Variant data)
    //{
    //    GD.Print($"Score: _CanDropData called at {atPosition} with {data.Obj}");
    //    _showPreview = false;
    //    if (_movie == null) return false;

    //    if (data.Obj is not ILingoMember member) return false;

    //    if (member.Type == LingoMemberType.Sound) return false;

    //    int channel = (int)(atPosition.Y / _gfxValues.ChannelHeight);
    //    if (channel < 0 || channel >= _movie.MaxSpriteChannelCount) return false;

    //    int start = Math.Clamp(Mathf.RoundToInt((atPosition.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1, 1, _movie.FrameCount);
    //    int end = _movie.GetNextLabelFrame(start) - 1;
    //    int nextSprite = _movie.GetNextSpriteStart(channel, start);
    //    if (nextSprite != -1)
    //        end = Math.Min(end, nextSprite - 1);
    //    if (_movie.GetPrevSpriteEnd(channel, start) >= start)
    //        return false;

    //    end = Math.Clamp(end, start, _movie.FrameCount);

    //    _previewChannel = channel;
    //    _previewBegin = start;
    //    _previewEnd = end;
    //    _previewMember = member;
    //    _showPreview = true;
    //    _spriteDirty = true;
    //    return true;
    //}

    //public override void _DropData(Vector2 atPosition, Variant data)
    //{
    //    GD.Print($"Score: _DropData called at {atPosition} with {_previewMember}");
    //    _showPreview = false;
    //    _spriteDirty = true;
    //    if (_movie == null) return;
    //    if (_previewMember == null) return;

    //    var sp = _movie.AddSprite(_previewChannel + 1, _previewBegin, _previewEnd, 0, 0, s =>
    //    {
    //        s.SetMember(_previewMember);
    //    });
    //    _previewMember = null;
    //    _spriteListDirty = true;
    //} 
    #endregion

    public override void _Process(double delta)
    {
        if (Visible)
            RefreshSprites();
        if (!Visible) return;
        int cur = _movie?.CurrentFrame ?? -1;
        if (_spriteDirty || cur != _lastFrame)
        {
            _spriteDirty = false;
            _lastFrame = cur;
            _spriteCanvas.QueueRedraw();
        }
    }

    public void SpriteSelected(ILingoSprite sprite)
    {
        var match = _sprites.FirstOrDefault(x => x.Sprite == sprite);
        SelectSprite(match, false);
    }

    private DirGodotScoreSprite? GetSpriteAt(Vector2 pos, int channel)
    {
        foreach (var sp in _sprites)
        {
            int sc = sp.Sprite.SpriteNum - 1;
            if (sc == channel)
            {
                float sx = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
                float ex = _gfxValues.LeftMargin + sp.Sprite.EndFrame * _gfxValues.FrameWidth;
                if (pos.X >= sx && pos.X <= ex)
                    return sp;
            }
        }
        return null;
    }

    private void OnContextMenuItem(long id)
    {
        if (id == 1 && _contextSprite?.Sprite.Member != null)
        {
            _mediator.RaiseFindMember(_contextSprite.Sprite.Member);
        }
        _contextSprite = null;
    }

    private void UpdateViewportSize()
    {
        if (_movie == null) return;

        float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float height = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

        Size = new Vector2(width, height);
        CustomMinimumSize = Size;

        _gridViewport.SetSize(new Vector2I((int)width, (int)height));
        _spriteViewport.SetSize(new Vector2I((int)width, (int)height));
        _gridTexture.CustomMinimumSize = new Vector2(width, height);
        _spriteTexture.CustomMinimumSize = new Vector2(width, height);
        _gridCanvas.FrameCount = _movie.FrameCount;
        _gridCanvas.ChannelCount = _movie.MaxSpriteChannelCount;
        _gridCanvas.QueueRedraw();
        _spriteCanvas.QueueRedraw();
    }

}

