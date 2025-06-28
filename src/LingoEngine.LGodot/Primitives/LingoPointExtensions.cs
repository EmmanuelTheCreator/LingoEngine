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
        
        public static Rect2 ToRect2(this LingoRect rect)
            => new Rect2(rect.Left, rect.Top, rect.Width, rect.Height);

        public static LingoRect ToLingoRect(this Rect2 rect)
            => LingoRect.New(rect.Position.X, rect.Position.Y, rect.Size.X, rect.Size.Y);
    }

}
