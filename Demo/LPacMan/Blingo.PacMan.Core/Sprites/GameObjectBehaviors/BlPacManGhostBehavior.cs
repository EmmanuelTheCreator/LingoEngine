using AbstUI.Primitives;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using System.Collections.Generic;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Implements the runtime behaviour for one of the four ghosts, handling scatter/chase mode
/// transitions, frightened timers, collision detection, and target selection for each named ghost.
/// </summary>
internal sealed class BlPacManGhostBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent
{
    // Sprite sheet rectangles for each ghost's default animation row.
    private static readonly IReadOnlyDictionary<MrGhost, ARect> _ghostRects = BlPacManTheme.Ghosts.Sprites;

    // Horizontal offsets to stagger the ghosts within the house at level start.
    private static readonly IReadOnlyDictionary<MrGhost, float> _horizontalOffsets = BlPacManTheme.Ghosts.HorizontalOffsets;

    // Initial direction per ghost to mirror the arcade openings.
    private static readonly Dictionary<MrGhost, BlPacManDirection> _initialDirections = new()
    {
        [MrGhost.Blinky] = BlPacManDirection.Left,
        [MrGhost.Pinky] = BlPacManDirection.Down,
        [MrGhost.Inky] = BlPacManDirection.Up,
        [MrGhost.Clyde] = BlPacManDirection.Up,
    };
    private bool _isConfigured;
    private readonly GlobalVars _globals;
    private GhostSettings? _settings;
    private BlPacManCharacter? _character;
    private BlPacManEventSubscription? _tileSubscription;
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
    private const float HouseBoundaryTolerance = 0.5f;
    private bool _startOutsideHouse;
    private bool _houseExiting;
    private bool _hasLeftHouse;
    private int _initialHouseReleaseDelay;
    private int _houseReleaseCounter;
    private float _houseTopBoundary;
    private float _houseBottomBoundary;
    private Tile? _houseEntranceTile;
    private Tile? _houseDoorTile;
    private BlPacManDirection _houseDirection = BlPacManDirection.Up;
    private bool _frightenedAnimationsConfigured;
    private bool _frightenedFlashing;
    private int _frightenedDurationFrames;
    private ARect? _defaultRect;
    private ARect? _savedNormalRect;

    private void UpdateHouseExitAllowance()
    {
        if (_character is { } character)
        {
            character.AllowHouseExit = _houseExiting;
        }
    }

   

    /// <summary>
    /// Gets or sets the friendly name of the ghost. The name determines sprite offsets and
    /// targeting rules.
    /// </summary>
    public MrGhost GhostName { get; set; } = MrGhost.Inky;

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


