using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Texts;

namespace LingoEngineSDL2.Texts;

public class SdlMemberField : SdlMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
{
    public SdlMemberField(ILingoFontManager fontManager) : base(fontManager) { }
}
