using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BlingoEngine.IO.Legacy.Texts.Data;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedTokenGrouper
    {
        public List<XmedTokenGroup> CreateGroups(List<BlXmedToken> tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            if (tokens.Count == 0)
                return new List<XmedTokenGroup>();

            var expanded = ExpandPadding(tokens);
            var normalized = ExpandRepeats(expanded);

            var groups = new List<XmedTokenGroup>();
            int index = 0;

            while (index < normalized.Count)
            {
                var candidate = normalized[index];
                if (!TryParseHeader(candidate, out string? blockId, out int tokenCount, out int itemCount))
                {
                    index++;
                    continue;
                }

                index++;
                var payload = new List<BlXmedToken>();
                int consumed = 0;

                while (index < normalized.Count)
                {
                    var next = normalized[index];
                    if (TryParseHeader(next, out _, out _, out _))
                        break;

                    payload.Add(next);
                    index++;
                    consumed++;

                    if (consumed >= tokenCount && tokenCount > 0)
                        break;
                }

                var main = new XmedMainTokenGroup(candidate, blockId!, tokenCount, itemCount);
                main.RawTokens.AddRange(payload);
                BuildMainGroup(main);
                groups.Add(main);
            }

            return groups;
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
                        ? CloneToken(lastValue)
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

        private static bool TryParseHeader(BlXmedToken token, out string? blockId, out int tokenCount, out int itemCount)
        {
            blockId = null;
            tokenCount = 0;
            itemCount = 0;

            if (token.Type != TokenType.PrefixedHex || token.TypeValue != 0x03 || string.IsNullOrEmpty(token.Ascii))
                return false;

            if (token.Ascii.Length < 20)
                return false;

            blockId = token.Ascii[..4];
            tokenCount = ParseHex(token.Ascii[4..12]);
            itemCount = ParseHex(token.Ascii[12..20]);
            return true;
        }

        private static int ParseHex(string value)
        {
            if (int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int parsed))
                return parsed;
            return 0;
        }

        private static void BuildMainGroup(XmedMainTokenGroup group)
        {
            group.PreTokens.Clear();
            group.Items.Clear();
            group.PostTokens.Clear();

            if (group.RawTokens.Count == 0)
            {
                group.GroupEnd = group.Start + group.Length;
                return;
            }

            switch (group.BlockId)
            {
                case "0004":
                case "0005":
                    BuildRunPairGroup(group);
                    break;
                case "0006":
                    BuildStyleGroup(group);
                    break;
                case "0007":
                    BuildParagraphGroup(group);
                    break;
                case "0008":
                    BuildFontGroup(group);
                    break;
                default:
                    group.PreTokens.AddRange(group.RawTokens);
                    break;
            }

            SetGroupEndFromRaw(group);
        }

        private static void BuildRunPairGroup(XmedMainTokenGroup group)
        {
            if (group.RawTokens.Count == 0)
                return;

            group.PreTokens.Add(group.RawTokens[0]);

            var body = new List<BlXmedToken>();
            for (int i = 1; i < group.RawTokens.Count; i++)
            {
                var token = group.RawTokens[i];
                if (token.Type != TokenType.B_82)
                    body.Add(token);
            }

            int pairCount = body.Count / 2;
            for (int i = 0; i < pairCount; i++)
            {
                int firstIndex = i * 2;
                var first = body[firstIndex];
                var second = body[firstIndex + 1];

                var child = new XmedChildTokenGroup(first);
                child.Items.Add(first);
                child.Items.Add(second);
                child.GroupEnd = second.Start + second.Length;
                child.Parent = group;
                group.Items.Add(child);
            }

            if (body.Count % 2 == 1)
                group.PostTokens.Add(body[^1]);
        }

        private static void BuildStyleGroup(XmedMainTokenGroup group)
        {
            if (group.RawTokens.Count == 0)
                return;

            group.PreTokens.Add(group.RawTokens[0]);
            if (group.RawTokens.Count == 1)
                return;

            var segments = SplitOn82(group.RawTokens.Skip(1).ToList());
            var collector = new List<BlXmedToken>();
            int target = Math.Max(0, group.DeclaredItemCount);
            int created = 0;

            foreach (var segment in segments)
            {
                if (segment.Count == 0)
                    continue;

                collector.AddRange(segment);

                bool shouldClose = IsStyleTerminator(segment);
                if (target > 0 && created + 1 == target)
                    shouldClose = true;

                if (!shouldClose)
                    continue;

                var child = BuildStyleChild(new List<BlXmedToken>(collector));
                collector.Clear();

                if (child == null)
                    continue;

                child.Parent = group;
                group.Items.Add(child);
                created++;
            }

            if (collector.Count > 0)
            {
                if (target > 0 && created < target)
                {
                    var child = BuildStyleChild(new List<BlXmedToken>(collector));
                    if (child != null)
                    {
                        child.Parent = group;
                        group.Items.Add(child);
                    }
                    else
                        group.PostTokens.AddRange(collector);
                }
                else
                    group.PostTokens.AddRange(collector);
            }
        }

        private static bool IsStyleTerminator(List<BlXmedToken> segment)
        {
            if (segment.Count == 0)
                return false;

            foreach (var token in segment)
            {
                if (token.Type == TokenType.C2)
                    return false;

                if (!token.TryGetNumericValue(out var numeric) || numeric != 0)
                    return false;
            }

            return true;
        }

        private static XmedChildTokenGroup? BuildStyleChild(List<BlXmedToken> tokens)
        {
            if (tokens.Count == 0)
                return null;

            var child = new XmedChildTokenGroup(tokens[0]);

            int tailIndex = tokens.Count;
            while (tailIndex > 0)
            {
                var token = tokens[tailIndex - 1];
                if (token.Type == TokenType.C2)
                    break;

                if (!token.TryGetNumericValue(out var numeric) || numeric != 0)
                    break;

                tailIndex--;
            }

            for (int i = tailIndex; i < tokens.Count; i++)
                child.PostTokens.Add(tokens[i]);

            tokens.RemoveRange(tailIndex, tokens.Count - tailIndex);

            int index = 0;
            if (index < tokens.Count)
            {
                child.PreTokens.Add(tokens[index]);
                index++;
            }

            while (index < tokens.Count && tokens[index].Type != TokenType.C2)
            {
                child.Items.Add(tokens[index]);
                child.GroupEnd = tokens[index].Start + tokens[index].Length;
                index++;
            }

            while (index < tokens.Count)
            {
                var token = tokens[index];

                if (token.Type == TokenType.C2)
                {
                    var c2 = new XmedC2TokenGroup(token);
                    index++;

                    while (index < tokens.Count && tokens[index].Type != TokenType.C2)
                    {
                        c2.Items.Add(tokens[index]);
                        c2.GroupEnd = tokens[index].Start + tokens[index].Length;
                        index++;
                    }

                    c2.Parent = child;
                    child.Items.Add(c2);
                    child.GroupEnd = c2.GroupEnd;
                    continue;
                }

                child.Items.Add(token);
                child.GroupEnd = token.Start + token.Length;
                index++;
            }

            if (child.GroupEnd == 0)
            {
                var last = child.PostTokens.LastOrDefault()
                    ?? child.PreTokens.LastOrDefault();

                if (last != null)
                    child.GroupEnd = last.Start + last.Length;
            }

            return child;
        }

        private static void BuildParagraphGroup(XmedMainTokenGroup group)
        {
            if (group.RawTokens.Count == 0)
                return;

            int index = 0;
            while (index < group.RawTokens.Count && group.RawTokens[index].Type != TokenType.C2)
            {
                if (group.RawTokens[index].Type != TokenType.B_82)
                    group.PreTokens.Add(group.RawTokens[index]);
                index++;
            }

            int remaining = group.RawTokens.Count - index;
            int itemCount = Math.Max(0, group.DeclaredItemCount);

            if (remaining <= 0 || itemCount <= 0)
                return;

            int baseLength = remaining / itemCount;
            int extra = remaining % itemCount;

            for (int i = 0; i < itemCount && index < group.RawTokens.Count; i++)
            {
                int length = baseLength + (i < extra ? 1 : 0);
                length = Math.Clamp(length, 0, group.RawTokens.Count - index);

                if (length == 0)
                    break;

                var slice = new List<BlXmedToken>();
                for (int j = 0; j < length; j++)
                    slice.Add(group.RawTokens[index + j]);

                index += length;

                var child = BuildParagraphChild(slice);
                if (child == null)
                    continue;

                child.Parent = group;
                group.Items.Add(child);
            }

            while (index < group.RawTokens.Count)
            {
                if (group.RawTokens[index].Type != TokenType.B_82)
                    group.PostTokens.Add(group.RawTokens[index]);
                index++;
            }
        }

        private static XmedChildTokenGroup? BuildParagraphChild(List<BlXmedToken> tokens)
        {
            if (tokens.Count == 0)
                return null;

            var child = new XmedChildTokenGroup(tokens[0]);
            int index = 0;
            bool firstGroupCaptured = false;

            while (index < tokens.Count && tokens[index].Type != TokenType.C2)
            {
                if (tokens[index].Type != TokenType.B_82)
                    child.PreTokens.Add(tokens[index]);
                index++;
            }

            while (index < tokens.Count)
            {
                var token = tokens[index];

                if (token.Type == TokenType.B_82)
                {
                    index++;
                    continue;
                }

                if (token.Type == TokenType.C2)
                {
                    var collected = new List<BlXmedToken> { token };
                    index++;

                    while (index < tokens.Count)
                    {
                        var next = tokens[index];

                        if (next.Type == TokenType.C2)
                            break;

                        if (next.Type == TokenType.B_82)
                        {
                            index++;
                            break;
                        }

                        collected.Add(next);
                        index++;
                    }

                    if (!firstGroupCaptured)
                    {
                        child.PreTokens.AddRange(collected);
                        firstGroupCaptured = true;
                        continue;
                    }

                    var c2 = new XmedC2TokenGroup(collected[0]);
                    for (int i = 1; i < collected.Count; i++)
                    {
                        var value = collected[i];
                        c2.Items.Add(value);
                        c2.GroupEnd = value.Start + value.Length;
                    }

                    c2.Parent = child;
                    child.Items.Add(c2);
                    child.GroupEnd = c2.GroupEnd;
                    continue;
                }

                child.Items.Add(token);
                child.GroupEnd = token.Start + token.Length;
                index++;
            }

            if (child.GroupEnd == 0)
            {
                var last = child.PreTokens.LastOrDefault();
                if (last != null)
                    child.GroupEnd = last.Start + last.Length;
            }

            return child;
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

                    if (token.Type == TokenType.B_82)
                    {
                        index++;
                        continue;
                    }

                    entryTokens.Add(token);
                    index++;
                }

                var child = BuildFontChild(entryTokens);
                if (child == null)
                    continue;

                child.Parent = group;
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

        private static XmedChildTokenGroup? BuildFontChild(List<BlXmedToken> tokens)
        {
            if (tokens.Count == 0)
                return null;

            var child = new XmedChildTokenGroup(tokens[0]);
            int index = 0;

            while (index < tokens.Count && tokens[index].Type == TokenType.Block00)
            {
                child.PreTokens.Add(tokens[index]);
                index++;
            }

            while (index < tokens.Count)
            {
                var token = tokens[index];

                if (token.Type == TokenType.C2)
                {
                    var c2 = new XmedC2TokenGroup(token);
                    index++;

                    while (index < tokens.Count && tokens[index].Type != TokenType.C2)
                    {
                        c2.Items.Add(tokens[index]);
                        c2.GroupEnd = tokens[index].Start + tokens[index].Length;
                        index++;
                    }

                    c2.Parent = child;
                    child.Items.Add(c2);
                    child.GroupEnd = c2.GroupEnd;
                    continue;
                }

                child.Items.Add(token);
                child.GroupEnd = token.Start + token.Length;
                index++;
            }

            if (child.GroupEnd == 0)
            {
                var last = child.PreTokens.LastOrDefault();
                if (last != null)
                    child.GroupEnd = last.Start + last.Length;
            }

            return child;
        }

        private static void SetGroupEndFromRaw(XmedMainTokenGroup group)
        {
            var last = group.RawTokens.LastOrDefault();
            if (last != null)
                group.GroupEnd = last.Start + last.Length;
            else
                group.GroupEnd = group.Start + group.Length;
        }

        private static List<List<BlXmedToken>> SplitOn82(List<BlXmedToken> tokens)
        {
            var segments = new List<List<BlXmedToken>>();
            var current = new List<BlXmedToken>();

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.B_82)
                {
                    segments.Add(current);
                    current = new List<BlXmedToken>();
                    continue;
                }

                current.Add(token);
            }

            segments.Add(current);
            return segments;
        }

        private static BlXmedToken CloneToken(BlXmedToken token)
        {
            return new BlXmedToken(token.Type, token.Start, token.Length, token.Ascii, token.Value, token.TypeValue, token.LinkToPrevious, token.Data);
        }

        private static BlXmedToken CreateZeroToken(BlXmedToken reference)
        {
            return new BlXmedToken(TokenType.PrefixedHex, reference.Start, 0, "0000", 0, 0x01);
        }

        public static string DumpGroupedTokens(List<XmedTokenGroup> groups, int startIndent = 0)
        {
            var sb = new StringBuilder();
            foreach (var group in groups)
                DumpGroup(sb, group, startIndent);
            return sb.ToString().TrimEnd();
        }

        private static void DumpGroup(StringBuilder sb, XmedTokenGroup group, int depth)
        {
            BlXmedTokenizer.WriteToken(sb, false, group, depth, true);

            foreach (var token in group.PreTokens)
                BlXmedTokenizer.WriteToken(sb, false, token, depth + 1, true);

            foreach (var item in group.Items)
            {
                if (item is XmedTokenGroup child)
                    DumpGroup(sb, child, depth + 1);
                else
                    BlXmedTokenizer.WriteToken(sb, false, item, depth + 1, true);
            }

            foreach (var token in group.PostTokens)
                BlXmedTokenizer.WriteToken(sb, false, token, depth + 1, true);
        }
    }
}
