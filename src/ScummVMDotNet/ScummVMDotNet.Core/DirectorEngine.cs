using Director.Primitives;

namespace Director
{
    /// <summary>
    /// Represents the main Director engine runtime.
    /// </summary>
    public class DirectorEngine
    {
        public StageWindow Stage { get; set; }

        public DirectorEngine(StageWindow stage)
        {
            Stage = stage;
        }




        /// <summary>
        /// Converts a 16-bit RGB 555 color to a LingoColor (full RGB).
        /// </summary>
        /// <param name="rgb555">The 16-bit color value.</param>
        public LingoColor TransformColor(ushort rgb555)
        {
            return new LingoColor(rgb555);
        }
    }
}
