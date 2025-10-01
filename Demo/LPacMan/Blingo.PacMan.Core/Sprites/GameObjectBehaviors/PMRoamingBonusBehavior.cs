using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Controls the roaming bonus fruit, including spawn timing, navigation, and score reveal logic.
/// </summary>
internal sealed class PMRoamingBonusBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent
{
    private static readonly IReadOnlyDictionary<PMCharacterAnimationType, ARect[]> _animations = BlPacManTheme.Bonus.Animations;
    internal static ARect DefaultAnimationRect => BlPacManTheme.Bonus.DefaultFrame;

    private readonly GlobalVars _globals;
    private GameSettings? _settings;
    private PMCharacter? _character;
    private BlPacManEventSubscription? _tileSubscription;
    private PMTile? _spawnTile;
    private PMTile? _targetTile;
    private bool _animationsConfigured;
    private bool _active;
    private int _scoreValue;
    private int _remainingTargetVisits;
    private PMDirection _direction = PMDirection.Left;

    /// <summary>
    /// Initialises the bonus behaviour with the movie environment and shared globals.
    /// </summary>
    public PMRoamingBonusBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    /// <summary>
    /// Applies the current level settings used to determine spawn/target tiles and score values.
    /// </summary>
    public void Configure(GameSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _scoreValue = settings.BonusScore;
        ParseMap();
    }

   

    /// <summary>
    /// Registers the bonus with the coordinator and starts listening to Pac-Man position updates.
    /// </summary>
    public void BeginSprite()
    {
        ApplyAppearance();
        var character = EnsureCharacter();
        character.BeginSprite();
        character.Hide();
        _globals.BonusManager.Attach(this);

        var settings = _globals.LevelManager.GetGameSettings();
        if (settings != null)
            Configure(settings);
    }

    /// <summary>
    /// Releases event subscriptions when the sprite is removed.
    /// </summary>
    public void EndSprite()
    {
        _globals.BonusManager.Detach(this);
        _tileSubscription?.Release();
        _tileSubscription = null;
        _character = null;
    }

    /// <summary>
    /// Moves the fruit once it is active in the maze.
    /// </summary>
    public void Tick()
    {
        if (!_active)
            return;

        var character = EnsureCharacter();
        var tile = character.GetTile();
        if (tile is not null)
        {
            var next = GetNextDirection(tile);
            if (next != PMDirection.None)
            {
                _direction = next;
            }
        }

        character.Move(_direction);
    }

    /// <summary>
    /// Spawns the fruit and primes its target visit counter.
    /// </summary>
    public void Activate()
    {
        var character = EnsureCharacter();
        character.Reset();
        character.Show();
        _active = true;
        _remainingTargetVisits = 2;
        _direction = PMDirection.Left;
        SetAnimation(PMCharacterAnimationType.BonusDefault);
    }

    /// <summary>
    /// Removes the fruit from the maze and resets animations.
    /// </summary>
    public void Deactivate()
    {
        _active = false;
        _remainingTargetVisits = 0;
        _direction = PMDirection.Left;
        var character = EnsureCharacter();
        character.Hide();
        SetAnimation(PMCharacterAnimationType.BonusDefault);
    }

    /// <summary>
    /// Hides the fruit when Pac-Man loses a life.
    /// </summary>
    public void ResetForLife()
    {
        Deactivate();
    }

    /// <summary>
    /// Ensures the fruit is hidden during the death animation.
    /// </summary>
    public void OnPacManEaten()
    {
        Deactivate();
    }

    internal bool IsActive => _active;

    internal PMTile? CurrentTile => _character?.GetTile();

    internal bool Collect()
    {
        if (!_active)
        {
            return false;
        }

        _active = false;
        ShowScore();
        if (!_globals.State.IsMuted)
        {
            _Player.SoundPlayBonus();
        }

        _globals.BonusManager.NotifyBonusEaten(this);
        return true;
    }

    /// <summary>
    /// Switches the sprite to display the awarded score after being eaten.
    /// </summary>
    public void ShowScore()
    {
        if (_scoreValue > 0)
        {
            var enumValue = Enum.Parse<PMCharacterAnimationType>($"BonusScore{_scoreValue}");
            SetAnimation(enumValue);
        }
    }

