// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using System;
using System.Collections.Generic;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.Behaviors
{
    /// <summary>
    /// Behaviour responsible for displaying the fruit bonuses earned across levels.
    /// </summary>
    public sealed class BonusesBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasEndSpriteEvent
    {
        private const int BonusCount = 8;
        private readonly List<BonusSprite> _bonuses = new();
        private readonly IBonusesModel _model;
        private bool _isInitialized;

        public float ScaleFactor { get; set; } = 1f;

        public float Spacing { get; set; } = 64f;

        public string BonusMemberName { get; set; } = "sprites";

        public string BonusCastLibName { get; set; } = "Data";

        public BonusesBehavior(IBlingoMovieEnvironment env, IBonusesModel model)
            : base(env)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void BeginSprite()
        {
            if (!_isInitialized)
            {
                InitializeBonuses();
                _model.LevelChanged += OnLevelChanged;
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

            _model.LevelChanged -= OnLevelChanged;

            foreach (var bonus in _bonuses)
            {
                bonus.Dispose();
            }

            _bonuses.Clear();
            _isInitialized = false;
        }

        private void InitializeBonuses()
        {
            var baseX = Me.LocH;
            var baseY = Me.LocV;
            var spacing = Spacing * (Math.Abs(ScaleFactor) <= float.Epsilon ? 1f : ScaleFactor);

            for (var i = 0; i < BonusCount; i++)
            {
                var bonusX = baseX - i * spacing;
                var bonus = CreateBonusSprite(i, bonusX, baseY);
                bonus.Hide();
                _bonuses.Add(bonus);
            }
        }

        private void OnLevelChanged(int _)
        {
            Render();
        }

        private void Render()
        {
            for (var i = 0; i < _bonuses.Count; i++)
            {
                if (i < _model.Level)
                {
                    _bonuses[i].Show();
                }
                else
                {
                    _bonuses[i].Hide();
                }
            }
        }

        private BonusSprite CreateBonusSprite(int index, float x, float y)
        {
            var name = $"{Me.SpriteNum}_{Me.Name}_Bonus_{index}";
            var sprite = _Movie.AddSprite(name, sprite2D =>
            {
                sprite2D.BeginFrame = 1;
                sprite2D.EndFrame = Math.Max(1, _Movie.FrameCount);
                sprite2D.LocH = x;
                sprite2D.LocV = y;
                sprite2D.LocZ = Me.LocZ;
                sprite2D.Puppet = true;
                sprite2D.Lock = true;

                var cast = CastLib(BonusCastLibName);
                var member = cast?.GetMember<BlingoMember>(BonusMemberName);
                if (member != null)
                {
                    sprite2D.Member = member;
                }
            });

            return new BonusSprite(_Movie, name, sprite);
        }

        private sealed class BonusSprite : IDisposable
        {
            private readonly IBlingoMovie _movie;
            private readonly BlingoSprite2D _sprite;
            private readonly string _name;
            private bool _disposed;

            public BonusSprite(IBlingoMovie movie, string name, BlingoSprite2D sprite)
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
}
