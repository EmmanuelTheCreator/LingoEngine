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
            for (int i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                var descriptor = i < _descriptors.Count ? _descriptors[i].Clone() : new XmedParagraphDescriptor();
                descriptor.Start = slice.Start;
                descriptor.Text = slice.Text ?? string.Empty;
                _document.Paragraphs.Add(descriptor);
            }
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

                var spacing = structGroup.GetC2Group(0x03);
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
            var alignmentGroup = structGroup.GetC2Group(0x0F);
            if (alignmentGroup == null)
                return XmedAlignment.Left;

            int raw = alignmentGroup.Items.Count > 2 ? alignmentGroup.ReadNumeric(2) : alignmentGroup.ReadNumeric(0);
            return GetAlignment(raw);
        }

        private static XmedAlignment GetAlignment(int raw)
        {
            return raw switch
            {
                1 => XmedAlignment.Right,
                2 => XmedAlignment.Left,
                3 => XmedAlignment.Justify,
                _ => XmedAlignment.Center
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
            var tabGroup = paragraphGroup.Items.OfType<XmedTokenGroup>()
                .FirstOrDefault(g => g.Type == BlXmedToken.TokenType.TabStops);
            if (tabGroup == null)
                return Enumerable.Empty<(int Position, BlXmedTabAlignment TabAlign)>();

            var stops = new List<(int Position, BlXmedTabAlignment TabAlign)>();

            for (int i = 3; i < tabGroup.Items.Count; i++)
            {
                if (tabGroup.Items[i] is not XmedTokenGroup entry)
                    continue;

                if (i == tabGroup.Items.Count - 1)
                    continue;

                var tabAlign = GetTabAlignment(entry.ReadNumeric(0));
                int position = entry.ReadNumeric(1);
                if (position > 0)
                    stops.Add((position, tabAlign));
            }

            return stops;
        }
    }
}
