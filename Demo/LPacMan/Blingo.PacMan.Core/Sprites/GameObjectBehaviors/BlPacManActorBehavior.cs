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

/// <summary>
/// Drives the Pac-Man avatar sprite, including keyboard input handling, animation swapping, and
/// dispatching position events to the rest of the gameplay behaviours.
/// </summary>
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
    private BlPacManAssetContainer? _assets;
    private BlPacManGameBehavior? _coordinator;
    private BlPacManDirection _requestedDirection;
    private BlPacManCharacter? _character;
    private BlPacManEventSubscription? _tileEnteredSubscription;
    private BlPacManEventSubscription? _positionSubscription;
    private bool _isActorRegistered;
    private bool _animationsConfigured;

    /// <summary>
    /// Initialises the behaviour with the movie environment and shared global references.
    /// </summary>
    public BlPacManActorBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    /// <summary>
    /// Applies the current level settings retrieved from the model.
    /// </summary>
    /// <param name="coordinator">The gameplay coordinator managing global state.</param>
    /// <param name="settings">Speed and mode settings for Pac-Man.</param>
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
        PublishPosition(character);
    }

    /// <summary>
    /// Registers the behaviour as an actor and starts listening for movement updates.
    /// </summary>
    public void BeginSprite()
    {
        if (!_isActorRegistered)
        {
            Me.AddActor(this);
            _isActorRegistered = true;
        }
        _coordinator = _globals.GameBehavior;
        _assets = _globals.Assets;
        var character = EnsureCharacter();
        character.BeginSprite();
        EnsureAppearance();
        _assets.AttachPacMan(this);

        _positionSubscription?.Release();
        _positionSubscription = character.SubscribePositionChanged(OnPositionChanged);
        PublishPosition(character);

        if (_coordinator is not null && _globals.CurrentPacmanSettings is { } settings)
        {
            Configure(_coordinator, settings);
        }
    }

    /// <summary>
    /// Stops tracking Pac-Man when the sprite is removed from the stage.
    /// </summary>
    public void EndSprite()
    {
        _assets?.DetachPacMan(this);
        _tileEnteredSubscription?.Release();
        _tileEnteredSubscription = null;
        _positionSubscription?.Release();
        _positionSubscription = null;
        _assets = null;
    }

    /// <summary>
    /// Moves Pac-Man in the last requested direction.
    /// </summary>
    public void StepFrame()
    {
        var character = EnsureCharacter();
        if (_globals.State.IsGameplayFrozen)
        {
            character.Update();
            _requestedDirection = BlPacManDirection.None;
            return;
        }

        character.Move(_requestedDirection);
        _requestedDirection = BlPacManDirection.None;
        CheckCollisions(character);
    }

    /// <summary>
    /// Translates keyboard input into direction requests that the next StepFrame will honour.
    /// </summary>
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

    /// <summary>
    /// Consumes dots or pills when Pac-Man enters a tile containing a consumable component.
    /// </summary>
    private void OnTileEntered(BlPacManTileContext args)
    {
        if (args.Tile.Item is BlPacManConsumableComponent consumable)
        {
            consumable.Consume(this);
        }
    }

    /// <summary>
    /// Exposes the underlying movement helper for other behaviours.
    /// </summary>
    internal BlPacManCharacter Character => EnsureCharacter();

    /// <summary>
    /// Hides Pac-Man's sprite.
    /// </summary>
    internal void Hide()
    {
        EnsureCharacter().Hide();
    }

    /// <summary>
    /// Shows Pac-Man's sprite.
    /// </summary>
    internal void Show()
    {
        EnsureCharacter().Show();
    }

    /// <summary>
    /// Resets Pac-Man's position and clears transient timers after a lost life.
    /// </summary>
    internal void ResetForLife()
    {
        var character = EnsureCharacter();
        character.Reset();
        ResetPosition();
        character.Update();
        PublishPosition(character);
    }

    /// <summary>
    /// Plays the appropriate sound effect when Pac-Man consumes an item.
    /// </summary>
    /// <param name="consumable">The component representing the consumed item.</param>
    internal void HandleConsumableEaten(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        var state = _globals.State;
        state.RegisterConsumableEaten();

        var model = _globals.GameModel;
        if (model is not null && consumable.ScoreValue > 0)
        {
            model.AddScore(consumable.ScoreValue);
            state.Score = model.Score;
            state.HighScore = model.HighScore;
        }

        if (!state.Muted)
        {
            switch (consumable.Type)
            {
                case BlPacManConsumableType.Pellet:
                    _Player.SoundPlayDot();
                    break;
                case BlPacManConsumableType.PowerPill:
                    _Player.SoundPlayFrightened();
                    break;
                case BlPacManConsumableType.Bonus:
                    _Player.SoundPlayBonus();
                    break;
            }
        }

        if (consumable.Type == BlPacManConsumableType.PowerPill)
        {
            state.ResetGhostChain();
            foreach (var ghost in _globals.Assets.Ghosts)
            {
                ghost.SetMode(GhostMode.Frightened);
            }
        }

        if (state.RemainingConsumables == 0)
        {
            state.Win = true;
        }
    }

    /// <summary>
    /// Handles the audio transition when Pac-Man is eaten by a ghost.
    /// </summary>
    internal void OnEatenByGhost()
    {
        if (!_globals.State.Muted)
        {
            _Player.SoundPlayEaten();
        }

        _Player.SoundStopBack();
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

    private void CheckCollisions(BlPacManCharacter character)
    {
        if (_assets is null || _globals.State.IsGameplayFrozen)
        {
            return;
        }

        var tile = character.GetTile();
        if (tile is null)
        {
            return;
        }

        foreach (var ghost in _assets.Ghosts)
        {
            var ghostTile = ghost.CurrentTile;
            if (ghostTile is null || !ReferenceEquals(ghostTile, tile) || ghost.IsDead)
            {
                continue;
            }

            if (ghost.IsFrightened)
            {
                ghost.SetMode(GhostMode.Dead);
                var state = _globals.State;
                var score = state.RegisterGhostEaten();
                var model = _globals.GameModel;
                if (model is not null && score > 0)
                {
                    model.AddScore(score);
                    state.Score = model.Score;
                    state.HighScore = model.HighScore;
                }

                state.SoundCooldown = 5;
                state.PauseFrames = Math.Max(state.PauseFrames, 15);
                ghost.OnEaten(score);
            }
            else
            {
                _globals.GameBehavior?.NotifyPacManEaten();
            }

            return;
        }

        var bonus = _assets.Bonus;
        if (bonus is not null && bonus.IsActive && ReferenceEquals(bonus.CurrentTile, tile))
        {
            bonus.Collect();
        }
    }

    private void OnPositionChanged(BlPacManPositionContext context)
    {
        if (context is null)
        {
            return;
        }

        _globals.State.PacManPosition = context;
        _assets?.UpdatePacManPosition(context);
    }

    private void PublishPosition(BlPacManCharacter character)
    {
        if (character is null)
        {
            return;
        }

        var snapshot = new BlPacManPositionContext(Me.LocH, Me.LocV, character.GetTile(), character.Direction);
        OnPositionChanged(snapshot);
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
