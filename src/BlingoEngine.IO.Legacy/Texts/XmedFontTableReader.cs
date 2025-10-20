using System.Collections.Generic;
using System.Linq;
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
            string familyName = string.Empty;
            string styleName = string.Empty;

            //if (entry.PreTokens.Count > 0)
            //{
            //    var nameToken = entry.PreTokens.FirstOrDefault(t => t.Type == TokenType.Block00);
            //    if (nameToken != null)
            //        familyName = nameToken.Ascii ?? string.Empty;

            //    var styleToken = entry.PreTokens.SkipWhile(t => t.Type != TokenType.Block00)
            //        .Skip(1)
            //        .FirstOrDefault(t => t.Type == TokenType.Block00);
            //    if (styleToken != null)
            //        styleName = styleToken.Ascii ?? string.Empty;
            //}

            var descriptor = new XmedFontDescriptor
            {
                FamilyName = familyName,
                StyleName = styleName
            };

            //var tokens = entry.EnumerateTokens()
            //    .Where(t => t.Type != TokenType.Block00)
            //    .ToList();

            //int index = 0;

            //descriptor.TableIndex = ReadNext01(tokens, ref index, descriptor.TableIndex);
            //int reserved = ReadNext01(tokens, ref index, 0);
            //descriptor.FontId = ReadRemaining01(tokens, ref index, reserved);

            //descriptor.CodePage = ReadNext02(tokens, ref index, descriptor.CodePage);
            //descriptor.Weight = ReadNext02(tokens, ref index, descriptor.Weight);
            //descriptor.Flags = ReadNext02(tokens, ref index, descriptor.Flags);
            //descriptor.FontKind = ReadNext02(tokens, ref index, descriptor.FontKind);
            //descriptor.CellHeight = ReadNext02(tokens, ref index, descriptor.CellHeight);
            //descriptor.PitchAndFamily = ReadNext02(tokens, ref index, descriptor.PitchAndFamily);
            //descriptor.Reserved = ReadNext02(tokens, ref index, descriptor.Reserved);

            //if (index < tokens.Count && tokens[index].Type == TokenType.C2 && tokens[index].TypeValue == 0x03)
            //    index++;

            //descriptor.ScriptId = ReadNext02(tokens, ref index, descriptor.ScriptId);
            //descriptor.NameIndex = ReadNext01(tokens, ref index, descriptor.NameIndex);

            return descriptor;
        }

        private static int ReadNext01(IReadOnlyList<BlXmedToken> tokens, ref int index, int fallback)
        {
            if (index < tokens.Count && tokens[index].IsPrefixedHex01() && tokens[index].TryGetNumericValue(out var value))
            {
                index++;
                return value;
            }

            return fallback;
        }

        private static int ReadRemaining01(IReadOnlyList<BlXmedToken> tokens, ref int index, int fallback)
        {
            int value = fallback;
            bool found = false;

            while (index < tokens.Count && tokens[index].IsPrefixedHex01())
            {
                if (tokens[index].TryGetNumericValue(out var numeric))
                {
                    value = numeric;
                    found = true;
                }

                index++;
            }

            return found ? value : fallback;
        }

        private static int ReadNext02(IReadOnlyList<BlXmedToken> tokens, ref int index, int fallback)
        {
            if (index < tokens.Count)
            {
                var token = tokens[index];
                if (token.Type == TokenType.C2)
                {
                    index++;
                    return fallback;
                }
                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var value))
                {
                    index++;
                    return value;
                }
            }

            return fallback;
        }
    }
}
