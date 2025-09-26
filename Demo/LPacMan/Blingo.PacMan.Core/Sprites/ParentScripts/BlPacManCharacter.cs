using System;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.ParentScripts;

internal sealed class BlPacManCharacter : BlingoParentScript
{
    private const float DefaultStep = 5f;
    private const float DefaultSpeed = 80f;
    private const float PositionTolerance = 0.5f;

    private readonly BlPacManEventMediator<BlPacManCharacter> _moveStarted = new();
    private readonly BlPacManEventMediator<BlPacManCharacter> _stopped = new();
    private readonly BlPacManEventMediator<BlPacManPositionEventData> _positionChanged = new();
    private readonly BlPacManEventMediator<BlPacManTileEventData> _tileEntered = new();

    private Map _map;
    private readonly BlingoSprite2D _sprite;

    private float _step;
    private float _speedSetting;
    private float _speed;
    private string? _animation;
    private string? _nextAnimation;
    private bool _moving;
    private bool _preTurnActive;
    private BlPacManDirection _nextDirection;
    private BlPacManDirection _previousDirection;
    private float _lastX;
    private float _lastY;
    private Tile? _lastTile;
    private bool _defaultsCaptured;
    private CharacterSnapshot? _defaults;

    public BlPacManCharacter(IBlingoMovieEnvironment env, Map map, BlingoSprite2D sprite, BlPacManCharacterOptions? options = null)
        : base(env)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
        _step = options?.Step ?? DefaultStep;
        Speed = options?.Speed ?? DefaultSpeed;
        Direction = options?.Direction ?? BlPacManDirection.None;
        _previousDirection = Direction;
        _nextDirection = BlPacManDirection.None;
        Preturn = options?.Preturn ?? false;
        Mode = options?.Mode;
    }

    public float Step
    {
        get => _step;
        set => _step = value;
    }

    public float Speed
    {
        get => _speedSetting;
        set
        {
            _speedSetting = value;
            _speed = value;
        }
    }

    public float EffectiveSpeed
    {
        get => _speed;
        set => _speed = value;
    }

    public BlPacManDirection Direction { get; private set; }

    public BlPacManDirection PreviousDirection => _previousDirection;

    public BlPacManDirection NextDirection
    {
        get => _nextDirection;
        set => _nextDirection = value;
    }

    public bool Preturn { get; set; }

    public string? Mode { get; set; }

    public string? Animation => _animation;

    private float X
    {
        get => _sprite.LocH;
        set => _sprite.LocH = value;
    }

    private float Y
    {
        get => _sprite.LocV;
        set => _sprite.LocV = value;
    }

    private Map Map => _map;

    public BlPacManEventSubscription SubscribeMoveStarted(Action<BlPacManCharacter> handler) => _moveStarted.Subscribe(handler);

    public BlPacManEventSubscription SubscribeStopped(Action<BlPacManCharacter> handler) => _stopped.Subscribe(handler);

    public BlPacManEventSubscription SubscribePositionChanged(Action<BlPacManPositionEventData> handler) => _positionChanged.Subscribe(handler);

    public BlPacManEventSubscription SubscribeTileEntered(Action<BlPacManTileEventData> handler) => _tileEntered.Subscribe(handler);

    public void SetMap(Map map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    public void BeginSprite()
    {
        _lastX = X;
        _lastY = Y;

        if (!_defaultsCaptured)
        {
            CaptureDefaults();
            _defaultsCaptured = true;
        }

        PauseCharacterAnimation();
    }

    public void Reset()
    {
        if (_defaults is not CharacterSnapshot snapshot)
        {
            return;
        }

        X = snapshot.X;
        Y = snapshot.Y;
        _lastX = snapshot.LastX;
        _lastY = snapshot.LastY;
        Direction = snapshot.Direction;
        _previousDirection = snapshot.PreviousDirection;
        _nextAnimation = snapshot.NextAnimation;
        _nextDirection = snapshot.NextDirection;
        _moving = snapshot.IsMoving;
        Mode = snapshot.Mode;
        _preTurnActive = false;
        _lastTile = null;
        SetAnimation(snapshot.Animation);
        PauseCharacterAnimation();
    }

    public void Move(BlPacManDirection direction = BlPacManDirection.None)
    {
        if (direction == BlPacManDirection.None)
        {
            direction = Direction;
        }

        if (direction == BlPacManDirection.None)
        {
            return;
        }

        var tile = GetTile();
        if (tile is null)
        {
            return;
        }

        float? step = null;
        var stepSize = GetStep();

        if ((direction != Direction || _preTurnActive) && CanGo(direction, tile))
        {
            if (((direction != Direction && direction != Direction.GetOpposite()) || _preTurnActive) && !IsCentered(tile))
            {
                if (direction.IsVertical())
                {
                    var diffX = Math.Abs(X - tile.CenterX);
                    if (Preturn)
                    {
                        if (!IsCentered(tile, Axis.X))
                        {
                            if (X > tile.CenterX)
                            {
                                X -= GetMin(diffX, stepSize);
                            }
                            else
                            {
                                X += GetMin(diffX, stepSize);
                            }

                            _preTurnActive = true;
                        }
                        else
                        {
                            _preTurnActive = false;
                        }
                    }
                    else
                    {
                        step = GetMin(diffX, stepSize);
                    }
                }

                if (direction.IsHorizontal())
                {
                    var diffY = Math.Abs(Y - tile.CenterY);
                    if (Preturn)
                    {
                        if (!IsCentered(tile, Axis.Y))
                        {
                            if (Y > tile.CenterY)
                            {
                                Y -= GetMin(diffY, stepSize);
                            }
                            else
                            {
                                Y += GetMin(diffY, stepSize);
                            }

                            _preTurnActive = true;
                        }
                        else
                        {
                            _preTurnActive = false;
                        }
                    }
                    else
                    {
                        step = GetMin(diffY, stepSize);
                    }
                }
            }

            if (step is null)
            {
                UpdateDirection(direction);
                SetNextAnimation();
            }
        }

        if (step is null)
        {
            if (CanGo(Direction, tile))
            {
                step = stepSize;
            }
            else
            {
                if (Direction.IsVertical())
                {
                    step = GetMin(Math.Abs(Y - tile.CenterY), stepSize);
                }
                else if (Direction.IsHorizontal())
                {
                    step = GetMin(Math.Abs(X - tile.CenterX), stepSize);
                }
            }
        }

        if (step is float distance && distance > 0)
        {
            switch (Direction)
            {
                case BlPacManDirection.Up:
                    Y -= distance;
                    break;
                case BlPacManDirection.Right:
                    X += distance;
                    break;
                case BlPacManDirection.Down:
                    Y += distance;
                    break;
                case BlPacManDirection.Left:
                    X -= distance;
                    break;
            }
        }

        WrapPosition();

        var newTile = GetTile();
        if (newTile is not null && !ReferenceEquals(newTile, _lastTile))
        {
            _lastTile = newTile;
            HandleTileEntered(newTile);
        }

        Update();
    }

    public void ForceDirection(BlPacManDirection direction)
    {
        _previousDirection = Direction;
        Direction = direction;
        _nextDirection = direction;
        SetNextAnimation();
    }

    public void Update()
    {
        var tile = GetTile();
        if (tile is not null)
        {
            if (Math.Abs(Y - tile.CenterY) < PositionTolerance)
            {
                Y = tile.CenterY;
            }

            if (Math.Abs(X - tile.CenterX) < PositionTolerance)
            {
                X = tile.CenterX;
            }
        }

        var currentX = X;
        var currentY = Y;

        if (!AreEqual(_lastX, currentX) || !AreEqual(_lastY, currentY))
        {
            _lastX = currentX;
            _lastY = currentY;

            if (!_moving)
            {
                OnMoveStarted();
                ResumeCharacterAnimation();
                _moving = true;
            }

            var positionTile = tile ?? GetTile();
            OnPositionChanged(new BlPacManPositionEventData(currentX, currentY, positionTile, Direction));
        }
        else if (_moving)
        {
            OnStopped();
            PauseCharacterAnimation();
            _moving = false;
        }

        if (_nextAnimation is not null && !string.Equals(_animation, _nextAnimation, StringComparison.Ordinal))
        {
            SetAnimation(_nextAnimation);
        }
    }

    public Tile? GetTile()
    {
        return Map.GetTile(X, Y, true);
    }

    public void Hide()
    {
        _sprite.Visibility = false;
    }

    public void Show()
    {
        _sprite.Visibility = true;
    }

    public void SetAnimation(string? animation)
    {
        if (string.Equals(_animation, animation, StringComparison.Ordinal))
        {
            return;
        }

        _animation = animation;
        ApplyAnimation(animation);
    }

    private void ApplyAnimation(string? animation)
    {
        if (animation is null)
        {
            TrySendSprite<BlPacManAnimationBehavior>(_sprite.SpriteNum, behavior => behavior.StopAnimation());
            return;
        }

        TrySendSprite<BlPacManAnimationBehavior>(_sprite.SpriteNum, behavior => behavior.Play(animation));
    }

    private void CaptureDefaults()
    {
        SetNextAnimation();
        _defaults = new CharacterSnapshot(X, Y, _lastX, _lastY, Direction, _previousDirection, _nextAnimation, _nextDirection, _moving, Mode, _animation);
    }

    private void HandleTileEntered(Tile tile)
    {
        OnTileEntered(tile);
        _tileEntered.Publish(new BlPacManTileEventData(tile));
    }

    private void OnMoveStarted()
    {
        _moveStarted.Publish(this);
    }

    private void OnStopped()
    {
        _stopped.Publish(this);
    }

    private void OnPositionChanged(BlPacManPositionEventData args)
    {
        _positionChanged.Publish(args);
    }

    private void PauseCharacterAnimation()
    {
        _sprite.Pause();
        TrySendSprite<BlPacManAnimationBehavior>(_sprite.SpriteNum, behavior => behavior.StopAnimation());
    }

    private void ResumeCharacterAnimation()
    {
        _sprite.Play();
        if (_animation is not null)
        {
            TrySendSprite<BlPacManAnimationBehavior>(_sprite.SpriteNum, behavior => behavior.Play(_animation));
        }
    }

    private void UpdateDirection(BlPacManDirection direction)
    {
        if (Direction == direction)
        {
            return;
        }

        _previousDirection = Direction;
        Direction = direction;
    }

    private void SetNextAnimation()
    {
        if (Direction == BlPacManDirection.None)
        {
            _nextAnimation = null;
            return;
        }

        var label = GetAnimationLabel(Direction);
        if (label is null)
        {
            _nextAnimation = null;
            return;
        }

        _nextAnimation = label;
        _nextDirection = Direction;
    }

    private float GetStep()
    {
        return _step * (_speed / 100f);
    }

    private bool CanGo(BlPacManDirection direction, Tile? currentTile = null)
    {
        var tile = currentTile ?? GetTile();
        if (tile is null)
        {
            return false;
        }

        var nextTile = tile.Get(direction);
        return nextTile is not null && !nextTile.IsHouse() && !nextTile.IsWall();
    }

    private bool IsCentered(Tile tile)
    {
        return IsCentered(tile, Axis.Both);
    }

    private bool IsCentered(Tile tile, Axis axis)
    {
        var centeredX = Math.Abs(X - tile.CenterX) < PositionTolerance;
        var centeredY = Math.Abs(Y - tile.CenterY) < PositionTolerance;

        return axis switch
        {
            Axis.X => centeredX,
            Axis.Y => centeredY,
            _ => centeredX && centeredY,
        };
    }

    private static float GetMin(float value1, float value2)
    {
        return value1 < value2 ? value1 : value2;
    }

    private static bool AreEqual(float a, float b)
    {
        return Math.Abs(a - b) < 0.001f;
    }

    private static string? GetAnimationLabel(BlPacManDirection direction)
    {
        return direction switch
        {
            BlPacManDirection.Left => "left",
            BlPacManDirection.Right => "right",
            BlPacManDirection.Up => "up",
            BlPacManDirection.Down => "down",
            _ => null,
        };
    }

    private void WrapPosition()
    {
        var mapWidth = Map.Width * Map.TileWidth;
        if (mapWidth > 0)
        {
            if (X < 0)
            {
                X = mapWidth;
            }
            else if (X > mapWidth)
            {
                X = 0;
            }
        }

        var mapHeight = Map.Height * Map.TileHeight;
        if (mapHeight > 0)
        {
            if (Y < 0)
            {
                Y = mapHeight;
            }
            else if (Y > mapHeight)
            {
                Y = 0;
            }
        }
    }

    private void OnTileEntered(Tile tile)
    {
        SetNextAnimation();
    }

    private enum Axis
    {
        Both,
        X,
        Y,
    }

    private readonly record struct CharacterSnapshot(
        float X,
        float Y,
        float LastX,
        float LastY,
        BlPacManDirection Direction,
        BlPacManDirection PreviousDirection,
        string? NextAnimation,
        BlPacManDirection NextDirection,
        bool IsMoving,
        string? Mode,
        string? Animation);
}
