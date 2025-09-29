using AbstUI.Primitives;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Game;

/// <summary>
/// Displays the remaining lives using miniature Pac-Man sprites and keeps them in sync with the model.
/// </summary>
public sealed class BlPacManLivesManager
{
    private const int _defaultIconCount = 9;

    private bool _isInitialized;
    private readonly List<LifeSprite> _pacmenLives = new();
    private readonly GlobalVars _globals;
    private IBlingoPlayer _lingoPlayer = null!;
    private int _lives = 3;
    private int _extraLives = 1;
  

    /// <summary>
    /// Scaling factor applied to each life sprite. The value is forwarded to the Pac-Man constructor.
    /// </summary>
    public float ScaleFactor { get; set; } = 1.8f;
    /// <summary>
    /// Horizontal spacing between consecutive life sprites.
    /// </summary>
    public float Spacing { get; set; } = 18f;

    public int Lives => _lives;
    public int ExtraLives => _extraLives;

    public BlPacManLivesManager(GlobalVars globals)
    {
        _globals = globals;
    }
    public void Init(IBlingoMovie lingoMovie, IBlingoPlayer player)
    {
        _lingoPlayer = player;
        if (!_isInitialized)
        {
            InitializeLives(lingoMovie);
            _isInitialized = true;
        }

        Render();
    }

    #region Lives
    public bool HasLives() => Lives > 0;
    public void ResetLives(int lives)
    {
        SetLives(Math.Max(0, lives));
    }
    private void SetLives(int lives)
    {
        if (_lives == lives)
            return;

        _lives = lives;

        Render();
        if (!_globals.State.IsGameOver && lives == 0)
            _globals.GameBehavior?.DoGameOver();
    }
    public void AddLive() => SetLives(_lives + 1);
    internal void OnPacManEaten()
    {
        var lives = Math.Max(0, Lives - 1);
        SetLives(lives);
    }

    public void AddExtraLiveIfPossible()
    {
        if (_extraLives <= 0)
            return;
        
        _extraLives--;
        SetLives(_lives + 1);
        if (!_globals.State.IsMuted)
            _lingoPlayer.SoundPlayLife();
    }

    #endregion

  


    public void RemoveAll()
    {
        if (!_isInitialized)
            return;

        foreach (var pacman in _pacmenLives)
            pacman.Dispose();

        _pacmenLives.Clear();
        _isInitialized = false;
    }

  

 

    private void Render()
    {
        var visibleCount = Math.Max(0, Lives - 1);

        for (var i = 0; i < _pacmenLives.Count; i++)
        {
            if (i < visibleCount)
                _pacmenLives[i].Show();
            else
                _pacmenLives[i].Hide();
        }
    }

    private void InitializeLives(IBlingoMovie lingoMovie)
    {
        var baseX = 20f;
        var baseY = lingoMovie.Height - 32f;
        var spacing = Spacing * (Math.Abs(ScaleFactor) <= float.Epsilon ? 1f : ScaleFactor);

        for (var i = 0; i < _defaultIconCount; i++)
        {
            var x = baseX + i * spacing;
            var lifeSprite = CreateLifeSprite(i, x, baseY, lingoMovie);
            lifeSprite.Hide();
            _pacmenLives.Add(lifeSprite);
        }
    }
    private LifeSprite CreateLifeSprite(int index, float x, float y, IBlingoMovie lingoMovie)
    {
        var spriteName = $"Life_{index}";
        var spriteNum = index + PCSpriteNums.LivesStart;
        lingoMovie.Channel(spriteNum).Puppet = true;
        BlingoSprite2D sprite2D = (BlingoSprite2D)lingoMovie.GetSprite(spriteNum)!;
        sprite2D.LocH = x;
        sprite2D.LocV = y;
        sprite2D.Name = spriteName;
        sprite2D.Visibility = false;
        sprite2D.SetMember("sprites");

        var frameSize = TileMath.SpriteSize;
        var rect = ARect.New(0,0, frameSize, frameSize);
        sprite2D.SetMemberRect(rect, new APoint(rect.Width / 2f, rect.Height / 2f));


        if (Math.Abs(ScaleFactor - 1f) > float.Epsilon)
        {
            var width = sprite2D.Width;
            var height = sprite2D.Height;

            if (width > 0)
                sprite2D.Width = width * ScaleFactor;

            if (height > 0)
                sprite2D.Height = height * ScaleFactor;
        }

        return new LifeSprite(spriteName, sprite2D);
    }

    private sealed class LifeSprite : IDisposable
    {
        private readonly string _name;
        private readonly BlingoSprite2D _sprite;
        private bool _disposed;
        public string Name => _name;
        public LifeSprite(string name, BlingoSprite2D sprite)
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
