using Godot;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Primitives
{
    public static class LingoColorExtensions
    {
        /// <summary>
        /// Converts a Godot Color to a LingoColor.
        /// </summary>
        public static LingoColor ToLingoColor(this Color color)
        {
            return LingoColor.FromRGB(
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255)
            );
        }

        /// <summary>
        /// Converts a LingoColor to a Godot Color.
        /// </summary>
        public static Color ToGodotColor(this LingoColor color)
        {
            return new Color(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f
            );
        }
    }

}
