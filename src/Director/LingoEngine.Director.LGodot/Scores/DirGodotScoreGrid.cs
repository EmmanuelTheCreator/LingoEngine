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

    private ILingoSprite? _dragSprite;
    private bool _dragBegin;
    private bool _dragEnd;
    private bool _isSpriteDragMove = false;
    private float _dragOffset = 0;
    private float _dragStartY = 0;

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
    private bool _spriteDirty = true;
    private bool _spriteListDirty;
    private int _lastFrame = -1;
    private bool _showPreview;
    private int _previewChannel;
    private int _previewBegin;
    private int _previewEnd;
    private bool _isDraggingSprite;
    private Rect2? _spritePreviewRect;
    private int _previewBeginFrame = -1;

    private ILingoMember? _previewMember;
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



    private void SelectSprite(DirGodotScoreSprite? sprite, bool raiseEvent = true)
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
        {
            HandleMouseButton(mb);
        }
        else if (@event is InputEventMouseMotion)
        {
            HandleMouseMotion();
        }
    }
    private void HandleMouseButton(InputEventMouseButton mb)
    {
        Vector2 pos = GetLocalMousePosition();
        int totalChannels = _movie!.MaxSpriteChannelCount;
        int channel = (int)(pos.Y / _gfxValues.ChannelHeight);

        if (mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                TryBeginInternalDrag(pos, channel);
            }
            else
            {
                FinishInternalDrag();
                TryDropExternalCastMember(pos);
                _showPreview = false;
            }
        }
        else if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
        {
            TryOpenContextMenu(pos, channel);
        }
    }
    private void TryBeginInternalDrag(Vector2 pos, int channel)
    {
        if (channel < 0 || channel >= _movie!.MaxSpriteChannelCount)
            return;

        foreach (var sp in _sprites)
        {
            int sc = sp.Sprite.SpriteNum - 1;
            if (sc != channel) continue;

            float sx = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
            float ex = _gfxValues.LeftMargin + sp.Sprite.EndFrame * _gfxValues.FrameWidth;

            if (pos.X < sx || pos.X > ex) continue;

            float firstFrameRight = sx + _gfxValues.FrameWidth;
            float lastFrameLeft = ex - _gfxValues.FrameWidth;

            if (pos.X <= firstFrameRight)
            {
                _dragSprite = sp.Sprite;
                _dragBegin = true;
                _dragEnd = false;
                Input.SetDefaultCursorShape(Input.CursorShape.Hsize);
            }
            else if (pos.X >= lastFrameLeft)
            {
                _dragSprite = sp.Sprite;
                _dragBegin = false;
                _dragEnd = true;
                Input.SetDefaultCursorShape(Input.CursorShape.Hsize);
            }
            else
            {
                // Center drag = move to another channel
                SelectSprite(sp);
                _dragSprite = sp.Sprite;
                _dragBegin = false;
                _dragEnd = false;
                _isSpriteDragMove = true;
                _isDraggingSprite = true;
                _dragStartY = pos.Y;
                Input.SetDefaultCursorShape(Input.CursorShape.Drag);
                _spriteDirty = true;
            }
            break;
        }
    }

    private void FinishInternalDrag()
    {
        if (_isSpriteDragMove && _dragSprite != null && _movie != null && _spritePreviewRect != null)
        {
            int originalChannel = _dragSprite.SpriteNum - 1;
            int newChannel = _previewChannel;
            int newBegin = _previewBeginFrame;
            int length = _dragSprite.EndFrame - _dragSprite.BeginFrame + 1;
            int newEnd = newBegin + length - 1;

            if (originalChannel != newChannel)
                _movie.ChangeSpriteChannel(_dragSprite, newChannel);
            if (newBegin != _dragSprite.BeginFrame)
            {
                _dragSprite.BeginFrame = newBegin;
                _dragSprite.EndFrame = newEnd;
            }
        }

        _dragSprite = null;
        _dragBegin = _dragEnd = false;
        _isSpriteDragMove = false;
        _isDraggingSprite = false;
        _spritePreviewRect = null;
        _spriteDirty = true;
        Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
    }

    private void TryDropExternalCastMember(Vector2 pos)
    {
        if (!DirDragDropHolder.IsDragging || DirDragDropHolder.Member == null) return;

        if (DirDragDropUtils.TryHandleMemberDrop(
            _movie!,
            new LingoPoint(pos.X, pos.Y),
            _gfxValues.ChannelHeight,
            _gfxValues.FrameWidth,
            _gfxValues.LeftMargin,
            0,
            out int channel, out int begin, out int end, out var member))
        {
            _movie.AddSprite(channel + 1, begin, end, 0, 0, s => s.SetMember(member!));
            _spriteListDirty = true;
            _spriteDirty = true;
        }

        DirDragDropHolder.EndDrag();
    }
    private void TryOpenContextMenu(Vector2 pos, int channel)
    {
        var sp = GetSpriteAt(pos, channel);
        if (sp == null || sp.Sprite.Member == null) return;

        _contextSprite = sp;
        _contextMenu.Clear();
        _contextMenu.AddItem("Find Cast Member", 1);
        var gp = GetGlobalMousePosition();
        _contextMenu.Popup(new Rect2I((int)gp.X, (int)gp.Y, 0, 0));
    }
    private void HandleMouseMotion()
    {
        if (_dragSprite != null)
        {
            if (_isSpriteDragMove)
            {
                if (_isDraggingSprite)
                {
                    UpdateSpriteDragPreview();
                }
            }
            else
            {
                TryResizeSprite();
            }
            return;
        }

        if (DirDragDropHolder.IsDragging && DirDragDropHolder.Member != null)
        {
            var pos = GetLocalMousePosition();
            if (DirDragDropUtils.TryHandleMemberDrop(
                _movie!,
                new LingoPoint(pos.X, pos.Y),
                _gfxValues.ChannelHeight,
                _gfxValues.FrameWidth,
                _gfxValues.LeftMargin,
                0,
                out int channel, out int begin, out int end, out var member))
            {
                _previewChannel = channel;
                _previewBegin = begin;
                _previewEnd = end;
                _previewMember = member;
                _showPreview = true;
                _spriteDirty = true;
            }
            else
            {
                _showPreview = false;
                _spriteDirty = true;
            }
        }
    }
    private void TryResizeSprite()
    {
        float frame = (GetLocalMousePosition().X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
        int newFrame = Math.Clamp((int)(frame + 1), 1, _movie!.FrameCount);
        int ch = _dragSprite!.SpriteNum - 1;

        if (_dragBegin)
        {
            int minBegin = _movie.GetPrevSpriteEnd(ch, _dragSprite.BeginFrame) + 1;
            _dragSprite.BeginFrame = Math.Min(Math.Max(newFrame, minBegin), _dragSprite.EndFrame);
        }
        else if (_dragEnd)
        {
            int next = _movie.GetNextSpriteStart(ch, _dragSprite.BeginFrame);
            int maxEnd = next == -1 ? _movie.FrameCount : next - 1;
            _dragSprite.EndFrame = Math.Max(Math.Min(newFrame, maxEnd), _dragSprite.BeginFrame);
        }

        _spriteDirty = true;
    }

    private void UpdateSpriteDragPreview()
    {
        var pos = GetLocalMousePosition();
        int newChannel = (int)(pos.Y / _gfxValues.ChannelHeight);
        int snapX = (int)((pos.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1;

        if (_dragSprite == null || _movie == null)
        {
            _spritePreviewRect = null;
            _spriteCanvas.QueueRedraw();
            return;
        }

        int originalChannel = _dragSprite.SpriteNum - 1;
        int originalBegin = _dragSprite.BeginFrame;
        int length = _dragSprite.EndFrame - originalBegin + 1;

        int newBegin = snapX;
        int newEnd = newBegin + length - 1;

        if (newChannel < 0 || newChannel >= _movie.MaxSpriteChannelCount ||
            newBegin < 1 || newEnd > _movie.FrameCount)
        {
            _spritePreviewRect = null;
            _spriteCanvas.QueueRedraw();
            return;
        }

        // Check for conflicts
        bool conflict = false;
        int idx = 1;
        while (_movie.TryGetAllTimeSprite(idx, out var other))
        {
            if (other != _dragSprite && other.SpriteNum - 1 == newChannel &&
                RangesOverlap(newBegin, newEnd, other.BeginFrame, other.EndFrame))
            {
                conflict = true;
                break;
            }
            idx++;
        }

        if (conflict)
        {
            _spritePreviewRect = null;
            _spriteCanvas.QueueRedraw();
            return;
        }

        _previewChannel = newChannel;
        _previewBeginFrame = newBegin;

        _spritePreviewRect = new Rect2(
            _gfxValues.LeftMargin + (newBegin - 1) * _gfxValues.FrameWidth,
            newChannel * _gfxValues.ChannelHeight,
            length * _gfxValues.FrameWidth,
            _gfxValues.ChannelHeight
        );

        _spriteCanvas.QueueRedraw();
    }






    private static bool RangesOverlap(int aStart, int aEnd, int bStart, int bEnd)
    {
        return aStart <= bEnd && bStart <= aEnd;
    }



    #region Old

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        GD.Print($"Score: _CanDropData called at {atPosition} with {data.Obj}");
        _showPreview = false;
        if (_movie == null) return false;

        if (data.Obj is not ILingoMember member) return false;

        if (member.Type == LingoMemberType.Sound) return false;

        int channel = (int)(atPosition.Y / _gfxValues.ChannelHeight);
        if (channel < 0 || channel >= _movie.MaxSpriteChannelCount) return false;

        int start = Math.Clamp(Mathf.RoundToInt((atPosition.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1, 1, _movie.FrameCount);
        int end = _movie.GetNextLabelFrame(start) - 1;
        int nextSprite = _movie.GetNextSpriteStart(channel, start);
        if (nextSprite != -1)
            end = Math.Min(end, nextSprite - 1);
        if (_movie.GetPrevSpriteEnd(channel, start) >= start)
            return false;

        end = Math.Clamp(end, start, _movie.FrameCount);

        _previewChannel = channel;
        _previewBegin = start;
        _previewEnd = end;
        _previewMember = member;
        _showPreview = true;
        _spriteDirty = true;
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        GD.Print($"Score: _DropData called at {atPosition} with {_previewMember}");
        _showPreview = false;
        _spriteDirty = true;
        if (_movie == null) return;
        if (_previewMember == null) return;

        var sp = _movie.AddSprite(_previewChannel + 1, _previewBegin, _previewEnd, 0, 0, s =>
        {
            s.SetMember(_previewMember);
        });
        _previewMember = null;
        _spriteListDirty = true;
    } 
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

