namespace Director.Primitives
{
    public class RTE2
    {
        public int Code { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public static RTE2 ReadFrom(BinaryReader reader)
        {
            var rte = new RTE2
            {
                Code = reader.ReadInt32(),
                Length = reader.ReadInt32()
            };

            if (rte.Length > 0)
                rte.Data = reader.ReadBytes(rte.Length);

            return rte;
        }
    }
    }


