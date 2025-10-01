using AbstUI.Primitives;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using System;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;


/// <summary>
/// Direct C# port of the original JavaScript ghost logic. The behaviour keeps the same state
/// machine so scatter/chase transitions, frightened handling, and house routines match the arcade.
/// </summary>
internal sealed class BlPacManGhostBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent
{
    private const float DeadSpeed = 130f;
    private const float HouseSpeed = 70f;

    private static readonly IReadOnlyDictionary<MrGhost, ARect> SpriteRects = BlPacManTheme.Ghosts.Sprites;
    private static readonly IReadOnlyDictionary<MrGhost, float> HouseOffsets = BlPacManTheme.Ghosts.HorizontalOffsets;
    private static readonly IReadOnlyDictionary<MrGhost, BlPacManDirection> InitialDirections = new Dictionary<MrGhost, BlPacManDirection>
    {
        [MrGhost.Blinky] = BlPacManDirection.Left,
        [MrGhost.Pinky] = BlPacManDirection.Down,
        [MrGhost.Inky] = BlPacManDirection.Up,
        [MrGhost.Sue] = BlPacManDirection.Up,
    };

    private readonly GlobalVars _globals;
    private readonly IPacManRandomSource _random;
    private readonly BlPacManEventMediator<BlPacManGhostBehavior> _modeFrightenedEntered = new();
    private readonly BlPacManEventMediator<BlPacManGhostBehavior> _modeFrightenedExited = new();

    private PMCharacter? _character;
    private BlPacManEventSubscription? _tileSubscription;

    private GhostSettings? _settings;
    private bool _configured;

    private GhostMode _mode = GhostMode.House;
    private GhostMode _globalMode = GhostMode.Scatter;

    private BlPacManDirection _dir = BlPacManDirection.Left;
    private BlPacManDirection _nextDir = BlPacManDirection.Left;
    private bool _turnBack;

    private float _baseSpeed = 75f;
    private float _tunnelSpeed = 75f;
    private float _frightenedSpeed = 60f;

    private int _frightenedFrames;
    private int _frightenedDurationFrames;
    private int _frightenedFlashWindow;
    private bool _frightenedBlink;
    private bool _frightenedAnimationsConfigured;

    private bool _startOutsideHouse;
    private bool _hasLeftHouse;

    private float _houseTop;
    private float _houseBottom;
    private Tile? _houseExitTile;
    private float _houseExitTileX;
    private float _houseCenterX;
    private float _houseCenterY;

    private Tile? _scatterTarget;
    private int _scatterTargetIndex;
    private int _frightenedTime;
    private int _waitTime;
    private Tile? _deadTarget;
    private Tile? _deadEndTile;
    private float _deadEndX;
    private float _deadEndY;
    private bool _deadPrepareEnter;

    private ARect? _defaultRect;
    private ARect? _savedRect;
    private PMCharacterAnimationType _defaultAnimation;
    private PMTimer? _houseTimer;
    private PMTimer? _frightenedTimer;
    private bool _housePrepareExit;
    private GhostMode _frightened;
    private int _initialHouseReleaseFrames;
    private int _houseReleaseFrames;
    private bool _frightenedNotificationActive;
    private bool _scoreAnimationsConfigured;
    private PMCharacterAnimationType _pendingScoreAnimation = PMCharacterAnimationType.Unknown;
    private PMCharacterAnimationType _nextAnimation = PMCharacterAnimationType.Unknown;

    public MrGhost GhostName { get; set; } = MrGhost.Inky;

    public bool IsFrightened => isFrightened();

    public bool IsDead => isDead();

    public bool isFrightened() => _mode == GhostMode.Frightened || _frightened != GhostMode.Unknown;

    public bool isDead() => _mode == GhostMode.Dead;

    internal Tile? CurrentTile => _character?.GetTile();

