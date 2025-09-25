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
    private const int _maxBonuses = 8;

    public int BonusAppearCountdown { get; private set; }

    public int BonusDestroyCountdown { get; private set; }
    public bool BonusLocked { get; private set; }
    
    
    public BlPacManBonusManager(GlobalVars globals)
    {
        _globals = globals;
    }

    public void Attach(BlPacManRoamingBonusBehavior bonus)
    {
        _bonus = bonus;
        if (_settings is not null)
            _bonus.Configure(_settings);
    }

    public void Detach(BlPacManRoamingBonusBehavior bonus)
    {
        if (ReferenceEquals(_bonus, bonus))
            _bonus = null;
    }

    public void Configure(GameSettings settings)
    {
        _settings = settings;
        if (_bonus is not null)
            _bonus.Configure(settings);
    }
    internal void MakeLevel(GameSettings settings)
    {
        Configure(settings);
        if (_bonus != null)
        {
            _bonus.ResetForLife();
            _bonus.Deactivate();
        }
    }


    public void ResetAfterLifeLost() => _bonus?.ResetForLife();

    public void OnPacManEaten()
    {
        _bonus?.OnPacManEaten();
        BonusLocked = false;
        BonusAppearCountdown = 250;
        BonusDestroyCountdown = 0;
    }

    public void Update(GameModel model)
    {
        if (_bonus is null || _settings is null)
            return;

        var state = _globals.State;
        if (BonusDestroyCountdown > 0)
        {
            BonusDestroyCountdown--;
            if (BonusDestroyCountdown == 0)
                _bonus.Deactivate();

            return;
        }

        if (BonusLocked)
            return;

        if (BonusAppearCountdown > 0)
        {
            BonusAppearCountdown--;
            if (BonusAppearCountdown == 0)
                _bonus.Activate();

            return;
        }

        _bonus.Tick();
    }

    public void HandleCollected()
    {
        if (_bonus is null || _settings is null)
            return;

        var state = _globals.State;
        if (BonusLocked)
            return;

        BonusLocked = true;
        BonusDestroyCountdown = 45;

        if (_settings.BonusScore > 0)
            _globals.GameModel?.AddScore(_settings.BonusScore);

        _bonus.ShowScore();
    }

    public void HandleExpired()
    {
        var state = _globals.State;
        BonusLocked = true;
        _bonus?.Deactivate();
    }
    internal void ResetForNewLevel()
    {
        BonusDestroyCountdown = 0;
        BonusAppearCountdown = 500;
    }
    public void Reset()
    {
        BonusLocked = false;
        _settings = null;
        _bonus = null;
        BonusAppearCountdown = 0;
        BonusDestroyCountdown = 0;
    }


    /// <summary>
    /// Handles Pac-Man collecting the roaming bonus by awarding score and scheduling its removal.
    /// </summary>
    public void NotifyBonusEaten(BlPacManRoamingBonusBehavior bonus)
    {
        if (bonus is null)
            throw new ArgumentNullException(nameof(bonus));

        HandleCollected();
    }

    /// <summary>
    /// Locks the bonus when it leaves the maze without being eaten.
    /// </summary>
    public void NotifyBonusExpired(BlPacManRoamingBonusBehavior bonus)
    {
        if (_bonus == bonus)
            HandleExpired();
    }

    internal void CollectOnTile(Tile tile)
    {
        if (_bonus is not null && _bonus.IsActive && ReferenceEquals(_bonus.CurrentTile, tile))
            _bonus.Collect();
    }

   
}
