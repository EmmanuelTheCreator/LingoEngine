using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlingoEngine.Core;
using Blingo.PacMan.Core.Maps;
using MapContent = Blingo.PacMan.Core.Maps.Maps;

namespace Blingo.PacMan.Core.Models;

/// <summary>
/// Provides access to level-specific parameters and keeps track of the player's progress.
/// </summary>
public interface IGameModel
{
    int Level { get; set; }

    int Score { get; }

    int HighScore { get; }

    int Lives { get; }

    int ExtraLives { get; }

    int ExtraLifeScore { get; }

    GhostMode? Mode { get; }

    event Action<int>? LevelChanged;

    event Action<int>? ScoreChanged;

    event Action<int>? HighScoreChanged;

    event Action<int>? LivesChanged;

    event Action<int>? ExtraLivesChanged;

    event Action<GhostMode?>? ModeChanged;

    void AddScore(int score);

    void ResetScore();

    void UpdateMode();

    void ResetLives(int lives);

    void Pause();

    void Resume();

    GameSettings GetGameSettings();

    PacmanSettings GetPacmanSettings();

    GhostSettings GetGhostSettings();
}

/// <summary>
/// Default implementation that mirrors the behaviour of the original JavaScript model.
/// </summary>
public sealed class GameModel : IGameModel
{
    private readonly IGameModelRepository _repository;
    private readonly IBlingoClock _clock;
    private TimeSpan _modeElapsed;
    private int? _lastModeTick;
    private bool _modePaused;
    private int _level = 1;
    private int _score;
    private int _highScore;
    private int _lives = 3;
    private int _extraLives = 1;
    private GhostMode? _mode;

    public GameModel(IGameModelRepository repository, IBlingoClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        Load();
    }

    public int Level
    {
        get => _level;
        set
        {
            var clamped = Math.Max(1, value);
            if (_level == clamped)
            {
                return;
            }

            _level = clamped;
            ResetModeTimer();
            LevelChanged?.Invoke(_level);
        }
    }

    public int Score => _score;

    public int HighScore => _highScore;

    public int Lives => _lives;

    public int ExtraLives => _extraLives;

    public int ExtraLifeScore { get; set; } = 10_000;

    public GhostMode? Mode => _mode;

    public event Action<int>? LevelChanged;

    public event Action<int>? ScoreChanged;

    public event Action<int>? HighScoreChanged;

    public event Action<int>? LivesChanged;

    public event Action<int>? ExtraLivesChanged;

    public event Action<GhostMode?>? ModeChanged;

    public void AddScore(int score)
    {
        if (score == 0)
        {
            return;
        }

        _score = Math.Max(0, _score + score);
        OnScoreChanged();
    }

    public void ResetScore()
    {
        if (_score == 0)
        {
            return;
        }

        _score = 0;
        OnScoreChanged();
    }

    public void ResetLives(int lives)
    {
        SetLives(Math.Max(0, lives));
    }

    public void UpdateMode()
    {
        var sequence = GetGameSettings().ModeSequence;
        if (sequence.Count == 0)
        {
            SetMode(null);
            return;
        }

        var frameRate = _clock.FrameRate;
        if (frameRate <= 0)
        {
            return;
        }

        var engineTickCount = _clock.EngineTickCount;
        if (_lastModeTick is null)
        {
            _lastModeTick = engineTickCount;
        }
        else if (!_modePaused)
        {
            var deltaTicks = Math.Max(0, engineTickCount - _lastModeTick.Value);
            if (deltaTicks > 0)
            {
                var deltaSeconds = deltaTicks / (double)frameRate;
                _modeElapsed += TimeSpan.FromSeconds(deltaSeconds);
            }
            _lastModeTick = engineTickCount;
        }
        else
        {
            _lastModeTick = engineTickCount;
        }

        var elapsed = _modeElapsed;
        var cumulative = TimeSpan.Zero;

        GhostMode? selected = null;
        for (var i = 0; i < sequence.Count; i++)
        {
            var timing = sequence[i];
            cumulative += timing.Duration;
            if (elapsed < cumulative || i == sequence.Count - 1)
            {
                selected = timing.Mode;
                break;
            }
        }

        SetMode(selected);
    }

    public void Pause()
    {
        _modePaused = true;
    }

    public void Resume()
    {
        _modePaused = false;
        _lastModeTick = _clock.EngineTickCount;
    }

    public GameSettings GetGameSettings()
    {
        return GetLevelSettings().Game;
    }

    public PacmanSettings GetPacmanSettings()
    {
        return GetLevelSettings().Pacman;
    }

    public GhostSettings GetGhostSettings()
    {
        return GetLevelSettings().Ghost;
    }

