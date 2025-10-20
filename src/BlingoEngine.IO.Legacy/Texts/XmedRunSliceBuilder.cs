using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using static BlingoEngine.IO.Legacy.Texts.XmedDiagnostics;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedRunSliceBuilder
    {
        private const XmedDiagnosticArea DiagnosticArea = XmedDiagnosticArea.RunSliceBuilder;
        private readonly XmedDocument _document;
        private readonly List<(int End, int StyleId)> _runBoundaries = new();
        private readonly BlXmedTokenStyleParser _styleParser;
        private readonly XmedParagraphDescriptorReader _paraDescriptorReader;
        private readonly ILogger _logger;
        private readonly XmedParagraphSliceBuilder _paragraphSliceBuilder;

        public XmedRunSliceBuilder(XmedDocument document, BlXmedTokenStyleParser styleParser, XmedParagraphDescriptorReader paraDescriptorReader, XmedParagraphSliceBuilder paragraphSliceBuilder, ILogger logger)
        {
            _document = document;
            _styleParser = styleParser;
            _paraDescriptorReader = paraDescriptorReader;
            _logger = logger;
            _paragraphSliceBuilder = paragraphSliceBuilder;
        }


        public void Reset()
        {
            _runBoundaries.Clear();
        }

        public void LoadRunBoundaries(XmedTokenGroup? block)
        {
            _runBoundaries.Clear();
            if (block == null)
                return;

            foreach (var item in block.Items)
            {
                if (item is not XmedTokenGroup segment)
                    continue;

                int? end = null;
                int? styleId = null;

                foreach (var token in segment.CollectTokens())
                {
                    if (styleId is null && token.IsPrefixedHex01() && token.TryGetNumericValue(out var style))
                    {
                        styleId = style;
                        continue;
                    }

                    if (end is null && token.IsPrefixedHex02() && token.TryGetNumericValue(out var numeric))
                    {
                        end = numeric;
                        continue;
                    }
                }

                if (end.HasValue)
                {
                    _runBoundaries.Add((end.Value, styleId ?? 0));
                    LogTrace(DiagnosticArea, _logger, "XMED run boundary parsed: end {End} style {StyleId}", end.Value, styleId ?? 0);
                }
            }
        }

        private List<TextSlice> BuildRunSlices(List<(int End, int StyleId)> boundaries, int textLength)
        {
            var slices = new List<TextSlice>();
            if (textLength <= 0) return slices;

            var map = new Dictionary<int, int>();
            foreach (var (end, style) in boundaries)
            {
                if (end <= 0) continue;
                var e = end > textLength ? textLength : end;
                map[e] = style; // last wins per end
            }

            int start = 0, lastStyle = 0;
            foreach (var kv in map.OrderBy(k => k.Key))
            {
                var e = kv.Key; var style = kv.Value;
                if (e <= start) { lastStyle = style; continue; }
                slices.Add(new TextSlice(start, e, style, _paragraphSliceBuilder.FindParagraphId(start)));
                start = e; lastStyle = style;
            }
            if (start < textLength)
                slices.Add(new TextSlice(start, textLength, lastStyle, _paragraphSliceBuilder.FindParagraphId(start)));
            return slices;
        }



        public void FinalizeRunsAndParagraphs()
        {
            var baseStyle = _styleParser.GetOrCreateStyle(0);

            _document.RunMap.Clear();
            _document.Runs.Clear();

            int textLength = _document.TextLength;
            if (textLength <= 0) { _document.Paragraphs.Clear(); return; }

            var runBounds = _runBoundaries.Select(b => (End: Math.Clamp(b.End, 0, textLength), b.StyleId))
                                          .Where(b => b.End > 0)
                                          .OrderBy(b => b.End).ToList();
            foreach (var boundary in runBounds)
                LogTrace(DiagnosticArea, _logger, "XMED run boundary normalized: end {End} style {StyleId}", boundary.End, boundary.StyleId);
            if (runBounds.Count == 0) runBounds.Add((textLength, 0));

            var paraBounds = _paragraphSliceBuilder.GetOrderedParagraphBoundaries();
            if (paraBounds.Count == 0) paraBounds.Add((textLength, false));

            var paraSlices = _paragraphSliceBuilder.BuildParagraphSlices(paraBounds, textLength);
            if (paraSlices.Count <= 1 && _document.TextLength > 0 && _document.Text.Contains('\r'))
                paraSlices = _paragraphSliceBuilder.BuildParagraphSlicesFromText(_document.Text);

            foreach (var styleId in _runBoundaries.Select(b => b.StyleId).Where(id => id >= 0).Distinct())
                _styleParser.GetOrCreateStyle(styleId);

            var runSlices = BuildRunSlices(runBounds, textLength);
            foreach (var slice in runSlices)
                LogTrace(DiagnosticArea, _logger, "XMED run slice computed: start {Start} end {End} style {StyleId}", slice.Start, slice.End, slice.StyleId);

            if (runSlices.Sum(s => s.Length) != textLength)
                runSlices = new List<TextSlice> { new TextSlice(0, textLength, 0, 0) };

            foreach (var s in runSlices) ReadSlice(baseStyle, s);
            if (_document.Runs.Count == 0) CreateRun(baseStyle, textLength);

            _paraDescriptorReader.BuildParagraphs(paraSlices, baseStyle);
        }

        // merge decorations (OR), inherit name/size.
        private bool ReadSlice(XmedStyleDescriptor baseStyle, TextSlice slice)
        {
            if (slice.Length <= 0) return false;

            _styleParser.TryGetStyle(slice.StyleId, out var style);
            XmedStyleDescriptor descriptor;
            if (style is null)
            {
                descriptor = _styleParser.GetOrCreateStyle(slice.StyleId);
                baseStyle.ApplyStyleInheritanceToChild(descriptor);
                LogTrace(DiagnosticArea, _logger, "XMED run slice: created stub style {StyleId} from base", slice.StyleId);
            }
            else
                descriptor = style;

            int len = Math.Clamp(slice.Length, 0, Math.Max(0, _document.TextLength - slice.Start));
            _document.RunMap.Add(new XmedRunMapEntry(0, 0, (ushort)Math.Min(len, ushort.MaxValue), 0,
                (ushort)Math.Min(slice.StyleId, ushort.MaxValue), slice.Start));

            var textSpan = len > 0 ? _document.Text.AsSpan(slice.Start, len) : ReadOnlySpan<char>.Empty;
            var resolvedColor = _styleParser.ResolveColor(descriptor, baseStyle);
            _document.Runs.Add(new XmedTextRun
            {
                Start = slice.Start,
                Length = len,
                Text = textSpan.ToString(),
                FontName = string.IsNullOrEmpty(descriptor.FontName) ? baseStyle.FontName : descriptor.FontName,
                FontSize = descriptor.FontSize != 0 ? descriptor.FontSize : baseStyle.FontSize,
                Bold = descriptor.Bold || baseStyle.Bold,
                Italic = descriptor.Italic || baseStyle.Italic,
                Underline = descriptor.Underline || baseStyle.Underline,
                ForeColor = resolvedColor
            });
            LogTrace(
                DiagnosticArea,
                _logger,
                "XMED run slice resolved: start {Start} len {Length} style {StyleId} colorIndex {ColorIndex} resolved {Resolved} baseColorIndex {BaseIndex}",
                slice.Start,
                len,
                descriptor.StyleId,
                descriptor.ColorIndex is { } colorIdx ? $"0x{colorIdx:X2}" : "<null>",
                resolvedColor.ToHex(),
                baseStyle.ColorIndex is { } baseIdx ? $"0x{baseIdx:X2}" : "<null>");
            return true;
        }


        private void CreateRun(XmedStyleDescriptor baseStyle, int available)
        {
            int len = Math.Clamp(available, 0, _document.TextLength);
            _document.RunMap.Add(new XmedRunMapEntry(0, 0, (ushort)Math.Min(len, ushort.MaxValue),
                                                     0, (ushort)Math.Min((int)baseStyle.StyleId, ushort.MaxValue), 0));

            var textSpan = _document.Text.AsSpan(0, len);
            _document.Runs.Add(new XmedTextRun
            {
                Start = 0,
                Length = len,
                Text = textSpan.ToString(),
                FontName = baseStyle.FontName,
                FontSize = baseStyle.FontSize,
                Bold = baseStyle.Bold,
                Italic = baseStyle.Italic,
                Underline = baseStyle.Underline,
                ForeColor = _styleParser.ResolveColor(baseStyle, baseStyle)
            });
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
