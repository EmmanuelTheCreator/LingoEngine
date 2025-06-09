using Godot;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Texts;

namespace LingoEngineGodot.Texts
{
    public class LingoGodotMemberField : LingoGodotMemberTextBase<LingoMemberField> , ILingoFrameworkMemberField
    {


        public LingoGodotMemberField(ILingoFontManager lingoFontManager) : base(lingoFontManager)
        {
        }

    }
}
