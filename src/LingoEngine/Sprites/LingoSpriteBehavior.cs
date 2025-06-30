using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngine.Sprites
{
    public abstract class LingoSpriteBehavior : LingoScriptBase
    {
        protected LingoSprite Me;
        public LingoSprite GetSprite() => Me;

        /// <summary>
        /// Properties configured by the user via the property dialog.
        /// </summary>
        public BehaviorPropertiesContainer UserProperties { get; } = new();
        public string Name { get; set; }

#pragma warning disable CS8618
        public LingoSpriteBehavior(ILingoMovieEnvironment env) : base(env)
#pragma warning restore CS8618
        {
            Name = GetType().Name;
        }

        internal void SetMe(LingoSprite sprite)
        {
            Me = sprite;
        }

    }
}
