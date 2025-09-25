using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.Behaviors;

internal sealed class PacManGhostBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent
{
    private static readonly Dictionary<string, ARect> GhostRects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Blinky"] = new ARect(0, 0, 32, 32),
        ["Pinky"] = new ARect(32, 0, 64, 32),
        ["Inky"] = new ARect(64, 0, 96, 32),
        ["Clyde"] = new ARect(96, 0, 128, 32),
    };

    private static readonly Dictionary<string, float> HorizontalOffsets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Blinky"] = -32f,
        ["Pinky"] = -16f,
        ["Inky"] = 16f,
        ["Clyde"] = 32f,
    };

    private readonly GlobalVars _globals;
    private PacManGameBehavior? _coordinator;
    private GhostMode _mode = GhostMode.Scatter;
    private GhostSettings? _settings;

    public PacManGhostBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public string GhostName { get; set; } = "Ghost";

    public void BeginSprite()
    {
        _coordinator = _globals.GameBehavior;
        ApplyAppearance();
        _coordinator?.RegisterGhost(this);
    }

    public void EndSprite()
    {
        _coordinator?.UnregisterGhost(this);
    }

    public void ExitFrame()
    {
        // Placeholder: ghosts remain static for now.
    }

    public void SetMode(GhostMode? mode)
    {
        _mode = mode ?? GhostMode.Scatter;
    }

    public void Configure(PacManGameBehavior coordinator, GhostSettings settings)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    private void ApplyAppearance()
    {
        var cast = CastLib("Data");
        var member = cast?.GetMember<BlingoMemberBitmap>("characters");
        if (member != null)
        {
            Me.Member = member;
        }

        if (GhostRects.TryGetValue(GhostName, out var rect))
        {
            Me.MemberSourceRect = rect;
        }
        else
        {
            Me.MemberSourceRect = new ARect(0, 0, 32, 32);
        }

        var map = _coordinator?.CurrentMap;
        var center = map?.HouseCenter ?? map?.House ?? map?.GetTile(map.Width / 2, map.Height / 2);
        if (center != null)
        {
            var offset = HorizontalOffsets.TryGetValue(GhostName, out var value) ? value : 0f;
            Me.LocH = center.CenterX + offset;
            Me.LocV = center.CenterY;
        }
    }
}
