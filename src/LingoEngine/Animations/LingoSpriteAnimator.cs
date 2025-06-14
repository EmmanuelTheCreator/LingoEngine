using LingoEngine.Primitives;
using LingoEngine.Movies;
using LingoEngine.Events;

namespace LingoEngine.Animations
{
    public class LingoSpriteAnimator : IHasEnterFrameEvent
    {
        public LingoTween<LingoPoint> Position { get; } = new();
        public LingoTween<float> Rotation { get; } = new();
        public LingoTween<float> Skew { get; } = new();

        private readonly LingoSprite _sprite;
        private readonly ILingoMovie _movie;
        private readonly ILingoEventMediator _mediator;

        public LingoSpriteAnimator(LingoSprite sprite, ILingoMovieEnvironment env)
        {
            _sprite = sprite;
            _movie = env.Movie;
            _mediator = env.Events;
            _mediator.Subscribe(this);
        }

        public void AddKeyFrame(int frame, float x, float y, float rotation, float skew)
        {
            Position.AddKeyFrame(frame, new LingoPoint(x, y));
            Rotation.AddKeyFrame(frame, rotation);
            Skew.AddKeyFrame(frame, skew);
        }

        private void Apply(int frame)
        {
            var p = Position.GetValue(frame);
            _sprite.Loc = p;
            _sprite.Rotation = Rotation.GetValue(frame);
            _sprite.Skew = Skew.GetValue(frame);
        }

        public void EnterFrame()
        {
            Apply(_movie.CurrentFrame);
        }
    }
}
