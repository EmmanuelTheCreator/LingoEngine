using LingoEngine.Primitives;
using LingoEngine.Movies;
using LingoEngine.Events;
using System.Linq;

namespace LingoEngine.Animations
{
    public class LingoSpriteAnimator : IPlayableActor
    {
        public LingoTween<LingoPoint> Position { get; } = new();
        public LingoTween<float> Rotation { get; } = new();
        public LingoTween<float> Skew { get; } = new();
        public LingoTween<LingoColor> ForegroundColor { get; } = new();
        public LingoTween<LingoColor> BackgroundColor { get; } = new();
        public LingoTween<float> Blend { get; } = new();

        private LingoSpriteMotionPath _cachedPath = new();
        private bool _cacheDirty = true;

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

        public void SetTweenOptions(bool positionEnabled, bool rotationEnabled, bool skewEnabled,
            bool foreColorEnabled, bool backColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool smoothSpeed, float easeIn, float easeOut)
        {
            Position.Options.Enabled = positionEnabled;
            Rotation.Options.Enabled = rotationEnabled;
            Skew.Options.Enabled = skewEnabled;
            ForegroundColor.Options.Enabled = foreColorEnabled;
            BackgroundColor.Options.Enabled = backColorEnabled;
            Blend.Options.Enabled = blendEnabled;

            var list = new[] { Position.Options, Rotation.Options, Skew.Options,
                ForegroundColor.Options, BackgroundColor.Options, Blend.Options };
            foreach (var opt in list)
            {
                opt.Curvature = curvature;
                opt.ContinuousAtEndpoints = continuousAtEnds;
                opt.SpeedChange = smoothSpeed ? LingoSpeedChangeType.Smooth : LingoSpeedChangeType.Sharp;
                opt.EaseIn = easeIn;
                opt.EaseOut = easeOut;
            }
        }

        public void AddKeyFrame(int frame, float x, float y, float rotation, float skew,
            LingoColor? foreColor = null, LingoColor? backColor = null, float? blend = null)
        {
            Position.AddKeyFrame(frame, new LingoPoint(x, y));
            Rotation.AddKeyFrame(frame, rotation);
            Skew.AddKeyFrame(frame, skew);
            if (foreColor.HasValue) ForegroundColor.AddKeyFrame(frame, foreColor.Value);
            if (backColor.HasValue) BackgroundColor.AddKeyFrame(frame, backColor.Value);
            if (blend.HasValue) Blend.AddKeyFrame(frame, blend.Value);
            _cacheDirty = true;
        }

        public void UpdateKeyFrame(int frame, float x, float y, float rotation, float skew,
            LingoColor? foreColor = null, LingoColor? backColor = null, float? blend = null)
        {
            Position.UpdateKeyFrame(frame, new LingoPoint(x, y));
            Rotation.UpdateKeyFrame(frame, rotation);
            Skew.UpdateKeyFrame(frame, skew);
            if (foreColor.HasValue) ForegroundColor.UpdateKeyFrame(frame, foreColor.Value);
            if (backColor.HasValue) BackgroundColor.UpdateKeyFrame(frame, backColor.Value);
            if (blend.HasValue) Blend.UpdateKeyFrame(frame, blend.Value);
            _cacheDirty = true;
        }

        internal void AddKeyFrames(params (int Frame, float X, float Y, float Rotation, float Skew)[] keyframes)
        {
            foreach (var (frame, x, y, rot, skew) in keyframes)
                AddKeyFrame(frame, x, y, rot, skew);
            RecalculateCache();
        }

        private void Apply(int frame)
        {
            if (Position.Options.Enabled)
                _sprite.Loc = Position.GetValue(frame);
            if (Rotation.Options.Enabled)
                _sprite.Rotation = Rotation.GetValue(frame);
            if (Skew.Options.Enabled)
                _sprite.Skew = Skew.GetValue(frame);
            if (ForegroundColor.Options.Enabled)
                _sprite.ForeColor = ForegroundColor.GetValue(frame);
            if (BackgroundColor.Options.Enabled)
                _sprite.BackColor = BackgroundColor.GetValue(frame);
            if (Blend.Options.Enabled)
                _sprite.Blend = Blend.GetValue(frame);
        }

        public void BeginSprite()
        {
            EnsureCache();
            Apply(_movie.CurrentFrame);
        }

        public void StepFrame()
        {
            Apply(_movie.CurrentFrame);
        }

        public void EndSprite()
        {
        }

        internal LingoSpriteMotionPath GetMotionPath(int startFrame, int endFrame)
        {
            EnsureCache();
            var path = new LingoSpriteMotionPath();
            foreach (var f in _cachedPath.Frames)
            {
                if (f.Frame < startFrame || f.Frame > endFrame) continue;
                path.Frames.Add(f);
            }
            return path;
        }

        internal void EnsureCache()
        {
            if (!_cacheDirty) return;
            RecalculateCache();
        }

        internal void RecalculateCache()
        {
            _cachedPath = new LingoSpriteMotionPath();
            if (Position.KeyFrames.Count > 0)
            {
                int start = Position.KeyFrames.First().Frame;
                int end = Position.KeyFrames.Last().Frame;
                for (int frame = start; frame <= end; frame++)
                {
                    var pos = Position.GetValue(frame);
                    bool isKey = Position.KeyFrames.Any(k => k.Frame == frame);
                    _cachedPath.Frames.Add(new LingoSpriteMotionFrame(frame, pos, isKey));
                }
            }
            _cacheDirty = false;
        }
    }
}
