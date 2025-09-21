// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Events;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    /// <summary>
    /// Navigates to the next frame when the sprite receives a mouse down event.
    /// </summary>
    public class MouseDownNavigateBehavior : BlingoSpriteBehavior, IHasMouseDownEvent
    {
        public int FrameOffsetOnClick = 1;
        public MouseDownNavigateBehavior(IBlingoMovieEnvironment env) : base(env)
        {
        }

        public void MouseDown(BlingoMouseEvent mouse)
        {
            _Movie.GoTo(_Movie.CurrentFrame + FrameOffsetOnClick);
        }
    }
    /// <summary>
    /// Navigates after a delay, optionally staying on the current frame before advancing.
    /// </summary>
    public class MouseDownNavigateWithStayBehavior : BlingoSpriteBehavior, IHasMouseDownEvent, IHasExitFrameEvent, IBlingoPropertyDescriptionList
    {
        private int i;
        public int TickWait = 10;
        public int FrameOffsetWhenDone { get; set; } = 1;
        public int FrameOffsetOnClick { get; set; } = 1;
        public MouseDownNavigateWithStayBehavior(IBlingoMovieEnvironment env) : base(env)
        {
        }
        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList() => new()
            {
                { this, x => x.FrameOffsetWhenDone, "FrameOffset When Done" },
                { this, x => x.FrameOffsetOnClick, "FrameOffset On Click" }
            };
        public string? GetBehaviorDescription() => "Navigate on mousedown";
        public string? GetBehaviorTooltip() => "Navigate to the next frame on mouse down, and stay on the current frame for a number of ticks before navigating again.";
        public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;

        /// <inheritdoc />
        public void MouseDown(BlingoMouseEvent mouse)
        {
            _Movie.GoTo(_Movie.CurrentFrame + FrameOffsetOnClick);
        }
        /// <inheritdoc />
        public void ExitFrame()
        {
            i++;
            if (i > TickWait)
                _Movie.GoTo(_Movie.CurrentFrame + FrameOffsetWhenDone);
            else
                _Movie.GoTo(_Movie.CurrentFrame);
        }
    }
}

