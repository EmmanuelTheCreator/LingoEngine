using LingoEngine.Core;
using LingoEngine.Primitives;

namespace LingoEngine.Movies
{
    public abstract class LingoSpriteBehavior : LingoScriptBase
    {
        protected LingoSprite Me;
        public LingoSprite GetSprite() => Me;

        /// <summary>
        /// Properties configured by the user via the property dialog.
        /// </summary>
        public BehaviorPropertiesContainer UserProperties { get; } = new();

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
