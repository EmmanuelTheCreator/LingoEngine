using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenRunParser
    {
        private readonly IReadOnlyList<BlXmedToken> _tokens;
        private readonly byte[] _buffer;
        private readonly XmedDocument _document;
        private readonly BlXmedTokenStyleParser _styleParser;

        private readonly List<BlXmedToken> _textTokens = new();
        private readonly List<(int End, int StyleId)> _runBoundaries = new();
        private readonly List<(int End, bool Flag)> _paragraphFlags = new();
        private readonly List<XmedParagraphDescriptor> _paragraphDescriptors = new();
        private readonly List<(int Before, int After)> _paragraphSpacing = new();

        public BlXmedTokenRunParser(
            ILogger logger,
            IReadOnlyList<BlXmedToken> tokens,
            byte[] buffer,
            XmedDocument document,
            BlXmedTokenStyleParser styleParser,
            IReadOnlyList<int> lastNumbers)
        {
            _ = logger;
            _tokens = tokens;
            _buffer = buffer;
            _document = document;
            _styleParser = styleParser;
            _ = lastNumbers;
        }

        public void AddTextToken(BlXmedToken token)
        {
            if (token.IsTextBlock())
            {
                _textTokens.Add(token);
            }
        }

        public void ReadRuns(BlXmedTokenReader reader)
        {
            reader.Skip();
            int? pendingEnd = null;

            foreach (var token in reader.GetFlatValues())
            {
                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var end))
                {
                    pendingEnd = end;
                    continue;
                }

                if (!pendingEnd.HasValue)
                {
                    continue;
                }

                if (token.IsPrefixedHex01() && token.TryGetNumericValue(out var styleId))
                {
                    _runBoundaries.Add((pendingEnd.Value, styleId));
                    pendingEnd = null;
                    continue;
                }

                if (token.IsBoolean() && token.TryGetBoolean(out var boolValue))
                {
                    _runBoundaries.Add((pendingEnd.Value, boolValue ? 1 : 0));
                    pendingEnd = null;
                }
            }
        }

        public void ReadParagraphFlags(BlXmedTokenReader reader)
        {
            reader.Skip();
            int? pendingEnd = null;

            foreach (var token in reader.GetFlatValues())
            {
                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var end))
                {
                    pendingEnd = end;
                    continue;
                }

                if (!pendingEnd.HasValue)
                {
                    continue;
                }

                if (token.IsBoolean() && token.TryGetBoolean(out var flag))
                {
                    _paragraphFlags.Add((pendingEnd.Value, flag));
                    pendingEnd = null;
                }
            }
        }

        public void ReadParagraphSpacing(BlXmedTokenReader reader)
        {
            reader.Skip();
            ReadParagraphSpacingInternal(reader);
        }

        private void ReadParagraphSpacingInternal(BlXmedTokenReader reader)
        {
            var values = reader.GetNumericValues();
            if (values.Count == 0)
            {
                return;
            }

            int before = values.Count > 0 ? values[0] : 0;
            int after = values.Count > 1 ? values[1] : 0;

            if (before >= -512 && before <= 0x2000 && after >= -512 && after <= 0x2000)
            {
                _paragraphSpacing.Add((before, after));
            }
        }

        public void ReadSpacing(BlXmedTokenReader reader)
        {
            reader.Skip();
            var values = reader.GetNumericValues();
            if (values.Count > 0 && values[0] >= 0)
            {
                _document.LineSpacing = (uint)values[0];
            }
        }

        public void ReadBox(BlXmedTokenReader reader)
        {
            reader.Skip();
            var numbers = new List<int>();
            int depth = 0;

            while (!reader.IsAtEnd)
            {
                var token = reader.Peek();
                if (token is null)
                {
                    break;
                }

                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var value))
                {
                    numbers.Add(value);
                    reader.Skip();
                    continue;
                }

                if (token.IsC1())
                {
                    _styleParser.TrackStyleMarker(token);
                    depth++;
                    reader.Skip();
                    continue;
                }

                if (token.IsC2())
                {
                    depth++;
                    reader.Skip();
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    reader.Skip();
                    if (depth == 0)
                    {
                        break;
                    }

                    depth--;
                    continue;
                }

                if (token.IsFieldSeparator() || token.IsBoolean())
                {
                    reader.Skip();
                    continue;
                }

                if (token.IsBlockBoundary())
                {
                    break;
                }

                reader.Skip();
            }

            if (numbers.Count >= 2)
            {
                long width = numbers[1] - numbers[0];
                if (width < 0)
                {
                    width = 0;
                }

                _document.Width = (uint)width;
            }
        }

        public void ReadParagraphDescriptor(BlXmedTokenReader reader)
        {
            reader.Skip();
            ParseParagraphBlock(reader, 0);
        }

        private void ParseParagraphBlock(BlXmedTokenReader reader, int depth)
        {
            var values = new List<int>();
            var tabStops = new List<int>();
            int fieldIndex = 0;

            while (!reader.IsAtEnd)
            {
                var token = reader.Peek();
                if (token is null)
                {
                    break;
                }

                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var numeric))
                {
                    if (fieldIndex < 4)
                    {
                        values.Add(numeric);
                    }
                    else
                    {
                        tabStops.Add(numeric);
                    }

                    reader.Skip();
                    continue;
                }

                if (token.IsFieldSeparator())
                {
                    fieldIndex++;
                    reader.Skip();
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    reader.Skip();
                    FinalizeParagraphDescriptor(values, tabStops);
                    return;
                }

                if (token.IsC1())
                {
                    _styleParser.TrackStyleMarker(token);
                    reader.Skip();

                    if (token.TypeValue == 0x03)
                    {
                        ParseParagraphBlock(reader, depth + 1);
                        continue;
                    }

                    if (token.TypeValue == 0x1C)
                    {
                        _styleParser.MarkStyleFlag(style =>
                        {
                            style.Underline = true;
                            style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                        });
                    }
                    else if (token.TypeValue == 0x1D)
                    {
                        _styleParser.MarkStyleFlag(style =>
                        {
                            style.Italic = true;
                            style.StyleFlags = (byte)(style.StyleFlags | 0x02);
                        });
                    }

                    continue;
                }

                if (token.IsC2())
                {
                    if (token.TypeValue == 0x03)
                    {
                        ReadParagraphSpacing(reader);
                        continue;
                    }

                    reader.Skip();
                    continue;
                }

                if (token.IsBlockBoundary())
                {
                    if (depth == 0)
                    {
                        FinalizeParagraphDescriptor(values, tabStops);
                    }
                    return;
                }

                reader.Skip();
            }

            FinalizeParagraphDescriptor(values, tabStops);
        }

        private void FinalizeParagraphDescriptor(List<int> values, List<int> tabStops)
        {
            if (values.Count == 0)
            {
                values.Clear();
                tabStops.Clear();
                return;
            }

            bool IsWithinRange(int value) => value >= -512 && value <= 0x2000;

            int leftRaw = values.ElementAtOrDefault(0);
            int rightRaw = values.ElementAtOrDefault(1);
            int firstLineRaw = values.ElementAtOrDefault(2);

            if (!IsWithinRange(leftRaw) || !IsWithinRange(rightRaw) || !IsWithinRange(firstLineRaw))
            {
                values.Clear();
                tabStops.Clear();
                return;
            }

            static int Normalize(int value) => value < 0 ? 0 : Math.Min(value, 0x2000);

            var descriptor = new XmedParagraphDescriptor
            {
                LeftMargin = Normalize(leftRaw),
                RightMargin = Normalize(rightRaw),
                FirstLineIndent = Normalize(firstLineRaw),
                AdditionalIndent = values.Count > 3 && IsWithinRange(values[3])
                    ? Normalize(values[3])
                    : null
            };

            if (tabStops.Count > 0)
            {
                foreach (int stop in tabStops.Where(IsWithinRange))
                {
                    descriptor.TabStops.Add(Normalize(stop));
                }
            }

            _paragraphDescriptors.Add(descriptor);

            values.Clear();
            tabStops.Clear();
        }

        public void CollectParagraphDescriptorsFromTokens()
        {
            if (_tokens.Count == 0)
            {
                return;
            }

            _paragraphSpacing.Clear();

            var descriptors = new List<XmedParagraphDescriptor>();
            for (int i = 0; i < _tokens.Count; i++)
            {
                var token = _tokens[i];
                if (token.IsCompositeC1(0x03))
                {
                    if (TryExtractParagraphDescriptor(i, out var descriptor, out var endIndex) && descriptor != null)
                    {
                        descriptors.Add(descriptor);
                        i = endIndex;
                    }
                }
            }

            if (descriptors.Count == 0)
            {
                return;
            }

            if (_paragraphDescriptors.Count < descriptors.Count)
            {
                int missing = descriptors.Count - _paragraphDescriptors.Count;
                for (int i = 0; i < missing; i++)
                {
                    _paragraphDescriptors.Insert(0, new XmedParagraphDescriptor());
                }
            }

            for (int i = 0; i < descriptors.Count; i++)
            {
                int targetIndex = _paragraphDescriptors.Count - descriptors.Count + i;
                if (targetIndex < 0 || targetIndex >= _paragraphDescriptors.Count)
                {
                    continue;
                }

                var target = _paragraphDescriptors[targetIndex];
                var source = descriptors[i];

                target.LeftMargin = source.LeftMargin;
                target.RightMargin = source.RightMargin;
                target.FirstLineIndent = source.FirstLineIndent;
                target.AdditionalIndent = source.AdditionalIndent;
                target.Alignment = source.Alignment;

                if (source.TabStops.Count > 0)
                {
                    target.TabStops.Clear();
                    target.TabStops.AddRange(source.TabStops);
                }
            }
        }

        private bool TryExtractParagraphSpacing(int startIndex, out int endIndex, out (int Before, int After)? spacing)
        {
            spacing = null;
            endIndex = startIndex;

            if (startIndex < 0 || startIndex >= _tokens.Count)
            {
                return false;
            }

            var token = _tokens[startIndex];
            if (!token.IsCompositeC2(0x03))
            {
                return false;
            }

            var values = new List<int>();
            int index = startIndex + 1;

            while (index < _tokens.Count)
            {
                var current = _tokens[index];

                if (current.IsPrefixedHex02() && current.TryGetNumericValue(out var numeric))
                {
                    values.Add(numeric);
                    index++;
                    continue;
                }

                if (current.IsFieldSeparator())
                {
                    index++;
                    continue;
                }

                if (current.IsFieldTerminator())
                {
                    index++;
                    break;
                }

                if (current.IsBlockBoundary())
                {
                    break;
                }

                index++;
            }

            endIndex = Math.Max(startIndex, index - 1);

            if (values.Count > 0 &&
                values[0] >= -512 && values[0] <= 0x2000 &&
                values.ElementAtOrDefault(1) >= -512 && values.ElementAtOrDefault(1) <= 0x2000)
            {
                spacing = (values.ElementAtOrDefault(0), values.ElementAtOrDefault(1));
            }

            return true;
        }

        private bool TryExtractParagraphDescriptor(int startIndex, out XmedParagraphDescriptor? descriptor, out int endIndex)
        {
            descriptor = null;
            endIndex = startIndex;
            var values = new List<int>();
            var tabStops = new List<int>();
            int depth = 0;
            int index = startIndex + 1;

            while (index < _tokens.Count)
            {
                var token = _tokens[index];

                if (token.IsFieldSeparator())
                {
                    index++;
                    continue;
                }

                if (token.IsCompositeC1(0x03))
                {
                    depth++;
                    index++;
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    if (depth == 0)
                    {
                        index++;
                        break;
                    }

                    depth--;
                    index++;
                    continue;
                }

                if (token.IsC2())
                {
                    if (depth == 0 &&
                        token.TypeValue == 0x03 &&
                        TryExtractParagraphSpacing(index, out var spacingEndIndex, out var spacing))
                    {
                        if (spacing.HasValue)
                        {
                            _paragraphSpacing.Add(spacing.Value);
                        }

                        index = spacingEndIndex + 1;
                        continue;
                    }

                    index++;
                    continue;
                }

                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var numeric))
                {
                    if (depth == 0)
                    {
                        if (values.Count < 4)
                        {
                            values.Add(numeric);
                        }
                        else
                        {
                            tabStops.Add(numeric);
                        }
                    }

                    index++;
                    continue;
                }

                if (token.IsBlockBoundary())
                {
                    break;
                }

                index++;
            }

            endIndex = Math.Max(startIndex, index - 1);

            if (values.Count >= 3 &&
                values[0] >= -512 && values[0] <= 0x2000 &&
                values[2] >= -512 && values[2] <= 0x2000)
            {
                descriptor = new XmedParagraphDescriptor
                {
                    LeftMargin = values.ElementAtOrDefault(0),
                    RightMargin = values.ElementAtOrDefault(1),
                    FirstLineIndent = values.ElementAtOrDefault(2),
                    AdditionalIndent = values.Count > 3 ? values[3] : null
                };

                if (tabStops.Count > 0)
                {
                    descriptor.TabStops.AddRange(tabStops);
                }

                return true;
            }

            return false;
        }

        public void BuildText()
        {
            if (_textTokens.Count == 0)
            {
                _document.Text = string.Empty;
                _document.TextLength = 0;
                return;
            }

            var builder = new StringBuilder();
            foreach (var token in _textTokens)
            {
                if (token.Data is { Length: > 0 } data)
                {
                    builder.Append(Encoding.ASCII.GetString(data));
                }
                else if (!string.IsNullOrEmpty(token.Ascii))
                {
                    builder.Append(token.Ascii);
                }
            }

            _document.Text = builder.ToString();
            _document.TextLength = _document.Text.Length;
        }

        public void FinalizeRunsAndParagraphs()
        {
            var baseStyle = _styleParser.GetOrCreateStyle(0);

            if (_document.TextLength <= 0)
            {
                _document.Runs.Clear();
                _document.RunMap.Clear();
                _document.Paragraphs.Clear();
                return;
            }

            int textLength = _document.TextLength;
            var orderedRunBoundaries = _runBoundaries.OrderBy(b => b.End).ToList();
            if (orderedRunBoundaries.Count == 0)
            {
                orderedRunBoundaries.Add((textLength, 0));
            }

            var orderedParagraphBoundaries = _paragraphFlags.OrderBy(p => p.End).ToList();
            if (orderedParagraphBoundaries.Count == 0)
            {
                orderedParagraphBoundaries.Add((textLength, false));
            }

            var paragraphSlices = BuildParagraphSlices(orderedParagraphBoundaries, textLength);
            var runSlices = BuildRunSlices(orderedRunBoundaries, textLength, paragraphSlices);

            if (orderedRunBoundaries.Any(b => b.End > textLength) || runSlices.Sum(slice => slice.Length) < textLength)
            {
                runSlices = new List<TextSlice>
                {
                    new TextSlice(0, textLength, 0, 0)
                };
            }

            if (orderedParagraphBoundaries.Any(b => b.End > textLength) || paragraphSlices.Count == 0)
            {
                paragraphSlices = BuildParagraphSlicesFromText(_document.Text);
            }

            _document.RunMap.Clear();
            _document.Runs.Clear();

            foreach (var slice in runSlices)
            {
                if (slice.Length <= 0)
                {
                    continue;
                }

                _styleParser.TryGetStyle(slice.StyleId, out var style);
                var descriptor = style ?? baseStyle;

                _document.RunMap.Add(new XmedRunMapEntry(0, 0,
                    (ushort)Math.Clamp(slice.Length, 0, (int)ushort.MaxValue),
                    0,
                    (ushort)Math.Clamp((int)descriptor.StyleId, 0, (int)ushort.MaxValue),
                    slice.Start));

                int available = Math.Min(slice.Length, Math.Max(0, _document.TextLength - slice.Start));
                var textSpan = available > 0 ? _document.Text.AsSpan(slice.Start, available) : ReadOnlySpan<char>.Empty;

                var run = new XmedTextRun
                {
                    Start = slice.Start,
                    Length = available,
                    Text = textSpan.ToString(),
                    FontName = !string.IsNullOrEmpty(descriptor.FontName) ? descriptor.FontName : baseStyle.FontName,
                    FontSize = descriptor.FontSize != 0 ? descriptor.FontSize : baseStyle.FontSize,
                    Bold = descriptor.Bold || baseStyle.Bold,
                    Italic = descriptor.Italic || baseStyle.Italic,
                    Underline = descriptor.Underline || baseStyle.Underline,
                    ForeColor = _styleParser.ResolveColor(descriptor, baseStyle)
                };

                _document.Runs.Add(run);
            }

            if (_document.Runs.Count == 0)
            {
                int available = _document.TextLength;
                if (available > 0)
                {
                    _document.RunMap.Add(new XmedRunMapEntry(0, 0,
                        (ushort)Math.Clamp(available, 0, (int)ushort.MaxValue),
                        0,
                        (ushort)Math.Clamp((int)baseStyle.StyleId, 0, (int)ushort.MaxValue),
                        0));

                    var textSpan = _document.Text.AsSpan(0, available);
                    _document.Runs.Add(new XmedTextRun
                    {
                        Start = 0,
                        Length = available,
                        Text = textSpan.ToString(),
                        FontName = baseStyle.FontName,
                        FontSize = baseStyle.FontSize,
                        Bold = baseStyle.Bold,
                        Italic = baseStyle.Italic,
                        Underline = baseStyle.Underline,
                        ForeColor = _styleParser.ResolveColor(baseStyle, baseStyle)
                    });
                }
            }

            BuildParagraphs(paragraphSlices, baseStyle);
        }

        private void BuildParagraphs(List<ParagraphSlice> paragraphSlices, XmedStyleDescriptor baseStyle)
        {
            var descriptors = _paragraphDescriptors
                .Select(descriptor => descriptor.Clone())
                .ToList();

            if (descriptors.Count < paragraphSlices.Count)
            {
                int missing = paragraphSlices.Count - descriptors.Count;
                descriptors.InsertRange(0, Enumerable.Repeat(new XmedParagraphDescriptor(), missing));
            }
            else if (descriptors.Count > paragraphSlices.Count)
            {
                descriptors = descriptors.Skip(descriptors.Count - paragraphSlices.Count).ToList();
            }

            var paragraphQueue = new Queue<XmedParagraphDescriptor>(descriptors);

            _document.Paragraphs.Clear();

            foreach (var slice in paragraphSlices)
            {
                var paragraph = paragraphQueue.Count > 0
                    ? paragraphQueue.Dequeue()
                    : new XmedParagraphDescriptor();

                paragraph.Start = slice.Start;
                paragraph.Length = Math.Max(0, slice.Length);
                paragraph.Alignment = slice.Flag ? XmedAlignment.Center : XmedAlignment.Left;

                _document.Paragraphs.Add(paragraph);
            }

            if (_paragraphSpacing.Count > 0)
            {
                var spacing = _paragraphSpacing.ToList();
                if (spacing.Count < _document.Paragraphs.Count)
                {
                    int missing = _document.Paragraphs.Count - spacing.Count;
                    spacing.InsertRange(0, Enumerable.Repeat((0, 0), missing));
                }
                else if (spacing.Count > _document.Paragraphs.Count)
                {
                    spacing = spacing.Skip(spacing.Count - _document.Paragraphs.Count).ToList();
                }

                for (int i = 0; i < _document.Paragraphs.Count && i < spacing.Count; i++)
                {
                    var (before, after) = spacing[i];
                    _document.Paragraphs[i].SpacingBefore = before;
                    _document.Paragraphs[i].SpacingAfter = after;
                }
            }
        }

        private List<ParagraphSlice> BuildParagraphSlices(List<(int End, bool Flag)> boundaries, int textLength)
        {
            var slices = new List<ParagraphSlice>();
            int start = 0;

            foreach (var (end, flag) in boundaries)
            {
                int clampedEnd = Math.Clamp(end, 0, textLength);
                if (clampedEnd < start)
                {
                    continue;
                }

                int length = clampedEnd - start;
                if (length > 0)
                {
                    slices.Add(new ParagraphSlice(start, clampedEnd, flag, slices.Count));
                }

                start = clampedEnd;
            }

            if (start < textLength)
            {
                slices.Add(new ParagraphSlice(start, textLength, false, slices.Count));
            }

            return slices;
        }

        private List<ParagraphSlice> BuildParagraphSlicesFromText(string text)
        {
            var spans = ExtractParagraphSpans(text);
            if (spans.Count == 0 && !string.IsNullOrEmpty(text))
            {
                spans.Add((0, text.Length));
            }

            var slices = new List<ParagraphSlice>();
            foreach (var (start, length) in spans)
            {
                if (length <= 0)
                {
                    continue;
                }

                slices.Add(new ParagraphSlice(start, start + length, false, slices.Count));
            }

            return slices;
        }

        private List<TextSlice> BuildRunSlices(List<(int End, int StyleId)> boundaries, int textLength, List<ParagraphSlice> paragraphs)
        {
            var slices = new List<TextSlice>();
            int start = 0;
            int lastStyleId = 0;

            foreach (var (end, styleId) in boundaries)
            {
                int clampedEnd = Math.Clamp(end, 0, textLength);
                if (clampedEnd < start)
                {
                    lastStyleId = styleId;
                    continue;
                }

                int length = clampedEnd - start;
                if (length > 0)
                {
                    int paragraphId = FindParagraphId(paragraphs, start);
                    slices.Add(new TextSlice(start, clampedEnd, styleId, paragraphId));
                    start = clampedEnd;
                }

                lastStyleId = styleId;
            }

            if (start < textLength)
            {
                int paragraphId = FindParagraphId(paragraphs, start);
                slices.Add(new TextSlice(start, textLength, lastStyleId, paragraphId));
            }

            return slices;
        }

        private static int FindParagraphId(List<ParagraphSlice> paragraphs, int start)
        {
            for (int i = 0; i < paragraphs.Count; i++)
            {
                if (start <= paragraphs[i].End)
                {
                    return i;
                }
            }

            return Math.Max(0, paragraphs.Count - 1);
        }

        private static List<(int Start, int Length)> ExtractParagraphSpans(string text)
        {
            var spans = new List<(int Start, int Length)>();
            if (string.IsNullOrEmpty(text))
            {
                return spans;
            }

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
            {
                spans.Add((start, text.Length - start));
            }

            return spans;
        }

        private readonly struct ParagraphSlice
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

        private readonly struct TextSlice
        {
            public TextSlice(int start, int end, int styleId, int paragraphId)
            {
                Start = start;
                End = end;
                StyleId = styleId;
                ParagraphId = paragraphId;
            }

            public int Start { get; }
            public int End { get; }
            public int StyleId { get; }
            public int ParagraphId { get; }
            public int Length => Math.Max(0, End - Start);
        }
    }
}
