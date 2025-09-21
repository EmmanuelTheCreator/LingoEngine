// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Demo.TetriGrounds.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;


namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{

    /// <summary>
    /// Legacy pause overlay behaviour kept for completeness of the original scripts.
    /// </summary>
    public partial class PauseBehaviour : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent
    {
        private readonly GlobalVars global;

        public PauseBehaviour(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            this.global = global;
        }

        /// <summary>
        /// Hides the pause sprite when the behaviour starts.
        /// </summary>
        public void BeginSprite()
        {
            Me.Visibility = false;
            //global.P = 0;
        }


        /// <summary>
        /// Placeholder for the original pause hotkey logic.
        /// </summary>
        public void ExitFrame()
        {
            //if ((Input.IsKeyPressed(Key.P) || Input.IsKeyPressed(Key.P)) && global.P == 0)
            //{
            //    global.P = 1;
            //    Me.Visibility = true;
            //}
            //else if (Input.IsKeyPressed(Key.Space) && global.P == 1)
            //{
            //    global.P = 0;
            //    Me.Visibility = false;
            //}
        }


    }



}

