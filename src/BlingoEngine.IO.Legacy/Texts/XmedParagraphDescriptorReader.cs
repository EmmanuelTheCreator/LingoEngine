using System.Collections.Generic;
using System.Linq;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedParagraphDescriptorReader
    {
        private readonly XmedDocument _document;
        private readonly XmedSpacingReader _spacingReader;
        private readonly List<XmedParagraphDescriptor> _descriptors = new();
        private readonly List<XmedSliceBuilder.Slice> _slices = new();

        public XmedParagraphDescriptorReader(XmedDocument document, XmedSpacingReader spacingReader, ILogger logger)
        {
            _document = document;
            _spacingReader = spacingReader;
            _ = logger;
        }

        public void Reset()
        {
            _descriptors.Clear();
            _slices.Clear();
        }

        public void LoadParagraphDescriptors(XmedTokenGroup? block)
        {
            _descriptors.Clear();
            _spacingReader.Reset();

            if (block == null)
                return;

            foreach (var paragraphGroup in block.Items.OfType<XmedTokenGroup>())
                _descriptors.Add(ParseDescriptor(paragraphGroup));
        }

        public void ApplyParagraphRuns(IReadOnlyList<XmedSliceBuilder.Slice> slices)
        {
            _slices.Clear();

            if (slices != null)
            {
                foreach (var slice in slices)
                {
                    if (slice.Length > 0)
                        _slices.Add(slice);
                }
            }

            if (_slices.Count == 0 && !string.IsNullOrEmpty(_document.Text))
                _slices.AddRange(CreateTextSlices(_document.Text));
        }

        public void BuildParagraphs()
        {
            if (_slices.Count == 0 && !string.IsNullOrEmpty(_document.Text))
                _slices.AddRange(CreateTextSlices(_document.Text));

            _document.Paragraphs.Clear();

            for (int i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                var descriptor = i < _descriptors.Count ? CloneDescriptor(_descriptors[i]) : new XmedParagraphDescriptor();
                descriptor.Start = slice.Start;
                descriptor.Length = slice.Length;
                descriptor.Text = slice.Text ?? string.Empty;
                _document.Paragraphs.Add(descriptor);
            }
        }

        private static IEnumerable<XmedSliceBuilder.Slice> CreateTextSlices(string text)
        {
            var slices = new List<XmedSliceBuilder.Slice>();
            int start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '\r')
                    continue;

                if (i > start)
                {
                    string segment = text.Substring(start, i - start);
                    slices.Add(new XmedSliceBuilder.Slice(start, i, 0, segment));
                }

                start = i + 1;
            }

            if (start < text.Length)
            {
                string trailing = text.Substring(start);
                slices.Add(new XmedSliceBuilder.Slice(start, text.Length, 0, trailing));
            }

            if (slices.Count == 0 && text.Length > 0)
                slices.Add(new XmedSliceBuilder.Slice(0, text.Length, 0, text));

            return slices;
        }

        private static XmedParagraphDescriptor CloneDescriptor(XmedParagraphDescriptor source)
        {
            var descriptor = new XmedParagraphDescriptor
            {
                LeftMargin = source.LeftMargin,
                RightMargin = source.RightMargin,
                FirstLineIndent = source.FirstLineIndent,
                AdditionalIndent = source.AdditionalIndent,
                SpacingBefore = source.SpacingBefore,
                SpacingAfter = source.SpacingAfter,
                LineSpacing = source.LineSpacing,
                Alignment = source.Alignment
            };

            if (source.TabStops.Count > 0)
                descriptor.TabStops.AddRange(source.TabStops);

            return descriptor;
        }

        private XmedParagraphDescriptor ParseDescriptor(XmedTokenGroup paragraphGroup)
        {
            var descriptor = new XmedParagraphDescriptor();

            var structGroup = paragraphGroup.Items.OfType<XmedTokenGroup>()
                .FirstOrDefault(g => g.Type == BlXmedToken.TokenType.B_82);

            if (structGroup != null)
            {
                descriptor.Alignment = ReadAlignment(structGroup);
                descriptor.LeftMargin = ReadPositive(structGroup.ReadNumeric(3));
                descriptor.RightMargin = ReadPositive(structGroup.ReadNumeric(4));
                descriptor.FirstLineIndent = ReadPositive(structGroup.ReadNumeric(5));

                int additional = structGroup.ReadNumeric(6);
                if (additional != 0)
                    descriptor.AdditionalIndent = ReadPositive(additional);

                var spacing = FindC2(structGroup, 0x03);
                if (spacing != null)
                {
                    descriptor.SpacingBefore = ReadPositive(spacing.ReadNumeric(0));
                    descriptor.SpacingAfter = ReadPositive(spacing.ReadNumeric(1));
                }
            }

            foreach (var stop in ReadTabStops(paragraphGroup))
                descriptor.TabStops.Add(stop);

            return descriptor;
        }

        private static int ReadPositive(int value)
        {
            if (value < 0)
                return 0;
            return value;
        }

        private static XmedAlignment ReadAlignment(XmedTokenGroup structGroup)
        {
            var alignmentGroup = FindC2(structGroup, 0x0F);
            if (alignmentGroup == null)
                return XmedAlignment.Left;

            int raw = alignmentGroup.Items.Count > 2 ? alignmentGroup.ReadNumeric(2) : alignmentGroup.ReadNumeric(0);
            return raw switch
            {
                1 => XmedAlignment.Right,
                2 => XmedAlignment.Left,
                3 => XmedAlignment.Justify,
                _ => XmedAlignment.Center
            };
        }

        private static XmedC2TokenGroup? FindC2(XmedTokenGroup parent, int typeValue)
        {
            foreach (var c2 in parent.Items.OfType<XmedC2TokenGroup>())
            {
                if (c2.TypeValue == typeValue)
                    return c2;
            }

            return null;
        }

        private static IEnumerable<int> ReadTabStops(XmedTokenGroup paragraphGroup)
        {
            var tabGroup = paragraphGroup.Items.OfType<XmedTokenGroup>()
                .FirstOrDefault(g => g.Type == BlXmedToken.TokenType.TabStops);
            if (tabGroup == null)
                return Enumerable.Empty<int>();

            var stops = new List<int>();

            for (int i = 3; i < tabGroup.Items.Count; i++)
            {
                if (tabGroup.Items[i] is not XmedTokenGroup entry)
                    continue;

                if (i == tabGroup.Items.Count - 1)
                    continue;

                int position = entry.ReadNumeric(1);
                if (position > 0)
                    stops.Add(position);
            }

            return stops;
        }
    }
}
