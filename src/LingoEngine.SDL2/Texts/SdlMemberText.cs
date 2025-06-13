using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Texts;

namespace LingoEngineSDL2.Texts;

public class SdlMemberText : SdlMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
{
    public SdlMemberText(ILingoFontManager fontManager) : base(fontManager) { }
}
