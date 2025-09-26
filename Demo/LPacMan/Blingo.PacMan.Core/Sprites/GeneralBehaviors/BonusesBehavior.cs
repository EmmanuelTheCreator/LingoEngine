using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using AbstUI.Primitives;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GeneralBehaviors
{
    /// <summary>
    /// Behaviour responsible for displaying the fruit bonuses earned across levels.
    /// </summary>
    public sealed class BonusesBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasEndSpriteEvent
    {
        private const int _bonusCount = 8;
        private readonly List<BonusSprite> _bonuses = new();
        private readonly GlobalVars _globals;
        private bool _isInitialized;
        private BlPacManEventSubscription? _levelSubscription;

        public BonusesBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
            : base(env)
        {
            _globals = globals ?? throw new ArgumentNullException(nameof(globals));
        }

        public float ScaleFactor { get; set; } = 1f;

        public float Spacing { get; set; } = 32f;

        /// <summary>
        /// Optional cropping rectangle applied to each bonus sprite.
        /// </summary>
        public ARect? MemberSourceRect { get; set; }

        public void BeginSprite()
        {
            if (!_isInitialized)
            {
                InitializeBonuses();
                _levelSubscription?.Release();
                _levelSubscription = _globals.LevelManager.SubscribeLevelChanged(OnLevelChanged);
                _isInitialized = true;
            }

            Render();
        }

        public void EndSprite()
        {
            if (!_isInitialized)
                return;

            _levelSubscription?.Release();
            _levelSubscription = null;

            foreach (var bonus in _bonuses)
                bonus.Dispose();

            _bonuses.Clear();
            _isInitialized = false;
        }

        private void InitializeBonuses()
        {
            var baseX = Me.LocH;
            var baseY = Me.LocV;
            var spacing = Spacing * (Math.Abs(ScaleFactor) <= float.Epsilon ? 1f : ScaleFactor);

            for (var i = 0; i < _bonusCount; i++)
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
            var level = _globals.LevelManager.Level;
            for (var i = 0; i < _bonuses.Count; i++)
            {
                if (i < level)
                    _bonuses[i].Show();
                else
                    _bonuses[i].Hide();
            }
        }

        private BonusSprite CreateBonusSprite(int index, float x, float y)
        {
            var name = $"{Me.SpriteNum}_{Me.Name}_Bonus_{index}";
            var sprite = _Movie.AddSprite(name, sprite2D =>
            {
                sprite2D.LocH = x;
                sprite2D.LocV = y;
                sprite2D.LocZ = Me.LocZ;
                sprite2D.Puppet = true;

                if (Me.Member != null)
                    sprite2D.SetMember(Me.Member);

                var sourceRect = MemberSourceRect ?? Me.MemberSourceRect;
                if (sourceRect is { })
                    sprite2D.MemberSourceRect = sourceRect;
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
                    return;

                _disposed = true;
                _movie.RemoveSprite(_name);
            }
        }
    }
}
