// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;

namespace BlingoEngine.Demo.TetriGrounds.Core
{
    /// <summary>
    /// Global runtime state for the TetriGrounds demo. This mirrors the global variables that were
    /// available inside the Director version and acts as a convenient place to pass shared objects around.
    /// </summary>
    public class GlobalVars : BlingoGlobalVars
    {
        /// <summary>
        /// Last informational message displayed to the player.
        /// </summary>
        public string LastInfo { get; internal set; } = string.Empty;

        // Globals created in StarMovieScript
        /// <summary>
        /// Handles sprite creation and pooling for gameplay.
        /// </summary>
        public ParentScripts.SpriteManager? SpriteManager { get; set; }
        /// <summary>
        /// Keeps a single mouse pointer instance alive across scenes.
        /// </summary>
        public ParentScripts.MousePointer? MousePointer { get; set; }
        /// <summary>
        /// Tracks whether the active movie is currently running the main gameplay loop.
        /// </summary>
        public bool GameIsRunning { get; internal set; }
        /// <summary>
        /// Stores the last known player name when entering highscores.
        /// </summary>
        public string PlayerName { get; internal set; } = "";
        /// <summary>
        /// Cached highscore table as read from disk.
        /// </summary>
        public TetrigroundsRootJson.ScoresContent Scores { get; internal set; } = new();

        /// <summary>
        /// Cleans up any objects created for gameplay so the next run starts with a blank slate.
        /// </summary>
        protected override void OnClearGlobals()
        {
            base.OnClearGlobals();
            SpriteManager = null;
            MousePointer = null;
            GameIsRunning = false;
            LastInfo = string.Empty;
            // No need to clear the score cache here because it represents persisted data.
        }
    }


}


