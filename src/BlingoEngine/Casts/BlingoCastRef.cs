using System;

namespace BlingoEngine.Casts;

public readonly struct BlingoCastRef : IEquatable<BlingoCastRef>
{
    public BlingoCastRef(int castLibNum)
    {
        CastLibNum = castLibNum;
    }

    public int CastLibNum { get; }

    public static BlingoCastRef FromCast(IBlingoCast cast)
    {
        if (cast == null)
            throw new ArgumentNullException(nameof(cast));

        return new BlingoCastRef(cast.Number);
    }

    public bool Equals(BlingoCastRef other) => CastLibNum == other.CastLibNum;

    public override bool Equals(object? obj) => obj is BlingoCastRef other && Equals(other);

    public override int GetHashCode() => CastLibNum;

    public override string ToString() => CastLibNum.ToString();
}
