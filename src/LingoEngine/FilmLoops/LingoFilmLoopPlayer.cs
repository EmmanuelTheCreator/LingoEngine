using LingoEngine.Bitmaps;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.FilmLoops
{
    /// <summary>
    /// Internal helper that plays a <see cref="LingoMemberFilmLoop"/> on a sprite.
    /// It subscribes to sprite events but is not exposed as a behaviour.
    /// </summary>
    public class LingoFilmLoopPlayer : IPlayableActor
    {
        private readonly LingoSprite _sprite;
        private readonly ILingoEventMediator _mediator;
        private int _currentIndex;

        internal LingoFilmLoopPlayer(LingoSprite sprite, ILingoMovieEnvironment env)
        {
            _sprite = sprite;
            _mediator = env.Events;
            _mediator.Subscribe(this);
        }

        private LingoMemberFilmLoop? FilmLoop => _sprite.Member as LingoMemberFilmLoop;

        public void BeginSprite()
        {
            _currentIndex = 0;
            ApplyFrame();
        }

        public void StepFrame()
        {
            var fl = FilmLoop;
            if (fl == null || fl.Frames.Count == 0)
                return;

            _currentIndex++;
            if (_currentIndex >= fl.Frames.Count)
            {
                if (fl.Loop)
                    _currentIndex = 0;
                else
                {
                    _currentIndex = fl.Frames.Count - 1;
                    return;
                }
            }
            ApplyFrame();
        }

        public void EndSprite()
        {
            // nothing to clean up
        }

        private void ApplyFrame()
        {
            var fl = FilmLoop;
            if (fl == null || _currentIndex >= fl.Frames.Count)
                return;
            var frame = fl.Frames[_currentIndex];
            _sprite.SetMember(frame.MemberNum, frame.CastLibNum);
        }
    }
}
