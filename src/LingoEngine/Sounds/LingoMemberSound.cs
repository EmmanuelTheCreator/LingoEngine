namespace LingoEngine.Sounds
{
    public class LingoMemberSound : LingoMember
    {
        /// <summary>
        /// Whether this sound member is stereo (true) or mono (false). Default is mono.
        /// Lingo: the stereo of member
        /// </summary>
        public bool Stereo { get; set; } = false;

        /// <summary>
        /// Indicates whether this sound loops by default.
        /// Lingo: the loop of member
        /// </summary>
        public bool Loop { get; set; } = false;

        /// <summary>
        /// Whether this member is externally linked (not embedded).
        /// Lingo: the linked of member
        /// </summary>
        public bool IsLinked { get; set; } = false;

        /// <summary>
        /// The path of the external file, if linked. Optional.
        /// </summary>
        public string LinkedFilePath { get; set; } = string.Empty;

        public LingoMemberSound(int index, string name = "")
            : base(LingoMemberType.Sound, index, name)
        {
        }

        public bool IsExternal => IsLinked && !string.IsNullOrWhiteSpace(LinkedFilePath);
    }


}
