using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game
{
    public enum MrGhost
    {
        Blinky,
        Pinky,
        Inky,
        Clyde,
    }
    internal class BlGhostManager
    {
        private static MrGhost[] _ghostNames = new[] { MrGhost.Blinky, MrGhost.Pinky, MrGhost.Inky, MrGhost.Clyde };
        private static readonly int[] _ghostScoreChain = { 200, 400, 800, 1_600 };
        private readonly List<BlPacManGhostBehavior> _ghosts = new();

        internal IReadOnlyList<BlPacManGhostBehavior> Ghosts => _ghosts;
        public static MrGhost[] GhostNames => _ghostNames;
        public int GhostChainIndex { get; private set; }


        internal void RemoveGhost(BlPacManGhostBehavior ghost) => _ghosts.Remove(ghost);
        internal void AddGhost(BlPacManGhostBehavior ghost)
        {
            if (_ghosts.Contains(ghost))
                return;
            _ghosts.Add(ghost);
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

        public void Reset()
        {
            _ghosts.Clear();
            ResetGhostChain();
        }

        /// <summary>
        /// Applies the current global ghost mode to every active ghost.
        /// </summary>
        public void SetMode(GhostMode? mode)
        {
            if (Ghosts.Count == 0)
                return;

            foreach (var ghost in Ghosts)
                ghost.SetMode(mode);
        }
        public bool HasDeathGhosts()=> Ghosts.Any(g => g.IsDead);
        public bool HasFrightenedGhosts()=> Ghosts.Any(g => g.IsFrightened);

        public IEnumerable<BlPacManGhostBehavior> GetGhostsOnTile(Tile tile) => 
            Ghosts.Where(ghost => ghost.CurrentTile != null && ReferenceEquals(ghost.CurrentTile, tile) && !ghost.IsDead);
       
        public void ResumeAfterPacManEaten()
        {
            foreach (var ghost in Ghosts)
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

            return Ghosts.FirstOrDefault(g => g.GhostName == name);
        }

        internal void OnEatenByGhost()
        {
            foreach (var ghost in Ghosts)
                ghost.OnPacManEaten();
            ResetGhostChain();
        }

        internal void MakeLevel(GhostSettings ghostSettings)
        {
            foreach (var ghost in Ghosts)
                ghost.Configure(ghostSettings);
        }

        internal void SetAllFrightened()
        {
            foreach (var ghost in Ghosts)
                ghost.SetMode(GhostMode.Frightened);
        }
    }
}
