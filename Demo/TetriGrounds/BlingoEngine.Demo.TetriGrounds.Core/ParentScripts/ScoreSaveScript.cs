// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using System;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 18_score_save.ls
    /// <summary>
    /// Placeholder for posting scores to the original TetriGrounds PHP backend.
    /// </summary>
    public class ScoreSaveScript : BlingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private int phpErr;
        private readonly ClassSubscribeScript ancestor;

        public ScoreSaveScript(IBlingoMovieEnvironment env) : base(env)
        {
            ancestor = new ClassSubscribeScript(env);
        }

        /// <summary>
        /// Configures the remote endpoint that receives the POSTed score data.
        /// </summary>
        public void SetURL(string scriptURL) => myURL = scriptURL;

        /// <summary>
        /// Sends a score to the remote service. Currently unimplemented in the port.
        /// </summary>
        public void PostScore(string name, int score)
        {

        }

        /// <summary>
        /// Polls the mock network handle to complete the upload.
        /// </summary>
        public void StepFrame()
        {
            // TODO: check network status via myNetID
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        /// <summary>
        /// Returns the last known error message.
        /// </summary>
        public string GetErr() => myErr;
        /// <summary>
        /// Returns the numeric error code that the PHP backend would provide.
        /// </summary>
        public int GetPhpErr() => phpErr;
        /// <summary>
        /// Indicates whether the upload finished.
        /// </summary>
        public bool IsDone() => myDone;

        /// <summary>
        /// Removes subscribers and stops polling the movie timeline.
        /// </summary>
        public void Destroy()
        {
            ancestor.SubscribersDestroy();
            _Movie.ActorList.Remove(this);
        }

        /// <summary>
        /// Legacy encryption helper from the Lingo project. Still unimplemented in the port.
        /// </summary>
        public int Encryptke(string name, int score)
        {

            return 0;
        }
    }
}

