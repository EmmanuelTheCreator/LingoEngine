using LingoEngine.Events;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.LGodot.Texts
{
    public class LingoGodotMemberText : LingoGodotMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
    {
        public LingoGodotMemberText(ILingoFontManager lingoFontManager) : base(lingoFontManager)
        {
        }
    }
}
