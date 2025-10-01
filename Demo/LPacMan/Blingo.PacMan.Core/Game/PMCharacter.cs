using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.ParentScripts;


public enum PMCharacterAnimationType
{
    Unknown,
    PacManUp,
    PacManDown,
    PacManLeft,
    PacManRight,

    GhostUp,
    GhostDown,
    GhostLeft,
    GhostRight,
    GhostScore200,
    GhostScore400,
    GhostScore800,
    GhostScore1600,
    GhostFrightenedBlue,
    GhostFrightenedWhite,
    BonusDefault,
    BonusScore100,
    BonusScore200,
    BonusScore500,
    BonusScore700,
    BonusScore1000,
    BonusScore2000,
    BonusScore5000,
}
internal sealed class PMCharacter : BlingoParentScript
{
    public enum CharacterType
    {
        Unknown,
        PacMan,
        Ghost,
        Bonus,
        Fruit
    }
    private const float _defaultSpeed = 80f;
    private const float _positionTolerance = 0.5f;

    private readonly BlPacManEventMediator<PMCharacter> _moveStarted = new();
    private readonly BlPacManEventMediator<PMCharacter> _stopped = new();
    private readonly BlPacManEventMediator<BlPacManPositionEventData> _positionChanged = new();
    private readonly BlPacManEventMediator<BlPacManTileEventData> _tileEntered = new();

    private Map _map;
    private readonly BlingoSprite2D _sprite;
    private float _step;
    private float _speedSetting;
    private float _speed;
    private PMCharacterAnimationType _animation = PMCharacterAnimationType.Unknown;
    private PMCharacterAnimationType _nextAnimation = PMCharacterAnimationType.Unknown;
    private PMCharacterAnimationType _animationOverride = PMCharacterAnimationType.Unknown;
    private bool _moving;
    private bool _preTurnActive;
    private BlPacManDirection _nextDirection;
    private BlPacManDirection _previousDirection;
    private float _lastX;
    private float _lastY;
    private Tile? _lastTile;
    private bool _defaultsCaptured;
    private CharacterSnapshot? _defaults;
    public bool RotateSprite { get; set; }
    public bool AllowHouseExit { get; set; }
    public CharacterType Type { get; private set; }

    #region Properties

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

    public GhostMode Mode { get; set; }

    public PMCharacterAnimationType Animation => _animation;

    public float X
    {
        get => _sprite.LocH;
        set => _sprite.LocH = value;
    }

    public float Y
    {
        get => _sprite.LocV;
        set => _sprite.LocV = value;
    }

    private Map Map => _map;

    public BlPacManEventSubscription SubscribeMoveStarted(Action<PMCharacter> handler) => _moveStarted.Subscribe(handler);

    public BlPacManEventSubscription SubscribeStopped(Action<PMCharacter> handler) => _stopped.Subscribe(handler);

    public BlPacManEventSubscription SubscribePositionChanged(Action<BlPacManPositionEventData> handler) => _positionChanged.Subscribe(handler);

    public BlPacManEventSubscription SubscribeTileEntered(Action<BlPacManTileEventData> handler) => _tileEntered.Subscribe(handler);

    #endregion


    public PMCharacter(IBlingoMovieEnvironment env, Map map, BlingoSprite2D sprite, CharacterType type, BlPacManCharacterOptions options)
        : base(env)
    {
        Type = type;
        _map = map;
        _sprite = sprite;
        _step = options.Step ?? TileMath.GetMovementStep(map);
        Speed = options.Speed ?? _defaultSpeed;
        Direction = options.Direction;
        _previousDirection = Direction;
        _nextDirection = BlPacManDirection.None;
        Preturn = options.Preturn ?? false;
        Mode = options.Mode;
    }



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

    public void UpdateStartPosition(float x, float y)
    {
        X = x;
        Y = y;
        _lastX = x;
        _lastY = y;

        if (_defaults is CharacterSnapshot snapshot)
        {
            _defaults = snapshot with
            {
                X = x,
                Y = y,
                LastX = x,
                LastY = y,
                AllowHouseExit = AllowHouseExit,
            };
        }
        else
        {
            _defaults = new CharacterSnapshot(
                x,
                y,
                x,
                y,
                Direction,
                _previousDirection,
                _nextAnimation,
                _nextDirection,
                _moving,
                Mode,
                _animation,
                _animationOverride,
                AllowHouseExit);

            _defaultsCaptured = true;
        }
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
        _animationOverride = snapshot.AnimationOverride;
        AllowHouseExit = snapshot.AllowHouseExit;

        _preTurnActive = false;
        _lastTile = null;
        if (_animationOverride != PMCharacterAnimationType.Unknown)
            SetAnimation(_animationOverride);
        else
            SetAnimation(snapshot.Animation);
        PauseCharacterAnimation();
    }

