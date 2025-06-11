using Director.Tools;

namespace Director
{
    /// <summary>
    /// Represents the Score timeline of a Director movie (frames, actions, sprites).
    /// </summary>
    public class Score
    {
        /// <summary>Current frame rate of the movie (frames per second).</summary>
        public ushort CurrentFrameRate { get; set; }

        /// <summary>
        /// Loads the frame data from a stream.
        /// </summary>
        /// <param name="stream">The input stream containing VWSC (score) data.</param>
        /// <param name="version">The Director file format version.</param>
        public void LoadFrames(SeekableReadStreamEndian stream, int version)
        {
            // TODO: Parse VWSC data (score stream)
            // This includes channel counts, frame offsets, keyframes, etc.
        }

        /// <summary>
        /// Loads scripted actions associated with the score.
        /// </summary>
        /// <param name="stream">The input stream containing VWAC (action list) data.</param>
        public void LoadActions(SeekableReadStreamEndian stream)
        {
            // TODO: Parse VWAC script/action data and link to timeline
        }
    }
}