    public BlPacManGhostBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
       : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }


    /// <summary>
    /// Initialises the ghost, binds to the coordinator, and subscribes to Pac-Man position updates.
    /// </summary>
    public void BeginSprite()
    {
        ApplyAppearance();

        var character = EnsureCharacter();
        character.AllowHouseExit = _houseExiting;
        character.BeginSprite();
        ConfigureTargets();
        UpdateSpeedForCurrentMode(character.GetTile());

        _globals.GhostManager.AddGhost(this);

        
    }

    /// <summary>
    /// Cleans up sprite state and unsubscribes from external events when the ghost is removed.
    /// </summary>
    public void EndSprite()
    {
        _globals.GhostManager.RemoveGhost(this);
        _tileSubscription?.Release();
        _tileSubscription = null;
        _character = null;
        _isConfigured = false;
    }

    /// <summary>
    /// Performs the per-frame movement and collision logic for the ghost.
    /// </summary>
    public void ExitFrame()
    {
        if (!_isConfigured) return;
        var character = EnsureCharacter();

        if (_globals.State.IsGameplayFrozen)
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

        if (_mode == GhostMode.House)
        {
            HandleHouseMode(character);
            return;
        }

        if (_mode == GhostMode.Dead && _deadTarget is not null && ReferenceEquals(tile, _deadTarget))
        {
            _mode = _globalMode;
            UpdateSpeedForCurrentMode(tile);
        }

        var currentDirection = character.Direction;
        if (currentDirection == BlPacManDirection.None) currentDirection = _requestedDirection;
        if (currentDirection == BlPacManDirection.None) currentDirection = BlPacManDirection.Left;

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
            _requestedDirection = GetRandomDirection(tile, currentDirection);
        else
        {
            var target = GetTargetTile();
            _requestedDirection = FindBestDirection(tile, currentDirection, target);
        }

        character.Move(_requestedDirection);
    }

    private void HandleHouseMode(BlPacManCharacter character)
    {
        character.AllowHouseExit = _houseExiting;

        if (!_hasLeftHouse && _globals.State.IsActivePlaying && _houseReleaseCounter > 0)
        {
            _houseReleaseCounter--;
            if (_houseReleaseCounter == 0)
            {
                _houseExiting = true;
                character.AllowHouseExit = true;
            }
        }

        if (!_houseExiting)
        {
            if (!_houseDirection.IsVertical())
            {
                _houseDirection = BlPacManDirection.Up;
                character.ForceDirection(_houseDirection);
            }

            _requestedDirection = _houseDirection;
            character.Move(_houseDirection);

            var currentY = Me.LocV;
            if (_houseDirection == BlPacManDirection.Up && currentY <= _houseTopBoundary + HouseBoundaryTolerance)
            {
                _houseDirection = BlPacManDirection.Down;
                character.ForceDirection(_houseDirection);
            }
            else if (_houseDirection == BlPacManDirection.Down && currentY >= _houseBottomBoundary - HouseBoundaryTolerance)
            {
                _houseDirection = BlPacManDirection.Up;
                character.ForceDirection(_houseDirection);
            }

            return;
        }

        if (_houseDoorTile is Tile door)
        {
            var targetX = door.CenterX;
            var deltaX = targetX - Me.LocH;
            if (Math.Abs(deltaX) > HouseBoundaryTolerance)
            {
                var direction = deltaX > 0 ? BlPacManDirection.Right : BlPacManDirection.Left;
                _requestedDirection = direction;
                character.Move(direction);
                return;
            }

            var exitY = door.CenterY;
            if (Me.LocV > exitY + HouseBoundaryTolerance)
            {
                _requestedDirection = BlPacManDirection.Up;
                character.Move(BlPacManDirection.Up);
                return;
            }
        }

        _mode = _globalMode;
        _hasLeftHouse = true;
        _houseExiting = false;
        character.AllowHouseExit = false;
        _requestedDirection = BlPacManDirection.Up;
        character.ForceDirection(BlPacManDirection.Up);
        _turnBack = false;
        UpdateSpeedForCurrentMode(character.GetTile());
        UpdateVisualForMode();
    }

    /// <summary>
    /// Updates the ghost's current mode, handling frightened overrides and scatter/chase flips.
    /// </summary>
    /// <param name="mode">The explicit mode to enter, or <c>null</c> to track the global mode.</param>
    public void SetMode(GhostMode? mode)
    {
        if (mode is null)
            mode = _mode == GhostMode.House && !_hasLeftHouse ? GhostMode.House : _globalMode;

        if (mode == GhostMode.House)
        {
            _mode = GhostMode.House;
            _frightenedFrames = 0;
            _houseExiting = _hasLeftHouse;
            UpdateHouseExitAllowance();
            UpdateSpeedForCurrentMode(_character?.GetTile());
            UpdateVisualForMode();
            return;
        }

        if (mode == GhostMode.Frightened)
        {
            EnterFrightenedMode();
            return;
        }

        if (mode == GhostMode.Scatter || mode == GhostMode.Chase)
        {
            if (_mode is GhostMode.Chase or GhostMode.Scatter && mode.Value != _mode)
                _turnBack = true;

            _globalMode = mode.Value;
        }

        _mode = mode.Value;
        if (_mode != GhostMode.Frightened)
            _frightenedFrames = 0;

        UpdateSpeedForCurrentMode(_character?.GetTile());
        UpdateVisualForMode();
    }

    /// <summary>
    /// Applies map references and per-level settings coming from the model.
    /// </summary>
    /// <param name="coordinator">The active gameplay coordinator.</param>
    /// <param name="settings">The ghost tuning for the current level.</param>
    public void Configure(GhostSettings settings, bool startOutsideHouse, int houseReleaseDelayFrames)
    {
        _settings = settings;

        _baseSpeed = settings.Speed;
        _tunnelSpeed = settings.TunnelSpeed;
        _frightenedSpeed = settings.FrightenedSpeed;

        _startOutsideHouse = startOutsideHouse;
        _initialHouseReleaseDelay = Math.Max(0, houseReleaseDelayFrames);
        _houseReleaseCounter = _initialHouseReleaseDelay;
        _houseExiting = startOutsideHouse;
        _hasLeftHouse = startOutsideHouse;
        UpdateHouseExitAllowance();

        var character = EnsureCharacter();
        var map = _globals.Map ?? throw new InvalidOperationException("Pac-Man map is not initialized.");
        character.SetMap(map);
        ConfigureHouseGeometry(map);
        SetStartposition();
        character.Speed = _baseSpeed;
        character.EffectiveSpeed = _baseSpeed;

        _initialDirection = DetermineInitialDirection();
        if (_startOutsideHouse)
        {
            _requestedDirection = _initialDirection;
            character.ForceDirection(_initialDirection);
            _mode = _globalMode;
        }
        else
        {
            _houseDirection = _initialDirection;
            if (!_houseDirection.IsVertical())
            {
                _houseDirection = BlPacManDirection.Up;
            }

            _requestedDirection = _houseDirection;
            character.ForceDirection(_houseDirection);
            _mode = GhostMode.House;
        }

        ConfigureTargets();
        UpdateSpeedForCurrentMode(character.GetTile());
        UpdateVisualForMode();
        _isConfigured = true;
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
        _frightenedFrames = 0;
        _frightenedDurationFrames = 0;
        _frightenedFlashing = false;
        _turnBack = false;
        _houseExiting = _startOutsideHouse;
        _hasLeftHouse = _startOutsideHouse;
        _houseReleaseCounter = _initialHouseReleaseDelay;
        UpdateHouseExitAllowance();
        if (_defaultRect is { } defaultRect)
        {
            _savedNormalRect = defaultRect;
        }

        if (_startOutsideHouse)
        {
            _mode = _globalMode;
            _requestedDirection = _initialDirection;
            character.ForceDirection(_initialDirection);
        }
        else
        {
            _mode = GhostMode.House;
            _houseDirection = _initialDirection;
            if (!_houseDirection.IsVertical())
            {
                _houseDirection = BlPacManDirection.Up;
            }

            _requestedDirection = _houseDirection;
            character.ForceDirection(_houseDirection);
        }

        UpdateSpeedForCurrentMode(character.GetTile());
    }

    /// <summary>
    /// Called when the ghost is eaten to adjust the frightened timer and ensure speed updates.
    /// </summary>
    /// <param name="score">The awarded score (unused, but retained for clarity).</param>
    public void OnEaten(int score)
    {
        if (!_globals.State.IsMuted)
            _Player.SoundPlayEat();

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
        //if (_ghostRects.TryGetValue(GhostName, out var rect))
        //{
        //    Me.MemberSourceRect = rect;
        //}
        //else
        //{
        //    Me.MemberSourceRect = new ARect(0, 0, 16, 16);
        //}

        
        if (_ghostRects.TryGetValue(GhostName, out var rect))
        {
            Me.MemberSourceRect = rect;
        }
        else
        {
            var size = BlPacManTheme.Ghosts.SpriteSize;
            Me.MemberSourceRect = new ARect(0, 0, size, size);
        }

        if (Me.MemberSourceRect is { } currentRect)
        {
            _defaultRect = currentRect;
            _savedNormalRect = currentRect;
        }
        else
        {
            _defaultRect = null;
            _savedNormalRect = null;
        }
        ConfigureFrightenedAnimations();

        SetStartposition();

        _initialDirection = DetermineInitialDirection();
        _requestedDirection = _initialDirection;
    }


    private bool SetStartposition()
    {
        var map = _globals.Map;
        if (map is null)
            return false;

        if (_startOutsideHouse && _houseDoorTile is not null)
        {
            Me.LocH = _houseDoorTile.CenterX;
            Me.LocV = _houseDoorTile.CenterY;
        }
        else
        {
            var center = map.HouseCenter ?? map.House ?? map.GetTile(map.Width / 2, map.Height / 2);
            if (center is null)
                return false;

            var offset = _horizontalOffsets.TryGetValue(GhostName, out var value) ? value : 0f;
            Me.LocH = center.CenterX + offset;
            Me.LocV = center.CenterY;
        }

        _character?.UpdateStartPosition(Me.LocH, Me.LocV);
        return true;
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

        var map = _globals.Map ?? throw new InvalidOperationException("Pac-Man map is not initialized.");
        var baseStep = TileMath.GetMovementStep(map);
        _character = new BlPacManCharacter(_env, map, Me, new BlPacManCharacterOptions
        {
            Step = baseStep,
            Speed = _baseSpeed,
            Direction = _requestedDirection,
            Preturn = true,
            Mode = BlPacManCharacterModes.Ghost,
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
        var map = _globals.Map;
        if (map is null)
            return;

        _scatterTarget = DetermineScatterTarget(map);
        _deadTarget = DetermineDeadTarget(map);
    }

    private void ConfigureFrightenedAnimations()
    {
        if (_frightenedAnimationsConfigured)
        {
            return;
        }

        var animations = BlPacManTheme.Ghosts.FrightenedAnimations;

        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior =>
        {
            foreach (var (name, definition) in animations)
            {
                behavior.SetAnimationRects(name, definition.Frames, definition.FrameDelay);
            }
        });

        _frightenedAnimationsConfigured = true;
    }

    private void UpdateVisualForMode()
    {
        if (_mode == GhostMode.Frightened)
        {
            ConfigureFrightenedAnimations();
            EnsureCharacter().SetAnimationOverride(
                _frightenedFlashing
                    ? BlPacManTheme.Ghosts.FrightenedFlashAnimation
                    : BlPacManTheme.Ghosts.FrightenedBlueAnimation);
        }
        else
        {
            RestoreNormalAppearance();
        }
    }

    private void RestoreNormalAppearance()
    {
        if (_character is { } character)
        {
            character.SetAnimationOverride(null);
        }

        if (_savedNormalRect is { } savedRect)
        {
            Me.MemberSourceRect = savedRect;
        }
        else if (_defaultRect is { } defaultRect)
        {
            Me.MemberSourceRect = defaultRect;
        }

        _frightenedFlashing = false;
    }

    private void UpdateFrightenedAnimationState()
    {
        if (_mode != GhostMode.Frightened || _settings is null)
        {
            return;
        }

        var flashes = _settings.FrightenedFlashes;
        if (flashes <= 0 || _frightenedDurationFrames <= 0)
        {
            return;
        }

        var flashWindow = Math.Min(_frightenedDurationFrames, flashes * BlPacManTheme.Ghosts.FrightenedFlashWindowFrames);
        if (flashWindow <= 0)
        {
            return;
        }

        var shouldFlash = _frightenedFrames <= flashWindow;

        if (shouldFlash == _frightenedFlashing)
        {
            return;
        }

        _frightenedFlashing = shouldFlash;
        EnsureCharacter().SetAnimationOverride(
            shouldFlash
                ? BlPacManTheme.Ghosts.FrightenedFlashAnimation
                : BlPacManTheme.Ghosts.FrightenedBlueAnimation);
    }

    private void ConfigureHouseGeometry(Map map)
    {
        _houseEntranceTile = map.HouseCenter;
        _houseDoorTile = _houseEntranceTile?.GetUp();

        var entranceY = _houseEntranceTile?.CenterY ?? Me.LocV;
        var doorY = _houseDoorTile?.CenterY ?? entranceY;

        _houseTopBoundary = Math.Min(entranceY, doorY);
        _houseBottomBoundary = Math.Max(entranceY, doorY);

        var lowerTile = _houseEntranceTile?.GetDown();
        if (lowerTile is not null && !lowerTile.IsWall())
        {
            _houseBottomBoundary = Math.Max(_houseBottomBoundary, lowerTile.CenterY);
        }

        if (Math.Abs(_houseBottomBoundary - _houseTopBoundary) < HouseBoundaryTolerance)
        {
            _houseBottomBoundary = _houseTopBoundary + map.TileHeight;
        }
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
            var name when name == MrGhost.Pinky => map.GetTile(0, 0),
            var name when name == MrGhost.Inky => map.GetTile(maxColumn, maxRow),
            var name when name == MrGhost.Clyde => map.GetTile(0, maxRow),
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
        return _initialDirections.TryGetValue(GhostName, out var direction) ? direction : BlPacManDirection.Left;
    }

    /// <summary>
    /// Reacts to tile changes by refreshing the movement speed for tunnels and frightened mode.
    /// </summary>
    private void OnTileEntered(BlPacManTileEventData context)
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

        UpdateFrightenedAnimationState();

        if (_frightenedFrames == 0)
        {
            _mode = _globalMode;
            _turnBack = true;
            UpdateSpeedForCurrentMode(_character?.GetTile());
            UpdateVisualForMode();
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
        var pacman = _globals.State.PacManPosition;
        if (pacman is not { Tile: { } pacTileNonNull })
            return null;

        var direction = pacman.Direction;

        if (GhostName == MrGhost.Pinky)
            return StepForward(pacTileNonNull, direction, 4) ?? pacTileNonNull;

        if (GhostName == MrGhost.Inky)
        {
            var ahead = StepForward(pacTileNonNull, direction, 2);
            var blinkyTile = _globals.GhostManager.FindGhost(MrGhost.Blinky)?.CurrentTile;

            if (ahead is null || blinkyTile is null)
            {
                return ahead ?? pacTileNonNull;
            }

            var offsetCol = ahead.Column - blinkyTile.Column;
            var offsetRow = ahead.Row - blinkyTile.Row;
            var targetMap = _globals.Map;
            return targetMap?.GetTile(ahead.Column + offsetCol, ahead.Row + offsetRow);
        }

        if (GhostName == MrGhost.Clyde)
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

        if (next.IsWall())
        {
            return false;
        }

        var insideHouseRegion = tile.IsHouse() || tile.IsGhostHouseEntrance();
        if (next.IsHouse() && !insideHouseRegion)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Switches the ghost into frightened mode, reversing its direction and slowing it down.
    /// </summary>
    private void EnterFrightenedMode()
    {
        if (_mode == GhostMode.Dead || (_mode == GhostMode.House && !_hasLeftHouse))
        {
            return;
        }

        _frightenedFrames = _settings is null
            ? 180
            : Math.Max(1, (int)Math.Round(_settings.FrightenedDuration.TotalSeconds * 60));

        if (Me.MemberSourceRect is { } frightenedEntryRect)
        {
            _savedNormalRect = frightenedEntryRect;
        }
        _frightenedDurationFrames = _frightenedFrames;
        _frightenedFlashing = false;

        _mode = GhostMode.Frightened;
        _turnBack = true;
        UpdateSpeedForCurrentMode(_character?.GetTile());
        UpdateVisualForMode();
    }
}
