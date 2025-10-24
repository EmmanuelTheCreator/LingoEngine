using BlingoEngine.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Members;
using AbstUI.Commands;
using BlingoEngine.Director.Core.Stages.Commands;
using BlingoEngine.Movies;
using System.Collections.Generic;
using System.Linq;
using BlingoEngine.Director.Core.Scores.Sprites2D;
using BlingoEngine.Scripts;
using System;
using BlingoEngine.Director.Core.Inspector.Commands;
using AbstUI.Inputs;
using BlingoEngine.Texts;

namespace BlingoEngine.Director.Core.Scores
{


    public interface IDirScoreManager
    {
        DirScoreGfxValues GfxValues { get; }
        IDirSpritesManager SpritesManager { get; }
        IBlingoFrameworkFactory Factory { get; }
        void DeselectSprite(BlingoSprite sprite);
        void HandleMouse(AbstMouseEvent mouseEvent, int channelNumber, int frameNumber);
        void SelectSprite(BlingoSprite sprite);

    }

    public class DirScoreManager : IDirScoreManager
    {
        private IDirSpritesManager? _spritesManager;
        private readonly Dictionary<int, DirScoreChannel> _channels = new();
        private readonly List<DirScoreSprite> _selected = new();
        private readonly IDirectorEventMediator _directorEventMediator;
        private readonly IAbstCommandManager _commandManager;
        private DirScoreSprite? _lastAddedSprite;
        private bool _lastMouseLeftDown;
        private bool _dragging;
        private bool _dragBegin;
        private bool _dragEnd;
        private bool _dragMiddle;
        private bool _dragKeyFrame;
        private DirScoreSprite? _dragKeyFrameSprite;
        private bool _isDropPreview;
        private DirScoreChannel? _dragPreviewChannel;
        private int _mouseDownFrame = -1;
        private readonly DirScoreRectangleSelection _rectangleSelection;
        private BlingoMovie? _movie;
        public DirScoreGfxValues GfxValues { get; } = new();
        public IDirSpritesManager SpritesManager => _spritesManager!;
        internal IDirSpritesManager? SpritesManagerNullable => _spritesManager;
        public IBlingoFrameworkFactory Factory { get; }
        public int MaxChannelNumber { get; private set; }
        internal Dictionary<int, DirScoreChannel> Channels => _channels;
        internal List<DirScoreSprite> Selected => _selected;
        internal bool LastMouseLeftDown { get => _lastMouseLeftDown; set => _lastMouseLeftDown = value; }

        public DirScoreManager(IBlingoFrameworkFactory factory, IDirectorEventMediator directorEventMediator, IAbstCommandManager blingoCommandManager)
        {
            Factory = factory;
            _directorEventMediator = directorEventMediator;
            _commandManager = blingoCommandManager;
            _rectangleSelection = new DirScoreRectangleSelection(this);
        }

        internal void SetSpritesManager(IDirSpritesManager manager)
        {
            _spritesManager = manager;
        }
        public void RegisterChannel(DirScoreChannel channel)
        {
            if (_channels.ContainsKey(channel.SpriteNumWithChannelNum))
                throw new InvalidOperationException($"Channel with sprite number {channel.SpriteNumWithChannelNum} already exists.");
            _channels[channel.SpriteNumWithChannelNum] = channel;
            MaxChannelNumber = Math.Max(MaxChannelNumber, channel.SpriteNumWithChannelNum);
        }
        public void UnregisterChannel(DirScoreChannel channel)
        {
            if (!_channels.ContainsKey(channel.SpriteNumWithChannelNum))
                return;
            _channels.Remove(channel.SpriteNumWithChannelNum);
            var lastChannel = _channels.Values.LastOrDefault();
            MaxChannelNumber = lastChannel != null ? lastChannel.SpriteNumWithChannelNum : 0;
        }

        public void CurrentMovieChanged(BlingoMovie? movie)
        {
            _movie = movie;
        }

        internal void ClearSelection(bool notify = true)
        {
            var clone = _selected.ToList();
            foreach (var s in clone)
                s.SetSelected(false, notify);
            _selected.Clear();
        }

        internal void AddSelection(DirScoreSprite sprite, bool notify = true)
        {
            if (sprite.IsLocked) return;
            if (_selected.Contains(sprite))
            {
                if (_spritesManager != null && (_spritesManager.Key.ControlDown || _spritesManager.Key.ShiftDown))
                {
                    sprite.SetSelected(false, notify);
                    _selected.Remove(sprite);
                }
                return;
            }
            _selected.Add(sprite);
            _lastAddedSprite = sprite;
            sprite.SetSelected(true, notify);
        }

