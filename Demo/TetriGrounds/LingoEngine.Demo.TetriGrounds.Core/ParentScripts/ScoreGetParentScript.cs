using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Texts;
using System.Collections.Generic;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 17_score_get.ls
    public class ScoreGetParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private List<List<string>>? myScores;
        private int myShowType = 1;

        public ScoreGetParentScript(ILingoMovieEnvironment env) : base(env) { }

        public void SetURL(string scriptURL) => myURL = scriptURL;

        public void DownloadScores()
        {
            myErr = string.Empty;
            myDone = false;
            myScores = null;
            // TODO: implement network download, store handle in myNetID
            myNetID = null;
            _Movie.ActorList.Add(this);
        }

        public void StepFrame()
        {
            // TODO: check network status via myNetID
            // Placeholder implementation just marks request done
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        public void OutputScores()
        {
            if (myScores == null) return;
            if (myScores.Count > 0)
            {
                var scores = myScores[0];
                Member<LingoMemberText>("T_InternetScoresNames").Text = "";
                Member<LingoMemberText>("T_InternetScores").Text = "";
                for (int i = 0; i + 1 < scores.Count; i += 2)
                {
                    Member<LingoMemberText>("T_InternetScoresNames").Text += scores[i] + "\n";
                    Member<LingoMemberText>("T_InternetScores").Text += scores[i + 1] + "\n";
                }
            }
            if (myScores.Count > 1)
            {
                var scores = myScores[1];
                Member<LingoMemberText>("T_InternetScoresNamesP").Text = "";
                Member<LingoMemberText>("T_InternetScoresP").Text = "";
                for (int i = 0; i + 1 < scores.Count; i += 2)
                {
                    Member<LingoMemberText>("T_InternetScoresNamesP").Text += scores[i] + "\n";
                    Member<LingoMemberText>("T_InternetScoresP").Text += scores[i + 1] + "\n";
                }
            }
        }

        public int GetLowestPersonalScore()
        {
            if (myScores == null || myScores.Count < 2 || myScores[1].Count < 10)
                return 0;
            return int.TryParse(myScores[1][myScores[1].Count - 1], out var v) ? v : 0;
        }

        public void SetShowType(int val) => myShowType = val;
        public List<List<string>>? GetHighScoreList() => myScores;
        public string GetErr() => myErr;
        public bool IsDone() => myDone;

        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}
