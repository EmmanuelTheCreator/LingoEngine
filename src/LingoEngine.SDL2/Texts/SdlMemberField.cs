using LingoEngine.Events;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.SDL2.Texts;

public class SdlMemberField : SdlMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
{
    public SdlMemberField(ILingoFontManager fontManager) : base(fontManager) { }
}
