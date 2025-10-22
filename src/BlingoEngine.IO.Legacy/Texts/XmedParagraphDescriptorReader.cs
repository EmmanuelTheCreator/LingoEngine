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
            _document.Paragraphs.Clear();
            if (slices.Count == 0)
                return;

            int descriptorOffset = 0;
            if (_descriptors.Count > slices.Count)
                descriptorOffset = _descriptors.Count - slices.Count;

            for (int i = 0; i < slices.Count; i++)
            {
                var slice = slices[i];
                int templateIndex = descriptorOffset + i;
                var descriptor = templateIndex >= 0 && templateIndex < _descriptors.Count
                    ? _descriptors[templateIndex].Clone()
                    : new XmedParagraphDescriptor();
                descriptor.Start = slice.Start;
                descriptor.Text = slice.Text ?? string.Empty;
                _document.Paragraphs.Add(descriptor);
            }

            _spacingReader.ApplyParagraphBounds(_document.Paragraphs);
            _spacingReader.ApplyParagraphFormats(_document.Paragraphs);
            _spacingReader.ApplyParagraphSpacing(_document.Paragraphs);
        }



        private XmedParagraphDescriptor ParseDescriptor(XmedTokenGroup paragraphGroup)
        {
            var descriptor = new XmedParagraphDescriptor();

            var tokens = paragraphGroup.Items.OfType<XmedTokenGroup>()
                .FirstOrDefault(g => g.GroupType == XmedTokenGroup.TokenGroupType.ParagraphTokens);

            if (tokens != null)
            {
                descriptor.Alignment = DecodeAlignment(tokens.ReadNumeric(0));
                descriptor.LeftMargin = ReadPositive(tokens.ReadNumeric(3));
                descriptor.RightMargin = ReadPositive(tokens.ReadNumeric(4));
                descriptor.FirstLineIndent = ReadPositive(tokens.ReadNumeric(5));

                int additional = tokens.ReadNumeric(6);
                if (additional > 0)
                    descriptor.AdditionalIndent = ReadPositive(additional);

                descriptor.LineSpacing = ReadPositive(tokens.ReadNumeric(8));
                descriptor.SpacingBefore = ReadPositive(tokens.ReadNumeric(10));
                descriptor.SpacingAfter = ReadPositive(tokens.ReadNumeric(11));
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

        private static XmedAlignment DecodeAlignment(int raw)
        {
            if (raw < 0)
                return XmedAlignment.Right;

            return raw switch
            {
                1 => XmedAlignment.Left,
                2 => XmedAlignment.Right,
                3 => XmedAlignment.Justify,
                _ => XmedAlignment.Left
            };
        }
        private static BlXmedTabAlignment GetTabAlignment(int raw)
        {
            return raw switch
            {
                1 => BlXmedTabAlignment.Left,
                2 => BlXmedTabAlignment.Right,
                3 => BlXmedTabAlignment.Center,
                _ => BlXmedTabAlignment.Decimal
            };
        }


        private static IEnumerable<(int Position, BlXmedTabAlignment TabAlign)> ReadTabStops(XmedTokenGroup paragraphGroup)
        {
            var tabContainer = paragraphGroup.Items.OfType<XmedTokenGroup>()
                .FirstOrDefault(g => g.GroupType == XmedTokenGroup.TokenGroupType.ParagraphTabs);
            if (tabContainer == null)
                return Enumerable.Empty<(int Position, BlXmedTabAlignment TabAlign)>();

            var stops = new List<(int Position, BlXmedTabAlignment TabAlign)>();

            foreach (var tabGroup in tabContainer.Items.OfType<XmedTokenGroup>())
            {
                if (tabGroup.GroupType == XmedTokenGroup.TokenGroupType.TabStopDefault)
                    continue;

                if (tabGroup.Items.Count < 2)
                    continue;

                int position = ReadPositive(tabGroup.ReadNumeric(1));
                if (position <= 0)
                    continue;

                var tabAlign = GetTabAlignment(tabGroup.ReadNumeric(0));
                stops.Add((position, tabAlign));
            }

            return stops;
        }
    }
}
