using System;
using System.Collections.Generic;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.Behaviors;

/// <summary>
/// Displays the remaining lives using miniature Pac-Man sprites, closely mirroring the
/// JavaScript implementation that populated five instances on the stage and toggled their
/// visibility based on the model.
/// </summary>
public sealed class LivesBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasEndSpriteEvent
{
    private const int DefaultIconCount = 5;

    private readonly List<LifeSprite> _pacmen = new();
    private readonly IGameModel _model;

    private bool _isInitialized;

    public LivesBehavior(IBlingoMovieEnvironment env, IGameModel model)
        : base(env)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    /// Scaling factor applied to each life sprite. Matches the semantics of the original
    /// implementation where a multiplicative factor was forwarded to the Pacman constructor.
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

    public void BeginSprite()
    {
        if (!_isInitialized)
        {
            InitializeLives();
            _model.LivesChanged += OnLivesChanged;
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

        _model.LivesChanged -= OnLivesChanged;

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

    private void Render()
    {
        var visibleCount = Math.Max(0, _model.Lives - 1);

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
            sprite2D.BeginFrame = 1;
            sprite2D.EndFrame = Math.Max(1, _Movie.FrameCount);
            sprite2D.LocH = x;
            sprite2D.LocV = y;
            sprite2D.LocZ = Me.LocZ;
            sprite2D.Puppet = true;
            sprite2D.Lock = true;
            sprite2D.Ink = Me.Ink;

            var cast = CastLib(CastLibName);
            var member = cast?.GetMember<BlingoMember>(MemberName);
            if (member != null)
            {
                sprite2D.Member = member;
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
