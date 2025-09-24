// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using System;

namespace Blingo.PacMan.Core.Models
{
    /// <summary>
    /// Represents the current bonus progression for the player.
    /// </summary>
    public interface IBonusesModel
    {
        int Level { get; set; }

        event Action<int>? LevelChanged;
    }

    /// <summary>
    /// Default implementation backed by a simple property with change notification.
    /// </summary>
    public sealed class BonusesModel : IBonusesModel
    {
        private const int MaxBonuses = 8;
        private int _level;

        public event Action<int>? LevelChanged;

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
                LevelChanged?.Invoke(_level);
            }
        }
    }
}
