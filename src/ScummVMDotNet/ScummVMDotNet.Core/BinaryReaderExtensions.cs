using Director.Graphics;

namespace Director
{
    public static class BinaryReaderExtensions
    {
        public static short ReadInt16BE(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static ushort ReadUInt16BE(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static int ReadInt32BE(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static uint ReadUInt32BE(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static Rect ReadRect(this BinaryReader reader)
        {
            return Rect.ReadFrom(reader);
        }
    }

}


