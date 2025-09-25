using System;
using System.Collections.Generic;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using AbstUI.Primitives;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GeneralBehaviors;

/// <summary>
/// Displays the remaining lives using miniature Pac-Man sprites and keeps them in sync with the model.
/// </summary>
public sealed class LivesBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasEndSpriteEvent
{
    private const int DefaultIconCount = 5;

    private readonly List<LifeSprite> _pacmen = new();
    private readonly GlobalVars _globals;
    private GameModel? _model;
    private BlPacManEventSubscription? _livesSubscription;
    private BlPacManEventSubscription? _extraLivesSubscription;

    private bool _isInitialized;

    public LivesBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    /// <summary>
    /// Scaling factor applied to each life sprite. The value is forwarded to the Pac-Man constructor.
    /// </summary>
    public float ScaleFactor { get; set; } = 1f;

    /// <summary>
    /// Horizontal spacing between consecutive life sprites.
    /// </summary>
    public float Spacing { get; set; } = 70f;

    /// <summary>
    /// Cast library used to source the Pac-Man member.
    /// </summary>
    public string CastLibName { get; set; } = "Data";

    /// <summary>
    /// Member that contains the Pac-Man artwork.
    /// </summary>
    public string MemberName { get; set; } = "sprites";

    /// <summary>
    /// Optional rectangle that selects the Pac-Man icon within the sprite sheet.
    /// </summary>
    public ARect? MemberSourceRect { get; set; }

    public void BeginSprite()
    {
        if (!_isInitialized)
        {
            _model ??= _globals.GameModel ?? throw new InvalidOperationException("GameModel was not initialised.");
            InitializeLives();
            _livesSubscription?.Release();
            _livesSubscription = _model.SubscribeLivesChanged(OnLivesChanged);
            _extraLivesSubscription?.Release();
            _extraLivesSubscription = _model.SubscribeExtraLivesChanged(OnExtraLivesChanged);
            _isInitialized = true;
        }

        Render();
    }

    public void EndSprite()
    {
        if (!_isInitialized)
        {
            return;
        }

        if (_model is not null)
        {
            _livesSubscription?.Release();
            _livesSubscription = null;
            _extraLivesSubscription?.Release();
            _extraLivesSubscription = null;
        }

        foreach (var pacman in _pacmen)
        {
            pacman.Dispose();
        }

        _pacmen.Clear();
        _isInitialized = false;
    }

    private void InitializeLives()
    {
        var baseX = Me.LocH;
        var baseY = Me.LocV;
        var spacing = Spacing * (Math.Abs(ScaleFactor) <= float.Epsilon ? 1f : ScaleFactor);

        for (var i = 0; i < DefaultIconCount; i++)
        {
            var x = baseX + i * spacing;
            var lifeSprite = CreateLifeSprite(i, x, baseY);
            lifeSprite.Hide();
            _pacmen.Add(lifeSprite);
        }
    }

    private void OnLivesChanged(int _)
    {
        Render();
    }

    private void OnExtraLivesChanged(int _)
    {
        if (!_globals.IsMuted)
        {
            _Player.SoundPlayLife();
        }
    }

    private void Render()
    {
        var model = _model ?? throw new InvalidOperationException("GameModel was not initialised.");
        var visibleCount = Math.Max(0, model.Lives - 1);

        for (var i = 0; i < _pacmen.Count; i++)
        {
            if (i < visibleCount)
            {
                _pacmen[i].Show();
            }
            else
            {
                _pacmen[i].Hide();
            }
        }
    }

    private LifeSprite CreateLifeSprite(int index, float x, float y)
    {
        var spriteName = $"{Me.SpriteNum}_{Me.Name}_Life_{index}";
        var sprite = _Movie.AddSprite(spriteName, sprite2D =>
        {
            sprite2D.LocH = x;
            sprite2D.LocV = y;
            sprite2D.LocZ = Me.LocZ;
            sprite2D.Puppet = true;
            sprite2D.Ink = Me.Ink;

            var cast = CastLib(CastLibName);
            var member = cast?.GetMember<BlingoMember>(MemberName);
            if (member != null)
            {
                sprite2D.Member = member;
            }

            var sourceRect = MemberSourceRect ?? Me.MemberSourceRect;
            if (sourceRect is { })
            {
                sprite2D.MemberSourceRect = sourceRect;
            }

            if (Math.Abs(ScaleFactor - 1f) > float.Epsilon)
            {
                var width = sprite2D.Width;
                var height = sprite2D.Height;

                if (width > 0)
                {
                    sprite2D.Width = width * ScaleFactor;
                }

                if (height > 0)
                {
                    sprite2D.Height = height * ScaleFactor;
                }
            }
        });

        return new LifeSprite(_Movie, spriteName, sprite);
    }

    private sealed class LifeSprite : IDisposable
    {
        private readonly IBlingoMovie _movie;
        private readonly string _name;
        private readonly BlingoSprite2D _sprite;
        private bool _disposed;

        public LifeSprite(IBlingoMovie movie, string name, BlingoSprite2D sprite)
        {
            _movie = movie ?? throw new ArgumentNullException(nameof(movie));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
        }

        public void Show() => _sprite.Visibility = true;

        public void Hide() => _sprite.Visibility = false;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _movie.RemoveSprite(_name);
        }
    }
}
