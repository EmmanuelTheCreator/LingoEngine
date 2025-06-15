namespace Director.Primitives
{
    public class ResourceChunk
    {
        public uint Tag { get; set; }
        public int Index { get; set; }

        public ResourceChunk(uint tag, int index)
        {
            Tag = tag;
            Index = index;
        }
    }
}
