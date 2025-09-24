using System;
using System.Collections.Generic;
using BlingoEngine.Movies;

namespace Blingo.PacMan.Core;

/// <summary>
/// Represents a single ghost character, ported from the original JavaScript implementation. The
/// class keeps the overall structure – mode transitions, frightened timers and score progression –
/// while adapting the movement logic to the <see cref="BlPacManCharacter"/> abstraction.
/// </summary>
internal class GhostCharacter : BlPacManCharacter
{
    private const float DeadSpeed = 130f;

    private readonly Random _random = new();
    private readonly Func<PacManState, Tile?> _getChaseTarget;
    private readonly Dictionary<int, int> _scoreProgression;
    private readonly Func<float> _normalizeRefreshRate;

    private readonly float _frightenedTime;
    private readonly float _waitTime;
    private readonly float? _tunnelSpeed;
    private readonly float? _frightenedSpeed;

    private GhostTimer? _frightenedTimer;
    private GhostTimer? _houseTimer;

    private GhostMode _mode;
    private GhostMode? _globalMode;
    private GhostMode? _queuedMode;

    private Tile? _deadTarget;
    private Tile? _deadEnd;
    private Tile? _houseExitTile;
    private Tile? _scatterTarget;

    private float _deadEndX;
    private float _deadEndY;
    private float _houseExitTileX;
    private float _spawnX;
    private float _spawnY;
    private bool _spawnCaptured;
    private bool _housePrepareExit;
    private bool _turnBack;
    private bool _eatEvent;

    private PacManState _pacmanState;

    public GhostCharacter(IBlingoMovieEnvironment env, Map map, GhostCharacterOptions options)
        : base(env, map, new PacManCharacterOptions
        {
            Speed = options?.Speed,
            Step = options?.Step,
            Direction = options?.InitialDirection,
            Preturn = options?.Preturn,
        })
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _frightenedTime = options.FrightenedTime;
        _waitTime = options.WaitTime;
        _tunnelSpeed = options.TunnelSpeed;
        _frightenedSpeed = options.FrightenedSpeed;
        _normalizeRefreshRate = options.NormalizeRefreshRate ?? (() => 1f);
        _getChaseTarget = options.GetChaseTarget ?? (state => state.Tile);
        _scoreProgression = new Dictionary<int, int>(options.ScoreProgression ?? new Dictionary<int, int>());
        Score = options.InitialScore;

        _scatterTarget = ResolveScatterTarget(map, options.ScatterTargetIndex);
        _deadTarget = map.House?.GetRight()?.GetUp();
        _houseExitTile = map.House?.GetRight();
        if (_houseExitTile != null)
        {
            _houseExitTileX = _houseExitTile.CenterX;
        }

        CurrentMode = options.InitialMode;

        TileEntered += (_, args) => OnTileChanged(args.Tile);

