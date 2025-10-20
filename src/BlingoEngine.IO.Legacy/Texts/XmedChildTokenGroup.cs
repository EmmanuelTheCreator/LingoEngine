using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedChildTokenGroup : XmedTokenGroup
    {
        public XmedChildTokenGroup(BlXmedToken seed)
            : base(seed.Type, seed.Start, seed.Length, seed.Ascii, seed.Value, seed.TypeValue, seed.LinkToPrevious, seed.Data)
        {
        }
    }
}
