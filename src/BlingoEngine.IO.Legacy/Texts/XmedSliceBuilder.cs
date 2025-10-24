using System;
using System.Collections.Generic;
using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedSliceBuilder
    {
        private readonly List<RunBoundary> _boundaries = new();

        public void Reset()
        {
            _boundaries.Clear();
        }

        public void LoadBoundaries(XmedTokenGroup? block)
        {
            _boundaries.Clear();
            if (block == null)
                return;

            foreach (var item in block.Items)
            {
                if (item is not XmedTokenGroup segment)
                    continue;

                int value = Math.Max(0, segment.ReadNumeric(0));
                int end = Math.Max(0, segment.ReadNumeric(1));
                if (end <= 0)
                    continue;

                _boundaries.Add(new RunBoundary(end, value));
            }
        }

        public List<Slice> BuildSlices(string fullText)
        {
            return BuildSlices(fullText, slice => slice);
        }

        public List<TReturnType> BuildSlices<TReturnType>(string fullText, Func<Slice, TReturnType> projector)
        {
            var slices = new List<TReturnType>();
            string text = fullText ?? string.Empty;
            if (text.Length == 0)
                return slices;

            int effectiveLength = text.Length;
            var normalized = NormalizeBoundaries(effectiveLength);
            if (normalized.Count == 0)
                normalized.Add(new RunBoundary(effectiveLength, 0));

            int start = 0;
            int lastValue = 0;

            foreach (var boundary in normalized)
            {
                int end = Math.Min(boundary.End, effectiveLength);
                int value = boundary.Value;
                if (end <= start)
                {
                    lastValue = value;
                    continue;
                }

                string sliceText = ExtractText(text, start, end);
                slices.Add(projector(new Slice(start, end, value, sliceText)));
                start = end;
                lastValue = value;
            }

            if (start < effectiveLength)
            {
                string trailing = ExtractText(text, start, effectiveLength);
                slices.Add(projector(new Slice(start, effectiveLength, lastValue, trailing)));
            }

            return slices;
        }

        private List<RunBoundary> NormalizeBoundaries(int textLength)
        {
            var normalized = new List<RunBoundary>(_boundaries.Count);
            foreach (var boundary in _boundaries)
            {
                int end = Math.Clamp(boundary.End, 0, textLength);
                if (end <= 0)
                    continue;

                normalized.Add(new RunBoundary(end, Math.Max(0, boundary.Value)));
            }

            normalized.Sort((left, right) => left.End.CompareTo(right.End));
            return normalized;
        }

        private static string ExtractText(string text, int start, int end)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            int safeStart = Math.Clamp(start, 0, text.Length);
            int safeEnd = Math.Clamp(end, safeStart, text.Length);
            int length = safeEnd - safeStart;
            if (length <= 0)
                return string.Empty;

            return text.Substring(safeStart, length);
        }

        internal readonly struct Slice
        {
            public Slice(int start, int end, int value, string text)
            {
                Start = Math.Max(0, start);
                Value = value;
                Text = text ?? string.Empty;
            }

            public int Start { get; }
            public int Value { get; }
            public string Text { get; }
        }

        private readonly struct RunBoundary
        {
            public RunBoundary(int end, int value)
            {
                End = end;
                Value = value;
            }

            public int End { get; }
            public int Value { get; }
        }
    }
}
