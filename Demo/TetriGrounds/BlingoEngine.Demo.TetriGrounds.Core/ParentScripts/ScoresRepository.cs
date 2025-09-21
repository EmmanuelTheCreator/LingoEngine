// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using AbstUI.Resources;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Texts;
using System.Net.Http.Json;
using static BlingoEngine.Demo.TetriGrounds.Core.TetrigroundsRootJson;

namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    /// <summary>
    /// Handles reading and writing the local high score store for TetriGrounds.
    /// </summary>
    public class ScoresRepository
    {
        private BlingoMemberText? _scoresNames;
        private BlingoMemberText? _scoresList;
        private readonly GlobalVars _global;
        private readonly IAbstResourceManager _resourceManager;


        public bool IsNewHighScore { get; private set; }
        public int HighestScore { get; private set; }
        public int LowestScore { get; private set; }
        public int MaxScoresToStore { get; private set; } = 100;
        public int MaxScoresVisual { get; private set; } = 15;

        /// <summary>
        /// Keeps references to the global state and resource manager for persistence.
        /// </summary>
        public ScoresRepository(IBlingoMovieEnvironment env, GlobalVars global, IAbstResourceManager resourceManager)
        {
            _global = global;
            _resourceManager = resourceManager;
        }

        /// <summary>
        /// Loads the saved scores from storage and updates the in-memory representation.
        /// </summary>
        public void LoadHighScores(IBlingoMovie movie)
        {
            _scoresNames = movie.GetMember<BlingoMemberText>("T_InternetScoresNames")!;
            _scoresList = movie.GetMember<BlingoMemberText>("T_InternetScores")!;
            _scoresNames.Width = 65;
            _scoresList.Width = 37;
            var jsonContent = _resourceManager.StorageRead<TetrigroundsRootJson>("HighScores");
            if (jsonContent == null || jsonContent.ScoresData == null || jsonContent.ScoresData.Scores == null) return;
            _global.Scores = jsonContent.ScoresData;
            RenderScores(jsonContent.ScoresData);
            HighestScore = _global.Scores.Scores.Select(x => x.Score).Max();
            LowestScore = _global.Scores.Scores.Select(x => x.Score).Min();
        }

        /// <summary>
        /// Writes the top scores into the appropriate text members.
        /// </summary>
        private void RenderScores(ScoresContent scoresContent)
        {
            if (_scoresNames == null || _scoresList == null) return;
            var sbNames = new System.Text.StringBuilder();
            foreach (var score in scoresContent.Scores.Take(MaxScoresVisual))
                sbNames.AppendLine($"{score.PlayerName}");
            var sbScores = new System.Text.StringBuilder();
            foreach (var score in scoresContent.Scores.Take(MaxScoresVisual))
                sbScores.AppendLine($"{score.Score}");
            _scoresNames.Text = sbNames.ToString().TrimEnd();
            _scoresList.Text = sbScores.ToString().TrimEnd();
        }

        /// <summary>
        /// Persists the current high score list to storage.
        /// </summary>
        public void SaveHighScores()
        {
            var jsonContent = new TetrigroundsRootJson
            {
                ScoresData = _global.Scores
            };
            _resourceManager.StorageWrite("HighScores", jsonContent);
        }

        /// <summary>
        /// Adds a new score entry, keeps only the top <see cref="MaxScoresToStore"/> entries and saves the result.
        /// </summary>
        internal void StoreScore(string name, int myPlayerScore, int myLevel, DateTime started, TimeSpan elapsed)
        {
            var myScores = _global.Scores;
            // Add new entry
            myScores.Scores.Add(new ScoreEntry(name, myPlayerScore, started, elapsed, myLevel));
            // Sort: score desc, then oldest first, then name
            myScores.Scores.Sort((a, b) =>
            {
                int c = b.Score.CompareTo(a.Score);       // higher score first
                if (c != 0) return c;
                c = a.when.CompareTo(b.when);             // older first (stable highscore tables)
                if (c != 0) return c;
                return string.Compare(a.PlayerName, b.PlayerName, StringComparison.Ordinal);
            });

            // Keep top MaxScoresToStore (100)
            if (myScores.Scores.Count > MaxScoresToStore)
                myScores.Scores.RemoveRange(MaxScoresToStore, myScores.Scores.Count - MaxScoresToStore);
            SaveHighScores();

            RenderScores(_global.Scores);
        }
        
    }
}

