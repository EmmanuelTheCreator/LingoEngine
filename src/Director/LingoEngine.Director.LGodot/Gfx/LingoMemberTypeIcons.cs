using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngine.Director.LGodot.Gfx
{
    internal static class LingoMemberTypeIcons
    {
        public static string GetIcon(ILingoMember member)
        {
            return member switch
            {
                LingoMemberPicture => "🖌",
                LingoMemberSound => "🔊",
                LingoMemberField => "F",
                ILingoMemberTextBase => "T",
                _ when member.Type == LingoMemberType.Shape || member.Type == LingoMemberType.VectorShape => "📐",
                _ => string.Empty
            };
        }
    }
}