    private LevelSettings GetLevelSettings()
    {
        var index = Math.Clamp(_level - 1, 0, LevelTable.Length - 1);
        return LevelTable[index];
    }

    private void OnScoreChanged()
    {
        ScoreChanged?.Invoke(_score);

        if (_extraLives > 0 && _score >= ExtraLifeScore)
        {
            _extraLives--;
            ExtraLivesChanged?.Invoke(_extraLives);
            SetLives(_lives + 1);
        }

        if (_score > _highScore)
        {
            _highScore = _score;
            HighScoreChanged?.Invoke(_highScore);
            Save();
        }
    }

    private void SetLives(int lives)
    {
        if (_lives == lives)
        {
            return;
        }

        _lives = lives;
        LivesChanged?.Invoke(_lives);
    }

    private void SetMode(GhostMode? mode)
    {
        if (_mode == mode)
        {
            return;
        }

        _mode = mode;
        ModeChanged?.Invoke(_mode);
    }

    private void ResetModeTimer()
    {
        _modeElapsed = TimeSpan.Zero;
        _lastModeTick = null;
        _modePaused = false;
        SetMode(null);
    }

    private void Load()
    {
        var data = _repository.Load();
        if (data is null)
        {
            return;
        }

        _highScore = Math.Max(0, data.HighScore);
    }

    private void Save()
    {
        _repository.Save(new PacManSaveData
        {
            HighScore = _highScore,
        });
    }

    private static readonly IReadOnlyList<ModeTiming> DefaultModeSequence = new ReadOnlyCollection<ModeTiming>(new[]
    {
        new ModeTiming(GhostMode.Scatter, TimeSpan.FromSeconds(7)),
        new ModeTiming(GhostMode.Chase, TimeSpan.FromSeconds(20)),
        new ModeTiming(GhostMode.Scatter, TimeSpan.FromSeconds(7)),
        new ModeTiming(GhostMode.Chase, TimeSpan.FromSeconds(20)),
        new ModeTiming(GhostMode.Scatter, TimeSpan.FromSeconds(5)),
        new ModeTiming(GhostMode.Chase, TimeSpan.FromSeconds(20)),
        new ModeTiming(GhostMode.Scatter, TimeSpan.FromSeconds(5)),
        new ModeTiming(GhostMode.Chase, TimeSpan.FromSeconds(1_000_000)),
    });

    private static readonly IReadOnlyList<string> Map1Layout = Array.AsReadOnly(MapContent.Map1);
    private static readonly IReadOnlyList<string> Map2Layout = Array.AsReadOnly(MapContent.Map2);
    private static readonly IReadOnlyList<string> Map3Layout = Array.AsReadOnly(MapContent.Map3);
    private static readonly IReadOnlyList<string> Map4Layout = Array.AsReadOnly(MapContent.Map4);

    private static readonly LevelSettings[] LevelTable =
    {
        CreateLevelSettings(DefaultModeSequence, 0, 100, 80f, 71f, 75f, 40f, 20, 80f, 10, 85f, 90f, 79f, 50f, 6, 5, Map1Layout, "maze-1"),
        CreateLevelSettings(DefaultModeSequence, 1, 200, 90f, 79f, 85f, 45f, 30, 90f, 15, 95f, 95f, 83f, 55f, 5, 5, Map1Layout, "maze-1"),
        CreateLevelSettings(DefaultModeSequence, 2, 500, 90f, 79f, 85f, 45f, 40, 90f, 20, 95f, 95f, 83f, 55f, 4, 5, Map2Layout, "maze-2"),
        CreateLevelSettings(DefaultModeSequence, 3, 500, 90f, 79f, 85f, 45f, 40, 90f, 20, 95f, 95f, 83f, 55f, 3, 5, Map2Layout, "maze-2"),
        CreateLevelSettings(DefaultModeSequence, 4, 700, 100f, 87f, 95f, 50f, 40, 100f, 20, 105f, 100f, 87f, 60f, 2, 5, Map2Layout, "maze-2"),
        CreateLevelSettings(DefaultModeSequence, 5, 700, 100f, 87f, 95f, 50f, 50, 100f, 25, 105f, 100f, 87f, 60f, 5, 5, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 6, 1000, 100f, 87f, 95f, 50f, 50, 100f, 25, 105f, 100f, 87f, 60f, 2, 5, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 7, 1000, 100f, 87f, 95f, 50f, 50, 100f, 25, 105f, 100f, 87f, 60f, 2, 5, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 0, 2000, 100f, 87f, 95f, 50f, 60, 100f, 30, 105f, 100f, 87f, 60f, 1, 3, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 1, 2000, 100f, 87f, 95f, 50f, 60, 100f, 30, 105f, 100f, 87f, 60f, 5, 5, Map4Layout, "maze-4"),
        CreateLevelSettings(DefaultModeSequence, 2, 2000, 100f, 87f, 95f, 50f, 60, 100f, 30, 105f, 100f, 87f, 60f, 2, 5, Map4Layout, "maze-4"),
        CreateLevelSettings(DefaultModeSequence, 3, 2000, 100f, 87f, 95f, 50f, 80, 100f, 40, 105f, 100f, 87f, 60f, 1, 3, Map4Layout, "maze-4"),
        CreateLevelSettings(DefaultModeSequence, 4, 5000, 100f, 87f, 95f, 50f, 80, 100f, 40, 105f, 100f, 87f, 60f, 1, 3, Map4Layout, "maze-4"),
        CreateLevelSettings(DefaultModeSequence, 5, 5000, 100f, 87f, 95f, 50f, 80, 100f, 40, 105f, 100f, 87f, 60f, 3, 5, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 6, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 100f, 87f, 60f, 1, 3, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 100f, 87f, 60f, 1, 3, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 0f, 0f, 0f, 0, 0, Map3Layout, "maze-3"),
        CreateLevelSettings(DefaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 100f, 87f, 60f, 1, 3, Map4Layout, "maze-4"),
        CreateLevelSettings(DefaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 120, 100f, 60, 105f, 0f, 0f, 0f, 0, 0, Map4Layout, "maze-4"),
        CreateLevelSettings(DefaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 120, 100f, 60, 105f, 0f, 0f, 0f, 0, 0, Map4Layout, "maze-4"),
        CreateLevelSettings(DefaultModeSequence, 7, 5000, 90f, 79f, 95f, 50f, 120, 100f, 60, 105f, 0f, 0f, 0f, 0, 0, Map4Layout, "maze-4"),
    };

