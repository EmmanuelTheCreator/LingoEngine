using AbstUI.Primitives;
using System.Runtime;

namespace BlingoEngine.Animations
{
    public class BlingoSpriteAnimatorProperties
    {
        private Dictionary<int, BlingoKeyFrameSetting> _settings = new();
        private ARect? _calculatedBoundingBox;
        private Dictionary<int, ARect> _calculatedFrameBoundingBoxes = new();
        private bool _cacheDirty = true;
        public bool CacheIsDirty => _cacheDirty;
        public void CacheApplied() => _cacheDirty = false;
        public BlingoTween<APoint> Position { get; private set; } = new();
        public BlingoTween<APoint> Size { get; private set; } = new();
        public BlingoTween<float> Rotation { get; private set; } = new();
        public BlingoTween<float> Skew { get; private set; } = new();
        public BlingoTween<AColor> ForegroundColor { get; private set; } = new();
        public BlingoTween<AColor> BackgroundColor { get; private set; } = new();
        public BlingoTween<float> Blend { get; private set; } = new();

        public BlingoSpriteAnimatorProperties Clone()
        {
            var clone = new BlingoSpriteAnimatorProperties
            {
                Position = Position.Clone(),
                Size = Size.Clone(),
                Rotation = Rotation.Clone(),
                Skew = Skew.Clone(),
                ForegroundColor = ForegroundColor.Clone(),
                BackgroundColor = BackgroundColor.Clone(),
                Blend = Blend.Clone()
            };
            return clone;
        }

        public void SetTweenOptions(bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled,
           bool foreColorEnabled, bool backColorEnabled, bool blendEnabled,
           float curvature, bool continuousAtEnds, bool smoothSpeed, float easeIn, float easeOut)
        {
            Position.Options.Enabled = positionEnabled;
            Size.Options.Enabled = sizeEnabled;
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
                opt.SpeedChange = smoothSpeed ? BlingoSpeedChangeType.Smooth : BlingoSpeedChangeType.Sharp;
                opt.EaseIn = easeIn;
                opt.EaseOut = easeOut;
            }
        }

        public IReadOnlyCollection<BlingoKeyFrameSetting>? GetKeyFrames() => _settings.Values;

        /// <summary>
        /// Returns the keyframe that should drive the supplied <paramref name="frame"/>.
        /// <list type="bullet">
        /// <item>
        /// <description>If an exact keyframe exists, it is returned.</description>
        /// </item>
        /// <item>
        /// <description>If the frame falls between two keyframes, the one on the left (the previous keyframe) is returned.</description>
        /// </item>
        /// <item>
        /// <description>If the frame precedes the first keyframe, the first keyframe is returned, and if it exceeds the last, the last keyframe is returned.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="frame">The frame to locate a keyframe for.</param>
        /// <returns>The keyframe setting that controls the requested frame, or <c>null</c> when there are no keyframes.</returns>
        public BlingoKeyFrameSetting? GetKeyFrameForFrame(int frame)
        {
            if (_settings.Count == 0)
                return null;

            if (_settings.TryGetValue(frame, out var exact))
                return exact;

            int? candidate = null;
            foreach (var key in _settings.Keys)
            {
                if (key <= frame && (candidate == null || key > candidate.Value))
                    candidate = key;
            }

            if (candidate.HasValue)
                return _settings[candidate.Value];

            int? first = null;
            foreach (var key in _settings.Keys)
            {
                if (first == null || key < first.Value)
                    first = key;
            }

            return first.HasValue ? _settings[first.Value] : null;
        }
        public void MoveKeyFrame(int from, int to)
        {
            if (from == to)
                return;
            if (!_settings.TryGetValue(from, out var setting))
                return;
            _settings.Remove(from);
            setting.Frame = to;
            _settings[to] = setting;

            Position.MoveKeyFrame(from, to);
            Size.MoveKeyFrame(from, to);
            Rotation.MoveKeyFrame(from, to);
            Skew.MoveKeyFrame(from, to);
            ForegroundColor.MoveKeyFrame(from, to);
            BackgroundColor.MoveKeyFrame(from, to);
            Blend.MoveKeyFrame(from, to);

            _cacheDirty = true;
        }
        public bool DeleteKeyFrame(int frame)
        {
            bool removed = false;
            removed |= Position.DeleteKeyFrame(frame);
            removed |= Size.DeleteKeyFrame(frame);
            removed |= Rotation.DeleteKeyFrame(frame);
            removed |= Skew.DeleteKeyFrame(frame);
            removed |= ForegroundColor.DeleteKeyFrame(frame);
            removed |= BackgroundColor.DeleteKeyFrame(frame);
            removed |= Blend.DeleteKeyFrame(frame);

            if (_settings.ContainsKey(frame))
            {
                _settings.Remove(frame);
                removed = true;
            }

            if (removed)
                _cacheDirty = true;

            return removed;
        }
        public void AddKeyframes(params BlingoKeyFrameSetting[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0)
                return;
            foreach (var keyframe in keyframes)
                AddKeyFrame(keyframe);
        }
        public void AddKeyFrame(BlingoKeyFrameSetting setting)
        {
            if (setting.Position != null) Position.AddKeyFrame(setting.Frame, setting.Position.Value);
            if (setting.Size != null) Size.AddKeyFrame(setting.Frame, setting.Size.Value);
            if (setting.Rotation != null) Rotation.AddKeyFrame(setting.Frame, setting.Rotation.Value);
            if (setting.Skew != null) Skew.AddKeyFrame(setting.Frame, setting.Skew.Value);
            if (setting.ForeColor != null) ForegroundColor.AddKeyFrame(setting.Frame, setting.ForeColor.Value);
            if (setting.BackColor != null) BackgroundColor.AddKeyFrame(setting.Frame, setting.BackColor.Value);
            if (setting.Blend != null) Blend.AddKeyFrame(setting.Frame, setting.Blend.Value);

            if (_settings.ContainsKey(setting.Frame))
                _settings[setting.Frame] = setting;
            else
                _settings.Add(setting.Frame, setting);
            _cacheDirty = true;
        }
        public void UpdateKeyFrame(BlingoKeyFrameSetting setting)
        {
            if (setting.Position != null) Position.UpdateKeyFrame(setting.Frame, setting.Position.Value);
            if (setting.Size != null) Size.UpdateKeyFrame(setting.Frame, setting.Size.Value);
            if (setting.Rotation != null) Rotation.UpdateKeyFrame(setting.Frame, setting.Rotation.Value);
            if (setting.Skew != null) Skew.UpdateKeyFrame(setting.Frame, setting.Skew.Value);
            if (setting.ForeColor != null) ForegroundColor.UpdateKeyFrame(setting.Frame, setting.ForeColor.Value);
            if (setting.BackColor != null) BackgroundColor.UpdateKeyFrame(setting.Frame, setting.BackColor.Value);
            if (setting.Blend != null) Blend.UpdateKeyFrame(setting.Frame, setting.Blend.Value);
            if (_settings.ContainsKey(setting.Frame))
                _settings[setting.Frame] = setting;
            else
                _settings.Add(setting.Frame, setting);
            _cacheDirty = true;
        }




