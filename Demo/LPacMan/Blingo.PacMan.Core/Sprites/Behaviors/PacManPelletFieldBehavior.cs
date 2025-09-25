using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.Behaviors;

internal sealed class PacManPelletFieldBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasEndSpriteEvent
{
    private static readonly ARect PelletRect = new(0, 0, 16, 24);

    private readonly List<PacManConsumableComponent> _spawnedConsumables = new();
    private readonly GlobalVars _globals;
    private PacManEventSubscription? _subscription;

    public PacManPelletFieldBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void BeginSprite()
    {
        Subscribe();
        TrySyncWithCurrentContext();
    }

    public void EndSprite()
    {
        _subscription?.Release();
        _subscription = null;
        ClearField();
    }

    private void Subscribe()
    {
        var mediator = _globals.ConsumableFieldMediator;
        if (mediator is null)
        {
            return;
        }

        _subscription?.Release();
        _subscription = mediator.Subscribe(OnFieldContext);
    }

    private void TrySyncWithCurrentContext()
    {
        var context = _globals.CurrentFieldContext;
        if (context is not null)
        {
            OnFieldContext(context);
        }
    }

    private void OnFieldContext(PacManFieldContext context)
    {
        if (context is null)
        {
            return;
        }

        ClearField();

        foreach (var tile in context.Map.Tiles)
        {
            if (tile.Code != '.')
            {
                continue;
            }

            CreateConsumableSprite(tile, context.Coordinator);
        }
    }

    private void CreateConsumableSprite(Tile tile, PacManGameBehavior coordinator)
    {
        var name = $"{Me.Name}_{tile.Column}_{tile.Row}";
        var sprite = _Movie.AddSprite(name, sprite2D =>
        {
            sprite2D.BeginFrame = PacManProjectFactory.GameStartFrame;
            sprite2D.EndFrame = Math.Max(PacManProjectFactory.GameStartFrame, _Movie.FrameCount);
            sprite2D.LocH = tile.CenterX;
            sprite2D.LocV = tile.CenterY;
            sprite2D.LocZ = Me.LocZ;
            sprite2D.Puppet = true;
            sprite2D.Lock = true;
            sprite2D.Visibility = true;

            var cast = CastLib("Data");
            var member = cast?.GetMember<BlingoMemberBitmap>("pills");
            if (member != null)
            {
                sprite2D.Member = member;
            }

            sprite2D.MemberSourceRect = PelletRect;
        });

        var behavior = sprite.SetBehavior<PacManPelletBehavior>();
        behavior.Initialize(tile, coordinator);
        _spawnedConsumables.Add(behavior.Component);
    }

    private void ClearField()
    {
        foreach (var consumable in _spawnedConsumables)
        {
            consumable.Destroy();
        }

        _spawnedConsumables.Clear();
    }
}
