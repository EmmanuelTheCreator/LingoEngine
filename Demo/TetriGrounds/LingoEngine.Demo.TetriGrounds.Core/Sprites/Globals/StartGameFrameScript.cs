using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    public class StartGameBehavior : LingoSpriteBehavior, IHasExitFrameEvent, IHasBeginSpriteEvent
    {
        private GlobalVars _global;
        private LingoMemberPicture? _memberBg;
        public StartGameBehavior(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        public void BeginSprite()
        {
            _memberBg = Member<LingoMemberPicture>("MyBG");
        }

        public void ExitFrame()
        {



        }
    }
}
