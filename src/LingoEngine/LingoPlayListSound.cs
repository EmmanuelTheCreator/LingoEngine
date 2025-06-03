using LingoEngine.Sounds;

namespace LingoEngine
{
    /// <summary>
    /// Represents a sound item that can be queued and played in a Lingo sound channel using setPlayList() or queue().
    /// This object encapsulates all playback parameters used for fine control over audio behavior.
    /// </summary>
    public class LingoPlayListSound
    {
        public LingoPlayListSound(LingoMemberSound member)
        {
            Member = member;
        }

        /// <summary>
        /// The sound cast member associated with this playlist entry.
        /// </summary>
        public LingoMemberSound Member { get; private set; }

        /// <summary>
        /// The start time (in milliseconds) from which the sound should begin playback.
        /// If not set, playback starts from the beginning.
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// The end time (in milliseconds) at which playback should stop.
        /// If not set, the sound will play to its natural end.
        /// </summary>
        public float EndTime { get; set; }

        /// <summary>
        /// Specifies how many times the sound should loop.
        /// A value of 0 means infinite looping.
        /// </summary>
        public int LoopCount { get; set; } = 1;

        /// <summary>
        /// The loop start time (in milliseconds). When looping, playback will jump back to this point.
        /// Defaults to StartTime if not explicitly set.
        /// </summary>
        public float LoopStartTime { get; set; }

        /// <summary>
        /// The loop end time (in milliseconds). When playback reaches this point, it loops back to LoopStartTime.
        /// </summary>
        public float LoopEndTime { get; set; }

        /// <summary>
        /// The amount of sound to preload (in milliseconds) before playback begins.
        /// Helps prevent stuttering and ensures accurate timing.
        /// Default is 1500ms in Director unless overridden.
        /// </summary>
        public float PreloadTime { get; set; }

        /// <summary>
        /// (Optional) The pan setting for the sound, ranging from -100 (left) to 100 (right). 0 is centered.
        /// Not part of the original playlist API, but aligns with the pan property of sound channels.
        /// </summary>
        public int? Pan { get; set; }

        /// <summary>
        /// (Optional) The volume setting for this sound (0–255). Overrides the channel’s volume if specified.
        /// </summary>
        public int? Volume { get; set; }
    }

}
