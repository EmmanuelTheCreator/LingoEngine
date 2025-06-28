using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngine.Director.Core.Icons;

/// <summary>
/// Helper to map cast member types to their representative editor icons.
/// </summary>
public static class LingoMemberTypeIcons
{
    public static DirectorIcon? GetIconType(ILingoMember member)
    {
        return member switch
        {
            LingoMemberBitmap => DirectorIcon.MemberTypeBitmap,
            LingoMemberSound => DirectorIcon.MemberTypSound,
            LingoMemberField => DirectorIcon.MemberTypeField,
            ILingoMemberTextBase => DirectorIcon.MemberTypeText,
            _ when member.Type == LingoMemberType.Shape || member.Type == LingoMemberType.VectorShape => DirectorIcon.MemberTypeShape,
            _ => null
        };
    }
}
