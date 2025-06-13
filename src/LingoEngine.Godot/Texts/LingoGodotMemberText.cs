using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Texts;

namespace LingoEngineGodot.Texts
{
    public class LingoGodotMemberText : LingoGodotMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
    {
        public LingoGodotMemberText(ILingoFontManager lingoFontManager) : base(lingoFontManager)
        {
        }
    }
}