        options.AddGameGlobalModeListener?.Invoke(OnGameGlobalMode);
        options.AddGameGhostEatenListener?.Invoke(OnOtherGhostEaten);
        options.AddPacmanEatPillListener?.Invoke(OnPacManAtePowerPill);
        options.AddPacmanPositionListener?.Invoke(OnPacManPositionUpdated);
    }

    public event EventHandler? Eaten;

    public event EventHandler? Eat;

    public event EventHandler? FrightenedEntered;

    public event EventHandler? FrightenedExited;

    public GhostCharacter? Blinky { get; set; }

    public int Score { get; private set; }

    public GhostMode CurrentMode
    {
        get => _mode;
        private set
        {
            _mode = value;
            base.Mode = value.ToString();
        }
    }

    public override void Move(PacManDirection direction = PacManDirection.None)
    {
        CaptureSpawnPoint();
        AdvanceTimers();

        if (ShouldExitMode())
        {
            OnExitMode();
        }
        else if (CurrentMode == GhostMode.House)
        {
            HandleHouseMovement();
        }
        else
        {
            base.Move(direction);
        }

        CheckCollisionWithPacMan();
    }

    public void SetMode(GhostMode? mode)
    {
        if (mode is null)
        {
            if (_queuedMode is GhostMode queued)
            {
                _queuedMode = null;
                ApplyMode(queued);
            }
            else if (_globalMode is GhostMode global)
            {
                ApplyMode(global);
            }

            return;
        }

        if (mode == GhostMode.Frightened && (CurrentMode == GhostMode.House || CurrentMode == GhostMode.Dead))
        {
            _queuedMode = mode;
            return;
        }

        ApplyMode(mode.Value);
    }

    public void Pause()
    {
        _houseTimer?.Pause();
        _frightenedTimer?.Pause();
    }

    public void Resume()
    {
        if (CurrentMode == GhostMode.Frightened)
        {
            _frightenedTimer?.Resume();
        }

        if (CurrentMode == GhostMode.House && !_housePrepareExit)
        {
            _houseTimer?.Resume();
        }
    }

    protected override void SetNextAnimation()
    {
        if (CurrentMode == GhostMode.Dead)
        {
            var label = Direction switch
            {
                PacManDirection.Up => "deadUp",
                PacManDirection.Right => "deadRight",
                PacManDirection.Down => "deadDown",
                PacManDirection.Left => "deadLeft",
                _ => null,
            };

            SetNextAnimationLabel(label);
            return;
        }

        if (CurrentMode == GhostMode.Frightened || (CurrentMode == GhostMode.House && _queuedMode == GhostMode.Frightened))
        {
            var frightened = _frightenedTimer;
            var label = frightened is null || !frightened.IsElapsed(_frightenedTime * 0.75f)
                ? "frightened"
                : "frightenedBlink";

            SetNextAnimationLabel(label);
            return;
        }

        base.SetNextAnimation();
    }

    private void ApplyMode(GhostMode mode)
    {
        CurrentMode = mode;
        OnEnterMode(mode);
    }

    private void OnEnterMode(GhostMode mode)
    {
        switch (mode)
        {
            case GhostMode.Dead:
                SetAnimation($"score{Score}");
                Update();
                break;
            case GhostMode.Frightened:
                _frightenedTimer = new GhostTimer(_frightenedTime, _normalizeRefreshRate);
                FrightenedEntered?.Invoke(this, EventArgs.Empty);
                break;
            case GhostMode.House:
                _housePrepareExit = false;
                EffectiveSpeed = 70f;
                break;
        }
    }

    private void OnExitMode()
    {
        var tile = GetTile();

        switch (CurrentMode)
        {
            case GhostMode.Dead:
                base.Reset();
                ApplyMode(_globalMode ?? GhostMode.Scatter);
                break;
            case GhostMode.Frightened:
                FrightenedExited?.Invoke(this, EventArgs.Empty);
                SetMode(null);
                break;
            case GhostMode.House:
                _houseTimer = null;
                ForceDirection(PacManDirection.Left);
                EffectiveSpeed = Speed;
                SetMode(null);
                break;
            default:
                if (tile is not null && !tile.IsHouse())
                {
                    _turnBack = true;
                }

                SetMode(null);
                break;
        }
    }

    private bool ShouldExitMode()
    {
        return CurrentMode switch
        {
            GhostMode.Dead => Equals(GetTile(), _deadEnd),
            GhostMode.Frightened => _frightenedTimer?.IsElapsed() ?? true,
            GhostMode.House => Equals(GetTile(), _houseExitTile?.GetUp()),
            _ => _globalMode is GhostMode global && CurrentMode != global,
        };
    }

    private void HandleHouseMovement()
    {
        var tile = GetTile();
        if (tile is null)
        {
            return;
        }

        _houseTimer ??= new GhostTimer(_waitTime, _normalizeRefreshRate);
        _houseTimer.Advance();

        if (!_housePrepareExit && _houseTimer.IsElapsed() && !tile.IsWall())
        {
            _housePrepareExit = true;
            Y = tile.CenterY;
            ForceDirection(PacManDirection.Up);
        }

        if (_queuedMode == GhostMode.Frightened && _frightenedTimer?.IsElapsed() == true)
        {
            _queuedMode = null;
        }

        if (_housePrepareExit)
        {
            if (Math.Abs(X - _houseExitTileX) > 1f)
            {
                var dir = X < _houseExitTileX ? PacManDirection.Right : PacManDirection.Left;
                ForceDirection(dir);
            }
            else
            {
                ForceDirection(PacManDirection.Up);
            }

            var step = GetStepSize();

            switch (Direction)
            {
                case PacManDirection.Up:
                    Y -= step;
                    if (_houseExitTile?.GetUp() is Tile exitTile && Y <= exitTile.CenterY)
                    {
                        Y = exitTile.CenterY;
                    }

                    break;
                case PacManDirection.Right:
                    X += MathF.Min(step, _houseExitTileX - X);
                    break;
                case PacManDirection.Left:
                    X -= MathF.Min(step, X - _houseExitTileX);
                    break;
            }

            SetNextAnimation();
            Update();
        }
        else
        {
            var top = tile.CenterY - tile.Height / 2f;
            var bottom = tile.CenterY + tile.Height / 2f;

            if (Direction == PacManDirection.None)
            {
                ForceDirection(PacManDirection.Up);
            }
            else if (Direction == PacManDirection.Up && Y <= top)
            {
                ForceDirection(PacManDirection.Down);
            }
            else if (Direction == PacManDirection.Down && Y >= bottom)
            {
                ForceDirection(PacManDirection.Up);
            }

            var step = GetStepSize();

            if (Direction == PacManDirection.Up)
            {
                Y -= MathF.Min(step, Y - top);
            }
            else if (Direction == PacManDirection.Down)
            {
                Y += MathF.Min(step, bottom - Y);
            }

            SetNextAnimation();
            Update();
        }
    }

    private void OnTileChanged(Tile tile)
    {
        if (CurrentMode == GhostMode.Frightened && _frightenedSpeed.HasValue)
        {
            EffectiveSpeed = _frightenedSpeed.Value;
        }
        else if (CurrentMode == GhostMode.Dead)
        {
            EffectiveSpeed = DeadSpeed;
        }
        else if (_tunnelSpeed.HasValue && tile.IsTunnel())
        {
            EffectiveSpeed = _tunnelSpeed.Value;
        }
        else
        {
            EffectiveSpeed = Speed;
        }

        if (_turnBack)
        {
            var opposite = Direction.GetOpposite();
            if (opposite != PacManDirection.None)
            {
                ForceDirection(opposite);
            }

            _turnBack = false;
        }
        else
        {
            var nextDirection = GetNextDirection(tile);
            if (nextDirection != PacManDirection.None && nextDirection != Direction)
            {
                ForceDirection(nextDirection);
            }
        }

        _eatEvent = false;
    }

    private PacManDirection GetNextDirection(Tile currentTile)
    {
        if (CurrentMode == GhostMode.Frightened)
        {
            return GetRandomDirection(currentTile, Direction);
        }

        var targetTile = CurrentMode switch
        {
            GhostMode.Chase => _getChaseTarget(_pacmanState) ?? _pacmanState.Tile,
            GhostMode.Scatter => _scatterTarget,
            GhostMode.Dead => _deadTarget ?? _deadEnd,
            _ => _pacmanState.Tile,
        };

        return GetShortestDirection(currentTile, Direction, targetTile);
    }

    private PacManDirection GetRandomDirection(Tile currentTile, PacManDirection currentDirection)
    {
        var candidates = new List<PacManDirection>();

        foreach (var direction in new[]
                 {
                     PacManDirection.Up,
                     PacManDirection.Right,
                     PacManDirection.Down,
                     PacManDirection.Left,
                 })
        {
            if (direction == currentDirection.GetOpposite())
            {
                continue;
            }

            if (CanMove(direction, currentTile))
            {
                candidates.Add(direction);
            }
        }

        if (candidates.Count == 0)
        {
            return currentDirection;
        }

        var index = _random.Next(candidates.Count);
        return candidates[index];
    }

    private PacManDirection GetShortestDirection(Tile currentTile, PacManDirection currentDirection, Tile? targetTile)
    {
        var bestDirection = PacManDirection.None;
        var bestDistance = float.PositiveInfinity;

        foreach (var direction in new[]
                 {
                     PacManDirection.Up,
                     PacManDirection.Left,
                     PacManDirection.Down,
                     PacManDirection.Right,
                 })
        {
            if (direction == currentDirection.GetOpposite())
            {
                continue;
            }

            if (!CanMove(direction, currentTile))
            {
                continue;
            }

            var nextTile = currentTile.Get(direction);
            if (nextTile is null)
            {
                continue;
            }

            var distance = TileMath.GetDistance(nextTile, targetTile);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = direction;
            }
        }

        return bestDirection == PacManDirection.None ? currentDirection : bestDirection;
    }

    private bool CanMove(PacManDirection direction, Tile tile)
    {
        var nextTile = tile.Get(direction);
        if (nextTile is null)
        {
            return false;
        }

        if (CurrentMode == GhostMode.Dead)
        {
            return !nextTile.IsWall();
        }

        return !nextTile.IsWall() && !nextTile.IsHouse();
    }

    private void AdvanceTimers()
    {
        _frightenedTimer?.Advance();
        if (CurrentMode == GhostMode.House && !_housePrepareExit)
        {
            _houseTimer?.Advance();
        }
    }

    private void CaptureSpawnPoint()
    {
        if (_spawnCaptured)
        {
            return;
        }

        _spawnX = X;
        _spawnY = Y;
        _spawnCaptured = true;

        _deadEndX = _spawnX;
        _deadEndY = Map.HouseCenter?.CenterY ?? _spawnY;
        _deadEnd = Map.GetTile(_deadEndX, _deadEndY, true);
    }

    private void CheckCollisionWithPacMan()
    {
        if (_eatEvent)
        {
            return;
        }

        var ghostTile = GetTile();
        var pacTile = _pacmanState.Tile;

        if (ghostTile is null || pacTile is null)
        {
            return;
        }

        var opposite = Direction.GetOpposite();

        if (ReferenceEquals(pacTile, ghostTile) ||
            (_pacmanState.Direction == opposite && ReferenceEquals(pacTile, ghostTile.Get(opposite))))
        {
            _eatEvent = true;

            if (CurrentMode == GhostMode.Frightened)
            {
                SetMode(GhostMode.Dead);
                Eaten?.Invoke(this, EventArgs.Empty);
            }
            else if (CurrentMode != GhostMode.Dead)
            {
                Eat?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void OnGameGlobalMode(GhostMode? mode)
    {
        _globalMode = mode;
    }

    private void OnPacManAtePowerPill()
    {
        SetMode(GhostMode.Frightened);
        Score = 200;
    }

    private void OnOtherGhostEaten()
    {
        if (_scoreProgression.TryGetValue(Score, out var next))
        {
            Score = next;
        }
    }

    private void OnPacManPositionUpdated(PacManState state)
    {
        _pacmanState = state;
    }

    private static Tile? ResolveScatterTarget(Map map, int index)
    {
        if (index < 0 || index >= map.Tiles.Count)
        {
            return null;
        }

        return map.Tiles[index];
    }

    internal readonly record struct PacManState(Tile? Tile, PacManDirection Direction);

    private sealed class GhostTimer
    {
        private readonly float _duration;
        private readonly Func<float> _normalizeRefreshRate;
        private float _elapsed;
        private bool _paused;

        public GhostTimer(float duration, Func<float> normalizeRefreshRate)
        {
            _duration = duration;
            _normalizeRefreshRate = normalizeRefreshRate ?? throw new ArgumentNullException(nameof(normalizeRefreshRate));
        }

        public void Advance()
        {
            if (_paused)
            {
                return;
            }

            var delta = _normalizeRefreshRate();
            if (delta < 0)
            {
                return;
            }

            _elapsed += delta;
        }

        public bool IsElapsed(float threshold)
        {
            return _elapsed >= threshold;
        }

        public bool IsElapsed()
        {
            return _elapsed >= _duration;
        }

        public void Pause() => _paused = true;

        public void Resume() => _paused = false;
    }
}

internal sealed class GhostCharacterOptions
{
    public float? Step { get; init; }

    public float? Speed { get; init; }

    public PacManDirection? InitialDirection { get; init; }

    public bool? Preturn { get; init; }

    public GhostMode InitialMode { get; init; } = GhostMode.House;

    public int ScatterTargetIndex { get; init; }

    public float FrightenedTime { get; init; } = 5f;

    public float WaitTime { get; init; } = 4f;

    public float? TunnelSpeed { get; init; }

    public float? FrightenedSpeed { get; init; }

    public int InitialScore { get; init; } = 200;

    public IReadOnlyDictionary<int, int>? ScoreProgression { get; init; }

    public Func<GhostCharacter.PacManState, Tile?>? GetChaseTarget { get; init; }

    public Func<float>? NormalizeRefreshRate { get; init; }

    public Action<Action<GhostMode?>>? AddGameGlobalModeListener { get; init; }

    public Action<Action>? AddGameGhostEatenListener { get; init; }

    public Action<Action>? AddPacmanEatPillListener { get; init; }

    public Action<Action<GhostCharacter.PacManState>>? AddPacmanPositionListener { get; init; }
}