        private DirScoreSprite? FindSprite(BlingoSprite sprite)
        {
            int ch = sprite.SpriteNum - 1;
            if (_channels.TryGetValue(ch, out var channel))
                return channel.FindSprite(sprite);

            return null;
        }

        /// <summary>Finds the score sprite at the specified channel and frame.</summary>
        public DirScoreSprite? GetSpriteAt(int channelNumber, int frameNumber)
        {
            if (_channels.TryGetValue(channelNumber, out var channel))
                return channel.GetSpriteAtFrame(frameNumber);
            return null;
        }

        internal (DirScoreChannel Channel, DirScoreSprite Sprite)? TryGetSpriteForMember(IBlingoMember member)
        {
            DirScoreSprite? bestSprite = null;
            DirScoreChannel? bestChannel = null;

            foreach (var ch in _channels.Values)
            {
                foreach (var candidate in ch.GetSprites())
                {
                    if (candidate.Sprite is not IBlingoSpriteWithMember withMember)
                        continue;

                    var candidateMember = withMember.GetMember();
                    if (candidateMember == null || !ReferenceEquals(candidateMember, member))
                        continue;

                    if (bestSprite == null || candidate.Sprite.BeginFrame < bestSprite.Sprite.BeginFrame)
                    {
                        bestSprite = candidate;
                        bestChannel = ch;
                    }
                }
            }

            if (bestSprite == null || bestChannel == null)
                return null;

            return (bestChannel, bestSprite);
        }

        public void SelectSprite(BlingoSprite sprite)
        {
            if (sprite.Lock) return;
            var scoreSprite = FindSprite(sprite);
            if (scoreSprite != null)
                AddSelection(scoreSprite);
        }

        public void DeselectSprite(BlingoSprite sprite)
        {
            var scoreSprite = _selected.FirstOrDefault(s => s.Sprite == sprite);
            if (scoreSprite == null)
                return;
            scoreSprite.SetSelected(false);
            _selected.Remove(scoreSprite);
        }

