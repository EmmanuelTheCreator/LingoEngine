namespace OldBlingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Represents a single entry in the Lingo script context table.
    /// </summary>
    public class BlingoContextEntry
    {
        public int Index { get; set; }
        public int NextUnused { get; set; }
        public bool Unused { get; set; }

        public BlingoContextEntry(int index, int nextUnused)
        {
            Index = index;
            NextUnused = nextUnused;
            Unused = false;
        }
    }

}

