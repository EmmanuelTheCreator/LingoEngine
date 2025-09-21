// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Texts;
using System.Collections.Generic;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 17_score_get.ls
    /// <summary>
    /// Placeholder implementation for downloading high scores from the legacy server.
    /// </summary>
    public class ScoreGetScript : BlingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private List<List<string>>? myScores;
        private int myShowType = 1;

        public ScoreGetScript(IBlingoMovieEnvironment env) : base(env) { }

        /// <summary>
        /// Specifies the remote endpoint used to retrieve the leaderboard.
        /// </summary>
        public void SetURL(string scriptURL) => myURL = scriptURL;

        /// <summary>
        /// Starts an asynchronous download of the leaderboard.
        /// </summary>
        public void DownloadScores()
        {
            myErr = string.Empty;
            myDone = false;
            myScores = null;
            // TODO: implement network download, store handle in myNetID
            myNetID = null;
            _Movie.ActorList.Add(this);
        }

        /// <summary>
        /// Polls the mocked network handle to know when the download completes.
        /// </summary>
        public void StepFrame()
        {
            // TODO: check network status via myNetID
            // Placeholder implementation just marks request done
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        /// <summary>
        /// Writes the retrieved scores into the corresponding text members.
        /// </summary>
        public void OutputScores()
        {
            if (myScores == null) return;
            if (myScores.Count > 0)
            {
                var scores = myScores[0];
                Member<BlingoMemberText>("T_InternetScoresNames")!.Text = "";
                Member<BlingoMemberText>("T_InternetScores")!.Text = "";
                for (int i = 0; i + 1 < scores.Count; i += 2)
                {
                    Member<BlingoMemberText>("T_InternetScoresNames")!.Text += scores[i] + "\n";
                    Member<BlingoMemberText>("T_InternetScores")!.Text += scores[i + 1] + "\n";
                }
            }
            if (myScores.Count > 1)
            {
                var scores = myScores[1];
                Member<BlingoMemberText>("T_InternetScoresNamesP")!.Text = "";
                Member<BlingoMemberText>("T_InternetScoresP")!.Text = "";
                for (int i = 0; i + 1 < scores.Count; i += 2)
                {
                    Member<BlingoMemberText>("T_InternetScoresNamesP")!.Text += scores[i] + "\n";
                    Member<BlingoMemberText>("T_InternetScoresP")!.Text += scores[i + 1] + "\n";
                }
            }
        }

        /// <summary>
        /// Returns the lowest score within the personal table, or zero if not available.
        /// </summary>
        public int GetLowestPersonalScore()
        {
            if (myScores == null || myScores.Count < 2 || myScores[1].Count < 10)
                return 0;
            return int.TryParse(myScores[1][myScores[1].Count - 1], out var v) ? v : 0;
        }

        /// <summary>
        /// Selects which leaderboard (global vs personal) should be displayed.
        /// </summary>
        public void SetShowType(int val) => myShowType = val;
        /// <summary>
        /// Returns the cached high score list for further processing.
        /// </summary>
        public List<List<string>>? GetHighScoreList() => myScores;
        /// <summary>
        /// Returns the last encountered error message.
        /// </summary>
        public string GetErr() => myErr;
        /// <summary>
        /// Indicates whether the download operation has completed.
        /// </summary>
        public bool IsDone() => myDone;

        /// <summary>
        /// Stops polling the movie and clears any subscriber state.
        /// </summary>
        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}

