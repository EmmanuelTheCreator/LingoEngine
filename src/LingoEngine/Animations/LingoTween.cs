using System;
using System.Collections.Generic;
using LingoEngine.Primitives;

namespace LingoEngine.Animations
{
    public class LingoTween<T>
    {
        private readonly List<LingoKeyFrame<T>> _keys = new();
        public IReadOnlyList<LingoKeyFrame<T>> KeyFrames => _keys;
        public LingoTweenOptions Options { get; } = new();

        public void AddKeyFrame(int frame, T value, LingoEaseType ease = LingoEaseType.Linear)
        {
            var k = new LingoKeyFrame<T>(frame, value) { Ease = ease };
            _keys.Add(k);
            _keys.Sort((a, b) => a.Frame.CompareTo(b.Frame));
        }

        public void UpdateKeyFrame(int frame, T value, LingoEaseType ease = LingoEaseType.Linear)
        {
            var idx = _keys.FindIndex(k => k.Frame == frame);
            if (idx >= 0)
            {
                _keys[idx].Value = value;
                _keys[idx].Ease = ease;
            }
            else
            {
                AddKeyFrame(frame, value, ease);
            }
        }

        public T GetValue(int frame)
        {
            if (_keys.Count == 0)
                return default!;
            if (frame <= _keys[0].Frame)
                return _keys[0].Value;
            if (frame >= _keys[^1].Frame)
                return _keys[^1].Value;

            for (int i = 0; i < _keys.Count - 1; i++)
            {
                var k0 = _keys[i];
                var k1 = _keys[i + 1];
                if (frame >= k0.Frame && frame <= k1.Frame)
                {
                    float t = (frame - k0.Frame) / (float)(k1.Frame - k0.Frame);
                    t = ApplyEase(t, k1.Ease);
                    t = ApplySpeedChange(t, Options.SpeedChange);
                    if (Options.EaseIn != 0)
                        t = MathF.Pow(t, 1 + Options.EaseIn);
                    if (Options.EaseOut != 0)
                        t = 1 - MathF.Pow(1 - t, 1 + Options.EaseOut);
                    if (Options.Curvature != 0)
                        t = t + Options.Curvature * t * (1 - t);
                    return Lerp(k0.Value, k1.Value, t);
                }
            }
            return _keys[^1].Value;
        }

        private static float ApplyEase(float t, LingoEaseType ease)
        {
            return ease switch
            {
                LingoEaseType.EaseIn => t * t,
                LingoEaseType.EaseOut => t * (2 - t),
                LingoEaseType.EaseInOut => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t,
                _ => t,
            };
        }

        private static float ApplySpeedChange(float t, LingoSpeedChangeType type)
        {
            return type == LingoSpeedChangeType.Smooth ? t * t * (3 - 2 * t) : t;
        }

        private static T Lerp(T a, T b, float t)
        {
            if (typeof(T) == typeof(float))
            {
                float aa = (float)(object)a;
                float bb = (float)(object)b;
                return (T)(object)(aa + (bb - aa) * t);
            }
            if (typeof(T) == typeof(LingoPoint))
            {
                var pa = (LingoPoint)(object)a;
                var pb = (LingoPoint)(object)b;
                return (T)(object)(pa + (pb - pa) * t);
            }
            return t < 1 ? a : b;
        }
    }
}
