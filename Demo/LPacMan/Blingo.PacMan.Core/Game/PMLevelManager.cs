using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Settings;
using System.Collections.ObjectModel;
using MapContent = Blingo.PacMan.Core.Datas.Maps;


namespace Blingo.PacMan.Core.Game
{
    internal class PMLevelManager
    {
        private int _level = 1;
        private readonly BlPacManEventMediator<int> _levelChanged = new();
        public BlPacManEventSubscription SubscribeLevelChanged(Action<int> handler) => _levelChanged.Subscribe(handler);
        private PMMap _map = new PMMap(_map1Layout);
        /// <summary>
        /// Gets the map currently being played.
        /// </summary>
        public PMMap Map => _map;

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
                //ResetModeTimer();
                _levelChanged.Publish(_level);
            }
        }

        public void Reset() => Level = 1;


        public GameSettings MakeLevel()
        {
            GameSettings settings = GetGameSettings();
            _map = new PMMap(settings.MapLayout);
            return settings;
        }


        #region Settings
        public GameSettings GetGameSettings() => GetLevelSettings().Game;

        public PacmanSettings GetPacmanSettings() => GetLevelSettings().Pacman;

        public GhostSettings GetGhostSettings() => GetLevelSettings().Ghost;

        
        private LevelSettings GetLevelSettings()
        {
            var index = Math.Clamp(_level - 1, 0, _levelTable.Length - 1);
            return _levelTable[index];
        }

        #endregion

        private static readonly IReadOnlyList<ModeTiming> _defaultModeSequence = new ReadOnlyCollection<ModeTiming>(new[]
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

        private static readonly IReadOnlyList<string> _map1Layout = Array.AsReadOnly(MapContent.Map1);
        private static readonly IReadOnlyList<string> _map2Layout = Array.AsReadOnly(MapContent.Map2);
        private static readonly IReadOnlyList<string> _map3Layout = Array.AsReadOnly(MapContent.Map3);
        private static readonly IReadOnlyList<string> _map4Layout = Array.AsReadOnly(MapContent.Map4);

        private static readonly LevelSettings[] _levelTable =
        {
        CreateLevelSettings(_defaultModeSequence, 0, 100, 80f, 71f, 75f, 40f, 20, 80f, 10, 85f, 90f, 79f, 50f, 6, 5, _map1Layout, "maze-1"),
        CreateLevelSettings(_defaultModeSequence, 1, 200, 90f, 79f, 85f, 45f, 30, 90f, 15, 95f, 95f, 83f, 55f, 5, 5, _map1Layout, "maze-1"),
        CreateLevelSettings(_defaultModeSequence, 2, 500, 90f, 79f, 85f, 45f, 40, 90f, 20, 95f, 95f, 83f, 55f, 4, 5, _map2Layout, "maze-2"),
        CreateLevelSettings(_defaultModeSequence, 3, 500, 90f, 79f, 85f, 45f, 40, 90f, 20, 95f, 95f, 83f, 55f, 3, 5, _map2Layout, "maze-2"),
        CreateLevelSettings(_defaultModeSequence, 4, 700, 100f, 87f, 95f, 50f, 40, 100f, 20, 105f, 100f, 87f, 60f, 2, 5, _map2Layout, "maze-2"),
        CreateLevelSettings(_defaultModeSequence, 5, 700, 100f, 87f, 95f, 50f, 50, 100f, 25, 105f, 100f, 87f, 60f, 5, 5, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 6, 1000, 100f, 87f, 95f, 50f, 50, 100f, 25, 105f, 100f, 87f, 60f, 2, 5, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 7, 1000, 100f, 87f, 95f, 50f, 50, 100f, 25, 105f, 100f, 87f, 60f, 2, 5, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 0, 2000, 100f, 87f, 95f, 50f, 60, 100f, 30, 105f, 100f, 87f, 60f, 1, 3, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 1, 2000, 100f, 87f, 95f, 50f, 60, 100f, 30, 105f, 100f, 87f, 60f, 5, 5, _map4Layout, "maze-4"),
        CreateLevelSettings(_defaultModeSequence, 2, 2000, 100f, 87f, 95f, 50f, 60, 100f, 30, 105f, 100f, 87f, 60f, 2, 5, _map4Layout, "maze-4"),
        CreateLevelSettings(_defaultModeSequence, 3, 2000, 100f, 87f, 95f, 50f, 80, 100f, 40, 105f, 100f, 87f, 60f, 1, 3, _map4Layout, "maze-4"),
        CreateLevelSettings(_defaultModeSequence, 4, 5000, 100f, 87f, 95f, 50f, 80, 100f, 40, 105f, 100f, 87f, 60f, 1, 3, _map4Layout, "maze-4"),
        CreateLevelSettings(_defaultModeSequence, 5, 5000, 100f, 87f, 95f, 50f, 80, 100f, 40, 105f, 100f, 87f, 60f, 3, 5, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 6, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 100f, 87f, 60f, 1, 3, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 100f, 87f, 60f, 1, 3, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 0f, 0f, 0f, 0, 0, _map3Layout, "maze-3"),
        CreateLevelSettings(_defaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 100, 100f, 50, 105f, 100f, 87f, 60f, 1, 3, _map4Layout, "maze-4"),
        CreateLevelSettings(_defaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 120, 100f, 60, 105f, 0f, 0f, 0f, 0, 0, _map4Layout, "maze-4"),
        CreateLevelSettings(_defaultModeSequence, 7, 5000, 100f, 87f, 95f, 50f, 120, 100f, 60, 105f, 0f, 0f, 0f, 0, 0, _map4Layout, "maze-4"),
        CreateLevelSettings(_defaultModeSequence, 7, 5000, 90f, 79f, 95f, 50f, 120, 100f, 60, 105f, 0f, 0f, 0f, 0, 0, _map4Layout, "maze-4"),
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

        internal void GameWon()
        {
            Level++;
        }

        internal void GameIsOver()
        {
            Level = 1;
        }

        internal void ResetForNewLevel()
        {
            
        }
    }
}
