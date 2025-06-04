using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine
{
    public abstract class LingoSpriteBehavior : LingoScriptBase
    {
        protected LingoSprite Me;
        public LingoSprite Sprite => Me;
#pragma warning disable CS8618 
        public LingoSpriteBehavior(ILingoMovieEnvironment env) : base(env)
#pragma warning restore CS8618 
        {
        }

        internal void SetMe(LingoSprite sprite)
        {
            Me = sprite;
        }

    }

    public class ExampleBehavior : LingoSpriteBehavior, IHasEnterFrameEvent
    {
        public ExampleBehavior(ILingoMovieEnvironment env) : base(env)
        {
        }

        public void EnterFrame()
        {
            var num = Me.SpriteNum;
            // ...
        }
    }
}
