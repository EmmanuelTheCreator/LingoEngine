using System;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.Behaviors;

internal sealed class PacManRoamingBonusBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent
{
    private const int FrameSize = 60;
    private const float VerticalOffset = -96f;

    private static readonly ARect[] DefaultAnimation = { CreateFrame(0, 0) };
    private static readonly ARect[] Score100Animation = { CreateFrame(0, FrameSize) };
    private static readonly ARect[] Score200Animation = { CreateFrame(FrameSize, FrameSize) };
    private static readonly ARect[] Score500Animation = { CreateFrame(FrameSize * 2, FrameSize) };
    private static readonly ARect[] Score700Animation = { CreateFrame(FrameSize * 3, FrameSize) };
    private static readonly ARect[] Score1000Animation = { CreateFrame(FrameSize * 4, FrameSize) };
    private static readonly ARect[] Score2000Animation = { CreateFrame(FrameSize * 5, FrameSize) };
    private static readonly ARect[] Score5000Animation = { CreateFrame(FrameSize * 6, FrameSize) };

    private readonly GlobalVars _globals;
    private PacManGameBehavior? _coordinator;
    private GameSettings? _settings;
    private bool _animationsConfigured;

    public PacManRoamingBonusBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void Configure(PacManGameBehavior coordinator, GameSettings settings)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void BeginSprite()
    {
        _coordinator = _globals.GameBehavior;
        ApplyAppearance();
        _coordinator?.RegisterBonus(this);
    }

    public void EndSprite()
    {
        _coordinator?.UnregisterBonus(this);
    }

    private void ApplyAppearance()
    {
        var cast = CastLib("Data");
        var member = cast?.GetMember<BlingoMemberBitmap>("misc");
        if (member != null)
        {
            Me.Member = member;
            Me.MemberSourceRect = DefaultAnimation[0];
        }

        var map = _coordinator?.CurrentMap;
        var center = map?.HouseCenter;
        if (center != null)
        {
            Me.LocH = center.CenterX;
            Me.LocV = center.CenterY + VerticalOffset;
        }

        EnsureAnimations();
        SetAnimation("default");
    }

    private void EnsureAnimations()
    {
        if (_animationsConfigured)
        {
            return;
        }

        SendSprite<BlPacmanAnimationBehavior>(Me.SpriteNum, behavior =>
        {
            behavior.SetAnimationRects("default", DefaultAnimation, 0);
            behavior.SetAnimationRects("score100", Score100Animation, 0);
            behavior.SetAnimationRects("score200", Score200Animation, 0);
            behavior.SetAnimationRects("score500", Score500Animation, 0);
            behavior.SetAnimationRects("score700", Score700Animation, 0);
            behavior.SetAnimationRects("score1000", Score1000Animation, 0);
            behavior.SetAnimationRects("score2000", Score2000Animation, 0);
            behavior.SetAnimationRects("score5000", Score5000Animation, 0);
        });

        _animationsConfigured = true;
    }

    private void SetAnimation(string name)
    {
        SendSprite<BlPacmanAnimationBehavior>(Me.SpriteNum, behavior => behavior.Play(name));
    }

    private static ARect CreateFrame(int offsetX, int offsetY)
    {
        return new ARect(offsetX, offsetY, offsetX + FrameSize, offsetY + FrameSize);
    }
}
