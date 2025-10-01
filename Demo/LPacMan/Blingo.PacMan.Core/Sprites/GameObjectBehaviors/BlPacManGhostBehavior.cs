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
using static System.Formats.Asn1.AsnWriter;

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
    private float _houseExitX;
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
    private bool _deadPreparingEntry;

    private ARect? _defaultRect;
    private ARect? _savedRect;
    private PMCharacterAnimationType _defaultAnimation;
    private PMTimer? _houseTimer;
    private PMTimer? _frightenedTimer;
    private bool _housePrepareExit;
    private GhostMode _pendingFrightened;
    private bool _deadPrepareEnter;

    public MrGhost GhostName { get; set; } = MrGhost.Inky;

    public bool IsFrightened => _mode == GhostMode.Frightened || _pendingFrightened != GhostMode.Unknown;

    public bool IsDead => _mode == GhostMode.Dead;

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
    public void Configure(GhostSettings settings, bool startOutsideHouse)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _startOutsideHouse = startOutsideHouse;

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
        _pendingFrightened =  GhostMode.Unknown;
        _deadPreparingEntry = false;
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

        _deadTarget = map.House!.GetRight()!.GetUp()!;
        _deadEndX = _deadEndTile?.X ?? _houseCenterX;
        _deadEndY = _deadEndTile?.Y ?? _houseCenterY;
        _deadEndTile = map.HouseCenter ?? map.House;

        _houseTop = _character.Y - GetTile().Height / 2;
        _houseBottom = _character.Y + GetTile().Height / 2;
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

    public void ExitFrame()
    {
        if (!_configured || _character is null)
            return;

        if (_globals.State.IsGameplayFrozen)
        {
            _character.Update();
            return;
        }

        AdvanceFrightenedTimer();

        var tile = _character.GetTile();
        if (tile is null)
            return;

        if (ShouldExitMode(tile))
        {
            OnExitMode();
            tile = _character.GetTile();
            if (tile is null)
            {
                _character.Update();
                return;
            }
        }

        switch (_mode)
        {
            case GhostMode.Dead:
                MoveDead(tile);
                break;
            case GhostMode.House:
                MoveInsideHouse();
                break;
            default:
                MoveNormal();
                break;
        }

        _character.Update();
    }

    public void OnEnterMode(GhostMode mode)
    {
        switch (mode)
        {
            case GhostMode.Dead:
                // TODO
                //_emit('item:modefrightened:exit');
                _deadPrepareEnter = false;
                //_nextAnimation = this.animations[`score${ this.score}`];
                //update();
                break;
            case GhostMode.Frightened:
                _frightenedTimer = new PMTimer(_frightenedTime);
                // TODO
                //_emit('item:modefrightened:enter');
                break;
            case GhostMode.House:
                _housePrepareExit = false;
                _character!.Speed = HouseSpeed; //70
                break;
        }

    }
    public void SetMode(GhostMode? mode)
    {
        if (mode is null || mode == GhostMode.Unknown)
        {
            if (_pendingFrightened != GhostMode.Unknown)
            {
                _mode = _pendingFrightened;
                _pendingFrightened = GhostMode.Unknown;
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
                _pendingFrightened = GhostMode.Frightened;
                return;
            }

            if (_mode == GhostMode.Dead)
            {
                return;
            }

            EnterFrightenedMode();
            return;
        }

        if (mode == GhostMode.Dead)
        {
            _mode = GhostMode.Dead;
            _pendingFrightened = GhostMode.Unknown;
            _frightenedFrames = 0;
            _deadPreparingEntry = false;
            UpdateHouseExitAllowance();
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            return;
        }

        if (mode == GhostMode.House)
        {
            _mode = GhostMode.House;
            _pendingFrightened = GhostMode.Unknown;
            _frightenedFrames = 0;
            PrepareHouseState();
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            return;
        }

        if (mode is GhostMode.Scatter or GhostMode.Chase)
        {
            if (_mode is GhostMode.Scatter or GhostMode.Chase && mode.Value != _mode)
            {
                _turnBack = true;
            }

            _globalMode = mode.Value;
            _mode = mode.Value;
            _pendingFrightened = GhostMode.Unknown;
            _frightenedFrames = 0;
            UpdateHouseExitAllowance();
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
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
        _character.Update();
        Show();
    }

    public void OnEaten(int score)
    {
        SetMode(GhostMode.Dead);
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
    }

    public void Show() => EnsureCharacter().Show();

    public void Hide() => EnsureCharacter().Hide();

    internal void OnExitMode()
    {
        switch (_mode)
        {
            case GhostMode.Dead:
                ReviveInHouse();
                break;
            case GhostMode.Frightened:
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

  

 
    private Tile GetTile() => _character!.GetTile()!;

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

    private void MoveNormal()
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


    public void Pause()
    {
        _houseTimer?.Pause();
        _frightenedTimer?.Pause();
    }

    public void Resume()
    {
        if (_mode == GhostMode.Frightened) _frightenedTimer?.Resume();
        if (_mode == GhostMode.House && !_housePrepareExit) _houseTimer?.Resume();
    }

    private void MoveInsideHouse()
    {
        if (_character is null)
            return;
        if (_houseTimer == null) _houseTimer = new PMTimer(_waitTime);
        var tile = GetTile(); 
        
        if (!_housePrepareExit && _houseTimer.HasElapsed() && !tile.IsWall())
        {
            _housePrepareExit = true;
            Me.LocV = tile.Y;
        }

        if (_pendingFrightened == GhostMode.Frightened && (_frightenedTimer != null && _frightenedTimer.HasElapsed()))
            _pendingFrightened = GhostMode.Unknown;

        if (!_housePreparingExit)
        {
            if (!_dir.IsVertical())
            {
                _dir = BlPacManDirection.Up;
                _character.ForceDirection(_dir);
            }

            _character.Move(_dir);

            var y = Me.LocV;
            if (_dir == BlPacManDirection.Up && y <= _houseTop)
            {
                _dir = BlPacManDirection.Down;
                _character.ForceDirection(_dir);
            }
            else if (_dir == BlPacManDirection.Down && y >= _houseBottom)
            {
                _dir = BlPacManDirection.Up;
                _character.ForceDirection(_dir);
            }

            return;
        }

        var x = Me.LocH;
        if (Math.Abs(x - _houseExitX) > 0.5f)
        {
            var direction = x < _houseExitX ? BlPacManDirection.Right : BlPacManDirection.Left;
            if (_dir != direction)
            {
                _dir = direction;
                _character.ForceDirection(direction);
            }

            _character.Move(direction);
            return;
        }

        if (_dir != BlPacManDirection.Up)
        {
            _dir = BlPacManDirection.Up;
            _character.ForceDirection(BlPacManDirection.Up);
        }

        _character.Move(BlPacManDirection.Up);
    }

    private void MoveDead(Tile tile)
    {
        if (_character is null)
        {
            return;
        }

        if (!_deadPreparingEntry && _deadTarget is not null && ReferenceEquals(tile, _deadTarget))
        {
            _deadPreparingEntry = true;
        }

        if (!_deadPreparingEntry)
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
        _deadPreparingEntry = false;
        Me.LocH = _deadEndX;
        Me.LocV = _deadEndY;
        _character?.UpdateStartPosition(Me.LocH, Me.LocV);
        PrepareHouseState();
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
    }

    private void PrepareHouseState()
    {
        _housePreparingExit = _startOutsideHouse;
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
        _housePreparingExit = false;
        _mode = _globalMode;
        _dir = BlPacManDirection.Up;
        _nextDir = BlPacManDirection.Up;
        _character?.ForceDirection(BlPacManDirection.Up);
        UpdateHouseExitAllowance();
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
    }

    private void AdvanceFrightenedTimer()
    {
        if (_mode != GhostMode.Frightened)
            return;

        if (_frightenedFrames < _frightenedDurationFrames)
            _frightenedFrames++;

        if (_frightenedFlashWindow > 0 && _frightenedFrames >= Math.Max(1, _frightenedDurationFrames - _frightenedFlashWindow))
            _frightenedBlink = true;

        if (_frightenedFrames >= _frightenedDurationFrames)
            OnExitMode();
    }

    private bool ShouldExitMode(Tile tile)
    {
        return _mode switch
        {
            GhostMode.Dead => _deadEndTile is not null && ReferenceEquals(tile, _deadEndTile),
            GhostMode.Frightened => _frightenedFrames >= _frightenedDurationFrames,
            GhostMode.House => _houseExitTile is not null && ReferenceEquals(tile, _houseExitTile.GetUp()),
            _ => _mode != _globalMode,
        };
    }

    private void EnterFrightenedMode()
    {
        _mode = GhostMode.Frightened;
        _frightenedFrames = 0;
        _frightenedBlink = false;
        _turnBack = true;
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
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

            _nextDir = DetermineNextDirection(context.Tile);
            _turnBack = false;
        }
        else
        {
            if (_nextDir != BlPacManDirection.None)
            {
                _dir = _nextDir;
                _character.ForceDirection(_dir);
            }

            _nextDir = DetermineNextDirection(context.Tile);
        }

    }

    private BlPacManDirection DetermineNextDirection(Tile tile)
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

            if (!CanMove(option, nextTile))
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
            if (reverse != BlPacManDirection.None && CanMove(reverse, nextTile))
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

            if (CanMove(option, nextTile))
            {
                return option;
            }
        }

        var reverse = currentDirection.GetOpposite();
        return reverse == BlPacManDirection.None ? currentDirection : reverse;
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
    }

    private BlPacManDirection DetermineInitialDirection()
    {
        return InitialDirections.TryGetValue(GhostName, out var value) ? value : BlPacManDirection.Left;
    }

    private bool CanMove(BlPacManDirection direction, Tile origin)
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

        _character.AllowHouseExit = _housePreparingExit || _mode == GhostMode.Dead;
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

   
}
