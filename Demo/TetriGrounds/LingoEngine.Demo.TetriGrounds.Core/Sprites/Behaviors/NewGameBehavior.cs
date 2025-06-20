using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 13_B_NewGame.ls
    public class NewGameBehavior : LingoSpriteBehavior, IHasMouseUpEvent, IHasMouseWithinEvent, IHasMouseLeaveEvent
    {
        public NewGameBehavior(ILingoMovieEnvironment env) : base(env) {}

        public void MouseUp(ILingoMouse mouse)
        {
            Cursor = -1;
            _Movie.GoTo("Game");
        }

        public void MouseWithin(ILingoMouse mouse) => Cursor = 280;
        public void MouseLeave(ILingoMouse mouse) => Cursor = -1;
    }
}
