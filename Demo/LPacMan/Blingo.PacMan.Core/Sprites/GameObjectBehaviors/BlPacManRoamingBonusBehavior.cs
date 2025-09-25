using System;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Controls the roaming bonus fruit, including spawn timing, navigation, and score reveal logic.
/// </summary>
internal sealed class BlPacManRoamingBonusBehavior : BlingoSpriteBehavior,
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
    private BlPacManGameBehavior? _coordinator;
    private GameSettings? _settings;
    private BlPacManCharacter? _character;
    private BlPacManEventSubscription? _pacManSubscription;
    private BlPacManEventSubscription? _tileSubscription;
    private BlPacManPositionContext? _pacManPosition;
    private Tile? _spawnTile;
    private Tile? _targetTile;
    private bool _animationsConfigured;
    private bool _active;
    private int _scoreValue;
    private int _remainingTargetVisits;
    private BlPacManDirection _direction = BlPacManDirection.Left;

    /// <summary>
    /// Initialises the bonus behaviour with the movie environment and shared globals.
    /// </summary>
    public BlPacManRoamingBonusBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    /// <summary>
    /// Applies the current level settings used to determine spawn/target tiles and score values.
    /// </summary>
    public void Configure(BlPacManGameBehavior coordinator, GameSettings settings)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _scoreValue = settings.BonusScore;

        if (_coordinator?.CurrentMap is { } map)
        {
            EnsureCharacter().SetMap(map);
            _targetTile = map.Tunnels.Count > 0 ? map.Tunnels[0] : map.HouseCenter;
            _spawnTile = map.Tunnels.Count > 0 ? map.Tunnels[^1] : map.HouseCenter;
        }
    }

    /// <summary>
    /// Registers the bonus with the coordinator and starts listening to Pac-Man position updates.
    /// </summary>
    public void BeginSprite()
    {
        _coordinator = _globals.GameBehavior;
        ApplyAppearance();
        var character = EnsureCharacter();
        character.BeginSprite();
        character.Hide();

        _pacManSubscription?.Release();
        _pacManSubscription = _coordinator?.SubscribePacManPosition(OnPacManPositionChanged);
        _coordinator?.RegisterBonus(this);
    }

    /// <summary>
    /// Releases event subscriptions when the sprite is removed.
    /// </summary>
    public void EndSprite()
    {
        _coordinator?.UnregisterBonus(this);
        _pacManSubscription?.Release();
        _pacManSubscription = null;
        _tileSubscription?.Release();
        _tileSubscription = null;
        _character = null;
    }

    /// <summary>
    /// Moves the fruit once it is active in the maze.
    /// </summary>
    public void Tick()
    {
        if (!_active)
        {
            return;
        }

        var character = EnsureCharacter();
        var tile = character.GetTile();
        if (tile is not null)
        {
            var next = GetNextDirection(tile);
            if (next != BlPacManDirection.None)
            {
                _direction = next;
            }
        }

        character.Move(_direction);
        CheckCollision(character);
    }

    /// <summary>
    /// Spawns the fruit and primes its target visit counter.
    /// </summary>
    public void Activate()
    {
        var character = EnsureCharacter();
        character.Reset();
        character.Show();
        _active = true;
        _remainingTargetVisits = 2;
        _direction = BlPacManDirection.Left;
        SetAnimation("default");
    }

    /// <summary>
    /// Removes the fruit from the maze and resets animations.
    /// </summary>
    public void Deactivate()
    {
        _active = false;
        _remainingTargetVisits = 0;
        _direction = BlPacManDirection.Left;
        var character = EnsureCharacter();
        character.Hide();
        SetAnimation("default");
    }

    /// <summary>
    /// Hides the fruit when Pac-Man loses a life.
    /// </summary>
    public void ResetForLife()
    {
        Deactivate();
    }

    /// <summary>
    /// Ensures the fruit is hidden during the death animation.
    /// </summary>
    public void OnPacManEaten()
    {
        Deactivate();
    }

    /// <summary>
    /// Switches the sprite to display the awarded score after being eaten.
    /// </summary>
    public void ShowScore()
    {
        if (_scoreValue > 0)
        {
            SetAnimation($"score{_scoreValue}");
        }
    }

    /// <summary>
    /// Configures the sprite's member and default animation frames.
    /// </summary>
    private void ApplyAppearance()
    {
        var cast = CastLib("Data");
        var member = cast?.GetMember<BlingoMemberBitmap>("misc");
        if (member != null)
        {
            Me.Member = member;
            Me.MemberSourceRect = DefaultAnimation[0];
        }

        var map = _coordinator?.CurrentMap ?? _globals.MapProvider?.CurrentMap;
        _spawnTile = map?.Tunnels.Count > 0 ? map.Tunnels[^1] : map?.HouseCenter;
        _targetTile = map?.Tunnels.Count > 0 ? map.Tunnels[0] : map?.HouseCenter;

        if (_spawnTile is not null)
        {
            Me.LocH = _spawnTile.CenterX;
            Me.LocV = _spawnTile.CenterY + VerticalOffset;
        }

        EnsureAnimations();
        SetAnimation("default");
    }

    /// <summary>
    /// Lazily instantiates the character helper so the bonus can traverse the tile map.
    /// </summary>
    private BlPacManCharacter EnsureCharacter()
    {
        if (_character is not null)
        {
            return _character;
        }

        var map = _coordinator?.CurrentMap ?? _globals.MapProvider?.CurrentMap ?? throw new InvalidOperationException("Pac-Man map is not initialized.");
        _character = new BlPacManCharacter(_env, map, Me, new BlPacManCharacterOptions
        {
            Step = 8f,
            Speed = 40f,
            Direction = BlPacManDirection.Left,
            Preturn = true,
        });

        _tileSubscription?.Release();
        _tileSubscription = _character.SubscribeTileEntered(OnTileEntered);

        return _character;
    }

    /// <summary>
    /// Registers each possible score animation lazily.
    /// </summary>
    private void EnsureAnimations()
    {
        if (_animationsConfigured)
        {
            return;
        }

        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior =>
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

    /// <summary>
    /// Applies the named animation if it has been registered.
    /// </summary>
    private void SetAnimation(string name)
    {
        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior => behavior.Play(name));
    }

    /// <summary>
    /// Stores Pac-Man's current tile for collision checks.
    /// </summary>
    private void OnPacManPositionChanged(BlPacManPositionContext context)
    {
        _pacManPosition = context;
    }

    /// <summary>
    /// Tracks visits to the target tile so the fruit despawns after reaching its end point twice.
    /// </summary>
    private void OnTileEntered(BlPacManTileContext context)
    {
        if (!_active || context is null)
        {
            return;
        }

        if (_targetTile is not null && ReferenceEquals(context.Tile, _targetTile))
        {
            _remainingTargetVisits--;
            if (_remainingTargetVisits <= 0)
            {
                _coordinator?.NotifyBonusExpired(this);
                Deactivate();
            }
        }
    }

    /// <summary>
    /// Determines the next direction that moves the fruit closer to its goal while avoiding walls.
    /// </summary>
    private BlPacManDirection GetNextDirection(Tile tile)
    {
        if (_targetTile is null)
        {
            return _direction;
        }

        var directions = new[]
        {
            BlPacManDirection.Up,
            BlPacManDirection.Left,
            BlPacManDirection.Down,
            BlPacManDirection.Right
        };

        var current = _direction == BlPacManDirection.None ? BlPacManDirection.Left : _direction;
        BlPacManDirection bestDirection = current;
        var bestDistance = float.PositiveInfinity;

        foreach (var direction in directions)
        {
            if (direction == current.GetOpposite())
            {
                continue;
            }

            if (!CanMove(direction, tile))
            {
                continue;
            }

            var candidate = tile.Get(direction);
            var distance = TileMath.GetDistance(candidate, _targetTile);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = direction;
            }
        }

        if (bestDistance.Equals(float.PositiveInfinity))
        {
            var fallback = current.GetOpposite();
            if (fallback != BlPacManDirection.None && CanMove(fallback, tile))
            {
                return fallback;
            }

            return current;
        }

        return bestDirection;
    }

    /// <summary>
    /// Indicates whether the fruit can travel into the specified tile.
    /// </summary>
    private bool CanMove(BlPacManDirection direction, Tile tile)
    {
        var next = tile.Get(direction);
        return next is not null && !next.IsWall() && !next.IsHouse();
    }

    /// <summary>
    /// Detects collisions with Pac-Man and notifies the coordinator when the fruit is eaten.
    /// </summary>
    private void CheckCollision(BlPacManCharacter character)
    {
        if (!_active || _coordinator is null || _pacManPosition is not { Tile: { } pacTile })
        {
            return;
        }

        var tile = character.GetTile();
        if (tile is null)
        {
            return;
        }

        var opposite = _pacManPosition.Direction.GetOpposite();
        var crossed = opposite != BlPacManDirection.None && ReferenceEquals(pacTile.Get(_pacManPosition.Direction), tile);

        if (!ReferenceEquals(tile, pacTile) && !crossed)
        {
            return;
        }

        _active = false;
        ShowScore();
        _coordinator.NotifyBonusEaten(this);
    }

    /// <summary>
    /// Utility for generating sprite-sheet rectangles.
    /// </summary>
    private static ARect CreateFrame(int offsetX, int offsetY)
    {
        return new ARect(offsetX, offsetY, offsetX + FrameSize, offsetY + FrameSize);
    }
}
