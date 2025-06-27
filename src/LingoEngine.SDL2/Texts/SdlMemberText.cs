using LingoEngine.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.SDL2.Texts;

public class SdlMemberText : SdlMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
{
    public SdlMemberText(ILingoFontManager fontManager) : base(fontManager) { }
}