    public void Move(BlPacManDirection direction = BlPacManDirection.None)
    {
        if (Type== CharacterType.Ghost)
        {

        }
        if (direction == BlPacManDirection.None)
            direction = Direction;

        if (direction == BlPacManDirection.None)
            return;

        var tile = GetTile();
        if (tile is null)
            return;

        float? step = null;
        var stepSize = GetStep();
        // Could go that direction.
        if ((direction != Direction || _preTurnActive) && CanGo(direction, tile))
        {
            if (((direction != Direction && direction != Direction.GetOpposite()) || _preTurnActive) && !IsCentered(tile))
            {
                // Not in the center of the tile. Befor turn, set step so on next frame we get into the center.
                if (direction.IsVertical())
                {
                    var diffX = Math.Abs(X - tile.X);
                    if (Preturn) // Set preturn to true to turn faster on corners.
                    {
                        if (!IsCentered(tile, Axis.X))
                        {
                            if (X > tile.X)
                                X -= GetMin(diffX, stepSize);
                            else
                                X += GetMin(diffX, stepSize);

                            _preTurnActive = true;
                        }
                        else
                            _preTurnActive = false;
                    }
                    else
                        step = GetMin(diffX, stepSize);
                }

                if (direction.IsHorizontal())
                {
                    var diffY = Math.Abs(Y - tile.Y);
                    if (Preturn) // Set preturn to true to turn faster on corners.
                    { 
                        if (!IsCentered(tile, Axis.Y))
                        {
                            if (Y > tile.Y)
                                Y -= GetMin(diffY, stepSize);
                            else
                                Y += GetMin(diffY, stepSize);

                            _preTurnActive = true;
                        }
                        else
                            _preTurnActive = false;
                    }
                    else
                        step = GetMin(diffY, stepSize);
                }
            }

            // No step. Means change direction.
            if (step is null)
            {
                UpdateDirection(direction);
                SetNextAnimation();
            }
        }
        if (step is null)
        {
            // Keep straight.
            if (CanGo(Direction, tile))
                step = stepSize;
            else
            {
                // Wall.
                if (Direction.IsVertical())
                    step = GetMin(Math.Abs(Y - tile.Y), stepSize);
                else if (Direction.IsHorizontal())
                    step = GetMin(Math.Abs(X - tile.X), stepSize);
            }
        }
        // Move.
        if (step is float distance && distance > 0)
        {
            switch (Direction)
            {
                case BlPacManDirection.Up:
                    if (RotateSprite)
                    {
                        _sprite.Rotation = 270;
                        _sprite.FlipH = false;
                    }
                    Y -= distance;
                    break;
                case BlPacManDirection.Right:
                    if (RotateSprite)
                    {
                        _sprite.Rotation = 0;
                        _sprite.FlipH = false;
                    }
                    X += distance;
                    break;
                case BlPacManDirection.Down:
                    if (RotateSprite)
                    {
                        _sprite.Rotation = 90;
                        _sprite.FlipH = false;
                    }
                    Y += distance;
                    break;
                case BlPacManDirection.Left:
                    if (RotateSprite)
                    {
                        _sprite.Rotation = 0;
                        _sprite.FlipH = true;
                    }
                    X -= distance;
                    break;
            }
        }
        // Pass away limits.
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
            if (Math.Abs(Y - tile.Y) < _positionTolerance)
                Y = tile.Y;

            if (Math.Abs(X - tile.X) < _positionTolerance)
                X = tile.X;
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

        if (_nextAnimation != PMCharacterAnimationType.Unknown && _animation !=  _nextAnimation)
            SetAnimation(_nextAnimation);
    }

    public Tile? GetTile() => Map.GetTile(X, Y, true);

    public void Hide() => _sprite.Visibility = false;

    public void Show() => _sprite.Visibility = true;

    public void SetAnimation(PMCharacterAnimationType animation)
    {
        if (_animation ==  animation)
            return;

        _animation = animation;
        ApplyAnimation(animation);
    }

    public void SetAnimationOverride(PMCharacterAnimationType animation)
    {
        if (_animationOverride ==  animation)
            return;

        _animationOverride = animation;

        if (animation == PMCharacterAnimationType.Unknown)
            SetNextAnimation();
        else
        {
            _nextAnimation = animation;
            _nextDirection = Direction;
            SetAnimation(animation);
        }
    }

