using AbstUI.Primitives;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Bitmaps;
using BlingoEngine.Events;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using System.Collections.Generic;
using System.Reflection;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Drives the Pac-Man avatar sprite, including keyboard input handling, animation swapping, and
/// dispatching position events to the rest of the gameplay behaviours.
/// </summary>
internal sealed class BlPacManActorBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasKeyDownEvent,
    IHasExitFrameEvent
{
    public static int SprSize => BlPacManTheme.Actor.SpriteSize;
    public static int SprY => BlPacManTheme.Actor.SpriteSheetY;
    private static readonly IReadOnlyDictionary<string, (ARect[] Frames, int FrameDelay)> _animationDefinitions =
        BlPacManTheme.Actor.Animations;

    // Legacy 16px sprite loops retained for reference. To reinstate them, move the values below into
    // BlPacManTheme.Actor.Animations.
    //var leftAnimation = new[]
    //{
    //    ARect.New(SprSize * 12, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 13, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 14, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 15, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 14, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 13, SprY, SprSize, SprSize),
    //};

    //var rightAnimation = new[]
    //{
    //    ARect.New(SprSize * 0, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 1, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 2, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 3, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 2, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 1, SprY, SprSize, SprSize),
    //};

    //var upAnimation = new[]
    //{
    //    ARect.New(SprSize * 8,  SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 9,  SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 10, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 11, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 10, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 9,  SprY, SprSize, SprSize),
    //};

    //var downAnimation = new[]
    //{
    //    ARect.New(SprSize * 4, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 5, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 6, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 7, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 6, SprY, SprSize, SprSize),
    //    ARect.New(SprSize * 5, SprY, SprSize, SprSize),
    //};

    private readonly GlobalVars _globals;
    private BlPacManGameBehavior? _coordinator;
    private BlPacManDirection _requestedDirection;
    private BlPacManCharacter? _character;
    private BlPacManEventSubscription? _tileEnteredSubscription;
    private BlPacManEventSubscription? _positionSubscription;
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
        character.SetMap(_globals.Map ?? throw new InvalidOperationException("Pac-Man map is not initialized."));
        character.Step = GetBaseStepSize();
        character.Speed = settings.Speed;
        character.EffectiveSpeed = settings.Speed;
        character.Reset();
        ResetPosition();
        PublishPosition(character);
    }

    /// <summary>
    /// Registers the behaviour as an actor and starts listening for movement updates.
    /// </summary>
    public void BeginSprite()
    {
        _coordinator = _globals.GameBehavior;
        var character = EnsureCharacter();
        character.BeginSprite();
        EnsureAppearance();
        _globals.GameModel?.AttachPacMan(this);

        _positionSubscription?.Release();
        _positionSubscription = character.SubscribePositionChanged(OnPositionChanged);
        PublishPosition(character);

        if (_coordinator is not null && _globals.CurrentPacmanSettings is { } settings)
            Configure(_coordinator, settings);
    }

    /// <summary>
    /// Stops tracking Pac-Man when the sprite is removed from the stage.
    /// </summary>
    public void EndSprite()
    {
        _globals.GameModel?.DetachPacMan(this);
        _tileEnteredSubscription?.Release();
        _tileEnteredSubscription = null;
        _positionSubscription?.Release();
        _positionSubscription = null;
    }

    /// <summary>
    /// Moves Pac-Man in the last requested direction.
    /// </summary>
    public void ExitFrame()
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
        if (key.KeyPressed(123))
            _requestedDirection = BlPacManDirection.Left;
        else if (key.KeyPressed(124))
            _requestedDirection = BlPacManDirection.Right;
        else if (key.KeyPressed(126))
            _requestedDirection = BlPacManDirection.Up;
        else if (key.KeyPressed(125))
            _requestedDirection = BlPacManDirection.Down;
    }

    /// <summary>
    /// Consumes dots or pills when Pac-Man enters a tile containing a consumable component.
    /// </summary>
    private void OnTileEntered(BlPacManTileEventData args)
    {
        if (args.Tile.Item is BlPacManConsumableComponent consumable)
            consumable.Consume(this);
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
            return;

        _globals.State.ConsumableEaten();

        if (consumable.ScoreValue > 0)
            _globals.ScoreManager.AddScore(consumable.ScoreValue);

        if (!_globals.State.IsMuted)
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
            _globals.GhostManager.ResetGhostChain();
            _globals.GhostManager.SetAllFrightened();
        }
    }

    /// <summary>
    /// Handles the audio transition when Pac-Man is eaten by a ghost.
    /// </summary>
    private void EatenByGhost()
    {
        if (!_globals.State.IsMuted)
            _Player.SoundPlayEaten();

        _Player.SoundStopBack();
        Hide();
        _globals.GameBehavior?.NotifyPacManEaten();
    }

    private void EnsureAppearance()
    {
        if (_animationsConfigured)
            return;
        
        ConfigureAnimations();
        _animationsConfigured = true;
    }

    private void CheckCollisions(BlPacManCharacter character)
    {
        if (_globals.State.IsGameplayFrozen)
            return;

        var tile = character.GetTile();
        if (tile is null)
            return;

        foreach (var ghost in _globals.GhostManager.GetGhostsOnTile(tile))
        {
            if (ghost.IsFrightened)
            {
                ghost.SetMode(GhostMode.Dead);
                var score = _globals.GhostManager.RegisterGhostEaten();
                if (score > 0)
                    _globals.ScoreManager.AddScore(score);

                var state = _globals.State;
                state.SoundCooldown = 5;
                state.PauseFrames = Math.Max(state.PauseFrames, 15);
                ghost.OnEaten(score);
            }
            else
                EatenByGhost();

            return;
        }
        _globals.BonusManager.CollectOnTile(tile);
        
    }

    private void OnPositionChanged(BlPacManPositionEventData context)
    {
        if (context is null)
            return;

        _globals.State.UpdatePacManPosition(context);
    }

    private void PublishPosition(BlPacManCharacter character)
    {
        if (character is null)
            return;

        var snapshot = new BlPacManPositionEventData(Me.LocH, Me.LocV, character.GetTile(), character.Direction);
        OnPositionChanged(snapshot);
    }

    private void ConfigureAnimations()
    {
        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior =>
        {
            foreach (var (name, definition) in _animationDefinitions)
            {
                behavior.SetAnimationRects(name, definition.Frames, definition.FrameDelay);
            }
        });
    }

    private void ResetPosition()
    {
        var map = _globals.Map;
        if (map == null) return;
        var startTile = map.HouseCenter;
        if (startTile is null)
            return;

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

        var map = _globals.Map ?? throw new InvalidOperationException("Pac-Man map is not initialized.");
        var baseStep = GetBaseStepSize();
        var baseSpeed = _globals.CurrentPacmanSettings?.Speed ?? 80f;
        _character = new BlPacManCharacter(_env, map, Me, new BlPacManCharacterOptions
        {
            Step = baseStep,
            Speed = baseSpeed,
            Direction = BlPacManDirection.Left,
            Preturn = true,
        });
        _tileEnteredSubscription = _character.SubscribeTileEntered(OnTileEntered);
        return _character;
    }

    private float GetBaseStepSize()
    {
        return TileMath.GetMovementStep(_globals.Map);
    }

}
