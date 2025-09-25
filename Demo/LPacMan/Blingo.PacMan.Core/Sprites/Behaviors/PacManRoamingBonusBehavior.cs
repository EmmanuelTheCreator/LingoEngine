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
    private const float VerticalOffset = -96f;

    private readonly GlobalVars _globals;
    private PacManGameBehavior? _coordinator;
    private GameSettings? _settings;

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
            Me.MemberSourceRect = new ARect(186, 4, 238, 56);
        }

        var map = _coordinator?.CurrentMap;
        var center = map?.HouseCenter;
        if (center != null)
        {
            Me.LocH = center.CenterX;
            Me.LocV = center.CenterY + VerticalOffset;
        }
    }
}
