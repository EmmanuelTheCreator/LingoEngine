using AbstUI.Primitives;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Settings;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Game
{
    /// <summary>
    /// Class responsible for displaying the fruit bonuses availabl across levels.
    /// </summary>
    public sealed class BlPacManBonusAvailableManager 
    {
        private const int _bonusCount = 8;
        private readonly List<BonusSprite> _bonuses = new();
        private readonly GlobalVars _globals;
        private bool _isInitialized;
        private BlPacManEventSubscription? _levelSubscription;

        public BlPacManBonusAvailableManager(GlobalVars globals)
        {
            _globals = globals;
        }

        public float ScaleFactor { get; set; } = 1; //  0.75f;
        public float Spacing { get; set; } = 24f;

        public void Init(IBlingoMovie lingoMovie)
        {
            if (!_isInitialized)
            {
                InitializeBonuses(lingoMovie);
                _levelSubscription?.Release();
                _levelSubscription = _globals.LevelManager.SubscribeLevelChanged(OnLevelChanged);
                _isInitialized = true;
            }

            Render();
        }

        public void RemoveAll()
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
        private void InitializeBonuses(IBlingoMovie lingoMovie)
        {
            var baseX = lingoMovie.Width - BlPacManTheme.Tiles.Size*2;
            var baseY = lingoMovie.Height - BlPacManTheme.Tiles.Size *2 +2;
            var spacing = Spacing * (Math.Abs(ScaleFactor) <= float.Epsilon ? 1f : ScaleFactor);

            for (var i = 0; i < _bonusCount; i++)
            {
                var bonusX = baseX - i * spacing;
                var bonus = CreateBonusSprite(i, bonusX, baseY, lingoMovie);
                bonus.Hide();
                _bonuses.Add(bonus);
            }
        }
        private BonusSprite CreateBonusSprite(int index, float x, float y, IBlingoMovie lingoMovie)
        {
            var name = $"GameBonus_{index}";
            var spriteNum = index + PCSpriteNums.BonusAvailable;
            lingoMovie.Channel(spriteNum).Puppet = true;
            BlingoSprite2D sprite2D = (BlingoSprite2D)lingoMovie.GetSprite(spriteNum)!;
            sprite2D.LocH = x;
            sprite2D.LocV = y;
            sprite2D.SetMember("misc");
            var frameSize = TileMath.SpriteSize - 1;
            sprite2D.SetMemberRect(ARect.New(frameSize * index, 0, frameSize, frameSize));

            return new BonusSprite(name, sprite2D);
        }

        private sealed class BonusSprite : IDisposable
        {
            private readonly BlingoSprite2D _sprite;
            private readonly string _name;
            private bool _disposed;
            public string Name => _name;

            public BonusSprite(string name, BlingoSprite2D sprite)
            {
                _name = name;
                _sprite = sprite;
            }

            public void Show() => _sprite.Visibility = true;

            public void Hide() => _sprite.Visibility = false;

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _sprite.Puppet = false;
            }
        }
    }
}
