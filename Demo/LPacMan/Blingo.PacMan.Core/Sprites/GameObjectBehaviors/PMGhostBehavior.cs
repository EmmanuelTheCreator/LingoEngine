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
internal sealed class PMGhostBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent
{
    #region Fields
    private const float _deadSpeed = 130f;
    private const float _houseSpeed = 70f;

    private static readonly IReadOnlyDictionary<MrGhost, ARect> _spriteRects = BlPacManTheme.Ghosts.Sprites;
    private static readonly IReadOnlyDictionary<MrGhost, float> _houseOffsets = BlPacManTheme.Ghosts.HorizontalOffsets;
   

    private readonly GlobalVars _globals;
    private readonly IPacManRandomSource _random;
    private readonly BlPacManEventMediator<PMGhostBehavior> _modeFrightenedEntered = new();
    private readonly BlPacManEventMediator<PMGhostBehavior> _modeFrightenedExited = new();

    private PMCharacter? _character;
    private BlPacManEventSubscription? _tileSubscription;

    private GhostSettings? _settings;
    private bool _configured;

    private GhostMode _mode => _character!.Mode;
    private GhostMode _globalMode = GhostMode.Scatter;

    private PMDirection _dir { get => _character!.Direction; set => _character!.Direction = value; }
    private PMDirection _moveDir = PMDirection.None;
    private PMDirection _nextDir = PMDirection.None;
    private PMTile? _lastTile { get => _character!.LastTile; set => _character!.LastTile = value; }
    private float _x { get => _character!.X; set => _character!.X = value; }
    private float _y { get => _character!.Y; set => _character!.Y = value; }
    private bool _turnBack;

    private float _baseSpeed = 75f;
    private float _tunnelSpeed = 75f;
    private float _frightenedSpeed = 60f;

    private int _frightenedFlashWindow;
    private bool _frightenedBlink;
    private bool _frightenedAnimationsConfigured;

    private bool _startOutsideHouse;
    private bool _hasLeftHouse;

    private float _houseTop;
    private float _houseBottom;
    private PMTile? _houseExitTile;
    private float _houseExitTileX;
    private float _houseCenterX;
    private float _houseCenterY;

    private PMTile? _scatterTarget;
    private int _scatterTargetIndex;
    private int _frightenedTime;
    private int _waitTime;
    private PMTile? _deadTarget;
    private PMTile? _deadEndTile;
    private float _deadEndX;
    private float _deadEndY;
    private bool _deadPrepareEnter;

    private ARect? _savedRect;
    private PMCharacterAnimationType _defaultAnimation;
    private PMTimer? _houseTimer;
    private PMTimer? _frightenedTimer;
    private bool _housePrepareExit;
    private GhostMode _frightened;
    private bool _frightenedNotificationActive;
    private PMCharacterAnimationType _pendingScoreAnimation = PMCharacterAnimationType.Unknown;
    private PMCharacterAnimationType _nextAnimation = PMCharacterAnimationType.Unknown;
    private bool _eatEvent;
    #endregion


    #region Properties

    public MrGhost GhostName { get; set; } = MrGhost.Inky;

    public bool IsFrightened => _mode == GhostMode.Frightened || _frightened != GhostMode.Unknown;

    public bool IsDead => _mode == GhostMode.Dead;

    internal PMTile? CurrentTile => _character?.GetTile(); 
    #endregion

