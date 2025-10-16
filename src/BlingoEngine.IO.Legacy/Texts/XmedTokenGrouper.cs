using BlingoEngine.IO.Legacy.Texts.Data;
using System.Text;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedTokenGrouper
    {
        public enum FfffKind { Color, FontSize, LineHeight, Spacing, Indent, Baseline, Unknown }

        public List<XmedTokenGroup> CreateGroups(List<BlXmedToken> tokens)
        {
            var roots = new List<XmedTokenGroup>();
            var preamble = new XmedTokenGroup(BlXmedToken.TokenType.Ascii, 0, 0) { GroupType = XmedTokenGroup.TokenGroupType.Unknown };
            roots.Add(preamble);

            XmedTokenGroup? cur = null;
            int i = 0, fieldIdx = 0;

            while (i < tokens.Count)
            {
                var t = tokens[i];

                // Open Cx and run immediate lookahead for C1(03|04|1E)
                if (t.Type is BlXmedToken.TokenType.C1 or BlXmedToken.TokenType.C2 or BlXmedToken.TokenType.C3)
                {
                    if (cur != null && cur.GroupEnd == 0) cur.GroupEnd = t.Start;
                    cur = new XmedTokenGroup(t.Type, t.Start, t.Length, t.Ascii, t.Value, t.TypeValue, t.LinkToPrevious, t.Data)
                    {
                        GroupType = t.Type == BlXmedToken.TokenType.C1 ? XmedTokenGroup.TokenGroupType.C1Group
                                 : t.Type == BlXmedToken.TokenType.C2 ? XmedTokenGroup.TokenGroupType.C2Group
                                 : XmedTokenGroup.TokenGroupType.C3Group
                    };
                    roots.Add(cur);
                    fieldIdx = 0;
                    i++;

                    if (t.Type == BlXmedToken.TokenType.C1 && (t.TypeValue == 0x03 || t.TypeValue == 0x04 || t.TypeValue == 0x1E) && i < tokens.Count)
                    {
                        var ctx0 = new SliceContext(t.Type, t.TypeValue ?? 0, fieldIdx);
                        if (TryReadGroupType(tokens, i, in ctx0, cur, out int next0)) { i = next0; continue; }
                    }
                    continue;
                }

                if (cur == null) { preamble.Items.Add(t); i++; continue; }

                var ctx = new SliceContext(
                    cur.GroupType == XmedTokenGroup.TokenGroupType.C1Group ? BlXmedToken.TokenType.C1 :
                    cur.GroupType == XmedTokenGroup.TokenGroupType.C2Group ? BlXmedToken.TokenType.C2 :
                    BlXmedToken.TokenType.C3,
                    cur.TypeValue ?? 0, fieldIdx);

                if (TryReadGroupType(tokens, i, in ctx, cur, out int nextIndex)) { i = nextIndex; continue; }

                cur.Items.Add(t);

                if (t.Type == BlXmedToken.TokenType.B_81)
                {
                    fieldIdx++;
                    var ctxF = new SliceContext(ctx.Tag, ctx.Sub, fieldIdx);
                    if (TryReadGroupType(tokens, i + 1, in ctxF, cur, out int nextF)) { i = nextF; continue; }
                }

                i++;
            }

            int end = tokens.Count > 0 ? tokens[^1].Start + tokens[^1].Length : 0;
            if (cur != null && cur.GroupEnd == 0) cur.GroupEnd = end;
            if (preamble.GroupEnd == 0) preamble.GroupEnd = end;

            return roots;
        }


        private static bool TryReadGroupType(List<BlXmedToken> tokens, int i, in SliceContext ctx, XmedTokenGroup cur, out int nextIndex)
        {
            // Text slice
            if (TrySliceTextSlice(tokens, i, in ctx, out int jText))
            {
                var g = NewGroupFromRange(cur, tokens[i], tokens[jText], XmedTokenGroup.TokenGroupType.FFFFGroup, XmedTokenGroup.SliceKind.TextSlice);
                for (int k = i; k <= jText; k++) g.Items.Add(tokens[k]);
                cur.Items.Add(g); nextIndex = jText + 1; return true;
            }
            // Run map
            if (TrySliceRunMap(tokens, i, in ctx, out int jRun))
            {
                var g = NewGroupFromRange(cur, tokens[i], tokens[jRun], XmedTokenGroup.TokenGroupType.FFFFGroup, XmedTokenGroup.SliceKind.RunMap);
                for (int k = i; k <= jRun; k++) g.Items.Add(tokens[k]);
                cur.Items.Add(g); nextIndex = jRun + 1; return true;
            }
            // Paragraph layout
            if (TrySliceParaLayout(tokens, i, in ctx, out int jPara))
            {
                var g = NewGroupFromRange(cur, tokens[i], tokens[jPara], XmedTokenGroup.TokenGroupType.FFFFGroup, XmedTokenGroup.SliceKind.ParaLayout);
                for (int k = i; k <= jPara; k++) g.Items.Add(tokens[k]);
                cur.Items.Add(g); nextIndex = jPara + 1; return true;
            }
            // inside TryReadGroupType (replace the old TrySlice03Record section)
            if (TrySlice03Record(tokens, i, in ctx, out int j03, out var sliceKind))
            {
                var g = NewGroupFromRange(cur, tokens[i], tokens[j03], XmedTokenGroup.TokenGroupType.RecordGroup, sliceKind);
                for (int k = i; k <= j03; k++)
                    g.Items.Add(tokens[k]);
                cur.Items.Add(g);
                nextIndex = j03 + 1;
                return true;
            }


            // FFFF (color/font/leading/…)
            if (TrySliceFfff(tokens, i, in ctx, out int jEnd, out FfffKind fkind))
            {
                var g = NewGroupFromRange(cur, tokens[i], tokens[jEnd], XmedTokenGroup.TokenGroupType.FFFFGroup, XmedTokenGroup.SliceKind.FFFF);
                for (int k = i; k <= jEnd; k++) g.Items.Add(tokens[k]);
                cur.Items.Add(g); nextIndex = jEnd + 1; return true;
            }

            nextIndex = i; return false;
        }



        private static XmedTokenGroup NewGroupFromRange(
            XmedTokenGroup parent, BlXmedToken first, BlXmedToken last,
            XmedTokenGroup.TokenGroupType kind, XmedTokenGroup.SliceKind sliceKind)
        {
            return new XmedTokenGroup(first.Type, first.Start, first.Length, first.Ascii, first.Value, first.TypeValue, first.LinkToPrevious, first.Data)
            {
                GroupType = kind,
                Parent = parent,
                GroupEnd = last.Start + last.Length,
                SliceType = sliceKind
            };
        }


        private static bool TrySliceFfff(List<BlXmedToken> ts, int idx, in SliceContext ctx, out int end, out FfffKind kind)
        {
            var tag = ctx.Tag; var sub = ctx.Sub; var fieldIdx = ctx.FieldIndex;
            end = idx - 1; kind = FfffKind.Unknown;

            var nums = new List<int>(5);
            int i = idx, seen01 = 0;

            // collect up to 5 01-values (skip <81>)
            while (i < ts.Count && seen01 < 5)
            {
                var t = ts[i];
                if (t.Type == BlXmedToken.TokenType.B_81) { i++; continue; }
                if (!Is01(t)) break;

                int v = t.Value ?? ParseHex(t.Ascii);
                nums.Add(v); seen01++; end = i;
                if (v == 0xFFFF) break;
                i++;
            }

            if (nums.Count >= 3 && nums[^1] == 0xFFFF)
            {
                // Color (full/partial)
                if (LooksRgb(nums) && tag == BlXmedToken.TokenType.C1 && (sub == 0x03 || sub == 0x04))
                { kind = FfffKind.Color; return true; }

                // Scalar triplet value,0,FFFF → classify by context
                if (nums.Count == 3 && nums[1] == 0x0000)
                {
                    if (tag == BlXmedToken.TokenType.C1 && sub == 0x04) { kind = FfffKind.FontSize; return true; }
                    if (tag == BlXmedToken.TokenType.C1 && sub == 0x03) { kind = (fieldIdx == 0) ? FfffKind.LineHeight : FfffKind.Spacing; return true; }
                    if (tag == BlXmedToken.TokenType.C1 && sub == 0x1E) { kind = FfffKind.Indent; return true; }
                    if (tag == BlXmedToken.TokenType.C2 && sub == 0x03) { kind = FfffKind.Baseline; return true; }
                }
            }

            end = idx - 1; return false;
        }


        // === Add to XmedTokenGrouper ===
        // Reuse SliceContext, FfffKind, Is01, ParseHex utilities already present.

       

        // Entry: call these BEFORE TrySliceFfff in CreateGroups()
        static bool TrySliceTextSlice(List<BlXmedToken> ts, int idx, in SliceContext ctx, out int end)
        {
            end = idx - 1;
            // Heuristic: C1(04)/C1(03) context; sequence of ASCII or PrefixedHex 00:.. until <81|82|C*>
            if (!(ctx.Tag == BlXmedToken.TokenType.C1 && (ctx.Sub == 0x04 || ctx.Sub == 0x03))) return false;

            int i = idx; bool any = false;
            while (i < ts.Count)
            {
                var t = ts[i];
                if (t.Type is BlXmedToken.TokenType.B_81 or BlXmedToken.TokenType.B_82
                    or BlXmedToken.TokenType.C1 or BlXmedToken.TokenType.C2 or BlXmedToken.TokenType.C3) break;

                if (t.Type is BlXmedToken.TokenType.Ascii
                    || (t.Type == BlXmedToken.TokenType.PrefixedHex && (t.TypeValue ?? 0) == 0x00)) { any = true; end = i; i++; continue; }

                break;
            }
            return any && end >= idx;
        }

        static bool TrySliceRunMap(List<BlXmedToken> ts, int idx, in SliceContext ctx, out int end)
        {
            end = idx - 1;
            // Heuristic: in C2(04)/C2(07) or header blocks; repeating pairs of 02:<offset> 02:<len> (allow <81>)
            if (!(ctx.Tag == BlXmedToken.TokenType.C2 && (ctx.Sub == 0x04 || ctx.Sub == 0x07))) return false;

            int i = idx; int pairs = 0; bool expect02 = true; bool seenAny = false;
            while (i < ts.Count)
            {
                var t = ts[i];
                if (t.Type == BlXmedToken.TokenType.B_81) { i++; continue; }
                if (t.Type == BlXmedToken.TokenType.B_82 || t.Type is BlXmedToken.TokenType.C1 or BlXmedToken.TokenType.C2 or BlXmedToken.TokenType.C3) break;

                if (t.Type == BlXmedToken.TokenType.PrefixedHex && (t.TypeValue ?? 0) == 0x02)
                {
                    seenAny = true; end = i; i++;
                    expect02 = !expect02;
                    if (expect02) pairs++; // we completed a pair
                    continue;
                }
                break;
            }
            return seenAny && pairs >= 1;
        }

        static bool TrySliceParaLayout(List<BlXmedToken> ts, int idx, in SliceContext ctx, out int end)
        {
            end = idx - 1;
            // Heuristic: C1(1E) paragraph layout: several scalar triplets (<val>00, 0000, FFFF) possibly separated by <81>
            if (!(ctx.Tag == BlXmedToken.TokenType.C1 && ctx.Sub == 0x1E)) return false;

            int i = idx; int scalars = 0; int state = 0; // 0: val, 1: zero, 2: ffff
            while (i < ts.Count)
            {
                var t = ts[i];
                if (t.Type == BlXmedToken.TokenType.B_81) { i++; state = 0; continue; }
                if (t.Type is BlXmedToken.TokenType.C1 or BlXmedToken.TokenType.C2 or BlXmedToken.TokenType.C3) break;

                if (Is01(t))
                {
                    int v = t.Value ?? ParseHex(t.Ascii);
                    if (state == 0) { /* val */ end = i; state = 1; i++; continue; }
                    if (state == 1 && v == 0x0000) { end = i; state = 2; i++; continue; }
                    if (state == 2 && v == 0xFFFF) { end = i; scalars++; state = 0; i++; continue; }
                    break;
                }
                break;
            }
            return scalars >= 1;
        }

        // detect a 03: record + its arguments (01/02), until boundary
        // TrySlice03Record: boundary = <81>, next 03:, next C*, or pair <82><82>; allow lone 03:
        static bool TrySlice03Record(List<BlXmedToken> ts, int idx, in SliceContext ctx,
                                     out int end, out XmedTokenGroup.SliceKind kind)
        {
            end = idx - 1; kind = XmedTokenGroup.SliceKind.Unknown;
            if (!(ts[idx].Type == BlXmedToken.TokenType.PrefixedHex && (ts[idx].TypeValue ?? 0) == 0x03)) return false;

            int i = idx; end = i++; bool prev82 = false; bool anyArg = false;
            while (i < ts.Count)
            {
                var t = ts[i];
                if (t.Type is BlXmedToken.TokenType.C1 or BlXmedToken.TokenType.C2 or BlXmedToken.TokenType.C3) break;
                if (t.Type == BlXmedToken.TokenType.B_81) { end = i - 1; break; }
                if (t.Type == BlXmedToken.TokenType.PrefixedHex && (t.TypeValue ?? 0) == 0x03) break;

                if (t.Type == BlXmedToken.TokenType.B_82)
                {
                    if (prev82) { end = i; break; }
                    prev82 = true; i++; continue;
                }
                prev82 = false;

                if (t.Type == BlXmedToken.TokenType.PrefixedHex && ((t.TypeValue ?? 0) is 0x01 or 0x02))
                { anyArg = true; end = i; i++; continue; }

                break;
            }

            kind = XmedTokenGroup.SliceKind.Record;
            if (!anyArg) end = idx; // wrap lone 03:
            return end >= idx;
        }









        public static string DumpGroupedTokens(List<XmedTokenGroup> groups, int startIndent = 5)
        {
            var sb = new StringBuilder();

            void Dump(List<XmedTokenGroup> list, int depth)
            {
                foreach (var g in list)
                {
                    sb.Append(new string(' ', depth * 2));
                    sb.AppendLine($"{g.GroupType.ToString().Replace("Group", "")}({g.TypeValue ?? 0:X2})");

                    int last00 = g.Items.FindLastIndex(t => t.Type == TokenType.Block00);
                    int onLine = 0;
                    int baseTokenDepth = depth + 1;

                    for (int i = 0; i < g.Items.Count; i++)
                    {
                        if (g.Items[i] is XmedTokenGroup child)
                        {
                            Dump(new List<XmedTokenGroup> { child }, depth + 1);
                            continue;
                        }

                        int tokenDepth = baseTokenDepth; // keep constant indent inside this group
                        onLine = BlXmedTokenizer.WriteToken(sb, last00, onLine, i, g.Items[i], ref tokenDepth, baseTokenDepth);
                    }
                    if (onLine > 0) sb.AppendLine();
                }
            }

            Dump(groups, startIndent);
            return sb.ToString().TrimEnd();
        }





        private static bool Is01(BlXmedToken t) => t.Type == BlXmedToken.TokenType.PrefixedHex && (t.TypeValue ?? 0) == 0x01;
        private static int ParseHex(string? s) => int.TryParse(s ?? "0", System.Globalization.NumberStyles.HexNumber, null, out var v) ? v : 0;
      
        private static bool LooksRgb(IReadOnlyList<int> a)
        {
            int n = Math.Min(3, a.Count - 2);
            if (a[^1] != 0xFFFF || a[^2] != 0x0000 || n <= 0) return false;
            for (int j = 0; j < n; j++) if (a[j] > 0xFF00) return false;
            return true;
        }

        // Add near XmedTokenGrouper
        public readonly struct SliceContext
        {
            public readonly BlXmedToken.TokenType Tag;
            public readonly int Sub;
            public readonly int FieldIndex;
            public SliceContext(BlXmedToken.TokenType tag, int sub, int fieldIndex)
            { Tag = tag; Sub = sub; FieldIndex = fieldIndex; }
        }

    }
}
