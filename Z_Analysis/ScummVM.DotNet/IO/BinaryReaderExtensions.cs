using Director.Graphics;
using System.Text;

namespace Director.IO
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

        //public static uint ReadUInt32BE(this BinaryReader reader)
        //{
        //    var bytes = reader.ReadBytes(4);
        //    if (BitConverter.IsLittleEndian)
        //        Array.Reverse(bytes);
        //    return BitConverter.ToUInt32(bytes, 0);
        //}
        public static uint ReadUInt32BE(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        public static Rect ReadRect(this BinaryReader reader)
        {
            return Rect.ReadFrom(reader);
        }
        public static string ReadChunkID(this BinaryReader reader)
        {
            string tagStr = Encoding.ASCII.GetString(reader.ReadBytes(4));
            return tagStr;
        }
        public static short ReadMotorolaInt16(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(2);
            return (short)((bytes[0] << 8) | bytes[1]);
        }
        public static int ReadMotorolaInt32(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }

       
        public static sbyte ReadInt8(this BinaryReader reader)
        {
            return (sbyte)reader.ReadByte();
        }
        public static ushort ReadMotorolaUInt16(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(2);
            if (bytes.Length < 2)
                throw new EndOfStreamException("Unexpected end of stream while reading UInt16.");

            return (ushort)((bytes[0] << 8) | bytes[1]);
        }

        public static sbyte ReadMotorolaInt8(this BinaryReader reader)
        {
            return (sbyte)reader.ReadByte();
        }
        public static uint ReadMotorolaUInt32(this BinaryReader reader)
        {
            byte[] b = reader.ReadBytes(4);
            if (b.Length < 4)
                throw new EndOfStreamException("Unexpected end of stream while reading UInt32.");
            return (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
        }
        public static float ReadFloat32BE(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }
        public static List<byte> ReadColor(this BinaryReader reader, ushort fileVersion)
        {
            var r = reader.ReadByte();
            if (fileVersion <= 0x4C7)
                r = 255;
            var g = reader.ReadByte();
            if (fileVersion <= 0x4C7)
                g = 255;

            var b = reader.ReadByte();
            if (fileVersion <= 0x4C7)
                b = 255;

            var bgColor = new List<byte> { r, g, b };
            return bgColor;
        }
        public static int[] ReadFArray(this BinaryReader reader, int count, int elementSize)
        {
            var result = new int[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = elementSize switch
                {
                    1 => reader.ReadByte(), // UINT8
                    2 => ReadMotorolaInt16(reader), // INT16
                    4 => ReadMotorolaInt32(reader), // INT32
                    _ => throw new NotSupportedException($"Unsupported FArray element size: {elementSize}")
                };
            }

            return result;
        }
        public static string ReadPascalString(this BinaryReader reader, int length)
        {
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(bytes);
        }

        public static uint ReadVarInt(this BinaryReader reader)
        {
            uint value = 0;
            byte b;
            do
            {
                b = reader.ReadByte();
                value = (value << 7) | (uint)(b & 0x7F);
            } while ((b & 0x80) != 0);
            return value;
        }

        public static Guid ReadGuidBE(this BinaryReader reader)
        {
            uint d1 = reader.ReadUInt32BE();
            ushort d2 = reader.ReadUInt16BE();
            ushort d3 = reader.ReadUInt16BE();
            byte[] d4 = reader.ReadBytes(8);
            return new Guid((int)d1, (short)d2, (short)d3, d4);
        }

        public static string ReadCString(this BinaryReader reader)
        {
            using var ms = new MemoryStream();
            byte ch;
            while ((ch = reader.ReadByte()) != 0)
                ms.WriteByte(ch);
            return Encoding.ASCII.GetString(ms.ToArray());
        }
    }

}


