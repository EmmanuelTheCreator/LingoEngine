namespace LingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Represents a single entry in the Lingo script context table.
    /// </summary>
    public class LingoContextEntry
    {
        public int Index { get; set; }
        public int NextUnused { get; set; }
        public bool Unused { get; set; }

        public LingoContextEntry(int index, int nextUnused)
        {
            Index = index;
            NextUnused = nextUnused;
            Unused = false;
        }
    }

}
