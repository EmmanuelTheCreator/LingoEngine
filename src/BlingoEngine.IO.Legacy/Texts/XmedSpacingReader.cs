using BlingoEngine.IO.Legacy.Texts.Data;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedSpacingReader
    {
        private readonly XmedDocument _document;
        private readonly ILogger _logger;
        private readonly List<ParagraphSpacingRecord> _paragraphSpacing = new();
        private readonly List<(int Baseline, int Width)> _paragraphBounds = new();
        private readonly List<ParagraphFormatRecord> _paragraphFormats = new();

        public XmedSpacingReader(XmedDocument document, ILogger logger)
        {
            _document = document;
            _logger = logger;
        }

        internal void Reset()
        {
            _paragraphSpacing.Clear();
            _paragraphBounds.Clear();
            _paragraphFormats.Clear();
        }

        public void ReadParagraphBounds(XmedMainTokenGroup? group)
        {
            if (group == null)
                return;

            var parsed = ExtractBounds(group.Items);
            if (parsed.Count == 0)
                return;

            if (group.MainType == XmedMainTokenGroup.MainGroupType.ParagraphBounds)
            {
                _paragraphBounds.Clear();
                _paragraphBounds.AddRange(parsed);
            }
            else if (_paragraphBounds.Count == 0)
                _paragraphBounds.AddRange(parsed);
        }

        public void ReadParagraphFormats(XmedMainTokenGroup? group)
        {
            if (group == null)
                return;

            var records = ExtractFormats(group.Items);
            if (records.Count == 0)
                return;

            _paragraphFormats.Clear();
            _paragraphFormats.AddRange(records);
        }

        public void ReadParagraphSpacing(XmedMainTokenGroup? group)
        {
            if (group == null)
                return;

            var records = ExtractSpacing(group.RawTokens);
            if (records.Count == 0)
                return;

            _paragraphSpacing.Clear();
            _paragraphSpacing.AddRange(records);
        }

        public void ApplyParagraphBounds(IReadOnlyList<XmedParagraphDescriptor> paragraphs)
        {
            if (paragraphs.Count == 0 || _paragraphBounds.Count == 0)
                return;

            int count = Math.Min(paragraphs.Count, _paragraphBounds.Count);
            for (int i = 0; i < count; i++)
            {
                var (baseline, width) = _paragraphBounds[i];
                var descriptor = paragraphs[i];
                descriptor.BaselineOffset = baseline;
                descriptor.ParagraphWidth = width;
            }
        }

        public void ApplyParagraphFormats(IReadOnlyList<XmedParagraphDescriptor> paragraphs)
        {
            if (paragraphs.Count == 0 || _paragraphFormats.Count == 0)
                return;

            foreach (var record in _paragraphFormats)
            {
                var descriptor = paragraphs.FirstOrDefault(p => p.End == record.EndOffset);
                if (descriptor == null)
                    continue;

                descriptor.FormatRecord = new XmedParagraphFormatRecord
                {
                    EndOffset = record.EndOffset,
                    LeadingMargin = record.Leading,
                    Span = record.Span,
                    Flags = record.Flags,
                    TrailingValue = record.Trailing,
                    AlignmentCode = record.Alignment
                };

                if (record.Span > 0 && descriptor.ParagraphWidth == 0)
                    descriptor.ParagraphWidth = record.Span;

                int computedIndent = descriptor.FormatRecord.FirstLineIndent;
                if (computedIndent > 0 && descriptor.FirstLineIndent == 0)
                    descriptor.FirstLineIndent = computedIndent;
            }
        }

        public void ApplyParagraphSpacing(IReadOnlyList<XmedParagraphDescriptor> paragraphs)
        {
            if (paragraphs.Count == 0 || _paragraphSpacing.Count == 0)
                return;

            foreach (var record in _paragraphSpacing)
            {
                if (record.ParagraphIndex < 0 || record.ParagraphIndex >= paragraphs.Count)
                    continue;

                var descriptor = paragraphs[record.ParagraphIndex];
                descriptor.SpacingTopOffset = record.TopOffset;
                descriptor.SpacingBottomOffset = record.BottomOffset;

            }
        }

        private static List<(int Baseline, int Width)> ExtractBounds(IReadOnlyList<BlXmedToken> tokens)
        {
            var bounds = new List<(int Baseline, int Width)>();
            if (tokens == null || tokens.Count == 0)
                return bounds;

            var numbers = new List<int>();
            foreach (var token in tokens)
            {
                if (token.TryGetNumericValue(out var numeric))
                    numbers.Add(numeric);
            }

            int index = 0;
            while (index + 2 < numbers.Count)
            {
                int sentinel = numbers[index];
                int baseline = numbers[index + 1];
                int width = numbers[index + 2];
                if (index + 5 < numbers.Count && numbers[index + 3] == sentinel && numbers[index + 4] == baseline && numbers[index + 5] == width)
                    index += 6;
                else
                    index += 3;

                bounds.Add((baseline, width));
            }

            return bounds;
        }

        private static List<ParagraphFormatRecord> ExtractFormats(IReadOnlyList<BlXmedToken> tokens)
        {
            var formats = new List<ParagraphFormatRecord>();
            if (tokens == null || tokens.Count == 0)
                return formats;

            int index = 0;
            while (index < tokens.Count)
            {
                int endOffset = ReadNumber(tokens, ref index);
                SkipNulls(tokens, ref index, 2);
                int lead = ReadNumber(tokens, ref index);
                int span = ReadNumber(tokens, ref index);
                int flags = ReadNumber(tokens, ref index);
                int trailing = 0;
                int alignment = 0;

                if (index < tokens.Count && TryReadNumber(tokens[index], out var extra))
                {
                    trailing = extra;
                    index++;
                }

                if (index < tokens.Count && IsNull(tokens[index]))
                    index++;

                if (index < tokens.Count && TryReadNumber(tokens[index], out var alignValue))
                {
                    alignment = alignValue;
                    index++;
                }

                formats.Add(new ParagraphFormatRecord(endOffset, lead, span, flags, trailing, alignment));
            }

            return formats;
        }

        private static List<ParagraphSpacingRecord> ExtractSpacing(IReadOnlyList<BlXmedToken> tokens)
        {
            var spacing = new List<ParagraphSpacingRecord>();
            if (tokens == null || tokens.Count == 0)
                return spacing;

            var values = new List<int>();
            foreach (var token in tokens)
            {
                if (token.Type == BlXmedToken.TokenType.PrefixedHex && token.TypeValue == 0x02 && token.TryGetNumericValue(out var numeric))
                    values.Add(numeric);
            }

            for (int i = 0; i + 1 < values.Count; i += 2)
                spacing.Add(new ParagraphSpacingRecord(i / 2, values[i], values[i + 1]));

            return spacing;
        }

        private static int ReadNumber(IReadOnlyList<BlXmedToken> tokens, ref int index)
        {
            int value = 0;
            if (index < tokens.Count && TryReadNumber(tokens[index], out var parsed))
                value = parsed;

            index++;
            return value;
        }

        private static bool TryReadNumber(BlXmedToken token, out int value)
        {
            if (token.TryGetNumericValue(out value))
                return true;

            value = 0;
            return false;
        }

        private static void SkipNulls(IReadOnlyList<BlXmedToken> tokens, ref int index, int expected)
        {
            int skipped = 0;
            while (index < tokens.Count && skipped < expected)
            {
                if (!IsNull(tokens[index]))
                    break;

                index++;
                skipped++;
            }
        }

        private static bool IsNull(BlXmedToken token)
        {
            return token.Type == BlXmedToken.TokenType.B_82_NULL || token.Type == BlXmedToken.TokenType.NULL;
        }

        private readonly struct ParagraphFormatRecord
        {
            public ParagraphFormatRecord(int endOffset, int leading, int span, int flags, int trailing, int alignment)
            {
                EndOffset = endOffset;
                Leading = leading;
                Span = span;
                Flags = flags;
                Trailing = trailing;
                Alignment = alignment;
            }

            public int EndOffset { get; }
            public int Leading { get; }
            public int Span { get; }
            public int Flags { get; }
            public int Trailing { get; }
            public int Alignment { get; }
        }

        private readonly struct ParagraphSpacingRecord
        {
            public ParagraphSpacingRecord(int paragraphIndex, int topOffset, int bottomOffset)
            {
                ParagraphIndex = paragraphIndex;
                TopOffset = topOffset;
                BottomOffset = bottomOffset;
            }

            public int ParagraphIndex { get; }
            public int TopOffset { get; }
            public int BottomOffset { get; }
        }
    }
}
