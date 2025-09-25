using System;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;

/// <summary>
/// Centralises the roaming bonus lifecycle so behaviours can trigger actions through the globals table.
/// </summary>
internal sealed class BlPacManBonusManager
{
    private readonly GlobalVars _globals;
    private GameSettings? _settings;
    private BlPacManRoamingBonusBehavior? _bonus;

    public BlPacManBonusManager(GlobalVars globals)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void Attach(BlPacManRoamingBonusBehavior bonus)
    {
        _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
        if (_settings is not null)
        {
            _bonus.Configure(_settings);
        }
    }

    public void Detach(BlPacManRoamingBonusBehavior bonus)
    {
        if (ReferenceEquals(_bonus, bonus))
        {
            _bonus = null;
        }
    }

    public void Configure(GameSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        if (_bonus is not null)
        {
            _bonus.Configure(settings);
        }
    }

    public void ResetForLevel()
    {
        _bonus?.ResetForLife();
        _bonus?.Deactivate();
    }

    public void ResetAfterLifeLost()
    {
        _bonus?.ResetForLife();
    }

    public void OnPacManEaten()
    {
        _bonus?.OnPacManEaten();
    }

    public void Update(GameModel model)
    {
        if (_bonus is null || _settings is null)
        {
            return;
        }

        var state = _globals.State;
        if (state.BonusDestroyCountdown > 0)
        {
            state.BonusDestroyCountdown--;
            if (state.BonusDestroyCountdown == 0)
            {
                _bonus.Deactivate();
            }

            return;
        }

        if (state.BonusLocked)
        {
            return;
        }

        if (state.BonusAppearCountdown > 0)
        {
            state.BonusAppearCountdown--;
            if (state.BonusAppearCountdown == 0)
            {
                _bonus.Activate();
            }

            return;
        }

        _bonus.Tick();
    }

    public void HandleCollected(GameModel model)
    {
        if (_bonus is null || _settings is null)
        {
            return;
        }

        var state = _globals.State;
        if (state.BonusLocked)
        {
            return;
        }

        state.BonusLocked = true;
        state.BonusDestroyCountdown = 45;

        if (_settings.BonusScore > 0)
        {
            model.AddScore(_settings.BonusScore);
            state.Score = model.Score;
            state.HighScore = model.HighScore;
        }

        _bonus.ShowScore();
    }

    public void HandleExpired()
    {
        var state = _globals.State;
        state.BonusLocked = true;
        _bonus?.Deactivate();
    }

    public void Reset()
    {
        _settings = null;
        _bonus = null;
    }
}