        public void HandleMouse(AbstMouseEvent mouseEvent, int channelNumber, int frameNumber)
        {
            if (!_channels.TryGetValue(channelNumber, out var channel))
                return;
            var spriteScore = channel.GetSpriteAtFrame(frameNumber);

            if (mouseEvent.Type == AbstMouseEventType.MouseDown && mouseEvent.Mouse.LeftMouseDown)
            {
                if (mouseEvent.Mouse.DoubleClick)
                {
                    HandleDoubleClick(channelNumber, frameNumber, channel, spriteScore);
                    return;
                }
                _lastMouseLeftDown = true;
                if (spriteScore != null)
                {
                    if (spriteScore.IsLocked)
                    {
                        mouseEvent.Mouse.SetCursor(AbstUI.Primitives.AMouseCursor.NotAllowed);
                        _directorEventMediator.RaiseSpriteSelected(spriteScore.Sprite);
                        _mouseDownFrame = -1;
                        return;
                    }
                    AddSelection(spriteScore);
                    if (frameNumber == spriteScore.Sprite.BeginFrame)
                    {
                        foreach (var s in _selected)
                            s.PrepareDragging(frameNumber);
                        _dragBegin = true;
                    }
                    else if (frameNumber == spriteScore.Sprite.EndFrame)
                    {
                        foreach (var s in _selected)
                            s.PrepareDragging(frameNumber);
                        _dragEnd = true;
                    }
                    else if (spriteScore.IsKeyFrame(frameNumber))
                    {
                        spriteScore.PrepareKeyFrameDragging(frameNumber);
                        _dragKeyFrame = true;
                        _dragKeyFrameSprite = spriteScore;
                    }
                    else
                    {
                        foreach (var s in _selected)
                            s.PrepareDragging(frameNumber);
                        _dragMiddle = true;
                    }
                    _mouseDownFrame = frameNumber;
                }
                else
                {
                    _rectangleSelection.Begin(channelNumber, frameNumber);
                }
            }
            else if (mouseEvent.Type == AbstMouseEventType.MouseMove)
            {
                if (_rectangleSelection.Update(channelNumber, frameNumber))
                    return;
                // Drag from castlib?
                if (DirectorDragDropHolder.IsDragging && !_isDropPreview && DirectorDragDropHolder.Member != null &&
                    DirectorDragDropHolder.Member is not BlingoMemberScript { ScriptType: BlingoScriptType.Behavior })
                    StartDropPreview();

                if (_isDropPreview)
                {
                    DropPreview(channelNumber, frameNumber);
                    return;
                }

                if (!_dragging)
                {
                    if (_mouseDownFrame >= 0 && Math.Abs(frameNumber - _mouseDownFrame) >= 1)
                    {
                        _dragging = true;
                        mouseEvent.Mouse.SetCursor(AbstUI.Primitives.AMouseCursor.Drag);
                    }
                    else
                        return;
                }
                if (_dragBegin)
                {
                    foreach (var s in _selected)
                        s.DragMoveBegin(frameNumber);
                }
                else if (_dragEnd)
                {
                    foreach (var s in _selected)
                        s.DragMoveEnd(frameNumber);
                }
                else if (_dragKeyFrame)
                {
                    _dragKeyFrameSprite?.DragKeyFrame(frameNumber);
                }
                else if (_dragMiddle)
                {
                    foreach (var s in _selected)
                        s.DragMove(frameNumber);

                    UpdateDragPreview(channelNumber);
                }
            }
            else if (mouseEvent.Type == AbstMouseEventType.MouseUp)
            {
                if (_rectangleSelection.Complete(mouseEvent, channelNumber, frameNumber))
                    return;
                if (_isDropPreview)
                {
                    if (DirectorDragDropHolder.Member != null)
                    {
                        // drop from cast lib
                        DropFromCastLib(DirectorDragDropHolder.Member);
                    }
                    StopPreview();
                    return;
                }
                if (DirectorDragDropHolder.Member is BlingoMemberScript { ScriptType: BlingoScriptType.Behavior } script)
                {
                    if (spriteScore?.Sprite is BlingoSprite2D sprite2D)
                        sprite2D.AttachBehavior(script, _commandManager);
                    else if (_movie != null)
                    {
                        // its a frame script, so we need to create a new sprite
                        if (!_movie.FrameScripts.Has(frameNumber))
                        { 
                            _movie.FrameScripts.Add(frameNumber,c =>c.SetMember(script));
                        }
                    }
                    DirectorDragDropHolder.EndDrag();
                    return;
                }
                if (_dragKeyFrame)
                {
                    _dragKeyFrameSprite?.StopKeyFrameDragging();
                    if (_dragKeyFrameSprite?.Channel != null)
                        _dragKeyFrameSprite.Channel.RequireRedraw();
                    _dragKeyFrame = false;
                    _dragKeyFrameSprite = null;
                    _dragging = false;
                    _mouseDownFrame = -1;
                    _lastAddedSprite = null;
                    mouseEvent.Mouse.SetCursor(AbstUI.Primitives.AMouseCursor.Arrow);
                    _lastMouseLeftDown = false;
                    return;
                }
                if (_lastMouseLeftDown)
                {
                    if (_dragPreviewChannel != null && _movie != null)
                    {
                        FinalizeChannelMove();
                    }
                    else if (_spritesManager != null && !_dragging && !_spritesManager.Key.ControlDown && !_spritesManager.Key.ShiftDown && _lastAddedSprite == null)
                        ClearSelection();
                    else
                    {
                        foreach (var s in _selected)
                        {
                            s.StopDragging();
                            if (s.Channel != null)
                                s.Channel.RequireRedraw();
                        }
                    }
                    _dragging = _dragBegin = _dragEnd = _dragMiddle = _dragKeyFrame = false;
                    _mouseDownFrame = -1;
                    _lastAddedSprite = null;
                    mouseEvent.Mouse.SetCursor(AbstUI.Primitives.AMouseCursor.Arrow);
                    _lastMouseLeftDown = false;
                }

            }
        }

        /// <summary>
        /// Handles previewing channel moves while dragging sprites.
        /// </summary>
        /// <param name="channelNumber">Channel currently hovered by the mouse.</param>
        private void UpdateDragPreview(int channelNumber)
        {
            // No selection -> no preview.
            if (_selected.Count == 0)
                return;

            int originChannel = _selected[0].DragStartChannel;

            // Only show preview when hovering over a different channel.
            if (channelNumber != originChannel)
            {
                // Allow previews only for Sprite2D channels.
                if (_channels.TryGetValue(channelNumber, out var target) && target is DirScoreSprite2DChannel)
                {
                    int minBegin = _selected.Min(s => s.Sprite.BeginFrame);
                    int maxEnd = _selected.Max(s => s.Sprite.EndFrame);

                    // Draw preview if the range fits the target channel.
                    if (target.DrawMovePreview(minBegin, maxEnd))
                    {
                        if (_dragPreviewChannel != null && _dragPreviewChannel != target)
                            _dragPreviewChannel.StopPreview();
                        _dragPreviewChannel = target;
                    }
                    else if (_dragPreviewChannel != null)
                    {
                        _dragPreviewChannel.StopPreview();
                        _dragPreviewChannel = null;
                    }
                }
                else if (_dragPreviewChannel != null)
                {
                    // Current channel cannot host the sprites -> clear preview.
                    _dragPreviewChannel.StopPreview();
                    _dragPreviewChannel = null;
                }
            }
            else if (_dragPreviewChannel != null)
            {
                // Returned to original channel -> remove preview.
                _dragPreviewChannel.StopPreview();
                _dragPreviewChannel = null;
            }
        }

