using BlingoEngine.IO.Legacy.Texts.Data;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

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

            var tailItems = entry.Items.Skip(2);
            var flattened = EnumerateTokens(tailItems).ToList();
            var numericTokens = flattened
                .Where(t => t.Type == TokenType.PrefixedHex)
                .ToList();

            int scriptId = 0;
            int nameIndex = 0;
            if (numericTokens.Count >= 2)
            {
                scriptId = TryReadNumeric(numericTokens[^2]);
                nameIndex = TryReadNumeric(numericTokens[^1]);
            }

            var coreTokens = numericTokens.Take(Math.Max(0, numericTokens.Count - 2)).ToList();

            var descriptor = new XmedFontDescriptor
            {
                FamilyName = familyName,
                StyleName = styleName,
                TableIndex = ReadNumeric(coreTokens, 0),
                FontId = ReadNumeric(coreTokens, 3),
                CodePage = ReadNumeric(coreTokens, 4),
                Weight = ReadNumeric(coreTokens, 5),
                Flags = ReadNumeric(coreTokens, 6),
                FontKind = ReadNumeric(coreTokens, 7),
                CellHeight = ReadNumeric(coreTokens, 8),
                PitchAndFamily = ReadNumeric(coreTokens, 9),
                Reserved = ReadNumeric(coreTokens, 10),
                ScriptId = scriptId,
                NameIndex = nameIndex
            };

            if (descriptor.PitchAndFamily == 0 && descriptor.FontKind > 0)
                descriptor.PitchAndFamily = (descriptor.FontKind << 18) | 0x8;

            return descriptor;
        }

        private static IEnumerable<BlXmedToken> EnumerateTokens(IEnumerable<BlXmedToken> items)
        {
            foreach (var item in items)
            {
                if (item is XmedTokenGroup group)
                {
                    foreach (var nested in EnumerateTokens(group.Items))
                        yield return nested;
                    continue;
                }

                yield return item;
            }
        }

        private static int ReadNumeric(IReadOnlyList<BlXmedToken> tokens, int index)
        {
            if (index < 0 || index >= tokens.Count)
                return 0;

            return TryReadNumeric(tokens[index]);
        }

        private static int TryReadNumeric(BlXmedToken token)
        {
            return token.TryGetNumericValue(out var numeric) ? numeric : 0;
        }
    }
}
