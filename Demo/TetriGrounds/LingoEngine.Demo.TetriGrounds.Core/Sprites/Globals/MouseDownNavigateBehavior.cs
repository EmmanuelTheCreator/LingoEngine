using LingoEngine.Inputs;
using LingoEngine.Inputs.Events;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    public class MouseDownNavigateBehavior : LingoSpriteBehavior, IHasMouseDownEvent
    {
        public int FrameOffsetOnClick = 1;
        public MouseDownNavigateBehavior(ILingoMovieEnvironment env) : base(env)
        {
        }

        public void MouseDown(ILingoMouse mouse)
        {
            _Movie.GoTo(_Movie.CurrentFrame + FrameOffsetOnClick);
        }
    }
    public class MouseDownNavigateWithStayBehavior : LingoSpriteBehavior, IHasMouseDownEvent, IHasExitFrameEvent, ILingoPropertyDescriptionList
    {
        private int i;
        public int TickWait = 10;
        public int FrameOffsetWhenDone { get; set; } = 1;
        public int FrameOffsetOnClick { get; set; } = 1;
        public MouseDownNavigateWithStayBehavior(ILingoMovieEnvironment env) : base(env)
        {
        }
        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList() => new()
            {
                { this, x => x.FrameOffsetWhenDone, "FrameOffset When Done" },
                { this, x => x.FrameOffsetOnClick, "FrameOffset On Click" }
            };
        public string? GetBehaviorDescription() => "Navigate on mpusedown";
        public string? GetBehaviorTooltip() => "Navigate to the next frame on mouse down, and stay on the current frame for a number of ticks before navigating again.";
        public bool IsOKToAttach(LingoSymbol spriteType, int spriteNum) => true;

        public void MouseDown(ILingoMouse mouse)
        {
            _Movie.GoTo(_Movie.CurrentFrame + FrameOffsetOnClick);
        }
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
