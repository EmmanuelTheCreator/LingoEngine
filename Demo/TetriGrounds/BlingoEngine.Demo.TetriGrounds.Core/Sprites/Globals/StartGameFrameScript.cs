// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    /// <summary>
    /// Experimental behaviour kept from the original scripts; loads a background member when starting.
    /// </summary>
    public class StartGameBehavior : BlingoSpriteBehavior, IHasExitFrameEvent, IHasBeginSpriteEvent
    {
        private GlobalVars _global;
        private BlingoMemberBitmap? _memberBg;
        public StartGameBehavior(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        /// <summary>
        /// Resolves the background member used by the behaviour.
        /// </summary>
        public void BeginSprite()
        {
            _memberBg = Member<BlingoMemberBitmap>("MyBG");
        }

        /// <summary>
        /// Placeholder for future functionality.
        /// </summary>
        public void ExitFrame()
        {



        }
    }
}

