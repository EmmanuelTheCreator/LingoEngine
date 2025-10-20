using BlingoEngine.IO.Legacy.Texts.Data;

using System;
using System.Collections.Generic;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal static class XmedTokenGroupExtensions
    {
        public static IEnumerable<BlXmedToken> EnumerateTokens(this XmedTokenGroup group)
        {
            foreach (var token in group.PreTokens)
                yield return token;

            foreach (var item in group.Items)
            {
                if (item is XmedTokenGroup child)
                {
                    foreach (var nested in child.EnumerateTokens())
                        yield return nested;
                }
                else
                    yield return item;
            }

            foreach (var token in group.PostTokens)
                yield return token;
        }

        public static IReadOnlyList<BlXmedToken> CollectSegmentTokens(this XmedTokenGroup? group)
        {
            if (group == null)
                return Array.Empty<BlXmedToken>();

            if (group.Items.Count == 0)
                return Array.Empty<BlXmedToken>();

            var list = new List<BlXmedToken>();
            foreach (var item in group.Items)
            {
                if (item is BlXmedToken token)
                    list.Add(token);
            }

            return list;
        }

        public static IReadOnlyList<BlXmedToken> CollectTokens(this XmedTokenGroup? group)
        {
            if (group == null)
                return Array.Empty<BlXmedToken>();

            var list = new List<BlXmedToken>();
            list.AddRange(group.PreTokens);
            foreach (var item in group.Items)
            {
                if (item is BlXmedToken token)
                    list.Add(token);
                else if (item is XmedTokenGroup child)
                    list.AddRange(child.CollectTokens());
            }
            list.AddRange(group.PostTokens);
            return list;
        }

        public static IEnumerable<XmedTokenGroup> EnumerateC2Groups(this XmedTokenGroup? group)
        {
            if (group == null)
                yield break;

            if (group.Type == BlXmedToken.TokenType.C2)
                yield return group;

            foreach (var item in group.Items)
            {
                if (item is XmedTokenGroup child)
                {
                    foreach (var nested in child.EnumerateC2Groups())
                        yield return nested;
                }
            }
        }

        public static IReadOnlyList<BlXmedToken> GetFieldTokens(this XmedTokenGroup group, int fieldIndex)
        {
            if (fieldIndex == 0)
                return group.PreTokens;

            int remaining = fieldIndex - 1;
            foreach (var item in group.Items)
            {
                if (item is not XmedTokenGroup segment)
                    continue;

                if (segment.Type == BlXmedToken.TokenType.C2)
                    continue;

                if (remaining == 0)
                    return segment.CollectSegmentTokens();

                remaining--;
            }

            return Array.Empty<BlXmedToken>();
        }

        public static int ReadNumericAt(this XmedTokenGroup? c2Group, int index)
        {
            if (c2Group == null || c2Group.Type != BlXmedToken.TokenType.C2)
                return 0;

            int cursor = 0;
            foreach (var item in c2Group.Items)
            {
                if (item is not BlXmedToken token)
                    continue;

                if (!token.TryGetNumericValue(out var numeric))
                    continue;

                if (cursor == index)
                    return numeric;

                cursor++;
            }

            return 0;
        }

        public static bool ReadBooleanAt(this XmedTokenGroup? c2Group, int index)
        {
            return c2Group.ReadNumericAt(index) != 0;
        }
    }
}