    private static LevelSettings CreateLevelSettings(
        IReadOnlyList<ModeTiming> modeSequence,
        int bonusIndex,
        int bonusScore,
        float pacmanSpeed,
        float pacmanDotSpeed,
        float ghostSpeed,
        float ghostTunnelSpeed,
        int cruiseElroyDots1,
        float cruiseElroySpeed1,
        int cruiseElroyDots2,
        float cruiseElroySpeed2,
        float pacmanFrightenedSpeed,
        float pacmanFrightenedDotSpeed,
        float ghostFrightenedSpeed,
        double frightenedTimeSeconds,
        int frightenedFlashes,
        IReadOnlyList<string> map,
        string mazeMemberName,
        int defaultLives = 3)
    {
        var game = new GameSettings(modeSequence, bonusIndex, bonusScore, map, mazeMemberName, defaultLives);
        var pacman = new PacmanSettings(pacmanSpeed, pacmanDotSpeed, pacmanFrightenedSpeed, pacmanFrightenedDotSpeed);
        var ghost = new GhostSettings(
            ghostSpeed,
            ghostTunnelSpeed,
            new CruiseElroySettings(cruiseElroyDots1, cruiseElroySpeed1, cruiseElroyDots2, cruiseElroySpeed2),
            ghostFrightenedSpeed,
            TimeSpan.FromSeconds(frightenedTimeSeconds),
            frightenedFlashes);
        return new LevelSettings(game, pacman, ghost);
    }
}

/// <summary>
/// Represents the duration of a ghost mode cycle.
/// </summary>
public readonly record struct ModeTiming(GhostMode Mode, TimeSpan Duration);

/// <summary>
/// Level-specific configuration for overall game behaviour.
/// </summary>
public sealed record GameSettings(
    IReadOnlyList<ModeTiming> ModeSequence,
    int BonusIndex,
    int BonusScore,
    IReadOnlyList<string> MapLayout,
    string MazeMemberName,
    int DefaultLives);

/// <summary>
/// Level-specific configuration for Pac-Man's speed profile.
/// </summary>
public sealed record PacmanSettings(
    float Speed,
    float DotSpeed,
    float FrightenedSpeed,
    float FrightenedDotSpeed);

/// <summary>
/// Cruise Elroy thresholds when ghosts speed up as pellets disappear.
/// </summary>
public sealed record CruiseElroySettings(
    int DotsThreshold1,
    float Speed1,
    int DotsThreshold2,
    float Speed2);

/// <summary>
/// Ghost-related level settings.
/// </summary>
public sealed record GhostSettings(
    float Speed,
    float TunnelSpeed,
    CruiseElroySettings CruiseElroy,
    float FrightenedSpeed,
    TimeSpan FrightenedDuration,
    int FrightenedFlashes);

/// <summary>
/// Bundles the settings for a particular level.
/// </summary>
public sealed record LevelSettings(
    GameSettings Game,
    PacmanSettings Pacman,
    GhostSettings Ghost);
