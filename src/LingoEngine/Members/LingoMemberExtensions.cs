using LingoEngine.Primitives;

namespace LingoEngine.Members;

public static class LingoMemberExtensions
{
    /// <summary>
    /// Calculates the offset required to shift a member's center
    /// to its registration point.
    /// </summary>
    /// <param name="member">The cast member.</param>
    /// <returns>The center-to-registration offset.</returns>
    public static LingoPoint CenterOffsetFromRegPoint(this ILingoMember member)
    {
        var center = new LingoPoint(member.Width / 2f, member.Height / 2f);
        // RegPoint coordinates originate from the picture's top-left corner
        return member.RegPoint - center;
    }
}
