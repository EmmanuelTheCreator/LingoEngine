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
                    ExtractC2Tokens(group.RawTokens, group);
                    break;
                case XmedMainTokenGroup.MainGroupType.RunHeader:
                case XmedMainTokenGroup.MainGroupType.Layout:
                    ExtractStructs(group.RawTokens, group);
                    break;
                case XmedMainTokenGroup.MainGroupType.FullText:
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
                case XmedMainTokenGroup.MainGroupType.SpacingDescriptor:
                case XmedMainTokenGroup.MainGroupType.SpacingDescriptor2:
                case XmedMainTokenGroup.MainGroupType.UnknownB:
                case XmedMainTokenGroup.MainGroupType.UnknownC:
                case XmedMainTokenGroup.MainGroupType.UnknownF:
                case XmedMainTokenGroup.MainGroupType.Unknown13:
                case XmedMainTokenGroup.MainGroupType.Unknown128:
                case XmedMainTokenGroup.MainGroupType.Unknown129:
                    ExtractStructs(group.RawTokens, group);
                    break;
                default:
                    break;
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

            group.PreTokens.Add(group.RawTokens[0]);
            if (group.RawTokens.Count == 1)
                return;
            group.Items.Clear();
            (var structs, var restTokens) = SplitOn82(group.RawTokens.Skip(1).ToList());
            var collector = new List<BlXmedToken>();
            int target = Math.Max(0, group.DeclaredItemCount);
            var structCount = 6;
            var styleTokenGroups = structs.Select((x, i) => new { Key = i / structCount, Value = x })
                       .GroupBy(x => x.Key, x => x.Value, (k, g) => g.ToArray()) //.Where(x => x.Length>1)
                       .ToArray();
            if (styleTokenGroups.Length != group.DeclaredItemCount)
                throw new Exception("Error creating style groups. Incorrect parsing");
            foreach (var styleTokenGroup in styleTokenGroups)
            {
                var style = new XmedTokenGroup(TokenType.Style, 0, 0);
                group.Items.Add(style);
                foreach (var styleTokenGroupList in styleTokenGroup)
                {
                    XmedTokenGroup styleStruct = ExtractC2TokensFromStruct(styleTokenGroupList);

                    style.Items.Add(styleStruct);
                }
            }
            if (group.Items.Count != group.DeclaredItemCount)
                throw new Exception("Error creating style groups. Incorrect parsing groups");
            group.Items.AddRange(restTokens);
        }
       

        private static void BuildParagraphGroup(XmedMainTokenGroup group)
        {
            group.PreTokens.Add(group.RawTokens[0]);
            if (group.RawTokens.Count == 1)
                return;
            group.Items.Clear();
            var paragraphsRaws = SplitOnC2_12(group.RawTokens.Skip(1).ToList());
            if (paragraphsRaws.Count != group.DeclaredItemCount)
                throw new Exception("Error creating paragraphs groups. Incorrect parsing");
            foreach (var paragraphsRaw in paragraphsRaws)
            {
                var paragraph = new XmedTokenGroup(TokenType.Paragraph, 0, 0);
                group.Items.Add(paragraph);

                // first we need to split the tabs in a separated group:
                // 6A03E2AE => is tab stops
                var indexC2_6 = paragraphsRaw.FindIndex(x => x.Type == TokenType.C2 && x.TypeValue == 0x6);
                var partsStart = paragraphsRaw.GetRange(0, indexC2_6);
                var partTabStops = paragraphsRaw.GetRange(indexC2_6 , paragraphsRaw.Count - (indexC2_6 ));
                int tabCount = partTabStops[2].Value.HasValue? partTabStops[2].Value!.Value: 0;
                if (tabCount>0)
                {

                }

                (var structsRaws, var restTokens) = SplitOn82(partsStart);
                foreach (List<BlXmedToken> structsRaw in structsRaws)
                {
                    XmedTokenGroup styleStruct = ExtractC2TokensFromStruct(structsRaw);
                    paragraph.Items.Add(styleStruct);
                }
                paragraph.Items.AddRange(restTokens);
                var tabStops = new XmedTokenGroup(TokenType.TabStops, 0, 0);
                paragraph.Items.Add(tabStops);
                tabStops.Items.Add(partTabStops[0]);
                tabStops.Items.Add(partTabStops[1]);
                tabStops.Items.Add(partTabStops[2]);
                (var structsRaws2, var  restTokens2) = SplitOn82(partTabStops.Skip(3).ToList());
                foreach (var structsRaw in structsRaws2.Where(x => x.Count > 0))
                {
                    XmedTokenGroup styleStruct = ExtractC2TokensFromStruct(structsRaw);
                    tabStops.Items.Add(styleStruct);
                }
                if (structsRaws2.Where(x => x.Count> 0).Count() -1 != tabCount) // minus 1 beacuse thr last is the default tab width
                    throw new Exception("Error creating paragraphs groups. Incorrect tabs parsing");
                paragraph.Items.AddRange(restTokens2);
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
                if (group.RawTokens[index].Type != TokenType.B_82)
                    group.PostTokens.Add(group.RawTokens[index]);
                index++;
            }
        }

        private static XmedTokenGroup? BuildFontChild(List<BlXmedToken> tokens)
        {
            if (tokens.Count == 0)
                return null;

            var child = new XmedTokenGroup(TokenType.Font,0,0);
            child.Items.Add(tokens[0]);
            child.Items.Add(tokens[1]);
            ExtractStructs(tokens.Skip(2).ToList(), child);

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
                    case XmedMainTokenGroup.MainGroupType.SpacingDescriptor:
                    case XmedMainTokenGroup.MainGroupType.SpacingDescriptor2:
                    case XmedMainTokenGroup.MainGroupType.UnknownB:
                    case XmedMainTokenGroup.MainGroupType.UnknownC:
                    case XmedMainTokenGroup.MainGroupType.UnknownF:
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
                if (!lastWasNewLine)
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
                if (token is XmedC2TokenGroup)
                {
                    //if (!lastWasNewLine2)
                        sb.AppendLine();
                    BlXmedTokenizer.WriteTab(sb, 0, depth + 2);
                    sb.Append($"{token.Type}({token.TypeValue ?? 0:X2}) ");
                }
                else if (token is XmedTokenGroup structItem)
                {
                    if (token.Type == TokenType.TabStops)
                    {
                        if (!lastWasNewLine2)
                            sb.AppendLine();
                        BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
                        sb.Append("// TabStops ");
                        DumpGroupRaw(sb, structItem, depth + 1, true);
                        BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
                        lastWasNewLine2 = true;
                        continue;
                    }

                    if (token.Type != TokenType.B_82)
                        throw new Exception("Wrong struct type");
                    if (!lastWasNewLine2)
                        sb.AppendLine();
                    BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
                    sb.Append('[');
                    foreach (var groupToken in structItem.Items)
                        DumpTokenValue(sb, groupToken, depth + 1, lastWasNewLine2);
                    
                    sb.Append("<82]");
                    sb.AppendLine();
                   
                    lastWasNewLine2 = true;
                }
                else
                {
                    BlXmedTokenizer.WriteTab(sb, 0, depth);
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
                case TokenType.C2:
                    //if (!lastWasNewLine)
                        sb.AppendLine();
                    BlXmedTokenizer.WriteTab(sb, 0, depth + 1);
                    sb.Append($"C2({t.TypeValue:X2}) ");
                    break;
                default:
                    //sb.AppendLine($"{t.Type}({t.Value}):{t.Ascii} TODO ERROR");
                    break;
            }
            return false;
        }

        #endregion



        #region Helpers
        private static void ExtractStructs(List<BlXmedToken> tokens, XmedTokenGroup parent)
        {
            (var structsRaw,var  restTokens) = SplitOn82(tokens);
            var structs = new List<XmedTokenGroup>();
            foreach (var structRawItem in structsRaw)
            {
                XmedTokenGroup structItem = ExtractC2TokensFromStruct(structRawItem);
                structs.Add(structItem);
            }
           
            parent.Items.AddRange(structs);
            parent.Items.AddRange(restTokens);
        }
        private static XmedTokenGroup ExtractC2TokensFromStruct(List<BlXmedToken> tokens)
        {
            var theStruct = new XmedTokenGroup(TokenType.B_82, 0, 0);
            ExtractC2Tokens(tokens, theStruct);
            return theStruct;
        }

        private static void ExtractC2Tokens(List<BlXmedToken> tokens, XmedTokenGroup parent)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (!token.IsC2())
                {
                    parent.Items.Add(token);
                    continue;
                }
                var c2Group = new XmedC2TokenGroup(token);
                parent.Items.Add(c2Group);
                while (i < tokens.Count)
                {
                    var c2itemToken = tokens[i];
                    if (c2itemToken.IsC2()) break;
                    c2Group.Items.Add(c2itemToken);
                    i++;
                }
            }
        }
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
                if (token.Type == TokenType.C1)
                {
                    int repeat = Math.Max(0, token.TypeValue ?? 0);
                    for (int i = 0; i < repeat; i++)
                        expanded.Add(CreateZeroToken(token));
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
                if (token.Type == TokenType.B_81)
                {
                    var clone = lastValue != null && lastValue.Type != TokenType.C2 && lastValue.Type != TokenType.B_82
                        ? lastValue.Clone()
                        : CreateZeroToken(token);
                    normalized.Add(clone);
                    lastValue = clone;
                    continue;
                }

                normalized.Add(token);

                if (token.Type != TokenType.C2 && token.Type != TokenType.B_82)
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
        private static (List<List<BlXmedToken>> Segments, List<BlXmedToken> RestTokens) SplitOn82(List<BlXmedToken> tokens)
        {
            var segments = new List<List<BlXmedToken>>();
            var current = new List<BlXmedToken>();
            var restTokens = new List<BlXmedToken>();
            var hasB_82 = true;
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.B_82)
                {
                    segments.Add(current);
                    current = new List<BlXmedToken>();
                    hasB_82 = false; // the new list has no 82 token
                    continue;
                }

                current.Add(token);
            }
            if (hasB_82)
                segments.Add(current);
            else
                restTokens = current;
            return (segments, current);
        }
        private static List<List<BlXmedToken>> SplitOnC2_12(List<BlXmedToken> tokens)
        {
            var segments = new List<List<BlXmedToken>>();
            var current = new List<BlXmedToken>();

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.C2)
                {
                    if (token.TypeValue == 0x12)
                    {
                        current.Add(token);
                        segments.Add(current);
                        current = new List<BlXmedToken>();
                        continue;
                    }
                }

                current.Add(token);
            }
            if (current.Count > 0) { 
                segments.Add(current);
                }
            return segments;
        }
        #endregion


    }
}
