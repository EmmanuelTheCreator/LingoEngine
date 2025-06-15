using LingoEngine.Events;
using LingoEngine.Movies;
using System;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 18_score_save.ls
    public class ScoreSaveParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private int phpErr;
        private readonly ClassSubscibeParentScript ancestor;

        public ScoreSaveParentScript(ILingoMovieEnvironment env) : base(env)
        {
            ancestor = new ClassSubscibeParentScript(env);
        }

        public void SetURL(string scriptURL) => myURL = scriptURL;

        public void PostScore(string name, int score)
        {
            int encrypt = Encryptke(name, score);
            int encrypt2 = 123456 + Random.Shared.Next(123456);
            myDone = false;
            myErr = string.Empty;
            // TODO: perform network post, store handle in myNetID
            myNetID = null;
            _Movie.ActorList.Add(this);
        }

        public void StepFrame()
        {
            // TODO: check network status via myNetID
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        public string GetErr() => myErr;
        public int GetPhpErr() => phpErr;
        public bool IsDone() => myDone;

        public void Destroy()
        {
            ancestor.Subscriberdestroy();
            _Movie.ActorList.Remove(this);
        }

        public int Encryptke(string name, int score)
        {
            int total = 0;
            foreach (char c in name)
                total += c;
            foreach (char c in score.ToString())
                total += c;
            total *= 12;
            int shiftreg = total;
            for (int i = 0; i < 3; i++)
            {
                int flag1 = (shiftreg & 1) != 0 ? 1 : 0;
                int flag2 = (shiftreg & 2) != 0 ? 1 : 0;
                int flag3 = (shiftreg & 3) != 0 ? 1 : 0;
                int flag4 = (shiftreg & 5) != 0 ? 1 : 0;
                int flag5 = (shiftreg & 7) != 0 ? 1 : 0;
                int shiftreg2 = shiftreg * 2;
                shiftreg2 = (flag1 ^ flag2 ^ flag3 ^ flag4 ^ flag5) | shiftreg2;
                shiftreg2 &= 0x7FFFFFFF;
                shiftreg = shiftreg2;
            }
            return shiftreg;
        }
    }
}
