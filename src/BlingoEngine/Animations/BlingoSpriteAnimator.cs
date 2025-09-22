using BlingoEngine.Events;
using BlingoEngine.Sprites;
using AbstUI.Primitives;

namespace BlingoEngine.Animations
{
    public class BlingoSpriteAnimator : IPlayableActor
    {
        private BlingoSpriteMotionPath _cachedPath = new();

        private readonly IBlingoSpritesPlayer _spritesPlayer;
        private readonly IBlingoSprite2DLight _sprite;
        //private readonly IBlingoMovie _movie;
        private readonly IBlingoEventMediator _mediator;

        private List<Action<int>> _applyActions = new();

        private BlingoSpriteAnimatorProperties _properties;
        public BlingoSpriteAnimatorProperties Properties => _properties;




        public BlingoSpriteAnimator(IBlingoSprite2DLight sprite, IBlingoSpritesPlayer spritesPlayer, IBlingoEventMediator mediator, BlingoSpriteAnimatorProperties? spriteAnimatorProperties , bool autoSubscribeEvents)
        {
            _sprite = sprite;
            _spritesPlayer = spritesPlayer;
            _mediator = mediator;
            _properties = spriteAnimatorProperties ?? new BlingoSpriteAnimatorProperties();
            if (autoSubscribeEvents)
                _mediator.Subscribe(this, sprite.SpriteNum + 6);
        }

        public void SetTweenOptions(bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled,
            bool foreColorEnabled, bool backColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool smoothSpeed, float easeIn, float easeOut)
        {
            _properties.SetTweenOptions(positionEnabled, sizeEnabled, rotationEnabled, skewEnabled,
                foreColorEnabled, backColorEnabled, blendEnabled,
                curvature, continuousAtEnds, smoothSpeed, easeIn, easeOut);
        }

        internal IReadOnlyCollection<BlingoKeyFrameSetting>? GetKeyframes() => _properties.GetKeyFrames();
        internal BlingoKeyFrameSetting? GetKeyFrameForFrame(int frame) => _properties.GetKeyFrameForFrame(frame);
        internal void MoveKeyFrame(int from, int to) => _properties.MoveKeyFrame(from, to);
        internal bool DeleteKeyFrame(int frame) => _properties.DeleteKeyFrame(frame);
        public void AddKeyFrame(BlingoKeyFrameSetting setting) => _properties.AddKeyFrame(setting);
        public void UpdateKeyFrame(BlingoKeyFrameSetting setting) => _properties.UpdateKeyFrame(setting);

        internal void AddKeyFrames(params BlingoKeyFrameSetting[] keyframes)
        {
            foreach (var item in keyframes)
                AddKeyFrame(item);
            RecalculateCache();
        }

        private void Apply(int frame)
        {
            foreach (var action in _applyActions)
                action.Invoke(frame);

        }

        private int ToRelativeFrame(int globalFrame)
        {
            if (_sprite is IBlingoSpriteBase baseSprite)
                return globalFrame - baseSprite.BeginFrame;
            return globalFrame;
        }

        public void BeginSprite()
        {
            EnsureCache();
            Apply(ToRelativeFrame(_spritesPlayer.CurrentFrame));
        }

        public void StepFrame()
        {
            Apply(ToRelativeFrame(_spritesPlayer.CurrentFrame));
        }

        public void EndSprite()
        {
        }

        internal BlingoSpriteMotionPath GetMotionPath(int startFrame, int endFrame)
        {
            EnsureCache();
            var path = new BlingoSpriteMotionPath();
            int offset = (_sprite as IBlingoSpriteBase)?.BeginFrame ?? 0;
            int relStart = startFrame - offset;
            int relEnd = endFrame - offset;
            foreach (var f in _cachedPath.Frames)
            {
                if (f.Frame < relStart || f.Frame > relEnd) continue;
                path.Frames.Add(new BlingoSpriteMotionFrame(f.Frame + offset, f.Position, f.IsKeyFrame));
            }
            return path;
        }

        internal void EnsureCache()
        {
            if (!_properties.CacheIsDirty) return;

            RecalculateCache();
        }

        internal void RecalculateCache()
        {
            _cachedPath = new BlingoSpriteMotionPath();
            if (_properties.Position.KeyFrames.Count > 0)
            {
                int start = _properties.Position.KeyFrames.First().Frame;
                int end = _properties.Position.KeyFrames.Last().Frame;
                for (int frame = start; frame <= end; frame++)
                {
                    var pos = _properties.Position.GetValue(frame);
                    bool isKey = _properties.Position.KeyFrames.Any(k => k.Frame == frame);
                    _cachedPath.Frames.Add(new BlingoSpriteMotionFrame(frame, pos, isKey));
                }
            }
            _applyActions.Clear();
            if (_properties.Position.Options.Enabled && _properties.Position.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Loc = _properties.Position.GetValue(frame));
            if (_properties.Size.Options.Enabled && _properties.Size.HasKeyFrames)
                _applyActions.Add(frame =>
                {
                    var size = _properties.Size.GetValue(frame);
                    _sprite.Width = size.X;
                    _sprite.Height = size.Y;
                });
            if (_properties.Rotation.Options.Enabled && _properties.Rotation.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Rotation = _properties.Rotation.GetValue(frame));
            if (_properties.Skew.Options.Enabled && _properties.Skew.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Skew = _properties.Skew.GetValue(frame));
            if (_properties.ForegroundColor.Options.Enabled && _properties.ForegroundColor.HasKeyFrames)
                _applyActions.Add(frame => _sprite.ForeColor = _properties.ForegroundColor.GetValue(frame));
            if (_properties.BackgroundColor.Options.Enabled && _properties.BackgroundColor.HasKeyFrames)
                _applyActions.Add(frame => _sprite.BackColor = _properties.BackgroundColor.GetValue(frame));
            if (_properties.Blend.Options.Enabled && _properties.Blend.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Blend = _properties.Blend.GetValue(frame));
            _properties.CacheApplied();
            _properties.RequestRecalculatedBoundingBox();// push to recalculate the boundingbox

        }


        #region Boundingbox
        public void SpriteRegPointOrSizeChanged() => _properties.RequestRecalculatedBoundingBox();

        public ARect GetBoundingBox() => _properties.GetBoundingBoxForFrame(_spritesPlayer.CurrentFrame, _sprite.RegPoint, _sprite.Width, _sprite.Height);


        public ARect GetBoundingBoxForFrame(int frame)
            => _properties.GetBoundingBoxForFrame(frame, _sprite.RegPoint, _sprite.Width, _sprite.Height);



        #endregion


    }
}

