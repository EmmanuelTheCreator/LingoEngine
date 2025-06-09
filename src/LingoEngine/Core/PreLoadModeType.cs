namespace LingoEngine.Core
{
    public enum PreLoadModeType
    {
        /// <summary>
        /// Load the cast library when needed. This is the default value
        /// </summary>
        WhenNeeded = 0,
        /// <summary>
        /// Load the cast library before frame 1
        /// </summary>
        BeforeFrame1,
        /// <summary>
        /// Load the cast library after frame 1
        /// </summary>
        AfterFrame1,
    }
}