        #region Boundingbox

        public ARect GetBoundingBox(APoint spriteRegpoint, ARect spriteRect, float spriteWidth, float spriteHeight)
        {
            if (_calculatedBoundingBox != null) return _calculatedBoundingBox.Value;

            var frames = new List<int>();
            if (Position.HasKeyFrames) frames.AddRange(Position.KeyFrames.Select(k => k.Frame));
            if (Size.HasKeyFrames) frames.AddRange(Size.KeyFrames.Select(k => k.Frame));
            if (Rotation.HasKeyFrames) frames.AddRange(Rotation.KeyFrames.Select(k => k.Frame));
            if (Skew.HasKeyFrames) frames.AddRange(Skew.KeyFrames.Select(k => k.Frame));

            var result = spriteRect;

            if (frames.Count > 0)
            {
                int start = frames.Min();
                int end = frames.Max();
                for (int f = start; f <= end; f++)
                {
                    var rect = GetBoundingBoxForFrame(f, spriteRegpoint, spriteWidth, spriteHeight);
                    result = result.Union(rect);
                }
            }

            _calculatedBoundingBox = result;
            return _calculatedBoundingBox.Value;
        }
        public ARect GetBoundingBoxForFrame(int frame, APoint spriteRegpoint, float spriteWidth, float spriteHeight)
        {
            if (_calculatedFrameBoundingBoxes.TryGetValue(frame, out var cachedBox))
                return cachedBox;
            var pos = Position.GetValue(frame);
            var size = Size.GetValue(frame);
            var rot = Rotation.GetValue(frame);
            var skew = Skew.GetValue(frame);

            if (size.X == 0 || size.Y == 0)
                size = new APoint(spriteWidth, spriteHeight);

            var reg = spriteRegpoint;
            var center = new APoint(pos.X - reg.X + size.X / 2f, pos.Y - reg.Y + size.Y / 2f);

            float hw = size.X / 2f;
            float hh = size.Y / 2f;

            var tl = new APoint(-hw, -hh);
            var tr = new APoint(hw, -hh);
            var br = new APoint(hw, hh);
            var bl = new APoint(-hw, hh);

            if (skew != 0)
            {
                float skewRad = skew * MathF.PI / 180f;
                float skewX = MathF.Tan(skewRad);
                tl.X += tl.Y * skewX;
                tr.X += tr.Y * skewX;
                br.X += br.Y * skewX;
                bl.X += bl.Y * skewX;
            }

            if (rot != 0)
            {
                float rad = rot * MathF.PI / 180f;
                float cos = MathF.Cos(rad);
                float sin = MathF.Sin(rad);

                tl = Rotate(tl, cos, sin);
                tr = Rotate(tr, cos, sin);
                br = Rotate(br, cos, sin);
                bl = Rotate(bl, cos, sin);
            }

            tl += center;
            tr += center;
            br += center;
            bl += center;

            var newRect = ARect.FromPoints(tl, tr, br, bl);
            _calculatedFrameBoundingBoxes.Add(frame, newRect);
            return newRect;
        }
        private static APoint Rotate(APoint pt, float cos, float sin)
        {
            return new APoint(
                pt.X * cos - pt.Y * sin,
                pt.X * sin + pt.Y * cos
            );
        }

        internal void RequestRecalculatedBoundingBox()
        {
            _calculatedBoundingBox = null;
            _calculatedFrameBoundingBoxes.Clear();
        }


        #endregion
    }
}

