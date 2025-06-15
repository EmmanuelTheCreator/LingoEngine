using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 20_StartData_get.ls
    public class StartDataGetParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private string myData = string.Empty;
        private readonly object? myParent;
        private int myShowType = 1;

        public StartDataGetParentScript(ILingoMovieEnvironment env, object? parent = null) : base(env)
        {
            myParent = parent;
        }

        public void SetURL(string scriptURL) => myURL = scriptURL;

        public void Download()
        {
            myErr = string.Empty;
            myDone = false;
            myData = string.Empty;
            // TODO: perform network download, store handle in myNetID
            myNetID = null;
            _Movie.ActorList.Add(this);
        }

        public void StepFrame()
        {
            // TODO: check network status via myNetID
            if (myParent != null)
            {
                var mi = myParent.GetType().GetMethod("DataLoaded");
                mi?.Invoke(myParent, new object[] { myData, this });
            }
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        public string GetData() => myData;
        public string GetErr() => myErr;
        public bool IsDone() => myDone;

        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}
