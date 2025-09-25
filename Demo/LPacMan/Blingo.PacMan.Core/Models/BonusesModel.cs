using System;
using Blingo.PacMan.Core.Game;

namespace Blingo.PacMan.Core.Models
{
    /// <summary>
    /// Tracks the current bonus progression for the player.
    /// </summary>
    public sealed class BonusesModel
    {
        private const int MaxBonuses = 8;
        private int _level;
        private readonly PacManEventMediator<int> _levelChanged = new();

        public int Level
        {
            get => _level;
            set
            {
                var clampedLevel = Math.Clamp(value, 0, MaxBonuses);
                if (_level == clampedLevel)
                {
                    return;
                }

                _level = clampedLevel;
                _levelChanged.Publish(_level);
            }
        }

        public PacManEventSubscription SubscribeLevelChanged(Action<int> handler) => _levelChanged.Subscribe(handler);
    }
}
