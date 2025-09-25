using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Implements the runtime behaviour for one of the four ghosts. The class mirrors the original
/// JavaScript logic by handling scatter/chase mode transitions, frightened timers, collision
/// detection, and target selection for each named ghost.
/// </summary>
internal sealed class BlPacManGhostBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent
{
    // Sprite sheet rectangles for each ghost's default animation row.
    private static readonly Dictionary<string, ARect> GhostRects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Blinky"] = new ARect(0, 0, 32, 32),
        ["Pinky"] = new ARect(32, 0, 64, 32),
        ["Inky"] = new ARect(64, 0, 96, 32),
        ["Clyde"] = new ARect(96, 0, 128, 32),
    };

    // Horizontal offsets to stagger the ghosts within the house at level start.
    private static readonly Dictionary<string, float> HorizontalOffsets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Blinky"] = -32f,
        ["Pinky"] = -16f,
        ["Inky"] = 16f,
        ["Clyde"] = 32f,
    };

    // Initial direction per ghost to mirror the arcade openings.
    private static readonly Dictionary<string, BlPacManDirection> InitialDirections = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Blinky"] = BlPacManDirection.Left,
        ["Pinky"] = BlPacManDirection.Down,
        ["Inky"] = BlPacManDirection.Up,
        ["Clyde"] = BlPacManDirection.Up,
    };

    private readonly GlobalVars _globals;
    private BlPacManGameBehavior? _coordinator;
    private GhostSettings? _settings;
    private BlPacManCharacter? _character;
    private BlPacManEventSubscription? _tileSubscription;
    private BlPacManEventSubscription? _pacManSubscription;
    private BlPacManPositionContext? _pacManPosition;
    private Tile? _scatterTarget;
    private Tile? _deadTarget;
    private BlPacManDirection _requestedDirection = BlPacManDirection.Left;
    private BlPacManDirection _initialDirection = BlPacManDirection.Left;
    private float _baseSpeed = 75f;
    private float _tunnelSpeed = 75f;
    private float _frightenedSpeed = 60f;
    private GhostMode _mode = GhostMode.Scatter;
    private GhostMode _globalMode = GhostMode.Scatter;
    private int _frightenedFrames;
    private bool _turnBack;
    private readonly Random _random = new();

    public BlPacManGhostBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    /// <summary>
    /// Gets or sets the friendly name of the ghost. The name determines sprite offsets and
    /// targeting rules.
    /// </summary>
    public string GhostName { get; set; } = "Ghost";

    /// <summary>
    /// Gets a value indicating whether the ghost is currently frightened.
    /// </summary>
    public bool IsFrightened => _mode == GhostMode.Frightened;

    /// <summary>
    /// Gets a value indicating whether the ghost is travelling back to the house as eyes.
    /// </summary>
    public bool IsDead => _mode == GhostMode.Dead;

    /// <summary>
    /// Returns the tile currently occupied by the ghost's collision centre.
    /// </summary>
    internal Tile? CurrentTile => _character?.GetTile();

    /// <summary>
    /// Initialises the ghost, binds to the coordinator, and subscribes to Pac-Man position updates.
    /// </summary>
    public void BeginSprite()
    {
        _coordinator = _globals.GameBehavior;
        ApplyAppearance();

        var character = EnsureCharacter();
        character.BeginSprite();
        ConfigureTargets();
        UpdateSpeedForCurrentMode(character.GetTile());

        _pacManSubscription?.Release();
        _pacManSubscription = _coordinator?.SubscribePacManPosition(OnPacManPositionChanged);

        _coordinator?.RegisterGhost(this);
    }

    /// <summary>
    /// Cleans up sprite state and unsubscribes from external events when the ghost is removed.
    /// </summary>
    public void EndSprite()
    {
        _coordinator?.UnregisterGhost(this);
        _pacManSubscription?.Release();
        _pacManSubscription = null;
        _tileSubscription?.Release();
        _tileSubscription = null;
        _character = null;
    }

    /// <summary>
    /// Performs the per-frame movement and collision logic for the ghost.
    /// </summary>
    public void ExitFrame()
    {
        if (_coordinator is null)
        {
            return;
        }

        var character = EnsureCharacter();

        if (_coordinator.IsGameplayFrozen)
        {
            character.Update();
            return;
        }

        UpdateFrightenedTimer();

        var tile = character.GetTile();
        if (tile is null)
        {
            return;
        }

        if (_mode == GhostMode.Dead && _deadTarget is not null && ReferenceEquals(tile, _deadTarget))
        {
            _mode = _globalMode;
            UpdateSpeedForCurrentMode(tile);
        }

        var currentDirection = character.Direction;
        if (currentDirection == BlPacManDirection.None)
        {
            currentDirection = _requestedDirection;
        }
        if (currentDirection == BlPacManDirection.None)
        {
            currentDirection = BlPacManDirection.Left;
        }

        if (_turnBack)
        {
            _turnBack = false;
            var opposite = currentDirection.GetOpposite();
            if (opposite != BlPacManDirection.None)
            {
                _requestedDirection = opposite;
                character.ForceDirection(opposite);
                currentDirection = opposite;
            }
        }

        if (_mode == GhostMode.Frightened)
        {
            _requestedDirection = GetRandomDirection(tile, currentDirection);
        }
        else
        {
            var target = GetTargetTile();
            _requestedDirection = FindBestDirection(tile, currentDirection, target);
        }

        character.Move(_requestedDirection);
        UpdateCollision(character);
    }

    /// <summary>
    /// Updates the ghost's current mode, handling frightened overrides and scatter/chase flips.
    /// </summary>
    /// <param name="mode">The explicit mode to enter, or <c>null</c> to track the global mode.</param>
    public void SetMode(GhostMode? mode)
    {
        if (mode is null)
        {
            mode = _globalMode;
        }

        if (mode == GhostMode.Frightened)
        {
            EnterFrightenedMode();
            return;
        }

        if (mode == GhostMode.Scatter || mode == GhostMode.Chase)
        {
            if (_mode is GhostMode.Chase or GhostMode.Scatter && mode.Value != _mode)
            {
                _turnBack = true;
            }

            _globalMode = mode.Value;
        }

        if (mode == GhostMode.House)
        {
            mode = _globalMode;
        }

        _mode = mode.Value;
        if (_mode != GhostMode.Frightened)
        {
            _frightenedFrames = 0;
        }

        UpdateSpeedForCurrentMode(_character?.GetTile());
    }

    /// <summary>
    /// Applies map references and per-level settings coming from the model.
    /// </summary>
    /// <param name="coordinator">The active gameplay coordinator.</param>
    /// <param name="settings">The ghost tuning for the current level.</param>
    public void Configure(BlPacManGameBehavior coordinator, GhostSettings settings)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        _baseSpeed = settings.Speed;
        _tunnelSpeed = settings.TunnelSpeed;
        _frightenedSpeed = settings.FrightenedSpeed;

        var character = EnsureCharacter();
        var map = coordinator.CurrentMap ?? _globals.MapProvider?.CurrentMap ?? throw new InvalidOperationException("Pac-Man map is not initialized.");
        character.SetMap(map);
        character.Speed = _baseSpeed;
        character.EffectiveSpeed = _baseSpeed;

        _initialDirection = DetermineInitialDirection();
        _requestedDirection = _initialDirection;
        character.ForceDirection(_initialDirection);

        ConfigureTargets();
        UpdateSpeedForCurrentMode(character.GetTile());
    }

    /// <summary>
    /// Responds to Pac-Man losing a life by hiding the ghost until the restart animation finishes.
    /// </summary>
    public void OnPacManEaten() => Hide();

    /// <summary>
    /// Resets mode, speed, and direction after a Pac-Man death.
    /// </summary>
    public void ResetForLife()
    {
        var character = EnsureCharacter();
        character.Reset();
        _mode = _globalMode;
        _frightenedFrames = 0;
        _turnBack = false;
        _requestedDirection = _initialDirection;
        character.ForceDirection(_initialDirection);
        UpdateSpeedForCurrentMode(character.GetTile());
    }

    /// <summary>
    /// Called when the ghost is eaten to adjust the frightened timer and ensure speed updates.
    /// </summary>
    /// <param name="score">The awarded score (unused, but retained for clarity).</param>
    public void OnEaten(int score)
    {
        // Dead mode and speed adjustments are handled through SetMode and UpdateSpeedForCurrentMode.
        UpdateSpeedForCurrentMode(_character?.GetTile());
    }

    /// <summary>
    /// Shows the ghost sprite.
    /// </summary>
    public void Show() => EnsureCharacter().Show();

    /// <summary>
    /// Hides the ghost sprite.
    /// </summary>
    public void Hide() => EnsureCharacter().Hide();

    /// <summary>
    /// Loads the sprite artwork and positions the ghost at the house entrance.
    /// </summary>
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

        var map = _coordinator?.CurrentMap ?? _globals.MapProvider?.CurrentMap;
        var center = map?.HouseCenter ?? map?.House ?? map?.GetTile(map.Width / 2, map.Height / 2);
        if (center != null)
        {
            var offset = HorizontalOffsets.TryGetValue(GhostName, out var value) ? value : 0f;
            Me.LocH = center.CenterX + offset;
            Me.LocV = center.CenterY;
        }

        _initialDirection = DetermineInitialDirection();
        _requestedDirection = _initialDirection;
    }

    /// <summary>
    /// Lazily creates the shared character helper used to move the sprite around the tile map.
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
            Speed = _baseSpeed,
            Direction = _requestedDirection,
            Preturn = true,
        });

        _tileSubscription?.Release();
        _tileSubscription = _character.SubscribeTileEntered(OnTileEntered);

        return _character;
    }

    /// <summary>
    /// Resolves the scatter and dead targets based on the active map.
    /// </summary>
    private void ConfigureTargets()
    {
        var map = _coordinator?.CurrentMap ?? _globals.MapProvider?.CurrentMap;
        if (map is null)
        {
            return;
        }

        _scatterTarget = DetermineScatterTarget(map);
        _deadTarget = DetermineDeadTarget(map);
    }

    /// <summary>
    /// Returns the corner tile that the ghost should orbit during scatter mode.
    /// </summary>
    private Tile? DetermineScatterTarget(Map map)
    {
        var maxColumn = Math.Max(0, map.Width - 1);
        var maxRow = Math.Max(0, map.Height - 1);

        return GhostName switch
        {
            var name when name.Equals("Pinky", StringComparison.OrdinalIgnoreCase) => map.GetTile(0, 0),
            var name when name.Equals("Inky", StringComparison.OrdinalIgnoreCase) => map.GetTile(maxColumn, maxRow),
            var name when name.Equals("Clyde", StringComparison.OrdinalIgnoreCase) => map.GetTile(0, maxRow),
            _ => map.GetTile(maxColumn, 0),
        };
    }

    /// <summary>
    /// Identifies the tile inside the ghost house used when returning as eyes.
    /// </summary>
    private static Tile? DetermineDeadTarget(Map map)
    {
        return map.HouseCenter ?? map.GetTile(map.Width / 2, map.Height / 2);
    }

    /// <summary>
    /// Looks up the initial direction for the named ghost.
    /// </summary>
    private BlPacManDirection DetermineInitialDirection()
    {
        return InitialDirections.TryGetValue(GhostName, out var direction) ? direction : BlPacManDirection.Left;
    }

    /// <summary>
    /// Reacts to tile changes by refreshing the movement speed for tunnels and frightened mode.
    /// </summary>
    private void OnTileEntered(BlPacManTileContext context)
    {
        if (context is null)
        {
            return;
        }

        UpdateSpeedForCurrentMode(context.Tile);
    }

    /// <summary>
    /// Stores the latest Pac-Man position for future target calculations.
    /// </summary>
    private void OnPacManPositionChanged(BlPacManPositionContext context)
    {
        _pacManPosition = context;
    }

    /// <summary>
    /// Applies the correct effective speed based on the current mode and tile type.
    /// </summary>
    private void UpdateSpeedForCurrentMode(Tile? tile)
    {
        if (_character is null)
        {
            return;
        }

        var speed = _baseSpeed;
        if (_mode == GhostMode.Frightened)
        {
            speed = _frightenedSpeed;
        }
        else if (_mode == GhostMode.Dead)
        {
            speed = 130f;
        }
        else if (tile?.IsTunnel() == true)
        {
            speed = _tunnelSpeed;
        }

        _character.EffectiveSpeed = speed;
    }

    /// <summary>
    /// Counts down the frightened timer and returns the ghost to the global mode when it expires.
    /// </summary>
    private void UpdateFrightenedTimer()
    {
        if (_mode != GhostMode.Frightened)
        {
            return;
        }

        if (_frightenedFrames > 0)
        {
            _frightenedFrames--;
        }

        if (_frightenedFrames == 0)
        {
            _mode = _globalMode;
            _turnBack = true;
            UpdateSpeedForCurrentMode(_character?.GetTile());
        }
    }

    /// <summary>
    /// Chooses the direction that brings the ghost closest to the target while avoiding reversals.
    /// </summary>
    private BlPacManDirection FindBestDirection(Tile tile, BlPacManDirection currentDirection, Tile? target)
    {
        var directions = new[]
        {
            BlPacManDirection.Up,
            BlPacManDirection.Left,
            BlPacManDirection.Down,
            BlPacManDirection.Right
        };

        BlPacManDirection bestDirection = currentDirection;
        var bestDistance = float.PositiveInfinity;

        foreach (var direction in directions)
        {
            if (direction == currentDirection.GetOpposite())
            {
                continue;
            }

            if (!CanMove(direction, tile))
            {
                continue;
            }

            var candidate = tile.Get(direction);
            var distance = TileMath.GetDistance(candidate, target);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = direction;
            }
        }

        if (bestDistance.Equals(float.PositiveInfinity))
        {
            var opposite = currentDirection.GetOpposite();
            if (opposite != BlPacManDirection.None && CanMove(opposite, tile))
            {
                return opposite;
            }

            return currentDirection;
        }

        return bestDirection;
    }

    /// <summary>
    /// Selects a random available direction during frightened mode, avoiding reversals where possible.
    /// </summary>
    private BlPacManDirection GetRandomDirection(Tile tile, BlPacManDirection currentDirection)
    {
        var options = new List<BlPacManDirection>
        {
            BlPacManDirection.Up,
            BlPacManDirection.Left,
            BlPacManDirection.Down,
            BlPacManDirection.Right
        };

        for (var i = options.Count - 1; i >= 0; i--)
        {
            var option = options[i];
            if (option == currentDirection.GetOpposite() || !CanMove(option, tile))
            {
                options.RemoveAt(i);
            }
        }

        if (options.Count == 0)
        {
            return currentDirection.GetOpposite();
        }

        var index = _random.Next(options.Count);
        return options[index];
    }

    /// <summary>
    /// Computes the tile the ghost should head towards based on its current mode.
    /// </summary>
    private Tile? GetTargetTile()
    {
        return _mode switch
        {
            GhostMode.Chase => GetChaseTargetTile(),
            GhostMode.Dead => _deadTarget,
            GhostMode.Scatter => _scatterTarget,
            _ => _scatterTarget,
        };
    }

    /// <summary>
    /// Calculates the chase target tile according to each ghost's personality rules.
    /// </summary>
    private Tile? GetChaseTargetTile()
    {
        if (_pacManPosition is not { Tile: { } pacTileNonNull })
        {
            return null;
        }

        var direction = _pacManPosition.Direction;

        if (GhostName.Equals("Pinky", StringComparison.OrdinalIgnoreCase))
        {
            return StepForward(pacTileNonNull, direction, 4) ?? pacTileNonNull;
        }

        if (GhostName.Equals("Inky", StringComparison.OrdinalIgnoreCase))
        {
            var ahead = StepForward(pacTileNonNull, direction, 2);
            var blinkyTile = _coordinator?.FindGhost("Blinky")?.CurrentTile;

            if (ahead is null || blinkyTile is null)
            {
                return ahead ?? pacTileNonNull;
            }

            var offsetCol = ahead.Column - blinkyTile.Column;
            var offsetRow = ahead.Row - blinkyTile.Row;
            var targetMap = _coordinator?.CurrentMap ?? _globals.MapProvider?.CurrentMap;
            return targetMap?.GetTile(ahead.Column + offsetCol, ahead.Row + offsetRow);
        }

        if (GhostName.Equals("Clyde", StringComparison.OrdinalIgnoreCase))
        {
            var currentTile = _character?.GetTile();
            if (currentTile is null)
            {
                return pacTileNonNull;
            }

            var distance = TileMath.GetDistance(currentTile, pacTileNonNull);
            if (distance <= 8 * pacTileNonNull.Width)
            {
                return _scatterTarget;
            }

            return pacTileNonNull;
        }

        return pacTileNonNull;
    }

    /// <summary>
    /// Returns the tile lying a number of steps ahead in the provided direction.
    /// </summary>
    private static Tile? StepForward(Tile? tile, BlPacManDirection direction, int steps)
    {
        var current = tile;
        for (var i = 0; i < steps && current is not null; i++)
        {
            current = current.Get(direction);
        }

        return current;
    }

    /// <summary>
    /// Determines whether the ghost may enter the next tile, considering walls and the house rules.
    /// </summary>
    private bool CanMove(BlPacManDirection direction, Tile tile)
    {
        var next = tile.Get(direction);
        if (next is null)
        {
            return false;
        }

        if (_mode == GhostMode.Dead)
        {
            return !next.IsWall();
        }

        if (next.IsWall() || next.IsHouse())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks for overlap with Pac-Man and notifies the coordinator of the outcome.
    /// </summary>
    private void UpdateCollision(BlPacManCharacter character)
    {
        if (_coordinator is null || _mode == GhostMode.Dead || _pacManPosition is not { Tile: { } pacTile })
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

        if (_mode == GhostMode.Frightened)
        {
            SetMode(GhostMode.Dead);
            _coordinator.NotifyGhostEaten(this);
        }
        else
        {
            _coordinator.NotifyPacManEaten();
        }
    }

    /// <summary>
    /// Switches the ghost into frightened mode, reversing its direction and slowing it down.
    /// </summary>
    private void EnterFrightenedMode()
    {
        if (_mode == GhostMode.Dead)
        {
            return;
        }

        _frightenedFrames = _settings is null
            ? 180
            : Math.Max(1, (int)Math.Round(_settings.FrightenedDuration.TotalSeconds * 60));

        _mode = GhostMode.Frightened;
        _turnBack = true;
        UpdateSpeedForCurrentMode(_character?.GetTile());
    }
}
