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

    public BlPacManRoamingBonusBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

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

    public void EndSprite()
    {
        _coordinator?.UnregisterBonus(this);
        _pacManSubscription?.Release();
        _pacManSubscription = null;
        _tileSubscription?.Release();
        _tileSubscription = null;
        _character = null;
    }

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

    public void Deactivate()
    {
        _active = false;
        _remainingTargetVisits = 0;
        _direction = BlPacManDirection.Left;
        var character = EnsureCharacter();
        character.Hide();
        SetAnimation("default");
    }

    public void ResetForLife()
    {
        Deactivate();
    }

    public void OnPacManEaten()
    {
        Deactivate();
    }

    public void ShowScore()
    {
        if (_scoreValue > 0)
        {
            SetAnimation($"score{_scoreValue}");
        }
    }

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

    private void SetAnimation(string name)
    {
        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior => behavior.Play(name));
    }

    private void OnPacManPositionChanged(BlPacManPositionContext context)
    {
        _pacManPosition = context;
    }

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

    private bool CanMove(BlPacManDirection direction, Tile tile)
    {
        var next = tile.Get(direction);
        return next is not null && !next.IsWall() && !next.IsHouse();
    }

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

    private static ARect CreateFrame(int offsetX, int offsetY)
    {
        return new ARect(offsetX, offsetY, offsetX + FrameSize, offsetY + FrameSize);
    }
}
