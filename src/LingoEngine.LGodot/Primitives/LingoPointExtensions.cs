using Godot;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Primitives
{
    public static class LingoPointExtensions
        {
            public static Vector2 ToVector2(this LingoPoint point)
                => new Vector2(point.X, point.Y);

            public static LingoPoint ToLingoPoint(this Vector2 vector)
                => new LingoPoint(vector.X, vector.Y);
        }
}
