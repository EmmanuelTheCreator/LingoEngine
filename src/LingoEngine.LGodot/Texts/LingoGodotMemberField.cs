using Godot;
using LingoEngine.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace LingoEngine.LGodot.Texts
{
    public class LingoGodotMemberField : LingoGodotMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
    {


        public LingoGodotMemberField(ILingoFontManager lingoFontManager, ILogger<LingoGodotMemberTextBase<LingoMemberField>> logger) : base(lingoFontManager, logger)
        {
        }

    }
}
