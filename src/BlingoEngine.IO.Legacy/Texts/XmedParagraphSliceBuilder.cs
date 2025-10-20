using BlingoEngine.IO.Legacy.Texts.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedParagraphSliceBuilder
    {
        private readonly List<(int End, bool Flag)> _paragraphFlags = new();
        private List<ParagraphSlice> _paragraphs = new();

        public List<(int End, bool Flag)> GetOrderedParagraphBoundaries() => _paragraphFlags.OrderBy(p => p.End).ToList();

        public void Reset()
        {
            _paragraphFlags.Clear();
            _paragraphs = new List<ParagraphSlice>();
        }

        public void LoadParagraphBoundaries(XmedTokenGroup? block)
        {
            _paragraphFlags.Clear();
            if (block == null)
                return;

            foreach (var item in block.Items)
            {
                if (item is not XmedTokenGroup child)
                    continue;

                int? end = null;
                bool? flag = null;

                //foreach (var token in child.CollectTokens())
                //{
                //    if (end is null && token.IsPrefixedHex02() && token.TryGetNumericValue(out var numeric))
                //    {
                //        end = numeric;
                //        continue;
                //    }

                //    if (flag is null && token.TryGetBool(out var boolValue))
                //    {
                //        flag = boolValue;
                //        continue;
                //    }
                //}

                //if (end.HasValue)
                //    _paragraphFlags.Add((end.Value, flag ?? false));
            }
        }



        internal List<ParagraphSlice> BuildParagraphSlices(List<(int End, bool Flag)> boundaries, int textLength)
        {
            var slices = new List<ParagraphSlice>();
            int start = 0;

            foreach (var (end, flag) in boundaries)
            {
                int clampedEnd = Math.Clamp(end, 0, textLength);
                if (clampedEnd < start)
                    continue;

                int length = clampedEnd - start;
                if (length > 0)
                    slices.Add(new ParagraphSlice(start, clampedEnd, flag, slices.Count));

                start = clampedEnd;
            }

            if (start < textLength)
                slices.Add(new ParagraphSlice(start, textLength, false, slices.Count));
            _paragraphs = slices;
            return slices;
        }

        internal List<ParagraphSlice> BuildParagraphSlicesFromText(string text)
        {
            var spans = ExtractParagraphSpans(text);
            if (spans.Count == 0 && !string.IsNullOrEmpty(text))
                spans.Add((0, text.Length));

            var slices = new List<ParagraphSlice>();
            foreach (var (start, length) in spans)
            {
                if (length <= 0)
                    continue;

                slices.Add(new ParagraphSlice(start, start + length, false, slices.Count));
            }

            _paragraphs = slices;
            return slices;
        }
        private static List<(int Start, int Length)> ExtractParagraphSpans(string text)
        {
            var spans = new List<(int Start, int Length)>();
            if (string.IsNullOrEmpty(text))
                return spans;

            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r')
                {
                    spans.Add((start, i - start));
                    start = i + 1;
                }
            }

            if (start <= text.Length)
                spans.Add((start, text.Length - start));

            return spans;
        }

        public int FindParagraphId(int start)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (start <= _paragraphs[i].End)
                    return i;
            }

            return Math.Max(0, _paragraphs.Count - 1);
        }

        internal readonly struct ParagraphSlice
        {
            public ParagraphSlice(int start, int end, bool flag, int paragraphId)
            {
                Start = start;
                End = end;
                Flag = flag;
                ParagraphId = paragraphId;
            }

            public int Start { get; }
            public int End { get; }
            public bool Flag { get; }
            public int ParagraphId { get; }
            public int Length => Math.Max(0, End - Start);
        }
    }
}