        /// <summary>
        /// Applies a pending preview move by transferring the selected sprites to
        /// the previewed channel and recording the operation for undo/redo.
        /// </summary>
        private void FinalizeChannelMove()
        {
            if (_dragPreviewChannel == null || _movie == null)
                return;

            // Target channel index corresponding to the preview channel.
            int newChannelIdx = _dragPreviewChannel.SpriteNumWithChannelNum - BlingoSprite2D.SpriteNumOffset - 1;

            foreach (var s in _selected)
            {
                var oldChannel = s.Channel;
                int origChannelIdx = s.Sprite.SpriteNum - 1;
                int origBegin = s.DragStartBeginFrame;
                int origEnd = s.DragStartEndFrame;
                int newBegin = s.Sprite.BeginFrame;
                int newEnd = s.Sprite.EndFrame;

                // Move the sprite to the target channel if it actually changed.
                if (origChannelIdx != newChannelIdx)
                    _movie.ChangeSpriteChannel(s.Sprite, newChannelIdx);

                // Record range change so the action can be undone/redone.
                _commandManager.Handle(new ChangeSpriteRangeCommand(
                    _movie, s.Sprite,
                    origChannelIdx, origBegin, origEnd,
                    newChannelIdx, newBegin, newEnd));

                // Stop dragging without snapping back to the original range.
                s.StopDragging(false);

                // Mark the old channel dirty to trigger a redraw.
                if (oldChannel != null)
                {
                    oldChannel.HasDirtySpriteList = true;
                    oldChannel.RequireRedraw();
                }
            }

            // Update the preview channel, clear preview state and selection.
            _dragPreviewChannel.HasDirtySpriteList = true;
            _dragPreviewChannel.RequireRedraw();
            _dragPreviewChannel.StopPreview();
            _dragPreviewChannel = null;
            ClearSelection();
        }

        private void HandleDoubleClick(int channelNumber, int frameNumber, DirScoreChannel channel, DirScoreSprite? spriteScore)
        {
            if (channelNumber >= 4)
                return;

            if (spriteScore == null)
            {
                channel.ShowCreateSpriteDialog(frameNumber, sprite =>
                {
                    if (sprite != null)
                    {
                        _directorEventMediator.RaiseSpriteSelected(sprite);
                    }
                });
            }
            else
                channel.ShowSpriteDialog(spriteScore.Sprite);
        }



        #region Preview and Drop
        private List<DirScoreChannel> _lastPreviewChannels = new List<DirScoreChannel>();


        public void StartDropPreview()
        {
            _isDropPreview = true;
        }
        private void DropPreview(int channelNumber, int frameNumber)
        {
            //var result = channel.DrawPreview(frameNumber);
            if (channelNumber > 0)
            {
                var peviewChannel = channelNumber;
                while (peviewChannel < MaxChannelNumber)
                {
                    if (_channels.TryGetValue(peviewChannel, out var previewChannel))
                    {
                        // todo : ability to drag multiple sprites wiuth preview
                        if (previewChannel.DrawPreview(frameNumber))
                        {
                            foreach (var channel in _lastPreviewChannels)
                            {
                                if (previewChannel != channel)
                                    channel.StopPreview();
                            }
                            _lastPreviewChannels.Clear();
                            if (!_lastPreviewChannels.Contains(previewChannel))
                                _lastPreviewChannels.Add(previewChannel);

                            break;
                        }
                    }
                    peviewChannel++;
                }
            }
        }
        private void StopPreview()
        {
            foreach (var channel in _lastPreviewChannels)
                channel.StopPreview();
            _lastPreviewChannels.Clear();
            _isDropPreview = false;
            DirectorDragDropHolder.EndDrag();
        }
        private void DropFromCastLib(IBlingoMember member)
        {
            if (_movie == null) return;
            var channel = _lastPreviewChannels.LastOrDefault();
            if (channel == null) return;
            if (member is IBlingoMemberTextBase textBase && textBase.Width == 1) // bug for some unknown reason.
                textBase.Width = 0;
            member.Preload();
            _commandManager.Handle(new AddSpriteCommand(_movie, member, channel.SpriteNumWithChannelNum, channel.PreviewBegin, channel.PreviewEnd));
            channel.RequireRedraw();
        }


        #endregion




        internal void ShowSpriteInfo(DirScoreSpriteLabelType type)
        {
            foreach (var channel in _channels.Values)
                channel.ShowSpriteInfo(type);
        }

    }
}

