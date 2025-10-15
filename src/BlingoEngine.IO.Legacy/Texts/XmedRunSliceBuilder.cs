using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedRunSliceBuilder
    {
        private readonly XmedDocument _document;
        private readonly List<(int End, int StyleId)> _runBoundaries = new();
        private readonly BlXmedTokenStyleParser _styleParser;
        private readonly XmedParagraphDescriptorReader _paraDescriptorReader;
        private readonly XmedParagraphSliceBuilder _paragraphSliceBuilder;

        public XmedRunSliceBuilder(XmedDocument document, BlXmedTokenStyleParser styleParser, XmedParagraphDescriptorReader paraDescriptorReader)
        {
            _document = document;
            _styleParser = styleParser;
            _paraDescriptorReader = paraDescriptorReader;
            _paragraphSliceBuilder = new XmedParagraphSliceBuilder();
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
                    continue;

                if (token.IsPrefixedHex01() && token.TryGetNumericValue(out var styleId))
                {
                    _runBoundaries.Add((pendingEnd.Value, styleId));
                    pendingEnd = null;
                    continue;
                }

                if (token.IsBoolean() && token.TryGetBool(out var boolValue))
                {
                    _runBoundaries.Add((pendingEnd.Value, boolValue ? 1 : 0));
                    pendingEnd = null;
                }
            }
        }

        private List<TextSlice> BuildRunSlices(List<(int End, int StyleId)> boundaries, int textLength)
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
                    int paragraphId = _paragraphSliceBuilder.FindParagraphId(start);
                    slices.Add(new TextSlice(start, clampedEnd, styleId, paragraphId));
                    start = clampedEnd;
                }

                lastStyleId = styleId;
            }

            if (start < textLength)
            {
                int paragraphId = _paragraphSliceBuilder.FindParagraphId(start);
                slices.Add(new TextSlice(start, textLength, lastStyleId, paragraphId));
            }

            return slices;
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
                orderedRunBoundaries.Add((textLength, 0));

            var orderedParagraphBoundaries = _paragraphSliceBuilder.GetOrderedParagraphBoundaries();
            if (orderedParagraphBoundaries.Count == 0)
                orderedParagraphBoundaries.Add((textLength, false));

            var paragraphSlices = _paragraphSliceBuilder.BuildParagraphSlices(orderedParagraphBoundaries, textLength);
            var runSlices = BuildRunSlices(orderedRunBoundaries, textLength);

            if (orderedRunBoundaries.Any(b => b.End > textLength) || runSlices.Sum(slice => slice.Length) < textLength)
            {
                runSlices = new List<TextSlice>
                {
                    new TextSlice(0, textLength, 0, 0)
                };
            }

            if (orderedParagraphBoundaries.Any(b => b.End > textLength) || paragraphSlices.Count == 0)
                paragraphSlices = _paragraphSliceBuilder.BuildParagraphSlicesFromText(_document.Text);

            _document.RunMap.Clear();
            _document.Runs.Clear();

            foreach (var slice in runSlices)
            {
                if (slice.Length <= 0)
                    continue;

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

            _paraDescriptorReader.BuildParagraphs(paragraphSlices, baseStyle);
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
