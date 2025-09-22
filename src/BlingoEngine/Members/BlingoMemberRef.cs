using System;

namespace BlingoEngine.Members;

public readonly struct BlingoMemberRef : IEquatable<BlingoMemberRef>
{
    public BlingoMemberRef(int castLibNum, int memberNum, BlingoMemberType memberType)
    {
        CastLibNum = castLibNum;
        MemberNum = memberNum;
        MemberType = memberType;
    }

    public int CastLibNum { get; }

    public int MemberNum { get; }

    public BlingoMemberType MemberType { get; }

    public static BlingoMemberRef FromMember(IBlingoMember member)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        return new BlingoMemberRef(member.CastLibNum, member.NumberInCast, member.Type);
    }

    public bool Equals(BlingoMemberRef other) => CastLibNum == other.CastLibNum && MemberNum == other.MemberNum && MemberType == other.MemberType;

    public override bool Equals(object? obj) => obj is BlingoMemberRef other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = CastLibNum;
            hash = (hash * 397) ^ MemberNum;
            hash = (hash * 397) ^ (int)MemberType;
            return hash;
        }
    }

    public override string ToString() => $"{MemberType},{CastLibNum},{MemberNum}";
}
