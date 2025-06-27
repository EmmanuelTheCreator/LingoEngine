using LingoEngine.Inputs;
using LingoEngine.Inputs.Events;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
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
    public class MouseDownNavigateWithStayBehavior : LingoSpriteBehavior, IHasMouseDownEvent, IHasExitFrameEvent
    {
        private int i;
        public int TickWait = 10;
        public int FrameOffsetWhenDone = 1;
        public int FrameOffsetOnClick = 1;
        public MouseDownNavigateWithStayBehavior(ILingoMovieEnvironment env) : base(env)
        {
        }

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
