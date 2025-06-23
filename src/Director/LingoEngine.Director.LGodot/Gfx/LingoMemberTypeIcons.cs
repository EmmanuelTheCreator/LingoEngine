using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Texts;


namespace LingoEngine.Director.LGodot.Gfx
{
    internal static class LingoMemberTypeIcons
    {
        public static DirGodotEditorIcon? GetIcon(ILingoMember member)
        {
            return member switch
            {
                LingoMemberPicture => DirGodotEditorIcon.MemberTypeBitmap,
                LingoMemberSound => DirGodotEditorIcon.MemberTypSound,
                LingoMemberField => DirGodotEditorIcon.MemberTypeField,
                ILingoMemberTextBase => DirGodotEditorIcon.MemberTypeText,
                _ when member.Type == LingoMemberType.Shape || member.Type == LingoMemberType.VectorShape => DirGodotEditorIcon.MemberTypeShape,
                _ => null
            };
        }
    }
}
