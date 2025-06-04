using LingoEngine.Core;

namespace LingoEngine.Movies
{
    public abstract class LingoSpriteBehavior : LingoScriptBase
    {
        protected LingoSprite Me;
        public new LingoSprite Sprite => Me;
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
}