    /// <summary>
    /// Configures the sprite's member and default animation frames.
    /// </summary>
    private void ApplyAppearance()
    {
        var cast = CastLib("Data");
        var member = cast?.GetMember<BlingoMemberBitmap>("misc");
        if (member != null)
        {
            Me.Member = member;
            Me.SetMemberRect(BlPacManTheme.Bonus.DefaultFrame);
        }

        ParseMap();

        if (_spawnTile is not null)
        {
            _character.X = _spawnTile.X;
            _character.Y = _spawnTile.Y + BlPacManTheme.Bonus.VerticalOffset;
        }

        EnsureAnimations();
        SetAnimation(PMCharacterAnimationType.BonusDefault);
    }

    private void ParseMap()
    {
        var map = _globals.Map;
        if (map is null)
            return;
        
        var character = EnsureCharacter();
        character.SetMap(map);
        character.Step = PMTileMath.GetMovementStep(map);
        _spawnTile = map.Tunnels.Count > 0 ? map.Tunnels[^1] : map.HouseCenter;
        _targetTile = map.Tunnels.Count > 0 ? map.Tunnels[0] : map.HouseCenter;
    }

    /// <summary>
    /// Lazily instantiates the character helper so the bonus can traverse the tile map.
    /// </summary>
    private PMCharacter EnsureCharacter()
    {
        if (_character is not null)
        {
            return _character;
        }

        var map = _globals.LevelManager.Map ?? throw new InvalidOperationException("Pac-Man map is not initialized.");
        var baseStep = PMTileMath.GetMovementStep(map);
        _character = new PMCharacter(_env, map, Me, PMCharacter.CharacterType.Fruit, new BlPacManCharacterOptions
        {
            Step = baseStep,
            Speed = 40f,
            Direction = PMDirection.Left,
            Preturn = true,
        });

        _tileSubscription?.Release();
        _tileSubscription = _character.SubscribeTileEntered(OnTileEntered);

        return _character;
    }

    /// <summary>
    /// Registers each possible score animation lazily.
    /// </summary>
    private void EnsureAnimations()
    {
        if (_animationsConfigured)
        {
            return;
        }

        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior =>
        {
            foreach (var (name, frames) in _animations)
            {
                behavior.SetAnimationRects(name, frames, 0);
            }
        });

        _animationsConfigured = true;
    }

    /// <summary>
    /// Applies the named animation if it has been registered.
    /// </summary>
    private void SetAnimation(PMCharacterAnimationType name)
    {
        SendSprite<BlPacManAnimationBehavior>(Me.SpriteNum, behavior => behavior.Play(name));
    }

    /// <summary>
    /// Stores Pac-Man's current tile for collision checks.
    /// </summary>
    /// <summary>
    /// Tracks visits to the target tile so the fruit despawns after reaching its end point twice.
    /// </summary>
    private void OnTileEntered(BlPacManTileEventData context)
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
                _globals.BonusManager.NotifyBonusExpired(this);
                Deactivate();
            }
        }
    }

    /// <summary>
    /// Determines the next direction that moves the fruit closer to its goal while avoiding walls.
    /// </summary>
    private PMDirection GetNextDirection(PMTile tile)
    {
        if (_targetTile is null)
        {
            return _direction;
        }

        var directions = new[]
        {
            PMDirection.Up,
            PMDirection.Left,
            PMDirection.Down,
            PMDirection.Right
        };

        var current = _direction == PMDirection.None ? PMDirection.Left : _direction;
        PMDirection bestDirection = current;
        var bestDistance = float.PositiveInfinity;

        foreach (var direction in directions)
        {
            if (direction == current.GetOpposite())
                continue;

            if (!CanMove(direction, tile))
                continue;

            var candidate = tile.Get(direction);
            var distance = PMTileMath.GetDistance(candidate, _targetTile);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = direction;
            }
        }

        if (bestDistance.Equals(float.PositiveInfinity))
        {
            var fallback = current.GetOpposite();
            if (fallback != PMDirection.None && CanMove(fallback, tile))
                return fallback;

            return current;
        }

        return bestDirection;
    }

    /// <summary>
    /// Indicates whether the fruit can travel into the specified tile.
    /// </summary>
    private bool CanMove(PMDirection direction, PMTile tile)
    {
        var next = tile.Get(direction);
        return next is not null && !next.IsWall() && !next.IsHouse();
    }
}
