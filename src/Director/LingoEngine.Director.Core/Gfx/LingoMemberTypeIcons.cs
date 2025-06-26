using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngine.Director.Core.Gfx;

/// <summary>
/// Helper to map cast member types to their representative editor icons.
/// </summary>
public static class LingoMemberTypeIcons
{
    public static DirEditorIcon? GetIconType(ILingoMember member)
    {
        return member switch
        {
            LingoMemberPicture => DirEditorIcon.MemberTypeBitmap,
            LingoMemberSound => DirEditorIcon.MemberTypSound,
            LingoMemberField => DirEditorIcon.MemberTypeField,
            ILingoMemberTextBase => DirEditorIcon.MemberTypeText,
            _ when member.Type == LingoMemberType.Shape || member.Type == LingoMemberType.VectorShape => DirEditorIcon.MemberTypeShape,
            _ => null
        };
    }
}
