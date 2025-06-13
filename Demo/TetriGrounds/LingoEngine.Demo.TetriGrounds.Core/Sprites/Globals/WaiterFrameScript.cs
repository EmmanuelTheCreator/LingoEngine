using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    public class WaiterFrameScript : LingoSpriteBehavior, IHasExitFrameEvent
    {
        private int i;
        public int TickWait = 10;
        public int FrameOffsetWhenDone = 1;
        public WaiterFrameScript(ILingoMovieEnvironment env) : base(env)
        {
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
