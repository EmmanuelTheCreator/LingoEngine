using Godot;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Stages;
using System;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sprites;
using LingoEngine.Commands;

namespace LingoEngine.Director.LGodot.Scores
{
    internal sealed class DirGodotScoreDragHandler
    {
        private LingoMovie _movie;
        private readonly DirGodotScoreGrid _grid;
        private readonly ILingoCommandManager _commandManager;
        private bool _showPreview;
        private int _previewChannel;
        
        private bool _isDraggingSprite;
        private bool _dragStarted;
        private Vector2 _dragStartPos;
        private Rect2? _spritePreviewRect;
        private int _previewBeginFrame = -1;
        private ILingoSprite? _dragSprite;
        private bool _dragBegin;
        private bool _dragEnd;
        private bool _isSpriteDragMove = false;
        private float _dragOffset = 0;
        private float _dragStartY = 0;
        private const float DragThresholdSq = 16f;
        private int _previewBegin;
        private int _previewEnd;

        private int _origChannel;
        private int _origBegin;
        private int _origEnd;

        private ILingoMember? _previewMember;
        private readonly DirScoreGfxValues _gfxValues;
        private readonly List<DirGodotScoreSprite> _sprites;

        internal Rect2? SpritePreviewRect => _spritePreviewRect;
        internal bool ShowPreview => _showPreview;
        internal int PreviewChannel => _previewChannel;
        internal int PreviewBegin => _previewBegin;
        internal int PreviewEnd => _previewEnd;


        public DirGodotScoreDragHandler(DirGodotScoreGrid grid, LingoMovie movie, DirScoreGfxValues gfxValues, List<DirGodotScoreSprite> sprites, ILingoCommandManager commandManager)
        {
            _movie = movie;
            _grid = grid;
            _gfxValues = gfxValues;
            _sprites = sprites;
            _commandManager = commandManager;
        }

        public void SetMovie(LingoMovie movie)
        {
            _movie = movie;
        }

        internal void HandleMouseButton(InputEventMouseButton mb)
        {
            Vector2 pos = _grid.GetLocalMousePosition();
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
                _grid.TryOpenContextMenu(pos, channel);
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
                    _origChannel = sc;
                    _origBegin = _dragSprite.BeginFrame;
                    _origEnd = _dragSprite.EndFrame;
                    _dragBegin = true;
                    _dragEnd = false;
                    Input.SetDefaultCursorShape(Input.CursorShape.Hsize);
                }
                else if (pos.X >= lastFrameLeft)
                {
                    _dragSprite = sp.Sprite;
                    _origChannel = sc;
                    _origBegin = _dragSprite.BeginFrame;
                    _origEnd = _dragSprite.EndFrame;
                    _dragBegin = false;
                    _dragEnd = true;
                    Input.SetDefaultCursorShape(Input.CursorShape.Hsize);
                }
                else
                {
                    // Center drag = move to another channel
                    _grid.SelectSprite(sp);
                    _dragSprite = sp.Sprite;
                    _origChannel = sc;
                    _origBegin = _dragSprite.BeginFrame;
                    _origEnd = _dragSprite.EndFrame;
                    _dragBegin = false;
                    _dragEnd = false;
                    _isSpriteDragMove = true;
                    _isDraggingSprite = false;
                    _dragStartPos = pos;
                    _dragStartY = pos.Y;
                    _dragStarted = false;
                    Input.SetDefaultCursorShape(Input.CursorShape.Drag);
                    _grid.SpriteDirty = true;
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
                if (originalChannel != newChannel || _origBegin != newBegin || _origEnd != newEnd)
                    _commandManager.Handle(new ChangeSpriteRangeCommand(_movie, (LingoSprite)_dragSprite, _origChannel, _origBegin, _origEnd, newChannel, newBegin, newEnd));
            }
            else if ((_dragBegin || _dragEnd) && _dragSprite != null && _movie != null)
            {
                int newChannel = _dragSprite.SpriteNum - 1;
                int newBegin = _dragSprite.BeginFrame;
                int newEnd = _dragSprite.EndFrame;
                if (_origChannel != newChannel || _origBegin != newBegin || _origEnd != newEnd)
                    _commandManager.Handle(new ChangeSpriteRangeCommand(_movie, (LingoSprite)_dragSprite, _origChannel, _origBegin, _origEnd, newChannel, newBegin, newEnd));
            }

            _dragSprite = null;
            _dragBegin = _dragEnd = false;
            _isSpriteDragMove = false;
            _isDraggingSprite = false;
            _dragStarted = false;
            _spritePreviewRect = null;
            _grid.SpriteDirty = true;
            Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        }

        private void TryDropExternalCastMember(Vector2 pos)
        {
            if (!DirectorDragDropHolder.IsDragging || DirectorDragDropHolder.Member == null) return;

            if (DirectorDragDropUtils.TryHandleMemberDrop(
                _movie!,
                new LingoPoint(pos.X, pos.Y),
                _gfxValues.ChannelHeight,
                _gfxValues.FrameWidth,
                _gfxValues.LeftMargin,
                0,
                out int channel, out int begin, out int end, out var member))
            {
                // Convert channel from zero-based to one-based when creating the command
                _commandManager.Handle(new AddSpriteCommand(_movie!, member!, channel + 1, begin, end));
            }

            DirectorDragDropHolder.EndDrag();
        }
       
        internal void HandleMouseMotion()
        {
            if (_dragSprite != null)
            {
                if (_isSpriteDragMove)
                {
                    if (!_dragStarted)
                    {
                        var pos = _grid.GetLocalMousePosition();
                        if (pos.DistanceSquaredTo(_dragStartPos) > DragThresholdSq)
                        {
                            _dragStarted = true;
                            _isDraggingSprite = true;
                        }
                    }
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

            if (DirectorDragDropHolder.IsDragging && DirectorDragDropHolder.Member != null)
            {
                var pos = _grid.GetLocalMousePosition();
                if (DirectorDragDropUtils.TryHandleMemberDrop(
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
                    _grid.SpriteDirty = true;
                }
                else
                {
                    _showPreview = false;
                    _grid.SpriteDirty = true;
                }
            }
        }
        private void TryResizeSprite()
        {
            float frame = (_grid.GetLocalMousePosition().X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
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

            _grid.SpriteDirty = true;
        }

        private void UpdateSpriteDragPreview()
        {
            var pos = _grid.GetLocalMousePosition();
            int newChannel = (int)(pos.Y / _gfxValues.ChannelHeight);
            int snapX = (int)((pos.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1;

            if (_dragSprite == null || _movie == null)
            {
                _spritePreviewRect = null;
                _grid.SpriteCanvasQueueRedraw();
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
                _grid.SpriteCanvasQueueRedraw();
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
                _grid.SpriteCanvasQueueRedraw();
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

            _grid.SpriteCanvasQueueRedraw();
        }






        private static bool RangesOverlap(int aStart, int aEnd, int bStart, int bEnd)
        {
            return aStart <= bEnd && bStart <= aEnd;
        }

    }
}
