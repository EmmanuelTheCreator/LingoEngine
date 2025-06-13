using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 14_start.ls
    public class StartBehavior : LingoSpriteBehavior, IHasExitFrameEvent
    {
        public StartBehavior(ILingoMovieEnvironment env) : base(env){}
        public void ExitFrame() => _Movie.GoTo("Game");
    }
}
