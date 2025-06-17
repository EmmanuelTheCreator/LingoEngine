namespace LingoEngine.Primitives3D;

/// <summary>
/// Represents a point or direction in 3D space.
/// Mirrors Lingo's vector(x, y, z) function.
/// </summary>
public struct LingoVector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public LingoVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static LingoVector3 operator +(LingoVector3 a, LingoVector3 b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static LingoVector3 operator -(LingoVector3 a, LingoVector3 b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static LingoVector3 operator *(LingoVector3 v, float scalar) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static LingoVector3 operator /(LingoVector3 v, float scalar) =>
        new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public override string ToString() => $"({X}, {Y}, {Z})";
}
