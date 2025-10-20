using BlingoEngine.IO.Legacy.Texts.Data;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedParagraphDescriptorReader
    {
        private readonly List<XmedParagraphDescriptor> _paragraphDescriptors = new();
        private readonly XmedDocument _document;
        private readonly XmedSpacingReader _spacingReader;
        private readonly ILogger _logger;

        public XmedParagraphDescriptorReader(XmedDocument document, XmedSpacingReader spacingReader, ILogger logger)
        {
            _document = document;
            _spacingReader = spacingReader;
            _logger = logger;
        }

        public void Reset()
        {
            _paragraphDescriptors.Clear();
        }

        public void LoadParagraphDescriptors(XmedTokenGroup? block)
        {
            _paragraphDescriptors.Clear();
            _spacingReader.Reset();

            if (block == null)
            {
                _logger.LogDebug("XMED: paragraph descriptor block missing");
                return;
            }

            foreach (var item in block.Items)
            {
                if (item is not XmedTokenGroup segment)
                    continue;

                var descriptor = new XmedParagraphDescriptor();
                ParseMargins(segment, descriptor);
                ParseSpacing(segment, descriptor);
                ParseTabStops(segment, descriptor);
                _paragraphDescriptors.Add(descriptor);
            }
        }

        private void ParseMargins(XmedTokenGroup group, XmedParagraphDescriptor descriptor)
        {
            //var numericValues = new List<int>();
            //foreach (var token in group.CollectTokens())
            //{
            //    if (!token.IsPrefixedHex02())
            //        continue;

            //    if (token.TryGetNumericValue(out var numeric))
            //        numericValues.Add(numeric);
            //}

            //if (numericValues.Count > 0)
            //    descriptor.LeftMargin = NormalizeMargin(numericValues.ElementAtOrDefault(0));
            //if (numericValues.Count > 1)
            //    descriptor.RightMargin = NormalizeMargin(numericValues.ElementAtOrDefault(1));
            //if (numericValues.Count > 2)
            //    descriptor.FirstLineIndent = NormalizeMargin(numericValues.ElementAtOrDefault(2));
            //if (numericValues.Count > 3)
            //    descriptor.AdditionalIndent = NormalizeMargin(numericValues.ElementAtOrDefault(3));
        }

        private void ParseSpacing(XmedTokenGroup group, XmedParagraphDescriptor descriptor)
        {
            //foreach (var c2 in group.EnumerateC2Groups().Where(g => g.TypeValue == 0x03))
            //{
            //    int before = c2.ReadNumericAt(0);
            //    int after = c2.ReadNumericAt(1);

            //    if (before >= 0)
            //        descriptor.SpacingBefore = before;
            //    if (after >= 0)
            //        descriptor.SpacingAfter = after;
            //}
        }

        private void ParseTabStops(XmedTokenGroup group, XmedParagraphDescriptor descriptor)
        {
            //foreach (var c2 in group.EnumerateC2Groups().Where(g => g.TypeValue == 0x06))
            //{
            //    var tokens = c2.Items.OfType<BlXmedToken>().ToList();
            //    for (int i = 0; i < tokens.Count; i++)
            //    {
            //        var candidate = tokens[i];
            //        if (!candidate.IsPrefixedHex02())
            //            continue;

            //        if (!candidate.TryGetNumericValue(out var numeric))
            //            continue;

            //        if (numeric <= 0)
            //            continue;

            //        descriptor.TabStops.Add(NormalizeMargin(numeric));
            //    }
            //}
        }

        private static int NormalizeMargin(int value)
        {
            if (value < 0)
                return 0;
            return Math.Min(value, 0x2000);
        }

        public void BuildParagraphs(List<XmedParagraphSliceBuilder.ParagraphSlice> paragraphSlices, XmedStyleDescriptor baseStyle)
        {
            var descriptors = _paragraphDescriptors.Select(descriptor => descriptor.Clone()).ToList();
            AlignDescriptorsToParagraphs(descriptors, paragraphSlices.Count);

            var queue = new Queue<XmedParagraphDescriptor>(descriptors);

            _document.Paragraphs.Clear();

            foreach (var slice in paragraphSlices)
            {
                var descriptor = queue.Count > 0 ? queue.Dequeue() : new XmedParagraphDescriptor();
                descriptor.Start = slice.Start;
                descriptor.Length = Math.Max(0, slice.Length);
                descriptor.Alignment = slice.Flag ? XmedAlignment.Center : XmedAlignment.Left;
                descriptor.Text = ExtractParagraphText(slice);
                _document.Paragraphs.Add(descriptor);
            }

            _spacingReader.InjectSpacings();
        }

        private string ExtractParagraphText(XmedParagraphSliceBuilder.ParagraphSlice slice)
        {
            if (_document.TextLength == 0 || string.IsNullOrEmpty(_document.Text))
                return string.Empty;

            int length = Math.Clamp(slice.Length, 0, Math.Max(0, _document.TextLength - slice.Start));
            if (length <= 0)
                return string.Empty;

            return _document.Text.Substring(slice.Start, length);
        }

        private static void AlignDescriptorsToParagraphs(List<XmedParagraphDescriptor> descriptors, int paragraphCount)
        {
            if (paragraphCount <= 0)
                return;

            if (descriptors.Count < paragraphCount)
            {
                int missing = paragraphCount - descriptors.Count;
                for (int i = 0; i < missing; i++)
                    descriptors.Insert(0, new XmedParagraphDescriptor());
                return;
            }

            if (descriptors.Count > paragraphCount)
            {
                int excess = descriptors.Count - paragraphCount;
                descriptors.RemoveRange(0, excess);
            }
        }
    }
}
