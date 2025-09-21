// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    /// <summary>
    /// Forces the movie to remain on the current frame each tick.
    /// </summary>
    internal class StayOnFrameFrameScript : BlingoSpriteBehavior, IHasEnterFrameEvent
    {
        public StayOnFrameFrameScript(IBlingoMovieEnvironment env) : base(env)
        {
        }

        /// <summary>
        /// Jumps back to the current frame, preventing progression.
        /// </summary>
        public void EnterFrame()
        {
            _Movie.GoTo(_Movie.CurrentFrame);
        }
    }
}

