using BlingoEngine.IO.Legacy.Texts.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedTokenGrouper
    {
        private int _readIndex = 0;
        private List<BlXmedToken> _tokens = new List<BlXmedToken>();

        public List<XmedMainTokenGroup> CreateGroups(List<BlXmedToken> compressedTokens)
        {
            if (compressedTokens == null)
                throw new ArgumentNullException(nameof(compressedTokens));

            if (compressedTokens.Count == 0)
                return new List<XmedMainTokenGroup>();

            var expanded = ExpandPadding(compressedTokens);
            _tokens = ExpandRepeats(expanded);

            List<XmedMainTokenGroup> groups = CreateMainGroups();
            //firstGroup.RawTokens.AddRange(payload);
            foreach (var group in groups)
                BuildMainGroup(group);

            return groups;
        }

        #region Main blocks
        private List<XmedMainTokenGroup> CreateMainGroups()
        {
            var groups = new List<XmedMainTokenGroup>();
            XmedMainTokenGroup? mainGroup = ReadMainBlockToken(ReadNext(), true);
            if (mainGroup == null) return groups;
            groups.Add(mainGroup);
            while (_readIndex < _tokens.Count)
            {
                var token = ReadNext();
                if (token.Type == TokenType.PrefixedHex && token.Length >= 12)
                {
                    mainGroup = ReadMainBlockToken(token, true);
                    if (mainGroup == null) break;
                    groups.Add(mainGroup);
                }
                else
                    mainGroup.RawTokens.Add(token);
            }

            return groups;
        }


        private static XmedMainTokenGroup? ReadMainBlockToken(BlXmedToken token, bool isFirst = false)
        {
            if ((token.Type != TokenType.PrefixedHex && token.Type != TokenType.Ascii) || (!isFirst && token.TypeValue != 0x03) || string.IsNullOrEmpty(token.Ascii))
                return null;

            if (token.Ascii.Length < 20)
                return null;

            var blockId = token.Ascii[..4];
            var tokenCount = ParseHex(token.Ascii[4..12]);
            var anId = ParseHex(token.Ascii[12..16]);
            var itemCount = ParseHex(token.Ascii[16..20]);
            var block = new XmedMainTokenGroup(token, blockId, tokenCount, itemCount);
            block.UnknownValue2 = anId;
            return block;
        }
        private static void BuildMainGroup(XmedMainTokenGroup group)
        {
            group.PreTokens.Clear();
            group.Items.Clear();
            group.PostTokens.Clear();

            switch (group.MainType)
            {
                case XmedMainTokenGroup.MainGroupType.RunHeaderFFFF:
                    group.Items.AddRange(group.RawTokens);  
                    break;
                case XmedMainTokenGroup.MainGroupType.RunHeader:
                case XmedMainTokenGroup.MainGroupType.Layout:
                    group.Items.AddRange(group.RawTokens);
                    break;
                case XmedMainTokenGroup.MainGroupType.FullText:
                    BuildFullTextGroup(group);
                    break;
                case XmedMainTokenGroup.MainGroupType.RunStyles:
                case XmedMainTokenGroup.MainGroupType.RunParagraphs:
                    BuildRunPairGroup(group);
                    break;
                case XmedMainTokenGroup.MainGroupType.Styles:
                    BuildStyleGroup(group);
                    break;
                case XmedMainTokenGroup.MainGroupType.Paragraphs:
                    BuildParagraphGroup(group);
                    break;
                case XmedMainTokenGroup.MainGroupType.Fonts:
                    BuildFontGroup(group);
                    break;
                case XmedMainTokenGroup.MainGroupType.ParagraphBounds:
                case XmedMainTokenGroup.MainGroupType.ParagraphBounds2:
                case XmedMainTokenGroup.MainGroupType.UnknownB:
                case XmedMainTokenGroup.MainGroupType.ParagraphFormats:
                case XmedMainTokenGroup.MainGroupType.ParagraphSpacing:
                case XmedMainTokenGroup.MainGroupType.Unknown13:
                case XmedMainTokenGroup.MainGroupType.Unknown128:
                case XmedMainTokenGroup.MainGroupType.Unknown129:
                    group.Items.AddRange(group.RawTokens);
                    break;
                default:
                    break;
            }

        }

        private static void BuildFullTextGroup(XmedMainTokenGroup group)
        {
            if (group.RawTokens.Count == 0)
                return;

            foreach (var token in group.RawTokens)
            {
                if (token is XmedTokenGroup nested)
                {
                    group.Items.Add(nested);
                    continue;
                }

                if (token.Type == TokenType.Block00 || token.Type == TokenType.Ascii)
                    group.Items.Add(token);
            }
        }
        #endregion


     

        private static void BuildRunPairGroup(XmedMainTokenGroup group)
        {
            if (group.RawTokens.Count == 0)
                return;
            group.Items.Clear();
            group.PreTokens.Add(group.RawTokens[0]);

            var body = group.RawTokens.Skip(1).ToList();

            int pairCount = body.Count / 2;
            for (int i = 0; i < group.DeclaredItemCount; i++)
            {
                int firstIndex = i * 2;
                var first = body[firstIndex];
                if (firstIndex + 1 >= body.Count)
                {
                    // The last style is empty
                    group.Items.Add(new XmedTokenGroup(TokenType.Run, 0, 0) { Items = [first], GroupType = XmedTokenGroup.TokenGroupType.Run });
                    break;
                }
                var second = body[firstIndex + 1];

                var child = new XmedTokenGroup(TokenType.Run, 0, 0) { GroupType = XmedTokenGroup.TokenGroupType.Run };
                child.Items.Add(first);
                child.Items.Add(second);
                group.Items.Add(child);
            }
        }

        private static void BuildStyleGroup(XmedMainTokenGroup group)
        {
            if (group.RawTokens.Count == 0)
                return;

            group.Items.Clear();
            var styleLength = 77;
            for (int i = 0; i < group.DeclaredItemCount; i++)
            {
                var style = new XmedTokenGroup(TokenType.Style, 0, 0);
                style.GroupType = XmedTokenGroup.TokenGroupType.Style;
                group.Items.Add(style);
                var styleTokens = group.RawTokens.Skip(i * styleLength).Take(styleLength);
                style.Items.AddRange(styleTokens);
            }
           
        }
       

        private static void BuildParagraphGroup(XmedMainTokenGroup group)
        {
            //group.PreTokens.Add(group.RawTokens[0]);
            group.Items.Clear();
            var readIndex = 0;
            for (int i = 0; i < group.DeclaredItemCount; i++)
            {
                var paragraph = new XmedTokenGroup(TokenType.Paragraph, 0, 0);
                paragraph.GroupType = XmedTokenGroup.TokenGroupType.Paragraph;
                group.Items.Add(paragraph);

                var tokensGroup = new XmedTokenGroup(TokenType.ParagraphTokens, 0, 0);
                tokensGroup.GroupType = XmedTokenGroup.TokenGroupType.ParagraphTokens;
                paragraph.Items.Add(tokensGroup);
                var tokens = group.RawTokens.Skip(readIndex).Take(28).ToList();
                tokensGroup.Items.AddRange(tokens);

                // Tabs
                var tabsMainGroup = new XmedTokenGroup(TokenType.ParagraphTabs, 0, 0);
                tabsMainGroup.GroupType = XmedTokenGroup.TokenGroupType.ParagraphTabs;
                paragraph.Items.Add(tabsMainGroup);

                var indexC2_6 = readIndex + 28;
                tabsMainGroup.Items.AddRange(group.RawTokens.Skip(indexC2_6).Take(2));

                int tabCount = group.RawTokens[indexC2_6 + 1].Value.HasValue? group.RawTokens[indexC2_6 + 1].Value!.Value: 0;
                for (int j = 0; j < tabCount; j++)
                {
                    var indexTab = indexC2_6 + 1 + j * 4;
                    var tabStops = new XmedTokenGroup(TokenType.TabStops, 0, 0);
                    tabStops.GroupType = XmedTokenGroup.TokenGroupType.TabStops;
                    tabsMainGroup.Items.Add(tabStops);
                    tabStops.Items.Add(group.RawTokens[indexTab + 0]);
                    tabStops.Items.Add(group.RawTokens[indexTab + 1]);
                    tabStops.Items.Add(group.RawTokens[indexTab + 2]);
                    tabStops.Items.Add(group.RawTokens[indexTab + 3]);
                }
                // Add default tab
                var indexTabDef = indexC2_6 + 2 + tabCount * 4;
                var tabStopsDefault = new XmedTokenGroup(TokenType.TabStops, 0, 0);
                tabStopsDefault.GroupType = XmedTokenGroup.TokenGroupType.TabStopDefault;
                tabStopsDefault.Items.AddRange(group.RawTokens.Skip(indexTabDef).Take(6+18)); // Add 3 + 3 0 values most of the time
                tabsMainGroup.Items.Add(tabStopsDefault);
                readIndex += 28 + (tabCount * 4) + 6 + 18+2; // 12 * NULL = 28 +(4*tabs) + 26 = 54 + (4  * tabs)
            }
        }

      
        private static void BuildFontGroup(XmedMainTokenGroup group)
        {
            if (group.RawTokens.Count == 0)
                return;

            int index = 0;
            int remaining = Math.Max(0, group.DeclaredItemCount);

            while (index < group.RawTokens.Count && remaining > 0)
            {
                var entryTokens = new List<BlXmedToken>();

                while (index < group.RawTokens.Count && group.RawTokens[index].Type == TokenType.Block00)
                {
                    entryTokens.Add(group.RawTokens[index]);
                    index++;
                }

                while (index < group.RawTokens.Count)
                {
                    var token = group.RawTokens[index];

                    if (token.Type == TokenType.Block00)
                        break;

 
                    entryTokens.Add(token);
                    index++;
                }

                var child = BuildFontChild(entryTokens);
                if (child == null)
                    continue;

                group.Items.Add(child);
                remaining--;
            }

            while (index < group.RawTokens.Count)
            {
                group.PostTokens.Add(group.RawTokens[index]);
                index++;
            }
        }

        private static XmedTokenGroup? BuildFontChild(List<BlXmedToken> tokens)
        {
            if (tokens.Count == 0)
                return null;

            var child = new XmedTokenGroup(TokenType.Font,0,0);
            child.GroupType = XmedTokenGroup.TokenGroupType.Font;
            child.Items.AddRange(tokens);
            return child;
        }




        #region Dump/log


        public static string DumpGroupedTokens(List<XmedMainTokenGroup> groups, int startIndent = 0)
        {
            var sb = new StringBuilder();
            foreach (var mainGroup in groups)
            {
                sb.AppendLine();
                sb.AppendLine($"{mainGroup.TypeValue:X2}:{mainGroup.Ascii}\t\t // {mainGroup.MainType.ToString()}");
                var lastWasNewLine = true;
                switch (mainGroup.MainType)
                {
                    case XmedMainTokenGroup.MainGroupType.RunHeaderFFFF:
                    case XmedMainTokenGroup.MainGroupType.RunHeader:
                    case XmedMainTokenGroup.MainGroupType.Layout:
                        lastWasNewLine = DumpGroupRaw(sb, mainGroup, 1, lastWasNewLine);
                        break;
                    case XmedMainTokenGroup.MainGroupType.FullText:
                        DumpTokenValue(sb, mainGroup.RawTokens[0], 1, true);
                        break;
                    case XmedMainTokenGroup.MainGroupType.RunStyles:
                    case XmedMainTokenGroup.MainGroupType.RunParagraphs:
                        DumpRunGroup(sb, mainGroup, 1, lastWasNewLine);
                        break;
                    case XmedMainTokenGroup.MainGroupType.Styles:
                    case XmedMainTokenGroup.MainGroupType.Paragraphs:
                        DumpGroupWithSubGroups(sb, mainGroup, 1, lastWasNewLine);
                        break;
                    case XmedMainTokenGroup.MainGroupType.Fonts:
                        DumpGroupWithSubGroups(sb, mainGroup, 1, lastWasNewLine);
                        break;
                    case XmedMainTokenGroup.MainGroupType.ParagraphBounds:
                    case XmedMainTokenGroup.MainGroupType.ParagraphBounds2:
                    case XmedMainTokenGroup.MainGroupType.UnknownB:
                    case XmedMainTokenGroup.MainGroupType.ParagraphFormats:
                    case XmedMainTokenGroup.MainGroupType.ParagraphSpacing:
                    case XmedMainTokenGroup.MainGroupType.Unknown13:
                    case XmedMainTokenGroup.MainGroupType.Unknown128:
                    case XmedMainTokenGroup.MainGroupType.Unknown129:
                        lastWasNewLine = DumpGroupRaw(sb, mainGroup, 1, lastWasNewLine);
                        break;
                }
                sb.AppendLine();

            }
            return sb.ToString().TrimEnd();
        }

        private static bool DumpRunGroup(StringBuilder sb, XmedMainTokenGroup group, int depth, bool lastWasNewLine)
        {
            BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
            foreach (var preToken in group.PreTokens)
                lastWasNewLine = DumpTokenValue(sb, preToken, depth + 1, lastWasNewLine);
            if (!lastWasNewLine)
                sb.AppendLine();
            BlXmedTokenizer.WriteTab(sb, 0, depth + 2);
            foreach (XmedTokenGroup groupItem in group.Items)
            {
                foreach (var groupTokenItem in groupItem.Items)
                    lastWasNewLine = DumpTokenValue(sb, groupTokenItem, depth + 1, lastWasNewLine);
                sb.Append(" ");
            }
            return lastWasNewLine;
        }
        private static bool DumpGroupWithSubGroups(StringBuilder sb, XmedMainTokenGroup group, int depth, bool lastWasNewLine)
        {
            BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
            foreach (var preToken in group.PreTokens)
                lastWasNewLine = DumpTokenValue(sb, preToken, depth + 1 , lastWasNewLine);
            if (!lastWasNewLine)
                sb.AppendLine();
            lastWasNewLine = true;
            for (int i = 0; i < group.Items.Count; i++)
            {
                XmedTokenGroup groupItem = (XmedTokenGroup)group.Items[i];
                //if (!lastWasNewLine)
                    sb.AppendLine();
                sb.AppendLine($"// {groupItem.Type} {i}");
                lastWasNewLine = true;
                //BlXmedTokenizer.WriteTab(sb, 0, depth + 2);
                lastWasNewLine = DumpGroupRaw(sb, groupItem, depth + 1, lastWasNewLine);
            }
            return lastWasNewLine;
        }
        private static bool DumpGroupRaw(StringBuilder sb, XmedTokenGroup group, int depth, bool lastWasNewLine)
        {
            var lastWasNewLine2 = lastWasNewLine;
            foreach (var token in group.Items)
            {
               if (token is XmedTokenGroup subGroup1)
               {
                        //if (!lastWasNewLine2)
                        sb.AppendLine();
                    BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
                    if (token.Type == TokenType.TabStops)
                        sb.AppendLine("// TabStops ");
                    DumpGroupRaw(sb, subGroup1, depth + 1, true);
                    BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
                    lastWasNewLine2 = true;
                }
                else
                {
                    //BlXmedTokenizer.WriteTab(sb, 0, depth);
                    lastWasNewLine2 = DumpTokenValue(sb, token, depth, lastWasNewLine2);
                }
            }
            return lastWasNewLine2;
        }

        private static bool DumpTokenValue(StringBuilder sb, BlXmedToken t, int depth, bool lastWasNewLine)
        {
            switch (t.Type)
            {
                case TokenType.PrefixedHex:
                    sb.Append($"{t.TypeValue ?? 0:X2}:{t.Ascii ?? "<empty>"} ");
                    break;
                case TokenType.B_82_NULL: 
                case TokenType.NULL: sb.Append($"NULL "); break;
                case TokenType.Zero: sb.Append($"0 "); break;
                case TokenType.Block00:
                    if (!lastWasNewLine)
                        sb.AppendLine();
                    BlXmedTokenizer.WriteTab(sb, 0, 3);
                    if (t.Value == 44)
                    {
                        var lastNumbers = t.ReadBlock00Numbers();
                        sb.Append($"00({t.Value}):{string.Join(',', lastNumbers)}");
                        sb.AppendLine();
                        return true;
                    }
                    else
                    {
                        sb.Append($"00({t.Value}):\"");
                        sb.Append(t.Ascii);
                        sb.Append('"' + Environment.NewLine);
                        return true;
                    }
                default:
                    //sb.AppendLine($"{t.Type}({t.Value}):{t.Ascii} TODO ERROR");
                    break;
            }
            return false;
        }

        #endregion



        #region Helpers
      
        private BlXmedToken ReadNext()
        {
            var token = _tokens[_readIndex];
            _readIndex++;
            return token;
        }
        private static List<BlXmedToken> ExpandPadding(List<BlXmedToken> tokens)
        {
            var expanded = new List<BlXmedToken>();

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.C1_PAD_0)
                {
                    int repeat = Math.Max(0, token.TypeValue ?? 0);
                    for (int i = 0; i < repeat; i++)
                        expanded.Add(CreateZeroToken(token));
                    continue;
                }
                if (token.Type == TokenType.C2_PAD_NULL)
                {
                    int repeat = Math.Max(0, token.TypeValue ?? 0);
                    for (int i = 0; i < repeat; i++)
                        expanded.Add(CreateNULLToken(token));
                    continue;
                }

                expanded.Add(token);
            }

            return expanded;
        }

        private static List<BlXmedToken> ExpandRepeats(List<BlXmedToken> tokens)
        {
            var normalized = new List<BlXmedToken>();
            BlXmedToken? lastValue = null;

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.B_81_REP)
                {
                    var clone = lastValue != null && lastValue.Type != TokenType.C2_PAD_NULL && lastValue.Type != TokenType.B_82_NULL
                        ? lastValue.Clone()
                        : CreateZeroToken(token);
                    normalized.Add(clone);
                    lastValue = clone;
                    continue;
                }

                normalized.Add(token);

                if (token.Type != TokenType.C2_PAD_NULL && token.Type != TokenType.B_82_NULL)
                    lastValue = token;
            }

            return normalized;
        }



        private static int ParseHex(string value)
        {
            if (int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int parsed))
                return parsed;
            return 0;
        }
       
      
        #endregion


    }
}