    public PMGhostBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
        _random = env.GetRequiredService<IPacManRandomSource>();
    }

     public void Configure(GhostSettings settings, bool startOutsideHouse, int initialHouseReleaseFrames = 0)
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
        _frightenedFlashWindow = (_settings?.FrightenedFlashes ?? 0) * BlPacManTheme.Ghosts.FrightenedFlashWindowFrames;

        _character!.SetMap(map);

        _houseCenterX = map.HouseCenter?.X ?? map.House?.X ?? _houseCenterX;
        _houseCenterY = map.HouseCenter?.Y ?? map.House?.Y ?? _houseCenterY;
        

        _deadTarget = map.House!.GetRight()!.GetUp()!;
        _deadEndX = _deadEndTile?.X ?? _houseCenterX;
        _deadEndY = _deadEndTile?.Y ?? _houseCenterY;
        _deadEndTile = map.HouseCenter ?? map.House;

        _houseTop = _character.Y - GetTile().Height / 2;
        _houseBottom = _character.Y + GetTile().Height / 2;
        _houseExitTile = map.House.GetRight()!;
        _houseExitTileX = _houseExitTile.X - map.TileWidth / 2;

        _scatterTarget = map.GetTileByIndex(_scatterTargetIndex);


        PrepareInitialState();

        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
        _configured = true;
    }

    private void PrepareInitialState()
    {
        _frightenedBlink = false;
        _frightened = GhostMode.Unknown;
        _deadPrepareEnter = false;
        NotifyModeFrightenedExited();
        _frightenedNotificationActive = false;
        _pendingScoreAnimation = PMCharacterAnimationType.Unknown;
        _moveDir = PMDirection.None;
        _nextDir = PMDirection.None;
    }

  

    private PMCharacter EnsureCharacter()
    {
        if (_character is not null)
            return _character;

        var map = _globals.Map ?? throw new InvalidOperationException("Pac-Man map is not initialised.");
        var baseStep = PMTileMath.GetMovementStep(map);
        var options = new BlPacManCharacterOptions
        {
            Step = baseStep,
            Speed = _baseSpeed,
            Direction = PMDirection.Up,
            Preturn = true,
            Mode = GhostMode.House,
        };
        _frightenedTime = 5;
        _scatterTargetIndex = 0;
        _defaultAnimation = PMCharacterAnimationType.GhostUp;
        PMTile? startTile = null;
        var ghostHOffset = map.TileWidth /2; // 16
        float x = 0;
        float y = 0;
        switch (GhostName)
        {
            case MrGhost.Pinky:
                options.Direction = PMDirection.Down;
                _defaultAnimation = PMCharacterAnimationType.GhostDown;
                _waitTime = 4;
                startTile = map.HouseCenter!.GetRight()!;
                x = startTile.X - map.TileWidth / 2;
                y = startTile.Y;
                break;
            case MrGhost.Blinky:
                options.Direction = PMDirection.Left;
                _defaultAnimation = PMCharacterAnimationType.GhostLeft;
                _waitTime = 0;
                _scatterTargetIndex = 25;
                startTile = map.HouseCenter!.GetUp()!.GetRight()!;
                x = startTile.X - map.TileWidth / 2;
                y = startTile.Y;
                break;
            case MrGhost.Inky:
                options.Direction = PMDirection.Up;
                _defaultAnimation = PMCharacterAnimationType.GhostUp;
                _waitTime = 6;
                _scatterTargetIndex = 979;
                startTile = map.HouseCenter!.GetLeft()!;
                x = startTile.X - ghostHOffset;
                y = startTile.Y;
                break;
            case MrGhost.Sue:
                options.Direction = PMDirection.Left;
                _defaultAnimation = PMCharacterAnimationType.GhostUp;
                _waitTime = 8;
                _scatterTargetIndex = 953;
                startTile = map.HouseCenter!.GetRight()!.GetRight()!;
                x = startTile.X + ghostHOffset;
                y = startTile.Y;
                break;
            default:
                break;
        }
        _scatterTarget = map.GetTileByIndex(_scatterTargetIndex);
        options.StartTile = startTile ?? map.House;
        _character = new PMCharacter(_env, map, Me, PMCharacter.CharacterType.Ghost, options);
        _x = x;
        _y = y;
        _tileSubscription?.Release();
        _tileSubscription = _character.SubscribeTileEntered(OnTileEntered);
        _nextAnimation = _defaultAnimation;
        _moveDir = PMDirection.None;
        _nextDir = PMDirection.None;
        return _character;
    }

    public void BeginSprite()
    {
        
        ApplyAppearance();
        var character = EnsureCharacter();
        
        _tileSubscription?.Release();
        _tileSubscription = character.SubscribeTileEntered(OnTileEntered);
        _globals.GhostManager.AddGhost(this);

        if (_settings is not null)
            ConfigureInternal();

        // Force Update position after begin sprite has override the default position;
        _character!.BeginSprite();
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
            return;

        var tile = _character.GetTile();
        if (tile is null)
            return;
        Move();
    }


    private void OnTileEntered(BlPacManTileEventData context)
    {
        if (context.Tile is null || _character is null)
            return;
#if DEBUG
        if (context.Tile.Column == 3 && context.Tile.Row == 14)
        {

        }
#endif

        UpdateSpeedForCurrentTile(context.Tile);

        if (_turnBack)
        {
            var opposite = _character.Direction.GetOpposite();
            _dir = opposite;
            _moveDir = PMDirection.None;

            _nextDir = GetNextDirection(context.Tile);
            _turnBack = false;
        }
        else
        {
            _moveDir = _nextDir;
            _nextDir = GetNextDirection(context.Tile);
        }
    }


    public void Reset()
    {
        _character!.Reset();
        _moveDir = PMDirection.None;
        _nextDir = PMDirection.None;
        SetMode(GhostMode.House);
    }
    public void Pause()
    {
        _houseTimer?.Pause();
        _frightenedTimer?.Pause();
    }

    public void Resume()
    {
        if (_mode == GhostMode.Frightened)
            _frightenedTimer?.Resume();
        if (_mode == GhostMode.House && !_housePrepareExit)
            _houseTimer?.Resume();
    }
  
    private void Update()
    {
        _character!.SetAnimation(_nextAnimation);
        _character!.Update();
    }

    internal void SetGlobalMode(GhostMode mode)
    {
        _globalMode = mode;
    }

    public void SetMode(GhostMode? mode)
    {
        if (mode is null || mode == GhostMode.Unknown)
        {
            if (_frightened != GhostMode.Unknown)
            {
                _character!.Mode = _frightened;
                _frightened = GhostMode.Unknown;
            }
            else
                _character!.Mode = _globalMode;

            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            return;
        }

        if (mode == GhostMode.Frightened)
        {
            if (_mode == GhostMode.House && !_hasLeftHouse)
            {
                _frightened = GhostMode.Frightened;
                _frightenedBlink = false;
                _frightenedTimer = new PMTimer(_frightenedTime);
                NotifyModeFrightenedEntered();
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
            if (_mode == GhostMode.Frightened)
                NotifyModeFrightenedExited();
            _character!.Mode = GhostMode.Dead;
            _frightened = GhostMode.Unknown;
            _deadPrepareEnter = false;
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            OnEnterMode(GhostMode.Dead);
            return;
        }

        if (mode == GhostMode.House)
        {
            if (_mode == GhostMode.Frightened)
                NotifyModeFrightenedExited();
            _character!.Mode = GhostMode.House;
            _frightened = GhostMode.Unknown;
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            OnEnterMode(GhostMode.House);
            return;
        }

        if (mode is GhostMode.Scatter or GhostMode.Chase)
        {
            if (_mode is GhostMode.Scatter or GhostMode.Chase && mode.Value != _mode)
            {
                _turnBack = true;
            }

            if (_mode == GhostMode.Frightened)
                NotifyModeFrightenedExited();
            _character!.Mode = mode.Value;
            _frightened = GhostMode.Unknown;
            UpdateSpeedForCurrentTile();
            UpdateModeVisual();
            OnEnterMode(_mode);
        }
    }

    private bool ShouldExitMode()
    {
        var tile = GetTile();
        return _mode switch
        {
            GhostMode.Dead => _deadEndTile is not null && ReferenceEquals(tile, _deadEndTile),
            GhostMode.Frightened => _frightenedTimer != null ? _frightenedTimer.HasElapsed() : false,
            GhostMode.House => _houseExitTile is not null && ReferenceEquals(tile, _houseExitTile.GetUp()),
            _ => _mode != _globalMode,
        };
    }

    public void OnEnterMode(GhostMode mode)
    {
        switch (mode)
        {
            case GhostMode.Dead:
                NotifyModeFrightenedExited();
                _deadPrepareEnter = false;
                _character?.SetAnimationOverride(GetScoreAnimation());
                Update();
                break;
            case GhostMode.Frightened:
                _frightenedTimer = new PMTimer(_frightenedTime);
                NotifyModeFrightenedEntered();
                break;
            case GhostMode.House:
                _housePrepareExit = false;
                _character!.Speed = _houseSpeed; //70
                break;
        }
    }
    internal void OnExitMode()
    {
        var tile = GetTile();
        switch (_mode)
        {
            case GhostMode.Dead:
                Reset();
                break;
            case GhostMode.Frightened:
                SetMode(null);
                NotifyModeFrightenedExited();
                break;
            case GhostMode.House:
                _houseTimer = null;
                _dir = PMDirection.Left;
                _moveDir = PMDirection.Left;
                _nextDir = PMDirection.Left;
                //_lastTile = tile.GetDown();
                UpdateSpeedForCurrentTile();
                SetMode(null);
                break;
            default:
                if (!tile.IsHouse())
                    _turnBack = true;
                SetMode(null);
                break;
        }
    }

    public void Move()
    {
        if (ShouldExitMode())
            OnExitMode();
        else
        {
            switch (_mode)
            {
                case GhostMode.Dead:
                    MoveDead();
                    break;
                case GhostMode.House:
                    MoveInsideHouse();
                    break;
                default:
                    _character!.Move(_moveDir);
                    break;
            }
        }
        // Eat or eaten!
        // This code inside pacman actor
        //if (!_eatEvent)
        //{
        //    var pt = _
        //    var t = getTile();
        //    var op = _dir.GetOpposite();
        //    if (pt == t || (pacmanData.dir == op && pt == t.get(op)))
        //    {
        //        _eatEvent = true;
        //        if (mode == MODE_FRIGHTENED)
        //        {
        //            // Ghost eaten by Pacman!
        //            SetMode(GhostMode.Dead);
        //            emit('item:eaten');
        //        }
        //        else if (mode !== MODE_DEAD)
        //        {
        //            // Eat Pacman!
        //            emit('item:eat');
        //        }
        //    }
        //}
    }


   


    public void OnPacManEaten() => Hide();

    public void ResetForLife()
    {
        if (!_configured || _character is null)
            return;
        Reset();
        _character.Reset();
        PrepareInitialState();
        _character.ForceDirection(_dir);
        _character.Move(_dir);
        _moveDir = PMDirection.None;
        _nextDir = PMDirection.None;
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
        Show();
    }

    public void OnEaten(int score)
    {
        _pendingScoreAnimation = GetScoreAnimation(score);
        SetMode(GhostMode.Dead);
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
        Update();
    }

    private PMCharacterAnimationType GetScoreAnimation(int? score = null)
    {
        if (score is null || score <= 0)
            return _pendingScoreAnimation;
        _pendingScoreAnimation = score switch
        {
            200 => PMCharacterAnimationType.GhostScore200,
            400 => PMCharacterAnimationType.GhostScore400,
            800 => PMCharacterAnimationType.GhostScore800,
            1_600 => PMCharacterAnimationType.GhostScore1600,
            _ => PMCharacterAnimationType.Unknown,
        };
        return _pendingScoreAnimation;
    }

    public void Show() => EnsureCharacter().Show();

    public void Hide() => EnsureCharacter().Hide();


  

    public BlPacManEventSubscription SubscribeModeFrightenedEntered(Action<PMGhostBehavior> handler)
        => _modeFrightenedEntered.Subscribe(handler);

    public BlPacManEventSubscription SubscribeModeFrightenedExited(Action<PMGhostBehavior> handler)
        => _modeFrightenedExited.Subscribe(handler);

  

 
    private PMTile GetTile() => _character!.GetTile()!;

   

    private void MoveInsideHouse()
    {
        if (_character is null) return;

        if (_houseTimer == null) _houseTimer = new PMTimer(_waitTime);

        var tile = GetTile();

        if (!_housePrepareExit && _houseTimer.HasElapsed() && !tile.IsWall())
        {
            _housePrepareExit = true;
             _y = tile.Y;
        }

        if (_frightened == GhostMode.Frightened && _frightenedTimer != null && _frightenedTimer.HasElapsed())
            _frightened = GhostMode.Unknown;

        if (_housePrepareExit)
        {
            if (_x < _houseExitTileX) _dir = PMDirection.Right;
            else if (_x > _houseExitTileX) _dir = PMDirection.Left;
            else _dir = PMDirection.Up;

            if (_dir == PMDirection.Up)
                _y -= GetMin(GetStep(), _y - (_houseExitTile?.GetUp()?.Y ?? _y));
            if (_dir == PMDirection.Right)
                _x += GetMin(GetStep(), _houseExitTileX - _x);
            if (_dir == PMDirection.Left)
                _x -= GetMin(GetStep(), _x - _houseExitTileX);

        }
        else
        {
            if (_y <= _houseTop && _dir == PMDirection.Up) _dir = PMDirection.Down;
            if (_y >= _houseBottom && _dir == PMDirection.Down) _dir = PMDirection.Up;

            if (_dir == PMDirection.Up)
                _y -= GetMin(GetStep(), _y - _houseTop);
            if (_dir == PMDirection.Down)
                _y += GetMin(GetStep(), _houseBottom - _y);

        }

        SetNextAnimation();
        Update();
    }

    private void MoveDead()
    {
        if (_character is null) return;
        PMTile tile = GetTile();

        if (!_deadPrepareEnter && _deadTarget is not null && ReferenceEquals(tile, _deadTarget))
            _deadPrepareEnter = true;

        if (_deadPrepareEnter)
        {
            var endX = _deadEndX;
            var endY = _deadEndY;
            // Should go to center first
            if (_y < endY) endX = _deadTarget!.X - tile.Map.TileWidth / 2;
            // Set direction
            if (_x < endX) _dir = PMDirection.Right;
            else if (_x > endX) _dir = PMDirection.Left;
            else if (_y < endY) _dir = PMDirection.Down;
            // Move
            if (_dir == PMDirection.Down)
                _y += GetMin(GetStep(), endY - _y);
            if (_dir == PMDirection.Right)
                _x += GetMin(GetStep(), endX - _x);
            if (_dir == PMDirection.Left)
                _x -= GetMin(GetStep(), _x - endX);

            SetNextAnimation();
            Update();
        }
        else
        {
            _character.Move(_dir);
            return;
        }
    }

    private bool CanGo(PMDirection direction, PMTile? tile = null)
    {
        if (tile == null) tile = GetTile();

        var nextTile = tile.Get(direction);

        if (_mode == GhostMode.Dead)
            return nextTile is null || !nextTile.IsWall();

        if (nextTile is null) return false;

        var ok = !nextTile.IsWall() && !nextTile.IsHouse();
        return ok;
    }


    private PMDirection GetNextDirection(PMTile tile)
    {
        if (_mode == GhostMode.Frightened)
            return GetRandomDirection(tile);

        var currentDirection = _moveDir != PMDirection.None
            ? _moveDir
            : _character?.Direction ?? PMDirection.Left;
        var direction = _globals.State.PacManPosition?.Direction ?? PMDirection.Left;
        var pivotTile = currentDirection != PMDirection.None ? tile.Get(currentDirection) : tile;

        var target = _mode == GhostMode.Chase
            ? PMGhostLogic.GetChaseTargetTile(_globals.GhostManager, GhostName, _character!, _globals.State.PacManPosition!.Tile!, direction, _scatterTarget!)
            : _mode == GhostMode.Scatter
                ? _scatterTarget
                : _deadTarget;

        var preferred = new[]
        {
            PMDirection.Up,
            PMDirection.Left,
            PMDirection.Down,
            PMDirection.Right,
        };

        var nextDirection = PMDirection.None;
        float? lastDistance = null;

        foreach (var dir in preferred)
        {
            if (dir == currentDirection.GetOpposite())
                continue;

            if (CanGo(dir, pivotTile))
            {
                var testTile = pivotTile?.Get(dir);
                var distance = PMTileMath.GetDistance(testTile, target);
                if (lastDistance == null || lastDistance > distance)
                {
                    nextDirection = dir;
                    lastDistance = distance;
                }
            }
        }

        return nextDirection;
    }

    private PMDirection GetRandomDirection(PMTile tile)
    {
        var currentDirection = _moveDir != PMDirection.None
            ? _moveDir
            : _character?.Direction ?? PMDirection.Left;
        var pivotTile = currentDirection != PMDirection.None ? tile.Get(currentDirection) : tile;

        var order = new[]
        {
            PMDirection.Up,
            PMDirection.Right,
            PMDirection.Down,
            PMDirection.Left
        };

        var start = _random.Next(4);
        
        for (var i = 0; i < 8; i++)
        {
            var option = order[(start + i) % 4];
            if (option == currentDirection.GetOpposite())
                continue;

            if (CanGo(option, pivotTile))
                return option;
        }

        return PMDirection.None;
    }


    private void EnterFrightenedMode()
    {
        _character!.Mode = GhostMode.Frightened;
        _frightenedBlink = false;
        _turnBack = true;
        UpdateSpeedForCurrentTile();
        UpdateModeVisual();
        OnEnterMode(GhostMode.Frightened);
    }







    private float GetStep() => _character!.GetStep();

    private static float GetMin(float a, float b) => MathF.Min(a, b);

    private void SetNextAnimation()
    {
        if (_mode == GhostMode.Dead)
        {
            switch (_dir)
            {
                case PMDirection.Up:
                    _nextAnimation = PMCharacterAnimationType.GhostDeathUp;
                    break;
                case PMDirection.Right:
                    _nextAnimation = PMCharacterAnimationType.GhostDeathRight;
                    break;
                case PMDirection.Down:
                    _nextAnimation = PMCharacterAnimationType.GhostDeathDown;
                    break;
                case PMDirection.Left:
                    _nextAnimation = PMCharacterAnimationType.GhostDeathLeft;
                    break;
            }
        }
        else if (_mode == GhostMode.Frightened || (_mode == GhostMode.House && _frightened == GhostMode.Frightened))
        {
            long threshold = Convert.ToInt64(_frightenedTime * 0.75f);
            if (_frightenedTimer == null || !_frightenedTimer.HasElapsed(threshold))
                _nextAnimation = PMCharacterAnimationType.GhostFrightenedBlue;
            else
                _nextAnimation = PMCharacterAnimationType.GhostFrightenedWhite;
        }
        else
        {
            _nextAnimation = _dir switch
            {
                PMDirection.Up => PMCharacterAnimationType.GhostUp,
                PMDirection.Right => PMCharacterAnimationType.GhostRight,
                PMDirection.Down => PMCharacterAnimationType.GhostDown,
                PMDirection.Left => PMCharacterAnimationType.GhostLeft,
                _ => _defaultAnimation,
            };
        }
    }

   
    private void UpdateSpeedForCurrentTile(PMTile? tile = null)
    {
        if (_character is null)
            return;

        var speed = _baseSpeed;

        if (_mode == GhostMode.Frightened) speed = _frightenedSpeed;
        else if (_mode == GhostMode.Dead) speed = _deadSpeed;
        else if (tile?.IsTunnel() == true) speed = _tunnelSpeed;
        if (_mode == GhostMode.House) speed = _houseSpeed;
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
        if (_spriteRects.TryGetValue(GhostName, out var rect))
        {
            Me.SetMemberRect(rect, new APoint(rect.Width / 2f, rect.Height / 2f));
            _savedRect = rect;
        }
        else
        {
            var size = BlPacManTheme.Ghosts.SpriteSize;
            var fallback = new ARect(0, 0, size, size);
            Me.SetMemberRect(fallback, new APoint(fallback.Width / 2f, fallback.Height / 2f));
            _savedRect = fallback;
        }

        _frightenedAnimationsConfigured = false;
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
                behavior.SetAnimationRects(name, definition.Frames, definition.FrameDelay);
        });

        _frightenedAnimationsConfigured = true;
    }

    private void NotifyModeFrightenedEntered()
    {
        if (_frightenedNotificationActive)
            return;

        _frightenedNotificationActive = true;
        _modeFrightenedEntered.Publish(this);
    }

    private void NotifyModeFrightenedExited()
    {
        if (!_frightenedNotificationActive)
            return;

        _frightenedNotificationActive = false;
        _modeFrightenedExited.Publish(this);
    }

   
}
