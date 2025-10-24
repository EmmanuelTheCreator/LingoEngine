using AbstUI.Primitives;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GeneralBehaviors;

internal sealed class BlPacManAnimationBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent
{
    private readonly Dictionary<PMCharacterAnimationType, AnimationSequence> _animations = new();

    private PMCharacterAnimationType _currentAnimation = PMCharacterAnimationType.Unknown;
    private int _currentFrameIndex;
    private int _waitCounter;
    private bool _isPlaying;

    public BlPacManAnimationBehavior(IBlingoMovieEnvironment env)
        : base(env)
    {
    }

    public void SetAnimationRects(PMCharacterAnimationType name, ARect[] targetMemberRects, int framesToWaitBeforeNext)
    {

        if (targetMemberRects.Length == 0)
            throw new ArgumentException("Animation requires at least one frame.", nameof(targetMemberRects));

        var frames = new ARect[targetMemberRects.Length];
        Array.Copy(targetMemberRects, frames, targetMemberRects.Length);
        _animations[name] = new AnimationSequence(frames, Math.Max(framesToWaitBeforeNext, 0));

        if (_currentAnimation == name)
        {
            _currentFrameIndex = Math.Min(_currentFrameIndex, frames.Length - 1);
            ApplyFrame(frames[_currentFrameIndex]);
        }
    }

    public void Play(PMCharacterAnimationType name)
    {
        if (!_animations.TryGetValue(name, out var sequence) || sequence.Frames.Length == 0)
            return;

        if (_currentAnimation == name && _isPlaying)
            return;

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
        if (_currentAnimation == PMCharacterAnimationType.Unknown)
            return;

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
        if (!_isPlaying || _currentAnimation == PMCharacterAnimationType.Unknown)
            return;

        if (!_animations.TryGetValue(_currentAnimation, out var sequence) || sequence.Frames.Length == 0)
            return;

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
        APoint? regPoint = null;
        if (string.Equals(Me.Member?.Name, "sprites", StringComparison.OrdinalIgnoreCase))
            regPoint = new APoint(rect.Width / 2f, rect.Height / 2f);

        Me.SetMemberRect(rect, regPoint);
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
