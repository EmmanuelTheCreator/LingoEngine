// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 13_B_NewGame.ls
    /// <summary>
    /// Simple behaviour that jumps to the game score label once the intro finishes.
    /// </summary>
    public class NewGameBehavior : BlingoSpriteBehavior, IHasMouseUpEvent, IHasMouseWithinEvent, IHasMouseLeaveEvent
    {
        public NewGameBehavior(IBlingoMovieEnvironment env) : base(env) {}

        /// <summary>
        /// Clicking the sprite navigates to the first frame of the game.
        /// </summary>
        public void MouseUp(BlingoMouseEvent mouse)
        {
            Cursor = -1;
            _Movie.GoTo("Game");
        }

        /// <summary>
        /// Highlights the cursor to indicate the sprite is interactive.
        /// </summary>
        public void MouseWithin(BlingoMouseEvent mouse) => Cursor = 280;
        /// <summary>
        /// Restores the default cursor when the mouse leaves.
        /// </summary>
        public void MouseLeave(BlingoMouseEvent mouse) => Cursor = -1;
    }
}

