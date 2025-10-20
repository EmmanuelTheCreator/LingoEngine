using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedC2TokenGroup : XmedTokenGroup
    {
        public XmedC2TokenGroup(BlXmedToken token)
            : base(token.Type, token.Start, token.Length, token.Ascii, token.Value, token.TypeValue, token.LinkToPrevious, token.Data)
        {
        }
    }
}
