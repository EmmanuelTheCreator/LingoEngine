namespace LingoEngine.Sounds
{
    public class LingoMemberSound : LingoMember
    {
        private readonly ILingoFrameworkMemberSound _lingoFrameworkMemberSound;
        public T FrameworkObj<T>() where T : ILingoFrameworkMemberSound => (T)_lingoFrameworkMemberSound;

        /// <summary>
        /// Whether this sound member is stereo (true) or mono (false). Default is mono.
        /// Lingo: the stereo of member
        /// </summary>
        public bool Stereo => _lingoFrameworkMemberSound.Stereo;
        /// <summary>
        ///  length of this audio stream, in seconds.
        /// </summary>
        public double Length => _lingoFrameworkMemberSound.Length;

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

        public LingoMemberSound(ILingoFrameworkMemberSound lingoMemberSound, int number, string name = "")
            : base(LingoMemberType.Sound, number, name)
        {
            _lingoFrameworkMemberSound = lingoMemberSound;
           
        }

        public bool IsExternal => IsLinked && !string.IsNullOrWhiteSpace(LinkedFilePath);
    }


}