    public BlPacManGhostBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
        _random = env.GetRequiredService<IPacManRandomSource>();
    }

    public void BeginSprite()
    {
        ApplyAppearance();
        var character = EnsureCharacter();
        _tileSubscription = character.SubscribeTileEntered(OnTileEntered);
        _globals.GhostManager.AddGhost(this);

        if (_settings is not null)
            ConfigureInternal();
    }
    public void Configure(GhostSettings settings, bool startOutsideHouse, int initialHouseReleaseFrames = 0)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _startOutsideHouse = startOutsideHouse;
        _initialHouseReleaseFrames = initialHouseReleaseFrames;
        _houseReleaseFrames = initialHouseReleaseFrames;

        if (_character is not null)
            ConfigureInternal();
    }

    private void ConfigureInternal()
    {
        var map = _globals.Map ?? throw new InvalidOperationException("Pac-Man map is not initialised.");

        _baseSpeed = _settings?.Speed ?? _baseSpeed;
        _tunnelSpeed = _settings?.TunnelSpeed ?? _tunnelSpeed;
        _frightenedSpeed = _settings?.FrightenedSpeed ?? _frightenedSpeed;
        _frightenedDurationFrames = Math.Max(1, (int)Math.Round((_settings?.FrightenedDuration ?? TimeSpan.Zero).TotalSeconds * 60));
        _frightenedFlashWindow = (_settings?.FrightenedFlashes ?? 0) * BlPacManTheme.Ghosts.FrightenedFlashWindowFrames;

        _character!.SetMap(map);
        ConfigureTargets(map);
        PlaceAtStart(map);
        PrepareInitialState();

        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
        UpdateHouseExitAllowance();
        _configured = true;
    }

    private void PrepareInitialState()
    {
        _frightenedFrames = 0;
        _frightenedBlink = false;
        _frightened = GhostMode.Unknown;
        _deadPrepareEnter = false;
        NotifyModeFrightenedExited();
        _frightenedNotificationActive = false;
        _pendingScoreAnimation = PMCharacterAnimationType.Unknown;
        if (_startOutsideHouse)
        {
            _mode = _globalMode;
            _hasLeftHouse = true;
            _dir = DetermineInitialDirection();
            _nextDir = _dir;
        }
        else
        {
            _mode = GhostMode.House;
            _hasLeftHouse = false;
            _dir = DetermineInitialDirection();
            if (!_dir.IsVertical())
                _dir = BlPacManDirection.Up;

            _nextDir = _dir;
        }

        UpdateHouseExitAllowance();
    }



    private void ConfigureTargets(Map map)
    {
        if (_character == null) return;

        _houseCenterX = map.HouseCenter?.X ?? map.House?.X ?? _houseCenterX;
        _houseCenterY = map.HouseCenter?.Y ?? map.House?.Y ?? _houseCenterY;

        _deadTarget = map.House!.GetRight()!.GetUp()!;
        _deadEndX = _deadEndTile?.X ?? _houseCenterX;
        _deadEndY = _deadEndTile?.Y ?? _houseCenterY;
        _deadEndTile = map.HouseCenter ?? map.House;

        _houseTop = _character.Y - getTile().Height / 2;
        _houseBottom = _character.Y + getTile().Height / 2;
        _houseExitTile = map.House.GetRight()!;
        _houseExitTileX = _houseExitTile.X - map.TileWidth / 2;

        _scatterTarget = map.GetTileByIndex(_scatterTargetIndex);
    }

    private PMCharacter EnsureCharacter()
    {
        if (_character is not null)
            return _character;

        var map = _globals.Map ?? throw new InvalidOperationException("Pac-Man map is not initialised.");
        var baseStep = TileMath.GetMovementStep(map);
        var options = new BlPacManCharacterOptions
        {
            Step = baseStep,
            Speed = _baseSpeed,
            Direction = _dir,
            Preturn = true,
            Mode = GhostMode.House,
        };
        _frightenedTime = 5;
        _scatterTargetIndex = 0;
        _defaultAnimation = PMCharacterAnimationType.GhostUp;
        switch (GhostName)
        {
            case MrGhost.Pinky:
                options.Direction = BlPacManDirection.Down;
                _defaultAnimation = PMCharacterAnimationType.GhostDown;
                _waitTime = 4;
                break;
            case MrGhost.Blinky:
                options.Direction = BlPacManDirection.Left;
                _defaultAnimation = PMCharacterAnimationType.GhostLeft;
                _waitTime = 0;
                _scatterTargetIndex = 25;
                break;
            case MrGhost.Inky:
                options.Direction = BlPacManDirection.Up;
                _defaultAnimation = PMCharacterAnimationType.GhostUp;
                _waitTime = 6;
                _scatterTargetIndex = 979;
                break;
            case MrGhost.Sue:
                options.Direction = BlPacManDirection.Left;
                _defaultAnimation = PMCharacterAnimationType.GhostUp;
                _waitTime = 8;
                _scatterTargetIndex = 953;
                break;
            default:
                break;
        }
        _scatterTarget = map.GetTileByIndex(_scatterTargetIndex);
        _character = new PMCharacter(_env, map, Me, PMCharacter.CharacterType.Ghost, options);

        _tileSubscription?.Release();
        _tileSubscription = _character.SubscribeTileEntered(OnTileEntered);
        return _character;
    }

    public void EndSprite()
    {
        _globals.GhostManager.RemoveGhost(this);
        _tileSubscription?.Release();
        _tileSubscription = null;
        _character = null;
        _configured = false;
    }

    public void ExitFrame() => move();

    public void Move() => move();

    public void move()
    {
        if (!_configured || _character is null)
            return;

        if (_globals.State.IsGameplayFrozen)
        {
            update();
            return;
        }

        advanceFrightenedTimer();

        var tile = _character.GetTile();
        if (tile is null)
            return;

        if (shouldExitMode(tile))
        {
            onExitMode();
            tile = _character.GetTile();
            if (tile is null)
            {
                update();
                return;
            }
        }

        switch (_mode)
        {
            case GhostMode.Dead:
                moveDead(tile);
                break;
            case GhostMode.House:
                moveInsideHouse();
                break;
            default:
                moveNormal();
                break;
        }
    }

    public void OnEnterMode(GhostMode mode) => onEnterMode(mode);

    public void onEnterMode(GhostMode mode)
    {
        switch (mode)
        {
            case GhostMode.Dead:
                NotifyModeFrightenedExited();
                _deadPrepareEnter = false;
                if (_pendingScoreAnimation != PMCharacterAnimationType.Unknown)
                {
                    EnsureScoreAnimations();
                    _character?.SetAnimationOverride(_pendingScoreAnimation);
                    update();
                    _character?.SetAnimationOverride(PMCharacterAnimationType.Unknown);
                    _pendingScoreAnimation = PMCharacterAnimationType.Unknown;
                }
                break;
            case GhostMode.Frightened:
                _frightenedTimer = new PMTimer(_frightenedTime);
                NotifyModeFrightenedEntered();
                break;
            case GhostMode.House:
                _housePrepareExit = false;
                _character!.Speed = HouseSpeed; //70
                break;
        }

    }
    public void SetMode(GhostMode? mode) => setMode(mode);

    public void setMode(GhostMode? mode)
    {
        if (mode is null || mode == GhostMode.Unknown)
        {
            if (_frightened != GhostMode.Unknown)
            {
                _mode = _frightened;
                _frightened = GhostMode.Unknown;
            }
            else
                _mode = _globalMode;

            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            return;
        }

        if (mode == GhostMode.Frightened)
        {
            if (_mode == GhostMode.House && !_hasLeftHouse)
            {
                _frightened = GhostMode.Frightened;
                _frightenedFrames = 0;
                _frightenedBlink = false;
                _frightenedTimer = new PMTimer(_frightenedTime);
                NotifyModeFrightenedEntered();
                return;
            }

            if (_mode == GhostMode.Dead)
            {
                return;
            }

            enterFrightenedMode();
            return;
        }

        if (mode == GhostMode.Dead)
        {
            if (_mode == GhostMode.Frightened)
                NotifyModeFrightenedExited();
            _mode = GhostMode.Dead;
            _frightened = GhostMode.Unknown;
            _frightenedFrames = 0;
            _deadPrepareEnter = false;
            UpdateHouseExitAllowance();
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            onEnterMode(GhostMode.Dead);
            return;
        }

        if (mode == GhostMode.House)
        {
            if (_mode == GhostMode.Frightened)
                NotifyModeFrightenedExited();
            _mode = GhostMode.House;
            _frightened = GhostMode.Unknown;
            _frightenedFrames = 0;
            PrepareHouseState();
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            onEnterMode(GhostMode.House);
            return;
        }

        if (mode is GhostMode.Scatter or GhostMode.Chase)
        {
            if (_mode is GhostMode.Scatter or GhostMode.Chase && mode.Value != _mode)
            {
                _turnBack = true;
            }

            _globalMode = mode.Value;
            if (_mode == GhostMode.Frightened)
                NotifyModeFrightenedExited();
            _mode = mode.Value;
            _frightened = GhostMode.Unknown;
            _frightenedFrames = 0;
            UpdateHouseExitAllowance();
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            onEnterMode(_mode);
        }
    }



    public void OnPacManEaten() => Hide();

    public void ResetForLife()
    {
        if (!_configured || _character is null)
        {
            return;
        }

        _character.Reset();
        PrepareInitialState();
        _character.ForceDirection(_dir);
        _character.Move(_dir);
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
        update();
        Show();
    }

    public void OnEaten(int score)
    {
        _pendingScoreAnimation = score switch
        {
            200 => PMCharacterAnimationType.GhostScore200,
            400 => PMCharacterAnimationType.GhostScore400,
            800 => PMCharacterAnimationType.GhostScore800,
            1_600 => PMCharacterAnimationType.GhostScore1600,
            _ => PMCharacterAnimationType.Unknown,
        };
        setMode(GhostMode.Dead);
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
    }

    public void Show() => EnsureCharacter().Show();

    public void Hide() => EnsureCharacter().Hide();

    internal void onExitMode()
    {
        switch (_mode)
        {
            case GhostMode.Dead:
                ReviveInHouse();
                break;
            case GhostMode.Frightened:
                NotifyModeFrightenedExited();
                _mode = _globalMode;
                UpdateSpeedForCurrentTile();
                UpdateModeVisual();
                break;
            case GhostMode.House:
                CompleteHouseExit();
                break;
            default:
                _mode = _globalMode;
                _turnBack = true;
                UpdateSpeedForCurrentTile();
                UpdateModeVisual();
                break;
        }
    }

    public void ExitCurrentMode() => onExitMode();

    internal void OnExitMode() => onExitMode();

    public BlPacManEventSubscription SubscribeModeFrightenedEntered(Action<BlPacManGhostBehavior> handler)
        => _modeFrightenedEntered.Subscribe(handler);

    public BlPacManEventSubscription SubscribeModeFrightenedExited(Action<BlPacManGhostBehavior> handler)
        => _modeFrightenedExited.Subscribe(handler);

  

 
    private Tile getTile() => _character!.GetTile()!;

    private void PlaceAtStart(Map map)
    {
        if (_startOutsideHouse)
        {
            var doorway = map.House?.GetRight();
            if (doorway is not null)
            {
                Me.LocH = doorway.X;
                Me.LocV = doorway.Y;
            }
            else
            {
                Me.LocH = _houseCenterX;
                Me.LocV = _houseCenterY;
            }
        }
        else
        {
            var center = map.HouseCenter ?? map.House;
            var offset = HouseOffsets.TryGetValue(GhostName, out var value) ? value : 0f;
            if (center is not null)
            {
                Me.LocH = center.X + offset;
                Me.LocV = center.Y;
            }
            else
            {
                Me.LocH = _houseCenterX + offset;
                Me.LocV = _houseCenterY;
            }
        }

        _character?.UpdateStartPosition(Me.LocH, Me.LocV);
    }

    private void moveNormal()
    {
        if (_character is null)
            return;

        var direction = _dir != BlPacManDirection.None ? _dir : _character.Direction;
        if (direction == BlPacManDirection.None)
        {
            direction = BlPacManDirection.Left;
        }

        _character.Move(direction);
    }


    public void pause()
    {
        _houseTimer?.Pause();
        _frightenedTimer?.Pause();
    }

    public void resume()
    {
        if (_mode == GhostMode.Frightened)
            _frightenedTimer?.Resume();
        if (_mode == GhostMode.House && !_housePrepareExit)
            _houseTimer?.Resume();
    }

    public void Pause() => pause();

    public void Resume() => resume();

    private void moveInsideHouse()
    {
        if (_character is null)
        {
            return;
        }

        if (_houseTimer == null) _houseTimer = new PMTimer(_waitTime);

        var tile = getTile();

        if (!_housePrepareExit && _houseTimer.HasElapsed() && !tile.IsWall())
        {
            _housePrepareExit = true;
            Me.LocV = tile.Y;
        }

        if (_frightened == GhostMode.Frightened && (_frightenedTimer != null && _frightenedTimer.HasElapsed()))
        {
            _frightened = GhostMode.Unknown;
        }

        if (_housePrepareExit)
        {
            if (Me.LocH < _houseExitTileX) _dir = BlPacManDirection.Right;
            else if (Me.LocH > _houseExitTileX) _dir = BlPacManDirection.Left;
            else _dir = BlPacManDirection.Up;

            if (_dir == BlPacManDirection.Up)
                Me.LocV -= getMin(getStep(), Me.LocV - (_houseExitTile?.GetUp()?.Y ?? Me.LocV));
            if (_dir == BlPacManDirection.Right)
                Me.LocH += getMin(getStep(), _houseExitTileX - Me.LocH);
            if (_dir == BlPacManDirection.Left)
                Me.LocH -= getMin(getStep(), Me.LocH - _houseExitTileX);

        }
        else
        {
            if (Me.LocV <= _houseTop && _dir == BlPacManDirection.Up) _dir = BlPacManDirection.Down;
            if (Me.LocV >= _houseBottom && _dir == BlPacManDirection.Down) _dir = BlPacManDirection.Up;

            if (_dir == BlPacManDirection.Up)
                Me.LocV -= getMin(getStep(), Me.LocV - _houseTop);
            if (_dir == BlPacManDirection.Down)
                Me.LocV += getMin(getStep(), _houseBottom - Me.LocV);

        }

        setNextAnimation();
        update();
    }

    private void moveDead(Tile tile)
    {
        if (_character is null)
        {
            return;
        }

        if (!_deadPrepareEnter && _deadTarget is not null && ReferenceEquals(tile, _deadTarget))
        {
            _deadPrepareEnter = true;
        }

        if (!_deadPrepareEnter)
        {
            _character.Move(_dir);
            return;
        }

        var targetX = _deadEndX;
        if (Me.LocV < _deadEndY && _deadTarget is not null)
        {
            targetX = _deadTarget.X;
        }

        if (Math.Abs(Me.LocH - targetX) > 0.5f)
        {
            var direction = Me.LocH < targetX ? BlPacManDirection.Right : BlPacManDirection.Left;
            if (_dir != direction)
            {
                _dir = direction;
                _character.ForceDirection(direction);
            }

            _character.Move(direction);
            return;
        }

        if (Me.LocV < _deadEndY - 0.5f)
        {
            if (_dir != BlPacManDirection.Down)
            {
                _dir = BlPacManDirection.Down;
                _character.ForceDirection(_dir);
            }

            _character.Move(BlPacManDirection.Down);
            return;
        }

        if (Me.LocV > _deadEndY + 0.5f)
        {
            if (_dir != BlPacManDirection.Up)
            {
                _dir = BlPacManDirection.Up;
                _character.ForceDirection(_dir);
            }

            _character.Move(BlPacManDirection.Up);
            return;
        }

        ReviveInHouse();
    }

    private void ReviveInHouse()
    {
        _mode = GhostMode.House;
        _hasLeftHouse = false;
        _deadPrepareEnter = false;
        Me.LocH = _deadEndX;
        Me.LocV = _deadEndY;
        _character?.UpdateStartPosition(Me.LocH, Me.LocV);
        PrepareHouseState();
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
    }

    private void PrepareHouseState()
    {
        _housePrepareExit = _startOutsideHouse;
        _dir = BlPacManDirection.Up;
        _nextDir = _dir;
        _character?.ForceDirection(_dir);
        if (_character is not null)
        {
            _character.Speed = HouseSpeed;
            _character.EffectiveSpeed = HouseSpeed;
        }
        UpdateHouseExitAllowance();
    }

    private void CompleteHouseExit()
    {
        _hasLeftHouse = true;
        _housePrepareExit = false;
        _mode = _globalMode;
        _dir = BlPacManDirection.Up;
        _nextDir = BlPacManDirection.Up;
        _character?.ForceDirection(BlPacManDirection.Up);
        UpdateHouseExitAllowance();
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
    }

    private void advanceFrightenedTimer()
    {
        if (_mode != GhostMode.Frightened)
            return;

        if (_frightenedFrames < _frightenedDurationFrames)
            _frightenedFrames++;

        if (_frightenedFlashWindow > 0 && _frightenedFrames >= Math.Max(1, _frightenedDurationFrames - _frightenedFlashWindow))
            _frightenedBlink = true;

        if (_frightenedFrames >= _frightenedDurationFrames)
            onExitMode();
    }

    private bool shouldExitMode(Tile tile)
    {
        return _mode switch
        {
            GhostMode.Dead => _deadEndTile is not null && ReferenceEquals(tile, _deadEndTile),
            GhostMode.Frightened => _frightenedFrames >= _frightenedDurationFrames,
            GhostMode.House => _houseExitTile is not null && ReferenceEquals(tile, _houseExitTile.GetUp()),
            _ => _mode != _globalMode,
        };
    }

    private void enterFrightenedMode()
    {
        _mode = GhostMode.Frightened;
        _frightenedFrames = 0;
        _frightenedBlink = false;
        _turnBack = true;
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
        onEnterMode(GhostMode.Frightened);
    }

    private void OnTileEntered(BlPacManTileEventData context)
    {
        if (context.Tile is null || _character is null)
        {
            return;
        }

        UpdateSpeedForCurrentTile(context.Tile);

        if (_turnBack)
        {
            var opposite = _character.Direction.GetOpposite();
            if (opposite != BlPacManDirection.None)
            {
                _dir = opposite;
                _character.ForceDirection(opposite);
            }

            _nextDir = getNextDirection(context.Tile);
            _turnBack = false;
        }
        else
        {
            if (_nextDir != BlPacManDirection.None)
            {
                _dir = _nextDir;
                _character.ForceDirection(_dir);
            }

            _nextDir = getNextDirection(context.Tile);
        }

    }

    private BlPacManDirection getNextDirection(Tile tile)
    {
        if (_mode == GhostMode.Frightened)
        {
            return GetRandomDirection(tile);
        }

        var currentDirection = _dir != BlPacManDirection.None ? _dir : _character?.Direction ?? BlPacManDirection.Left;
        var nextTile = tile.Get(currentDirection) ?? tile;
        var direction = _globals.State.PacManPosition?.Direction ?? BlPacManDirection.Left;
        var target = _mode == GhostMode.Chase
            ? PMGhostLogic.GetChaseTargetTile(_globals.GhostManager, GhostName, _character!, _globals.State.PacManPosition!.Tile!, direction, _scatterTarget!)
            : _mode == GhostMode.Scatter
                ? _scatterTarget
                : _deadTarget;

        var preferred = new[]
        {
            BlPacManDirection.Up,
            BlPacManDirection.Left,
            BlPacManDirection.Down,
            BlPacManDirection.Right,
        };

        var best = currentDirection;
        var bestDistance = float.PositiveInfinity;

        foreach (var option in preferred)
        {
            if (option == currentDirection.GetOpposite())
            {
                continue;
            }

            if (!CanGo(option, nextTile))
            {
                continue;
            }

            var candidate = nextTile.Get(option);
            var distance = TileMath.GetDistance(candidate, target);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = option;
            }
        }

        if (float.IsPositiveInfinity(bestDistance))
        {
            var reverse = currentDirection.GetOpposite();
            if (reverse != BlPacManDirection.None && CanGo(reverse, nextTile))
            {
                return reverse;
            }
        }

        return best;
    }

    private BlPacManDirection GetRandomDirection(Tile tile)
    {
        var currentDirection = _dir != BlPacManDirection.None ? _dir : _character?.Direction ?? BlPacManDirection.Left;
        var nextTile = tile.Get(currentDirection) ?? tile;

        var directions = new[] { BlPacManDirection.Up, BlPacManDirection.Right, BlPacManDirection.Down, BlPacManDirection.Left };
        var start = _random.Next(4);

        for (var i = 0; i < directions.Length; i++)
        {
            var option = directions[(start + i) % directions.Length];
            if (option == currentDirection.GetOpposite())
            {
                continue;
            }

            if (CanGo(option, nextTile))
            {
                return option;
            }
        }

        var reverse = currentDirection.GetOpposite();
        return reverse == BlPacManDirection.None ? currentDirection : reverse;
    }

   

    private float getStep()
    {
        if (_character is null)
        {
            return 0f;
        }

        return _character.Step * (_character.EffectiveSpeed / 100f);
    }

    private static float getMin(float a, float b) => MathF.Min(a, b);

    private void setNextAnimation()
    {
        if (_character is null)
        {
            return;
        }

        if (_dir != BlPacManDirection.None)
        {
            _character.ForceDirection(_dir);
        }

        _nextAnimation = _defaultAnimation;

        if (_mode == GhostMode.Dead)
        {
            switch (_dir)
            {
                case BlPacManDirection.Up:
                    _nextAnimation = PMCharacterAnimationType.GhostUp;
                    break;
                case BlPacManDirection.Right:
                    _nextAnimation = PMCharacterAnimationType.GhostRight;
                    break;
                case BlPacManDirection.Down:
                    _nextAnimation = PMCharacterAnimationType.GhostDown;
                    break;
                case BlPacManDirection.Left:
                    _nextAnimation = PMCharacterAnimationType.GhostLeft;
                    break;
            }
        }
        else if (_mode == GhostMode.Frightened || (_mode == GhostMode.House && _frightened == GhostMode.Frightened))
        {
            var threshold = (long)MathF.Max(1f, _frightenedTime * 0.75f);
            if (_frightenedTimer == null || !_frightenedTimer.HasElapsed(threshold))
            {
                _nextAnimation = PMCharacterAnimationType.GhostFrightenedBlue;
            }
            else
            {
                _nextAnimation = PMCharacterAnimationType.GhostFrightenedWhite;
            }
        }
        else
        {
            _nextAnimation = _dir switch
            {
                BlPacManDirection.Up => PMCharacterAnimationType.GhostUp,
                BlPacManDirection.Right => PMCharacterAnimationType.GhostRight,
                BlPacManDirection.Down => PMCharacterAnimationType.GhostDown,
                BlPacManDirection.Left => PMCharacterAnimationType.GhostLeft,
                _ => _defaultAnimation,
            };
        }
    }

    private void update()
    {
        if (_character is null)
        {
            return;
        }

        if (_nextAnimation != PMCharacterAnimationType.Unknown)
        {
            _character.SetAnimation(_nextAnimation);
        }

        _character.Update();
    }

    private void UpdateSpeedForCurrentTile(Tile? tile = null)
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
            speed = DeadSpeed;
        }
        else if (tile?.IsTunnel() == true)
        {
            speed = _tunnelSpeed;
        }

        if (_mode == GhostMode.House)
        {
            speed = HouseSpeed;
        }

        _character.EffectiveSpeed = speed;
    }

    private void UpdateModeVisual()
    {
        if (_mode == GhostMode.Frightened)
        {
            EnsureFrightenedAnimations();

            var animation = _frightenedBlink
                ? PMCharacterAnimationType.GhostFrightenedBlue
                : PMCharacterAnimationType.GhostFrightenedWhite;

            SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior => behavior.Play(animation));
            return;
        }

        if (_savedRect is not null)
            Me.SetMemberRect(_savedRect.Value, new APoint(_savedRect.Value.Width / 2f, _savedRect.Value.Height / 2f));
    }

    private void ApplyAppearance()
    {
        if (SpriteRects.TryGetValue(GhostName, out var rect))
        {
            Me.SetMemberRect(rect, new APoint(rect.Width / 2f, rect.Height / 2f));
            _defaultRect = rect;
            _savedRect = rect;
        }
        else
        {
            var size = BlPacManTheme.Ghosts.SpriteSize;
            var fallback = new ARect(0, 0, size, size);
            Me.SetMemberRect(fallback, new APoint(fallback.Width / 2f, fallback.Height / 2f));
            _defaultRect = fallback;
            _savedRect = fallback;
        }

        _frightenedAnimationsConfigured = false;
        _scoreAnimationsConfigured = false;
    }

    private BlPacManDirection DetermineInitialDirection()
    {
        return InitialDirections.TryGetValue(GhostName, out var value) ? value : BlPacManDirection.Left;
    }

    private bool CanGo(BlPacManDirection direction, Tile origin)
    {
        if (origin.IsGhostHouseEntrance() && direction == BlPacManDirection.Up)
        {
            return true;
        }

        var next = origin.Get(direction);
        if (next is null)
        {
            return false;
        }

        if (next.IsWall())
        {
            return false;
        }

        var insideHouse = origin.IsHouse() || origin.IsGhostHouseEntrance();
        if ((next.IsHouse() || next.IsGhostHouseEntrance()) && !insideHouse)
        {
            return false;
        }

        return !next.IsHouse();
    }

    private void UpdateHouseExitAllowance()
    {
        if (_character is null)
        {
            return;
        }

        _character.AllowHouseExit = _housePrepareExit || _mode == GhostMode.Dead;
    }

    private void EnsureFrightenedAnimations()
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

    private void EnsureScoreAnimations()
    {
        if (_scoreAnimationsConfigured)
        {
            return;
        }

        var animations = BlPacManTheme.Ghosts.ScoreAnimations;
        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior =>
        {
            foreach (var (name, definition) in animations)
            {
                behavior.SetAnimationRects(name, definition.Frames, definition.FrameDelay);
            }
        });

        _scoreAnimationsConfigured = true;
    }

    private void NotifyModeFrightenedEntered()
    {
        if (_frightenedNotificationActive)
        {
            return;
        }

        _frightenedNotificationActive = true;
        _modeFrightenedEntered.Publish(this);
    }

    private void NotifyModeFrightenedExited()
    {
        if (!_frightenedNotificationActive)
        {
            return;
        }

        _frightenedNotificationActive = false;
        _modeFrightenedExited.Publish(this);
    }

   
}
