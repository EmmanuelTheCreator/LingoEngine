using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using System.Collections.Generic;
using System.Linq;

namespace Blingo.PacMan.Core.Game
{
    public enum MrGhost
    {
        Pinky,
        Blinky,
        Inky,
        Sue,
    }
    internal class BlGhostManager
    {
        private static MrGhost[] _ghostNames = new[] { MrGhost.Blinky, MrGhost.Pinky, MrGhost.Inky, MrGhost.Sue };
        private GhostSettings? _ghostSettings;
        private static readonly int[] _ghostScoreChain = { 200, 400, 800, 1_600 };
        private readonly List<BlPacManGhostBehavior> _ghosts = new();
        private readonly struct GhostHouseSetup
        {
            public GhostHouseSetup(bool startsOutside, int releaseDelayFrames)
            {
                StartsOutside = startsOutside;
                ReleaseDelayFrames = releaseDelayFrames;
            }

            public bool StartsOutside { get; }
            public int ReleaseDelayFrames { get; }
        }

        private static readonly IReadOnlyDictionary<MrGhost, GhostHouseSetup> _initialHouseStates =
            new Dictionary<MrGhost, GhostHouseSetup>
            {
                [MrGhost.Blinky] = new GhostHouseSetup(true, 0),
                [MrGhost.Pinky] = new GhostHouseSetup(false, 120),
                [MrGhost.Inky] = new GhostHouseSetup(false, 240),
                [MrGhost.Sue] = new GhostHouseSetup(false, 360),
            };

        internal IReadOnlyList<BlPacManGhostBehavior> Ghosts => _ghosts;
        public static MrGhost[] GhostNames => _ghostNames;
        public int GhostChainIndex { get; private set; }


        public BlGhostManager()
        {
            
        }


        internal void RemoveGhost(BlPacManGhostBehavior ghost) => _ghosts.Remove(ghost);
        internal void AddGhost(BlPacManGhostBehavior ghost)
        {
            if (_ghosts.Contains(ghost))
                return;
            _ghosts.Add(ghost);
            ApplyConfiguration(ghost);
        }
        
        public int RegisterGhostEaten()
        {
            var index = Math.Clamp(GhostChainIndex, 0, _ghostScoreChain.Length - 1);
            var score = _ghostScoreChain[index];
            if (GhostChainIndex < _ghostScoreChain.Length - 1)
                GhostChainIndex++;

            return score;
        }

        public void ResetGhostChain() => GhostChainIndex = 0;

        public void ResetAllForFreshNewGame()
        {
            _ghosts.Clear();
            ResetGhostChain();
        }

        /// <summary>
        /// Applies the current global ghost mode to every active ghost.
        /// </summary>
        public void SetMode(GhostMode? mode)
        {
            if (_ghosts.Count == 0)
                return;

            foreach (var ghost in _ghosts)
                ghost.SetMode(mode);
        }
        public bool HasDeathGhosts()=> _ghosts.Any(g => g.IsDead);
        public bool HasFrightenedGhosts()=> _ghosts.Any(g => g.IsFrightened);

        public IEnumerable<BlPacManGhostBehavior> GetGhostsOnTile(Tile tile) =>
            _ghosts.Where(ghost => ghost.CurrentTile != null && ReferenceEquals(ghost.CurrentTile, tile) && !ghost.IsDead);
       
        public void ResumeAfterPacManEaten()
        {
            foreach (var ghost in _ghosts)
            {
                ghost.ResetForLife();
                ghost.Show();
            }
        }

        /// <summary>
        /// Searches for a registered ghost by name.
        /// </summary>
        public BlPacManGhostBehavior? FindGhost(MrGhost? name)
        {
            if (name == null)
                return null;

            return _ghosts.FirstOrDefault(g => g.GhostName == name);
        }

        internal void OnEatenByGhost()
        {
            foreach (var ghost in _ghosts)
                ghost.OnPacManEaten();
            ResetGhostChain();
        }

        internal void Reset()
        {
            
        }
        internal void MakeLevel(GhostSettings ghostSettings)
        {
            _ghostSettings = ghostSettings;
            foreach (var ghost in _ghosts)
                ApplyConfiguration(ghost);
        }

        internal void SetAllFrightened()
        {
            foreach (var ghost in _ghosts)
                ghost.SetMode(GhostMode.Frightened);
        }

        private void ApplyConfiguration(BlPacManGhostBehavior ghost)
        {
            if (_ghostSettings is null)
                return;

            var state = _initialHouseStates.TryGetValue(ghost.GhostName, out var info)
                ? info
                : new GhostHouseSetup(false, 0);

            ghost.Configure(_ghostSettings, state.StartsOutside, state.ReleaseDelayFrames);
        }


    }
}
