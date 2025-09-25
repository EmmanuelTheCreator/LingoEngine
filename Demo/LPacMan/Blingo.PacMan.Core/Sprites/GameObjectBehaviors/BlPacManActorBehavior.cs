using System;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Events;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Members;
using AbstUI.Primitives;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

internal sealed class BlPacManActorBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasKeyDownEvent,
    IHasStepFrameEvent
{
    private static readonly ARect[] LeftAnimation =
    {
        new(0, 0, 32, 32),
        new(64, 0, 96, 32),
    };

    private static readonly ARect[] RightAnimation =
    {
        new(32, 0, 64, 32),
        new(128, 0, 160, 32),
    };

    private static readonly ARect[] UpAnimation =
    {
        new(0, 32, 32, 64),
        new(64, 32, 96, 64),
    };

    private static readonly ARect[] DownAnimation =
    {
        new(288, 32, 320, 64),
        new(352, 32, 384, 64),
    };

    private readonly GlobalVars _globals;
    private BlPacManGameBehavior? _coordinator;
    private BlPacManDirection _requestedDirection;
    private BlPacManCharacter? _character;
    private BlPacManEventSubscription? _tileEnteredSubscription;
    private bool _isActorRegistered;
    private bool _animationsConfigured;

    public BlPacManActorBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void Configure(BlPacManGameBehavior coordinator, PacmanSettings settings)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }
        var character = EnsureCharacter();
        character.SetMap(_coordinator.CurrentMap ?? _globals.MapProvider?.CurrentMap ?? throw new InvalidOperationException("Pac-Man map is not initialized."));
        character.Speed = settings.Speed;
        character.EffectiveSpeed = settings.Speed;
        character.Step = 8f;
        character.Reset();
        ResetPosition();
    }

    public void BeginSprite()
    {
        if (!_isActorRegistered)
        {
            Me.AddActor(this);
            _isActorRegistered = true;
        }
        _coordinator = _globals.GameBehavior;
        var character = EnsureCharacter();
        character.BeginSprite();
        EnsureAppearance();
        _coordinator?.RegisterPacMan(this);
    }

    public void EndSprite()
    {
        _coordinator?.UnregisterPacMan(this);
        _tileEnteredSubscription?.Release();
        _tileEnteredSubscription = null;
    }

    public void StepFrame()
    {
        EnsureCharacter().Move(_requestedDirection);
        _requestedDirection = BlPacManDirection.None;
    }

    public void KeyDown(BlingoKeyEvent key)
    {
        if (key is null)
        {
            return;
        }

        if (key.KeyPressed(37))
        {
            _requestedDirection = BlPacManDirection.Left;
        }
        else if (key.KeyPressed(39))
        {
            _requestedDirection = BlPacManDirection.Right;
        }
        else if (key.KeyPressed(38))
        {
            _requestedDirection = BlPacManDirection.Up;
        }
        else if (key.KeyPressed(40))
        {
            _requestedDirection = BlPacManDirection.Down;
        }
    }

    private void OnTileEntered(BlPacManTileContext args)
    {
        if (args.Tile.Item is BlPacManConsumableComponent consumable)
        {
            consumable.Consume(this);
        }
    }

    internal BlPacManCharacter Character => EnsureCharacter();

    internal BlPacManEventSubscription SubscribePacManPosition(Action<BlPacManPositionContext> handler)
    {
        return EnsureCharacter().SubscribePositionChanged(handler);
    }

    internal void Hide()
    {
        EnsureCharacter().Hide();
    }

    internal void Show()
    {
        EnsureCharacter().Show();
    }

    internal void ResetForLife()
    {
        var character = EnsureCharacter();
        character.Reset();
        ResetPosition();
        character.Update();
    }

    private void EnsureAppearance()
    {
        if (Me.Member is null)
        {
            var cast = CastLib("Data");
            var member = cast?.GetMember<BlingoMemberBitmap>("mspacman") ?? cast?.GetMember<BlingoMemberBitmap>("characters");
            if (member != null)
            {
                Me.Member = member;
                Me.MemberSourceRect = new ARect(0, 0, 32, 32);
            }
        }

        if (!_animationsConfigured)
        {
            ConfigureAnimations();
            _animationsConfigured = true;
        }
    }

    private void ConfigureAnimations()
    {
        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior =>
        {
            behavior.SetAnimationRects("left", LeftAnimation, 2);
            behavior.SetAnimationRects("right", RightAnimation, 2);
            behavior.SetAnimationRects("up", UpAnimation, 2);
            behavior.SetAnimationRects("down", DownAnimation, 2);
        });
    }

    private void ResetPosition()
    {
        var map = _coordinator?.CurrentMap ?? _globals.MapProvider?.CurrentMap;
        var startTile = map?.HouseCenter ?? map?.GetTile(map.Width / 2, map.Height - 3);
        if (startTile is null)
        {
            return;
        }

        Me.LocH = startTile.CenterX;
        Me.LocV = startTile.CenterY;
    }

    private BlPacManCharacter EnsureCharacter()
    {
        if (_character is not null)
        {
            if (_tileEnteredSubscription is null)
            {
                _tileEnteredSubscription = _character.SubscribeTileEntered(OnTileEntered);
            }
            return _character;
        }

        var map = _coordinator?.CurrentMap ?? _globals.MapProvider?.CurrentMap ?? throw new InvalidOperationException("Pac-Man map is not initialized.");
        _character = new BlPacManCharacter(_env, map, Me, new BlPacManCharacterOptions
        {
            Step = 8f,
            Speed = 80f,
            Direction = BlPacManDirection.Left,
            Preturn = true,
        });
        _tileEnteredSubscription = _character.SubscribeTileEntered(OnTileEntered);
        return _character;
    }
}
