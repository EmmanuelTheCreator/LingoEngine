// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    /// <summary>
    /// Keeps the movie on the current frame for a given number of ticks before advancing.
    /// </summary>
    public class WaiterFrameScript : BlingoSpriteBehavior, IHasExitFrameEvent, IBlingoPropertyDescriptionList
    {
        private int i;
        public int TickWait = 10;
        public int FrameOffsetWhenDone { get; set; } = 1;
        public WaiterFrameScript(IBlingoMovieEnvironment env) : base(env)
        {
        }
        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList() => new()
            {
                { this, x => x.FrameOffsetWhenDone, "FrameOffset When Done" },
            };
        public string? GetBehaviorDescription() => "Navigate on wait";
        public string? GetBehaviorTooltip() => "Stay on the current frame for a number of ticks before navigating again.";
        public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;

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

