using BlingoEngine.IO.Legacy.Texts.Data;

using System.Collections.Generic;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedTokenGroup : BlXmedToken
    {
        public enum TokenGroupType
        {
             Unknown,
            C1Group, C2Group, C3Group,
             FFFFGroup,
             RecordGroup,
        }
        public enum SliceKind { FFFF, TextSlice, RunMap, ParaLayout, Record, Unknown }

        public TokenGroupType GroupType { get; set; }
        public List<BlXmedToken> PreTokens { get; } = new ();
        public List<BlXmedToken> PostTokens { get; } = new ();
        public List<BlXmedToken> Items { get; set; } = new ();
        public XmedTokenGroup? Parent { get; set; }
        public int GroupEnd { get; set; }
        public SliceKind SliceType { get; internal set; }

        public XmedTokenGroup(TokenType type, int start, int length, string? ascii = null, int? value = null, int? typeValue = null, bool linkToPrevious = false, byte[]? data = null) : base(type, start, length, ascii, value, typeValue, linkToPrevious, data)
        {
        }
    }
}
