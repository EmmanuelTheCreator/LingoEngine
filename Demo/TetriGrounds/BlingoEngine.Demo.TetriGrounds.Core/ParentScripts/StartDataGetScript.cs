// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 20_StartData_get.ls
    /// <summary>
    /// Simulates fetching start-level configuration from the original backend.
    /// </summary>
    public class StartDataGetScript : BlingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private string myData = string.Empty;
        private readonly object? myParent;
        private int myShowType = 1;

        public StartDataGetScript(IBlingoMovieEnvironment env, object? parent = null) : base(env)
        {
            myParent = parent;
        }

        /// <summary>
        /// Sets the remote script address.
        /// </summary>
        public void SetURL(string scriptURL) => myURL = scriptURL;

        /// <summary>
        /// Begins downloading the JSON configuration.
        /// </summary>
        public void Download()
        {
            myErr = string.Empty;
            myDone = false;
            myData = string.Empty;
            // TODO: perform network download, store handle in myNetID
            myNetID = null;
            _Movie.ActorList.Add(this);
        }

        /// <summary>
        /// Polls the mock request and notifies the optional parent when finished.
        /// </summary>
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

        /// <summary>
        /// Returns the downloaded raw data.
        /// </summary>
        public string GetData() => myData;
        /// <summary>
        /// Returns the last error message.
        /// </summary>
        public string GetErr() => myErr;
        /// <summary>
        /// Indicates whether the mock download is complete.
        /// </summary>
        public bool IsDone() => myDone;

        /// <summary>
        /// Removes the actor from the movie so it no longer polls.
        /// </summary>
        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}

