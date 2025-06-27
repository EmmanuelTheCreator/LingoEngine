using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 26_StartData_save.ls
    public class StartDataSaveParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private int phpErr;
        private readonly object? myParent;

        public StartDataSaveParentScript(ILingoMovieEnvironment env, object? parent = null) : base(env)
        {
            myParent = parent;
        }

        public void SetURL(string scriptURL) => myURL = scriptURL;

        public void Post(int myStartLevel, int myStartLines)
        {
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
            _Movie.ActorList.Remove(this);
        }
    }
}
