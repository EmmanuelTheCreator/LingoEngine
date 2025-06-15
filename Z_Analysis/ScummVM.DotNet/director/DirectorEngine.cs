using Director.Primitives;

namespace Director
{
    /// <summary>
    /// Represents the main Director engine runtime.
    /// </summary>
    public class DirectorEngine
    {
        public StageWindow Stage { get; set; }

        public DirectorGameDescription Description { get; }

        public ushort Version { get; private set; }

        public Platform Platform => Description.Platform;

        public uint GameFlags => Description.Flags;

        public string GameId => Description.GameId;

        public DirectorEngine(StageWindow stage, DirectorGameDescription desc)
        {
            Stage = stage;
            Description = desc;
            Version = desc.Version;
        }

        public void SetVersion(ushort version)
        {
            Version = version;
        }

        public void ParseOptions()
        {
            // TODO: implement option parsing
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
