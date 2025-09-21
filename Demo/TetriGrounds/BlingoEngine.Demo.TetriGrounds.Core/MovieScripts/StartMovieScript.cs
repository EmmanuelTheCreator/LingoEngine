// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;

namespace BlingoEngine.Demo.TetriGrounds.Core.MovieScripts
{
    // Converted from 4_StarMovie.ls
    /// <summary>
    /// Handles start/stop events for the root movie. This mirrors the Lingo "StartMovie" behaviour.
    /// </summary>
    public class StartMovieScript : BlingoMovieScript, IHasStartMovieEvent, IHasStopMovieEvent
    {
        private readonly GlobalVars _global;

        /// <summary>
        /// Stores the <see cref="GlobalVars"/> reference used by the various parent scripts.
        /// </summary>
        public StartMovieScript(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        /// <summary>
        /// Ensures long-lived helpers exist and fades in the looping ambient soundtrack.
        /// </summary>
        public void StartMovie()
        {
            if (_global.SpriteManager == null)
            {
                _global.SpriteManager = new SpriteManager(_env);
                _global.SpriteManager.Init(100);
            }
            if (_global.MousePointer == null)
            {
                _global.MousePointer = new MousePointer(_env);
                _global.MousePointer.Init(90);
            }
            _Player.SoundPlayNature();
        }

        /// <summary>
        /// Reverts the movie to the pre-start state so the user can replay it without restarting the engine.
        /// </summary>
        public void StopMovie()
        {
            _global.SpriteManager?.Destroy();
            _global.MousePointer?.Destroy();
            _global.SpriteManager = null;
            _global.MousePointer = null;
            _Movie.ActorList.Clear();
        }

        /// <summary>
        /// Utility ported from Lingo that sanitises player names before writing them into text members.
        /// </summary>
        public string ReplaceSpaces(string str, int leng)
        {
            string thisField = str;
            for (int i = 0; i < thisField.Length; i++)
            {
                if (thisField[i] == ' ')
                    thisField = thisField.Substring(0, i) + "_" + thisField[(i + 1)..];
            }
            if (thisField.Length > leng) thisField = thisField.Substring(0, leng);
            return thisField;
        }
    }
}

