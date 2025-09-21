// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 26_StartData_save.ls
    /// <summary>
    /// Simulates saving start-level configuration back to the server.
    /// </summary>
    public class StartDataSaveScript : BlingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private int phpErr;
        private readonly object? myParent;

        public StartDataSaveScript(IBlingoMovieEnvironment env, object? parent = null) : base(env)
        {
            myParent = parent;
        }

        /// <summary>
        /// Sets the endpoint to which configuration is sent.
        /// </summary>
        public void SetURL(string scriptURL) => myURL = scriptURL;

        /// <summary>
        /// Posts the desired level and line configuration. Not yet implemented in the port.
        /// </summary>
        public void Post(int myStartLevel, int myStartLines)
        {
            myDone = false;
            myErr = string.Empty;
            // TODO: perform network post, store handle in myNetID
            myNetID = null;
            _Movie.ActorList.Add(this);
        }

        /// <summary>
        /// Polls the mock request handle to complete the upload.
        /// </summary>
        public void StepFrame()
        {
            // TODO: check network status via myNetID
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        /// <summary>
        /// Returns the last error message from the simulated upload.
        /// </summary>
        public string GetErr() => myErr;
        /// <summary>
        /// Returns the PHP error code returned by the backend.
        /// </summary>
        public int GetPhpErr() => phpErr;
        /// <summary>
        /// Indicates whether the operation is complete.
        /// </summary>
        public bool IsDone() => myDone;

        /// <summary>
        /// Stops polling the movie timeline for updates.
        /// </summary>
        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}

