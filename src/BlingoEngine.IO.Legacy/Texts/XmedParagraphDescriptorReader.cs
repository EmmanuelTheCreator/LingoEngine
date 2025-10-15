using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using static BlingoEngine.IO.Legacy.Texts.XmedParagraphSliceBuilder;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedParagraphDescriptorReader
    {
        private readonly List<XmedParagraphDescriptor> _paragraphDescriptors = new();
        private readonly XmedDocument _document;
        private readonly BlXmedTokenStyleParser _styleParser;
        private readonly XmedSpacingReader _spacingReader;
        private readonly ILogger _logger;

        public XmedParagraphDescriptorReader(XmedDocument document, BlXmedTokenStyleParser styleParser, XmedSpacingReader spacingReader, ILogger logger)
        {
            _document = document;
            _styleParser = styleParser;
            _spacingReader = spacingReader;
            _logger = logger;
        }

        public void Reset()
        {
            _paragraphDescriptors.Clear();
        }

        public bool TryExtractParagraphDescriptor(BlXmedTokenReader reader, out XmedParagraphDescriptor? descriptor)
        {
            descriptor = null;
            if (!reader.Peek()?.IsCompositeC1(0x03) ?? true) return false;

            reader.ReadNext(); // consume C1(03)
            var values = new List<int>();
            var tabStops = new List<int>();
            int depth = 0, fieldIndex = 0;

            while (!reader.IsAtEnd)
            {
                var t = reader.Peek();
                if (t is null) break;

                if (t.IsPrefixedHex02() && t.TryGetNumericValue(out var n))
                {
                    if (depth == 0 && fieldIndex < 4) values.Add(n);
                    else if (depth == 0) tabStops.Add(n);
                    reader.ReadNext(); continue;
                }

                if (t.IsFieldSeparator()) { fieldIndex++; reader.ReadNext(); continue; }
                if (t.IsFieldTerminator()) { reader.ReadNext(); if (depth == 0) break; depth--; continue; }
                if (t.IsCompositeC1(0x03)) { depth++; reader.ReadNext(); continue; }
                if (t.IsCompositeC2(0x03)) { _spacingReader.ReadParagraphSpacing(reader); continue; }
                if (t.IsBlockBoundary()) { LogUnknown("Paragraph", "<block-boundary>"); break; }

                LogUnknown("Paragraph", t.ToString());
                reader.ReadNext();
            }

            bool InRange(int v) => v >= -512 && v <= 0x2000;
            if (values.Count >= 3 && InRange(values[0]) && InRange(values[2]))
            {
                
                descriptor = new XmedParagraphDescriptor
                {
                    LeftMargin = Norm(values.ElementAtOrDefault(0)),
                    RightMargin = Norm(values.ElementAtOrDefault(1)),
                    FirstLineIndent = Norm(values.ElementAtOrDefault(2)),
                    AdditionalIndent = values.Count > 3 && InRange(values[3]) ? Norm(values[3]) : null
                };
                if (tabStops.Count > 0) descriptor.TabStops.AddRange(tabStops.Where(InRange).Select(Norm));
                return true;
            }
            return false;
        }

        private static int Norm(int v) => v < 0 ? 0 : Math.Min(v, 0x2000);

        public void BuildParagraphs(List<ParagraphSlice> paragraphSlices, XmedStyleDescriptor baseStyle)
        {
            var descriptors = _paragraphDescriptors
                .Select(descriptor => descriptor.Clone())
                .ToList();

            if (!EnsureAndAddMissingDescriptors(descriptors))
            {
                if (descriptors.Count > paragraphSlices.Count)
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

            _spacingReader.InjectSpacings();
        }
        private bool EnsureAndAddMissingDescriptors(List<XmedParagraphDescriptor> descriptors)
        {
            if (_paragraphDescriptors.Count < descriptors.Count)
            {
                int missing = descriptors.Count - _paragraphDescriptors.Count;
                for (int i = 0; i < missing; i++)
                    _paragraphDescriptors.Insert(0, new XmedParagraphDescriptor());
                return true;
            }
            return false;
        }
        private void LogUnknown(string category, string token)
        {
            _logger.LogDebug("XMED: {Category} unknown paragraph token {Token}", category, token);
        }


        #region OLD
        //public void CollectParagraphDescriptorsFromTokens(BlXmedTokenReader reader)
        //{
        //    if (reader.IsAtEnd)
        //        return;

        //    _spacingReader.Reset();

        //    var descriptors = ReadDescriptors(reader);

        //    if (descriptors.Count == 0)
        //        return;

        //    EnsureAndAddMissingDescriptors(descriptors);

        //    for (int i = 0; i < descriptors.Count; i++)
        //    {
        //        int targetIndex = _paragraphDescriptors.Count - descriptors.Count + i;
        //        if (targetIndex < 0 || targetIndex >= _paragraphDescriptors.Count)
        //            continue;

        //        var target = _paragraphDescriptors[targetIndex];
        //        var source = descriptors[i];
        //        target.ParseValuesFrom(source);
        //    }
        //}

        //private List<XmedParagraphDescriptor> ReadDescriptors(BlXmedTokenReader reader)
        //{
        //    var descriptors = new List<XmedParagraphDescriptor>();
        //    while (!reader.IsAtEnd)
        //    {
        //        var token = reader.ReadNext();
        //        if (token == null) break;
        //        if (token.IsCompositeC1(0x03))
        //        {
        //            if (TryExtractParagraphDescriptor(reader, out var descriptor))
        //                descriptors.Add(descriptor!);
        //        }
        //    }

        //    return descriptors;
        //}

        //private void ParseParagraphBlock(BlXmedTokenReader reader, int depth)
        //{
        //    var values = new List<int>();
        //    var tabStops = new List<int>();
        //    int fieldIndex = 0;

        //    while (!reader.IsAtEnd)
        //    {
        //        var token = reader.Peek();
        //        if (token is null)
        //            break;

        //        if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var numeric))
        //        {
        //            if (fieldIndex < 4)
        //                values.Add(numeric);
        //            else
        //                tabStops.Add(numeric);

        //            reader.Skip();
        //            continue;
        //        }

        //        if (token.IsFieldSeparator())
        //        {
        //            fieldIndex++;
        //            reader.Skip();
        //            continue;
        //        }

        //        if (token.IsFieldTerminator())
        //        {
        //            reader.Skip();
        //            FinalizeParagraphDescriptor(values, tabStops);
        //            return;
        //        }

        //        if (token.IsC1())
        //        {
        //            // old TODO
        //            //_styleParser.TrackStyleMarker(token);
        //            var styleIndex = ????
        //            reader.Skip();

        //            if (token.TypeValue == 0x03)
        //            {
        //                ParseParagraphBlock(reader, depth + 1);
        //                continue;
        //            }

        //            if (token.TypeValue == 0x1C)
        //            {
        //                _styleParser.MarkStyleFlag(styleIndex, style =>
        //                {
        //                    style.Underline = true;
        //                    style.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Underline,true);
        //                });
        //            }
        //            else if (token.TypeValue == 0x1D)
        //            {
        //                _styleParser.MarkStyleFlag(styleIndex, style =>
        //                {
        //                    style.Italic = true;
        //                    style.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Italic, true);
        //                });
        //            }

        //            continue;
        //        }

        //        if (token.IsC2())
        //        {
        //            if (token.TypeValue == 0x03)
        //            {
        //                _spacingReader.ReadParagraphSpacing(reader);
        //                continue;
        //            }

        //            reader.Skip();
        //            continue;
        //        }

        //        if (token.IsBlockBoundary())
        //        {
        //            if (depth == 0)
        //            {
        //                FinalizeParagraphDescriptor(values, tabStops);
        //            }
        //            return;
        //        }

        //        reader.Skip();
        //    }

        //    FinalizeParagraphDescriptor(values, tabStops);
        //} 
        //private void FinalizeParagraphDescriptor(List<int> values, List<int> tabStops)
        //{
        //    if (values.Count == 0)
        //    {
        //        values.Clear();
        //        tabStops.Clear();
        //        return;
        //    }

        //    bool IsWithinRange(int value) => value >= -512 && value <= 0x2000;

        //    int leftRaw = values.ElementAtOrDefault(0);
        //    int rightRaw = values.ElementAtOrDefault(1);
        //    int firstLineRaw = values.ElementAtOrDefault(2);

        //    if (!IsWithinRange(leftRaw) || !IsWithinRange(rightRaw) || !IsWithinRange(firstLineRaw))
        //    {
        //        values.Clear();
        //        tabStops.Clear();
        //        return;
        //    }

        //    static int Normalize(int value) => value < 0 ? 0 : Math.Min(value, 0x2000);

        //    var descriptor = new XmedParagraphDescriptor
        //    {
        //        LeftMargin = Normalize(leftRaw),
        //        RightMargin = Normalize(rightRaw),
        //        FirstLineIndent = Normalize(firstLineRaw),
        //        AdditionalIndent = values.Count > 3 && IsWithinRange(values[3])
        //            ? Normalize(values[3])
        //            : null
        //    };

        //    if (tabStops.Count > 0)
        //    {
        //        foreach (int stop in tabStops.Where(IsWithinRange))
        //            descriptor.TabStops.Add(Normalize(stop));
        //    }

        //    _paragraphDescriptors.Add(descriptor);

        //    values.Clear();
        //    tabStops.Clear();
        //}
        #endregion
    }
    internal static class ParagraphExtensions
    {

        public static void FillParagraphTexts(this XmedDocument document)
        {
            if (document.Paragraphs.Count == 0) return;
            foreach (var p in document.Paragraphs)
                p.Text = GetParagraphText(document.Text, p);
        }
        private static string GetParagraphText(string text, XmedParagraphDescriptor paragraph)
        {
            if (string.IsNullOrEmpty(text) || paragraph.Length <= 0 || paragraph.Start < 0)
                return string.Empty;

            if (paragraph.Start >= text.Length)
                return string.Empty;

            int length = Math.Min(paragraph.Length, text.Length - paragraph.Start);
            return length > 0 ? text.Substring(paragraph.Start, length) : string.Empty;
        }

    }

}
