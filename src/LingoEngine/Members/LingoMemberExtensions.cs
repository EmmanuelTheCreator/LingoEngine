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
        // RegPoint coordinates originate from the picture's top-right corner
        var regFromTopRight = new LingoPoint(member.Width - member.RegPoint.X,
            member.RegPoint.Y);
        return regFromTopRight - center;
    }
}
