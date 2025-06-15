namespace Director.Scripts
{
    /// <summary>
    /// Flags used when registering or processing Lingo scripts.
    /// </summary>
    [Flags]
    public enum ScriptFlags
    {
        /// <summary>No special behavior.</summary>
        None = 0,

        /// <summary>Trim trailing garbage bytes from the script.</summary>
        TrimGarbage = 1 << 0,

        /// <summary>The script is stored in compressed form.</summary>
        Compressed = 1 << 1,

        /// <summary>The script is encrypted.</summary>
        Encrypted = 1 << 2,

        /// <summary>Skip parsing source; use bytecode only.</summary>
        BytecodeOnly = 1 << 3
    }

}
