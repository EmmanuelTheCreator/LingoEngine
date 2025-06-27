using Godot;
using LingoEngine.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.LGodot.Texts
{
    public class LingoGodotMemberField : LingoGodotMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
    {


        public LingoGodotMemberField(ILingoFontManager lingoFontManager) : base(lingoFontManager)
        {
        }

    }
}
