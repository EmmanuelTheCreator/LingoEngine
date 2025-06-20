﻿using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Events;
using LingoEngine.Members;
using System;
using System.Linq;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreGrid : Control, IHasSpriteSelectedEvent
{
    private LingoMovie? _movie;

    private ILingoSprite? _dragSprite;
    private bool _dragBegin;
    private bool _dragEnd;
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
    private readonly GridCanvas _gridCanvas;
    private readonly SpriteCanvas _spriteCanvas;
    private bool _spriteDirty = true;
    private bool _spriteListDirty;
    private int _lastFrame = -1;
    private bool _showPreview;
    private int _previewChannel;
    private int _previewBegin;
    private int _previewEnd;
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
        _gridCanvas = new GridCanvas(this);
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
            _movie.SpriteListChanged += OnSpritesChanged;

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
            Vector2 pos = GetLocalMousePosition();
            int totalChannels = _movie.MaxSpriteChannelCount;
            int channel = (int)(pos.Y / _gfxValues.ChannelHeight);
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    if (channel >= 0 && channel < totalChannels)
                    {
                        foreach (var sp in _sprites)
                        {
                            int sc = sp.Sprite.SpriteNum - 1;
                            if (sc == channel)
                            {
                                float sx = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
                                float ex = _gfxValues.LeftMargin + sp.Sprite.EndFrame * _gfxValues.FrameWidth;
                                if (pos.X >= sx && pos.X <= ex)
                                {
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
                                        SelectSprite(sp);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _dragSprite = null;
                    _dragBegin = _dragEnd = false;
                    _spriteDirty = true;
                    Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
                }
            }
            else if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
            {
                if (channel >= 0 && channel < totalChannels)
                {
                    var sp = GetSpriteAt(pos, channel);
                    if (sp != null && sp.Sprite.Member != null)
                    {
                        _contextSprite = sp;
                        _contextMenu.Clear();
                        _contextMenu.AddItem("Find Cast Member", 1);
                        var gp = GetGlobalMousePosition();
                        _contextMenu.Popup(new Rect2I((int)gp.X, (int)gp.Y, 0, 0));
                    }
                }
            }
        }
        else if (@event is InputEventMouseMotion motion && _dragSprite != null)
        {
            float frame = (GetLocalMousePosition().X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
            int newFrame = Math.Clamp(Mathf.RoundToInt(frame) + 1, 1, _movie.FrameCount);
            int ch = _dragSprite.SpriteNum - 1;
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
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        _showPreview = false;
        if (_movie == null) return false;

        if (data.Obj is not GodotObject obj) return false;
        if (obj is not ILingoMember member) return false;

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
        _showPreview = false;
        _spriteDirty = true;
        if (_movie == null) return;
        if (_previewMember == null) return;

        var sp = _movie.AddSprite(_previewChannel + 1, _previewBegin, _previewEnd, 0, 0, s =>
        {
            s.SetMember(_previewMember);
        });
        _spriteListDirty = true;
    }

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

        _gridCanvas.QueueRedraw();
        _spriteCanvas.QueueRedraw();
    }

    private partial class GridCanvas : Control
    {
        private readonly DirGodotScoreGrid _owner;
        public GridCanvas(DirGodotScoreGrid owner) => _owner = owner;
        public override void _Draw()
        {
            var movie = _owner._movie;
            if (movie == null) return;
            int channelCount = movie.MaxSpriteChannelCount;
            int frameCount = movie.FrameCount;

            DrawRect(new Rect2(0, 0, _owner.Size.X, _owner.Size.Y), Colors.White);

            for (int f = 0; f < frameCount; f++)
            {
                float x = _owner._gfxValues.LeftMargin + f * _owner._gfxValues.FrameWidth;
                if (f % 5 == 0)
                    DrawRect(new Rect2(x, 0, _owner._gfxValues.FrameWidth, channelCount * _owner._gfxValues.ChannelHeight), Colors.DarkGray);
            }

            for (int f = 0; f <= frameCount; f++)
            {
                float x = _owner._gfxValues.LeftMargin + f * _owner._gfxValues.FrameWidth;
                DrawLine(new Vector2(x, 0), new Vector2(x, channelCount * _owner._gfxValues.ChannelHeight), Colors.DarkGray);
            }
        }
    }

    private partial class SpriteCanvas : Control
    {
        private readonly DirGodotScoreGrid _owner;
        public SpriteCanvas(DirGodotScoreGrid owner) => _owner = owner;
        public override void _Draw()
        {
            var movie = _owner._movie;
            if (movie == null) return;

            int channelCount = movie.MaxSpriteChannelCount;
            var font = ThemeDB.FallbackFont;

            foreach (var sp in _owner._sprites)
            {
                int ch = sp.Sprite.SpriteNum - 1;
                if (ch < 0 || ch >= channelCount) continue;
                float x = _owner._gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _owner._gfxValues.FrameWidth;
                float width = (sp.Sprite.EndFrame - sp.Sprite.BeginFrame + 1) * _owner._gfxValues.FrameWidth;
                float y = ch * _owner._gfxValues.ChannelHeight;
                sp.Draw(this, new Vector2(x, y), width, _owner._gfxValues.ChannelHeight, font);
            }

            int cur = movie.CurrentFrame - 1;
            if (cur < 0) cur = 0;
            float barX = _owner._gfxValues.LeftMargin + cur * _owner._gfxValues.FrameWidth + _owner._gfxValues.FrameWidth / 2f;
            DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * _owner._gfxValues.ChannelHeight), Colors.Red, 2);

            if (_owner._showPreview)
            {
                float px = _owner._gfxValues.LeftMargin + (_owner._previewBegin - 1) * _owner._gfxValues.FrameWidth;
                float pw = (_owner._previewEnd - _owner._previewBegin + 1) * _owner._gfxValues.FrameWidth;
                float py = _owner._previewChannel * _owner._gfxValues.ChannelHeight;
                DrawRect(new Rect2(px, py, pw, _owner._gfxValues.ChannelHeight), new Color(0, 0, 1, 0.3f));
            }
        }
    }

}

