using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    public class WaiterFrameScript : LingoSpriteBehavior, IHasExitFrameEvent, ILingoPropertyDescriptionList
    {
        private int i;
        public int TickWait = 10;
        public int FrameOffsetWhenDone { get; set; } = 1;
        public WaiterFrameScript(ILingoMovieEnvironment env) : base(env)
        {
        }
        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList() => new()
            {
                { this, x => x.FrameOffsetWhenDone, "FrameOffset When Done" },
            };
        public string? GetBehaviorDescription() => "Navigate on wait";
        public string? GetBehaviorTooltip() => "Stay on the current frame for a number of ticks before navigating again.";
        public bool IsOKToAttach(LingoSymbol spriteType, int spriteNum) => true;

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
