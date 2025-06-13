using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 11_Stop Menu.ls
    public class StopMenuBehavior : LingoSpriteBehavior, IHasExitFrameEvent
    {
        public StopMenuBehavior(ILingoMovieEnvironment env) : base(env){}
        public void ExitFrame()
        {
            _Movie.GoTo(_Movie.CurrentFrame);
        }
    }
}
