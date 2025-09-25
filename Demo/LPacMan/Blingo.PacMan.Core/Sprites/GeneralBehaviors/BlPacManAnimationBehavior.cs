using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GeneralBehaviors;

internal sealed class BlPacManAnimationBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent
{
    private readonly Dictionary<string, AnimationSequence> _animations = new(StringComparer.OrdinalIgnoreCase);

    private string? _currentAnimation;
    private int _currentFrameIndex;
    private int _waitCounter;
    private bool _isPlaying;

    public BlPacManAnimationBehavior(IBlingoMovieEnvironment env)
        : base(env)
    {
    }

    public void SetAnimationRects(string name, ARect[] targetMemberRects, int framesToWaitBeforeNext)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Animation name is required.", nameof(name));
        }

        if (targetMemberRects is null)
        {
            throw new ArgumentNullException(nameof(targetMemberRects));
        }

        if (targetMemberRects.Length == 0)
        {
            throw new ArgumentException("Animation requires at least one frame.", nameof(targetMemberRects));
        }

        var frames = new ARect[targetMemberRects.Length];
        Array.Copy(targetMemberRects, frames, targetMemberRects.Length);
        _animations[name] = new AnimationSequence(frames, Math.Max(framesToWaitBeforeNext, 0));

        if (string.Equals(_currentAnimation, name, StringComparison.OrdinalIgnoreCase))
        {
            _currentFrameIndex = Math.Min(_currentFrameIndex, frames.Length - 1);
            ApplyFrame(frames[_currentFrameIndex]);
        }
    }

    public void Play(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (!_animations.TryGetValue(name, out var sequence) || sequence.Frames.Length == 0)
        {
            return;
        }

        if (string.Equals(_currentAnimation, name, StringComparison.Ordinal) && _isPlaying)
        {
            return;
        }

        _currentAnimation = name;
        _currentFrameIndex = 0;
        _waitCounter = 0;
        _isPlaying = true;
        ApplyFrame(sequence.Frames[_currentFrameIndex]);
    }

    public void StopAnimation()
    {
        _isPlaying = false;
        _waitCounter = 0;
    }

    public void BeginSprite()
    {
        if (_currentAnimation is null)
        {
            return;
        }

        if (_animations.TryGetValue(_currentAnimation, out var sequence) && sequence.Frames.Length > 0)
        {
            _currentFrameIndex = Math.Min(_currentFrameIndex, sequence.Frames.Length - 1);
            ApplyFrame(sequence.Frames[_currentFrameIndex]);
        }
    }

    public void EndSprite()
    {
        _isPlaying = false;
        _waitCounter = 0;
    }

    public void ExitFrame()
    {
        if (!_isPlaying || _currentAnimation is null)
        {
            return;
        }

        if (!_animations.TryGetValue(_currentAnimation, out var sequence) || sequence.Frames.Length == 0)
        {
            return;
        }

        if (sequence.FrameDelay == 0)
        {
            AdvanceFrame(sequence);
            return;
        }

        _waitCounter++;
        if (_waitCounter >= sequence.FrameDelay)
        {
            _waitCounter = 0;
            AdvanceFrame(sequence);
        }
    }

    private void AdvanceFrame(AnimationSequence sequence)
    {
        if (sequence.Frames.Length == 0)
        {
            return;
        }

        _currentFrameIndex++;
        if (_currentFrameIndex >= sequence.Frames.Length)
        {
            _currentFrameIndex = 0;
        }

        ApplyFrame(sequence.Frames[_currentFrameIndex]);
    }

    private void ApplyFrame(in ARect rect)
    {
        Me.MemberSourceRect = rect;
    }

    private readonly struct AnimationSequence
    {
        public AnimationSequence(ARect[] frames, int frameDelay)
        {
            Frames = frames;
            FrameDelay = frameDelay;
        }

        public ARect[] Frames { get; }

        public int FrameDelay { get; }
    }
}
