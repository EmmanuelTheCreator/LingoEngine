﻿using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    internal class StayOnFrameFrameScript : LingoSpriteBehavior, IHasEnterFrameEvent
    {
        public StayOnFrameFrameScript(ILingoMovieEnvironment env) : base(env)
        {
        }

        public void EnterFrame()
        {
            _Movie.GoTo(_Movie.CurrentFrame);
        }
    }
}
