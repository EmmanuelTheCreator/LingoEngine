using LingoEngine.Demo.TetriGrounds.Core;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;


namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{

    public partial class PauseBehaviour : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent
    {
        private readonly GlobalVars global;

        public PauseBehaviour(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            this.global = global;
        }

        public void BeginSprite()
        {
            Me.Visibility = false;
            //global.P = 0;
        }


        public void ExitFrame()
        {
            //if ((Input.IsKeyPressed(Key.P) || Input.IsKeyPressed(Key.P)) && global.P == 0)
            //{
            //    global.P = 1;
            //    Me.Visibility = true;
            //}
            //else if (Input.IsKeyPressed(Key.Space) && global.P == 1)
            //{
            //    global.P = 0;
            //    Me.Visibility = false;
            //}
        }


    }



}
