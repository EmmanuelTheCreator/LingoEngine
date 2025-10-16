using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedRunSliceBuilder
    {
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


        public void ReadRuns(BlXmedTokenReader reader)
        {
            var token = reader.Peek();
            if (token == null)
                return;

            bool clearedForRunMap = false;

            LogRunBlockPreview(reader);

            while (!reader.IsAtEnd)
            {
                var t = reader.Peek();
                if (t == null)
                    break;

                if (t.IsPrefixedHex03() && t.Ascii is { Length: >= 4 } ascii && ascii.StartsWith("0004", StringComparison.OrdinalIgnoreCase))
                {
                    if (!clearedForRunMap)
                    {
                        _runBoundaries.Clear();
                        clearedForRunMap = true;
                    }

                    reader.ReadNext();
                    _logger.LogInformation("XMED run reader: begin 03:0004 run map at token {Position}", reader.Position - 1);
                    ReadRunMapEntries(reader);
                    continue;
                }

                if (t.IsBlockBoundary())
                {
                    _logger.LogInformation("XMED run reader encountered boundary token {Token} at position {Position}", t, reader.Position);
                    reader.ReadNext();
                    break;
                }

                if (_paraDescriptorReader.TryExtractParagraphDescriptor(reader, out _))
                    continue;

                reader.ReadNext();
            }
        }

        private void LogRunBlockPreview(BlXmedTokenReader reader)
        {
            var token = reader.Peek();
            if (token == null)
                return;

            if (!token.IsPrefixedHex03())
                return;

            string blockId = token.Ascii is { Length: >= 4 } blockAscii ? blockAscii[..4] : "<null>";
            _logger.LogInformation("XMED run reader inspecting 03 block {BlockId} at token index {Index}", blockId, reader.Position);

            bool headerLogged = false;
            for (int offset = 0; offset < 32; offset++)
            {
                var preview = reader.Peek(offset);
                if (preview == null)
                    break;

                string ascii = preview.Ascii ?? "<null>";
                string value = preview.Value.HasValue ? preview.Value.Value.ToString(CultureInfo.InvariantCulture) : "<null>";
                string typeValue = preview.TypeValue.HasValue ? $"0x{preview.TypeValue.Value:X2}" : "<null>";

                _logger.LogInformation(
                    "XMED run 03:{BlockId} preview[{Offset:D2}]: type {TokenType} ascii {Ascii} value {Value} typeValue {TypeValue}",
                    blockId,
                    offset,
                    preview.Type,
                    ascii,
                    value,
                    typeValue);

                if (!headerLogged)
                {
                    headerLogged = true;
                    continue;
                }

                if (preview.IsPrefixedHex03())
                    break;
            }
        }




        private void ReadRunMapEntries(BlXmedTokenReader reader)
        {
            int? pendingEnd = null;

            while (!reader.IsAtEnd)
            {
                var token = reader.Peek();
                if (token == null)
                    break;

                if (token.IsPrefixedHex03())
                    break;

                if (token.IsBlockBoundary())
                {
                    reader.ReadNext();
                    break;
                }

                if (token.IsFieldSeparator())
                {
                    reader.ReadNext();
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    reader.ReadNext();
                    continue;
                }

                if (_paraDescriptorReader.TryExtractParagraphDescriptor(reader, out _))
                    continue;

                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var end))
                {
                    pendingEnd = end;
                    _logger.LogInformation("XMED run reader: pending run end {End}", end);
                    reader.ReadNext();
                    continue;
                }

                if (token.IsPrefixedHex01() && token.TryGetNumericValue(out var styleId))
                {
                    if (pendingEnd.HasValue)
                    {
                        _runBoundaries.Add((pendingEnd.Value, styleId));
                        _logger.LogInformation("XMED run reader: boundary end {End} style {StyleId}", pendingEnd.Value, styleId);
                        pendingEnd = null;
                    }
                    else
                        _logger.LogInformation("XMED run reader: encountered style {StyleId} without pending end", styleId);

                    reader.ReadNext();
                    continue;
                }

                reader.ReadNext();
            }

            if (pendingEnd.HasValue)
                _logger.LogInformation("XMED run reader: trailing end {End} without style", pendingEnd.Value);
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
                _logger.LogInformation("XMED run boundary normalized: end {End} style {StyleId}", boundary.End, boundary.StyleId);
            if (runBounds.Count == 0) runBounds.Add((textLength, 0));

            var paraBounds = _paragraphSliceBuilder.GetOrderedParagraphBoundaries();
            if (paraBounds.Count == 0) paraBounds.Add((textLength, false));

            var paraSlices = _paragraphSliceBuilder.BuildParagraphSlices(paraBounds, textLength);
            if (paraSlices.Count <= 1 && _document.TextLength > 0 && _document.Text.Contains('\r'))
                paraSlices = _paragraphSliceBuilder.BuildParagraphSlicesFromText(_document.Text);
            var runSlices = BuildRunSlices(runBounds, textLength);
            foreach (var slice in runSlices)
                _logger.LogInformation("XMED run slice computed: start {Start} end {End} style {StyleId}", slice.Start, slice.End, slice.StyleId);

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
            var d = style ?? baseStyle;

            int len = Math.Clamp(slice.Length, 0, Math.Max(0, _document.TextLength - slice.Start));
            _document.RunMap.Add(new XmedRunMapEntry(0, 0, (ushort)Math.Min(len, ushort.MaxValue), 0,
                (ushort)Math.Min((int)d.StyleId, ushort.MaxValue), slice.Start));

            var textSpan = len > 0 ? _document.Text.AsSpan(slice.Start, len) : ReadOnlySpan<char>.Empty;
            var resolvedColor = _styleParser.ResolveColor(d, baseStyle);
            _document.Runs.Add(new XmedTextRun
            {
                Start = slice.Start,
                Length = len,
                Text = textSpan.ToString(),
                FontName = string.IsNullOrEmpty(d.FontName) ? baseStyle.FontName : d.FontName,
                FontSize = d.FontSize != 0 ? d.FontSize : baseStyle.FontSize,
                Bold = d.Bold || baseStyle.Bold,
                Italic = d.Italic || baseStyle.Italic,
                Underline = d.Underline || baseStyle.Underline,
                ForeColor = resolvedColor
            });
            _logger.LogInformation(
                "XMED run slice resolved: start {Start} len {Length} style {StyleId} colorIndex {ColorIndex} resolved {Resolved} baseColorIndex {BaseIndex}",
                slice.Start,
                len,
                d.StyleId,
                d.ColorIndex is { } colorIdx ? $"0x{colorIdx:X2}" : "<null>",
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
