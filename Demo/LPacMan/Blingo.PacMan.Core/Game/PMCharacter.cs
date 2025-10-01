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

    GhostDeathUp,
    GhostDeathDown,
    GhostDeathLeft,
    GhostDeathRight,

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
    #region Fields
    private const float _defaultSpeed = 80f;
    private const float _positionTolerance = 0.5f;

    private readonly BlPacManEventMediator<PMCharacter> _moveStarted = new();
    private readonly BlPacManEventMediator<PMCharacter> _stopped = new();
    private readonly BlPacManEventMediator<BlPacManPositionEventData> _positionChanged = new();
    private readonly BlPacManEventMediator<BlPacManTileEventData> _tileEntered = new();

    private PMMap _map;
    private readonly BlingoSprite2D _sprite;
    private float _step;
    private float _speedSetting;
    private float _speed;
    private PMCharacterAnimationType _animation = PMCharacterAnimationType.Unknown;
    private PMCharacterAnimationType _nextAnimation = PMCharacterAnimationType.Unknown;
    private PMCharacterAnimationType _animationOverride = PMCharacterAnimationType.Unknown;
    private bool _moving;
    private bool _preTurnActive;
    private PMDirection _nextDirection;
    private PMDirection _previousDirection;
    private float _lastX;
    private float _lastY;
    private PMTile? _lastTile;
    private bool _defaultsCaptured;
    private CharacterSnapshot? _defaults;
    #endregion



    #region Properties
    public PMTile? LastTile { get => _lastTile; set => _lastTile = value; }
    public bool RotateSprite { get; set; }
    public bool AllowHouseExit { get; set; }
    public CharacterType Type { get; private set; }
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

    public PMDirection Direction { get; set; }

    public PMDirection PreviousDirection => _previousDirection;

    public PMDirection NextDirection
    {
        get => _nextDirection;
        set => _nextDirection = value;
    }

    public bool Preturn { get; set; }

    public GhostMode Mode { 
        get => _mode; 
        set => _mode = value; 
    }

    public PMCharacterAnimationType Animation => _animation;

    private float _x;
    public float X
    {
        get => _x;
        set
        {
            if (_x == value) return;
            _x = value;
            _sprite.LocH = value;
        }
    }
    private float _y;
    private GhostMode _mode;

    public float Y
    {
        get => _y;
        set
        {
            if (_y == value)
                return;
            _y = value;
            _sprite.LocV = value;
        }
    }

    private PMMap Map => _map;

    public BlPacManEventSubscription SubscribeMoveStarted(Action<PMCharacter> handler) => _moveStarted.Subscribe(handler);

    public BlPacManEventSubscription SubscribeStopped(Action<PMCharacter> handler) => _stopped.Subscribe(handler);

    public BlPacManEventSubscription SubscribePositionChanged(Action<BlPacManPositionEventData> handler) => _positionChanged.Subscribe(handler);

    public BlPacManEventSubscription SubscribeTileEntered(Action<BlPacManTileEventData> handler) => _tileEntered.Subscribe(handler);

    #endregion


    public PMCharacter(IBlingoMovieEnvironment env, PMMap map, BlingoSprite2D sprite, CharacterType type, BlPacManCharacterOptions options)
        : base(env)
    {
        Type = type;
        _map = map;
        _sprite = sprite;
        _step = options.Step ?? PMTileMath.GetMovementStep(map);
        Speed = options.Speed ?? _defaultSpeed;
        Direction = options.Direction;
        _previousDirection = Direction;
        _nextDirection = PMDirection.None;
        Preturn = options.Preturn ?? false;
        Mode = options.Mode;
        _lastTile = options.StartTile;
    }



    public void SetMap(PMMap map)
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
            return;

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

    
    public void ForceDirection(PMDirection direction)
    {
        _previousDirection = Direction;
        Direction = direction;
        _nextDirection = direction;
        SetNextAnimation();
    }

    public void Update()
    {
        var tile = GetTile();
        if (tile is null)
            return;

        // Fix float point offset.
        if (Math.Abs(Y - tile.Y) < _positionTolerance) Y = tile.Y;
        if (Math.Abs(X - tile.X) < _positionTolerance) X = tile.X;

        // Position change, move.
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
            // Not moving.
            OnStopped();
            PauseCharacterAnimation();
            _moving = false;
        }
        // Changed animation.
        if (_nextAnimation != PMCharacterAnimationType.Unknown && _animation !=  _nextAnimation)
            SetAnimation(_nextAnimation);
    }
    public void Move(PMDirection direction = PMDirection.None)
    {
        if (direction == PMDirection.None)
            direction = Direction;

        if (direction == PMDirection.None)
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
            if (step is null || step == 0f)
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
                case PMDirection.Up:
                    if (RotateSprite)
                    {
                        _sprite.Rotation = 270;
                        _sprite.FlipH = false;
                    }
                    Y -= distance;
                    break;
                case PMDirection.Right:
                    if (RotateSprite)
                    {
                        _sprite.Rotation = 0;
                        _sprite.FlipH = false;
                    }
                    X += distance;
                    break;
                case PMDirection.Down:
                    if (RotateSprite)
                    {
                        _sprite.Rotation = 90;
                        _sprite.FlipH = false;
                    }
                    Y += distance;
                    break;
                case PMDirection.Left:
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
        if (X < 0) X = Map.Width * Map.TileWidth;
        if (X > Map.Width * Map.TileWidth) X = 0;
        if (Y < 0) Y = Map.Height * Map.TileHeight;
        if (Y > Map.Height * Map.TileHeight) Y = 0;

        var newTile = GetTile();
        if (newTile is not null && !ReferenceEquals(newTile, _lastTile))
        {
            _lastTile = newTile;
            HandleTileEntered(newTile);
        }

        Update();
    }

    public float GetStep() => _step * (_speed / 100f);

    private void SetNextAnimation()
    {
        if (_animationOverride != PMCharacterAnimationType.Unknown)
        {
            _nextAnimation = _animationOverride;
            _nextDirection = Direction;
            return;
        }

        if (Direction == PMDirection.None)
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


    private bool CanGo(PMDirection direction, PMTile? currentTile = null)
    {
        var tile = currentTile ?? GetTile();
        if (tile is null)
            return false;

        var nextTile = tile.Get(direction);
        var canGo = nextTile is not null && !nextTile.IsHouse() && !nextTile.IsWall();
        return canGo;
    }

    private bool IsCentered(PMTile tile) => IsCentered(tile, Axis.Both);

    private bool IsCentered(PMTile tile, Axis axis)
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

    public float GetMin(float value1, float value2) => value1 < value2 ? value1 : value2;




    public void SetAnimation(PMCharacterAnimationType animation)
    {
        if (_animation == animation)
            return;

        _animation = animation;
        ApplyAnimation(animation);
    }

    public PMTile? GetTile() => Map.GetTile(X, Y, true);

    public void Hide() => _sprite.Visibility = false;

    public void Show() => _sprite.Visibility = true;

  

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

    private void HandleTileEntered(PMTile tile)
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

    private void UpdateDirection(PMDirection direction)
    {
        if (Direction == direction)
        {
            return;
        }

        _previousDirection = Direction;
        Direction = direction;
    }


  

    private static bool AreEqual(float a, float b) => Math.Abs(a - b) < 0.001f;

    private PMCharacterAnimationType GetAnimationLabel(PMDirection direction)
    {
        if (Type == CharacterType.PacMan)
        {
            return direction switch
            {
                PMDirection.Left => PMCharacterAnimationType.PacManLeft,
                PMDirection.Right => PMCharacterAnimationType.PacManRight,
                PMDirection.Up => PMCharacterAnimationType.PacManUp,
                PMDirection.Down => PMCharacterAnimationType.PacManDown,
                _ => PMCharacterAnimationType.Unknown,
            };
        }
        return direction switch
        {
            PMDirection.Left => PMCharacterAnimationType.GhostLeft,
            PMDirection.Right => PMCharacterAnimationType.GhostRight,
            PMDirection.Up => PMCharacterAnimationType.GhostUp,
            PMDirection.Down => PMCharacterAnimationType.GhostDown,
            _ => PMCharacterAnimationType.Unknown,
        };
    }

  

    private void OnTileEntered(PMTile tile)
    {
        SetNextAnimation();
    }

    internal void ForceUpdateBlingoPosition()
    {
        _sprite.LocH = X;
        _sprite.LocV = Y;
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
        PMDirection Direction,
        PMDirection PreviousDirection,
        PMCharacterAnimationType NextAnimation,
        PMDirection NextDirection,
        bool IsMoving,
        GhostMode Mode,
        PMCharacterAnimationType Animation,
        PMCharacterAnimationType AnimationOverride,
        bool AllowHouseExit);

}
