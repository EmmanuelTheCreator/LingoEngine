namespace Director
{
    public class Resource
    {
        public uint CastTag { get; set; } = 0;
        public int Flags1 { get; set; } = 0;

        public Resource(uint castTag, int flags1)
        {
            CastTag = castTag;
            Flags1 = flags1;
        }

        public Resource() { }
    }

}
