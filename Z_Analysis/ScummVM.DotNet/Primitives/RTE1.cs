namespace Director.Primitives
{
    public class RTE1
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int Value3 { get; set; }
        public int Value4 { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();

        public static RTE1 ReadFrom(BinaryReader reader)
        {
            var rte = new RTE1
            {
                Value1 = reader.ReadInt32(),
                Value2 = reader.ReadInt32(),
                Value3 = reader.ReadInt32(),
                Value4 = reader.ReadInt32()
            };
            // Optionally read raw block
            int rawLength = reader.ReadInt32();
            if (rawLength > 0)
                rte.RawData = reader.ReadBytes(rawLength);

            return rte;
        }
    }
}
