
namespace BlingoEngine.IO.Legacy.Texts.Data
{
    internal class XmedTokenGroup : BlXmedToken
    {
        public enum TokenGroupType
        {
             Unknown,
             C2Group,
             MainGroup,
             Struct,
            Run
        }

        public TokenGroupType GroupType { get; set; }
       
        public List<BlXmedToken> Items { get; set; } = new ();


        public XmedTokenGroup(TokenType type, int start, int length, string? ascii = null, int? value = null, int? typeValue = null, bool linkToPrevious = false, byte[]? data = null) : base(type, start, length, ascii, value, typeValue, linkToPrevious, data)
        {
        }

   
    }
    internal sealed class XmedC2TokenGroup : XmedTokenGroup
    {
        public XmedC2TokenGroup(BlXmedToken token)
            : base(token.Type, token.Start, token.Length, token.Ascii, token.Value, token.TypeValue, token.LinkToPrevious, token.Data)
        {
        }
    }

    internal sealed class XmedMainTokenGroup : XmedTokenGroup
    {

        public enum MainGroupType
        {
            RunHeaderFFFF = 0xFFFF,
            RunHeader = 0x0000,
            Layout = 0x0001,
            FullText = 0x0002,
            RunStyles = 0x0004,
            RunParagraphs = 0x0005,
            Styles = 0x0006,
            Paragraphs = 0x0007,
            Fonts = 0x0008,
            SpacingDescriptor = 0x0009,
            SpacingDescriptor2 = 0x00A,
            UnknownB = 0x00B,
            UnknownC = 0x00C,
            UnknownF = 0x000F,
            Unknown13 = 0x0013,
            Unknown128 = 0x0128,
            Unknown129 = 0x0129,
        }

        public List<BlXmedToken> PreTokens { get; } = new();
        public List<BlXmedToken> PostTokens { get; } = new();
        public string BlockId { get; }
        public int DeclaredItemCount { get; }
        public List<BlXmedToken> RawTokens { get; } = new();
        public int UnknownValue1 { get; }
        public int UnknownValue2 { get; set; }
        public MainGroupType MainType { get; set; }

        public XmedMainTokenGroup(BlXmedToken header, string blockId, int tokenCount, int itemCount)
            : base(header.Type, header.Start, header.Length, header.Ascii, header.Value, header.TypeValue, header.LinkToPrevious, header.Data)
        {
            BlockId = blockId;
            UnknownValue1 = tokenCount;
            DeclaredItemCount = itemCount;
            var intBlockType = Convert.ToInt32(blockId, 16);
            MainType = (MainGroupType)intBlockType;
        }
    }
}
