using System.Collections.Generic;
using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedMainTokenGroup : XmedTokenGroup
    {
        public string BlockId { get; }
        public int DeclaredTokenCount { get; }
        public int DeclaredItemCount { get; }
        public List<BlXmedToken> RawTokens { get; } = new();

        public XmedMainTokenGroup(BlXmedToken header, string blockId, int tokenCount, int itemCount)
            : base(header.Type, header.Start, header.Length, header.Ascii, header.Value, header.TypeValue, header.LinkToPrevious, header.Data)
        {
            BlockId = blockId;
            DeclaredTokenCount = tokenCount;
            DeclaredItemCount = itemCount;
        }
    }
}
