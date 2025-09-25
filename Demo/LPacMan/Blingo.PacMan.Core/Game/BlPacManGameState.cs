using System;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Models;

namespace Blingo.PacMan.Core.Game;

/// <summary>
/// Holds runtime flags and counters that multiple behaviours share while the Pac-Man game runs.
/// </summary>
internal sealed class BlPacManGameState
{
    private static readonly int[] GhostScoreChain = { 200, 400, 800, 1_600 };

    public bool Muted { get; set; }

    public bool Paused { get; set; }

    public bool Win { get; set; }

    public bool GameOver { get; set; }

    public bool PacManEatenPending { get; set; }

    public bool BonusLocked { get; set; }

    public int PauseFrames { get; set; }

    public int StartCountdown { get; set; }

    public int SoundCooldown { get; set; }

    public int RemainingConsumables { get; set; }

    public int GhostChainIndex { get; set; }

    public int BonusAppearCountdown { get; set; }

    public int BonusDestroyCountdown { get; set; }

    public int Score { get; set; }

    public int HighScore { get; set; }

    public int Lives { get; set; }

    public int Level { get; set; }

    public BlPacManPositionContext? PacManPosition { get; set; }

    public bool IsGameplayFrozen => Paused || PauseFrames > 0 || StartCountdown > 0 || Win || GameOver || PacManEatenPending;

    public void Reset()
    {
        Muted = false;
        Paused = false;
        Win = false;
        GameOver = false;
        PacManEatenPending = false;
        BonusLocked = false;
        PauseFrames = 0;
        StartCountdown = 2;
        SoundCooldown = 0;
        RemainingConsumables = 0;
        GhostChainIndex = 0;
        BonusAppearCountdown = 0;
        BonusDestroyCountdown = 0;
        Score = 0;
        HighScore = 0;
        Lives = 0;
        Level = 1;
        PacManPosition = null;
    }

    public void ApplyModelSnapshot(GameModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        Score = model.Score;
        HighScore = model.HighScore;
        Lives = model.Lives;
        Level = model.Level;
    }

    public void ResetForNewLevel(GameSettings settings, GameModel model)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        BonusLocked = false;
        BonusDestroyCountdown = 0;
        BonusAppearCountdown = 500;
        PauseFrames = 80;
        StartCountdown = 2;
        SoundCooldown = 0;
        GhostChainIndex = 0;
        PacManEatenPending = false;
        Win = false;
        GameOver = false;
        RemainingConsumables = 0;
        ApplyModelSnapshot(model);
    }

    public void RegisterConsumableSpawn()
    {
        RemainingConsumables++;
    }

    public void RegisterConsumableEaten()
    {
        if (RemainingConsumables > 0)
        {
            RemainingConsumables--;
        }
    }

    public int RegisterGhostEaten()
    {
        var index = Math.Clamp(GhostChainIndex, 0, GhostScoreChain.Length - 1);
        var score = GhostScoreChain[index];
        if (GhostChainIndex < GhostScoreChain.Length - 1)
        {
            GhostChainIndex++;
        }

        return score;
    }

    public void ResetGhostChain()
    {
        GhostChainIndex = 0;
    }
}
