// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

namespace BlingoEngine.Demo.TetriGrounds.Core
{
    /// <summary>
    /// Serializable root document describing stored high score information for TetriGrounds.
    /// </summary>
    public class TetrigroundsRootJson
    {
        /// <summary>
        /// Collection of high score entries grouped with metadata.
        /// </summary>
        public ScoresContent ScoresData { get; set; } = new();
        /// <summary>
        /// Version flag persisted alongside the JSON file so migrations can occur in the future.
        /// </summary>
        public string Version { get; set; } = "1.0";
        /// <summary>
        /// Represents a single leaderboard entry.
        /// </summary>
        public record ScoreEntry(string PlayerName, int Score, DateTime when, TimeSpan duration, int Level);
        /// <summary>
        /// Scores payload as it appears on disk.
        /// </summary>
        public record ScoresContent
        {
            public DateTime LastUpdated { get; set; }
            public List<ScoreEntry> Scores { get; set; } = [];
        }
    }


}


