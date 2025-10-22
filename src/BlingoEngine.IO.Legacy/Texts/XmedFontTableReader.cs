using BlingoEngine.IO.Legacy.Texts.Data;
namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedFontTableReader
    {
        private readonly XmedDocument _document;
        public XmedFontTableReader(XmedDocument document)
        {
            _document = document;
        }

        public void Reset() => _document.Fonts.Clear();

        public void ReadFontTable(XmedTokenGroup? block)
        {
            _document.Fonts.Clear();

            if (block == null)
                return;

            foreach (var item in block.Items)
            {
                if (item is not XmedTokenGroup entry)
                    continue;

                var descriptor = ParseFontDescriptor(entry);
                _document.Fonts.Add(descriptor);
            }
        }

        private XmedFontDescriptor ParseFontDescriptor(XmedTokenGroup entry)
        {
            string familyName = entry.ReadAscii(0, token => token.IsFontTable00());
            string styleName = entry.ReadAscii(1, token => token.IsFontTable00());

            var descriptor = new XmedFontDescriptor
            {
                FamilyName = familyName,
                StyleName = styleName,
                TableIndex = entry.ReadNumeric(2),
                FontId = entry.ReadNumeric(5),
                CodePage = entry.ReadNumeric(6),
                Weight = entry.ReadNumeric(7),
                Flags = entry.ReadNumeric(8),
                FontKind = entry.ReadNumeric(11),
                CellHeight = entry.ReadNumeric(12),
                PitchAndFamily = entry.ReadNumeric(14),
                Reserved = entry.ReadNumeric(15),
                ScriptId = entry.ReadNumeric(17),
                NameIndex = entry.ReadNumeric(18)
            };

            if (descriptor.PitchAndFamily == 0 && descriptor.FontKind > 0)
                descriptor.PitchAndFamily = (descriptor.FontKind << 18) | 0x8;

            return descriptor;
        }
    }
}