    private void ApplyAnimation(PMCharacterAnimationType animation)
    {
        if (animation == PMCharacterAnimationType.Unknown)
        {
            TrySendSprite<BlPacManAnimationBehavior>(_sprite.SpriteNum, behavior => behavior.StopAnimation());
            return;
        }

        TrySendSprite<BlPacManAnimationBehavior>(_sprite.SpriteNum, behavior => behavior.Play(animation));
    }

    private void CaptureDefaults()
    {
        SetNextAnimation();
        _defaults = new CharacterSnapshot(X, Y, _lastX, _lastY, Direction, _previousDirection, _nextAnimation, _nextDirection, _moving, Mode, _animation, _animationOverride, AllowHouseExit);

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
        if (_animation != PMCharacterAnimationType.Unknown)
            TrySendSprite<BlPacManAnimationBehavior>(_sprite.SpriteNum, behavior => behavior.Play(_animation));
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
        if (_animationOverride  != PMCharacterAnimationType.Unknown)
        {
            _nextAnimation = _animationOverride;
            _nextDirection = Direction;
            return;
        }

        if (Direction == BlPacManDirection.None)
        {
            _nextAnimation = PMCharacterAnimationType.Unknown;
            return;
        }

        var label = GetAnimationLabel(Direction);
        if (label == PMCharacterAnimationType.Unknown)
        {
            _nextAnimation = PMCharacterAnimationType.Unknown;
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
            return false;

        var nextTile = tile.Get(direction);
        //if (Mode == BlPacManCharacterModes.Ghost)
        //{
        //    var insideHouse = tile.IsHouse() || tile.IsGhostHouseEntrance();
        //    if (insideHouse)
        //    {
        //        if (AllowHouseExit && nextTile is not null && !nextTile.IsWall())
        //            return true;
        //        return nextTile is not null && (nextTile.IsHouse() || nextTile.IsGhostHouseEntrance());
        //    }

        //    if (AllowHouseExit && nextTile is not null && (nextTile.IsHouse() || nextTile.IsGhostHouseEntrance()))
        //        return !nextTile.IsWall();
        //}

        //var canGo = nextTile is not null && !nextTile.IsHouse() && !nextTile.IsWall();
        var canGo = nextTile is not null && !nextTile.IsHouse() && !nextTile.IsWall();
        return canGo;
    }

    private bool IsCentered(Tile tile)
    {
        return IsCentered(tile, Axis.Both);
    }

    private bool IsCentered(Tile tile, Axis axis)
    {
        var centeredX = Math.Abs(X - tile.X) < _positionTolerance;
        var centeredY = Math.Abs(Y - tile.Y) < _positionTolerance;

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

    private PMCharacterAnimationType GetAnimationLabel(BlPacManDirection direction)
    {
        if (Type == CharacterType.PacMan)
        {
            return direction switch
            {
                BlPacManDirection.Left => PMCharacterAnimationType.PacManLeft,
                BlPacManDirection.Right => PMCharacterAnimationType.PacManRight,
                BlPacManDirection.Up => PMCharacterAnimationType.PacManUp,
                BlPacManDirection.Down => PMCharacterAnimationType.PacManDown,
                _ => PMCharacterAnimationType.Unknown,
            };
        }
        return direction switch
        {
            BlPacManDirection.Left => PMCharacterAnimationType.GhostLeft,
            BlPacManDirection.Right => PMCharacterAnimationType.GhostRight,
            BlPacManDirection.Up => PMCharacterAnimationType.GhostUp,
            BlPacManDirection.Down => PMCharacterAnimationType.GhostDown,
            _ => PMCharacterAnimationType.Unknown,
        };
    }

    private void WrapPosition()
    {
        var mapWidth = Map.Width * Map.TileWidth;
        if (mapWidth > 0)
        {
            if (X < 0)
                X = mapWidth;
            else if (X > mapWidth)
                X = 0;
        }

        var mapHeight = Map.Height * Map.TileHeight;
        Y = TileMath.ClampVerticalPosition(Y, mapHeight);
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
        PMCharacterAnimationType NextAnimation,
        BlPacManDirection NextDirection,
        bool IsMoving,
        GhostMode Mode,
        PMCharacterAnimationType Animation,
        PMCharacterAnimationType AnimationOverride,
        bool AllowHouseExit);

}
