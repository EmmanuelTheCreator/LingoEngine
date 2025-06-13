namespace Director
{
    public class Resource
    {

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public Resource(byte[] data)
        {
            Data = data ?? Array.Empty<byte>();
        }

        public Resource() { }
    }

}
