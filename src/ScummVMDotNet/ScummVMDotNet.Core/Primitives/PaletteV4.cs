using Director.IO;

namespace Director.Primitives
{
    public class PaletteV4
    {
        public byte[] Palette
        {
            get => palette;
            set => palette = value ?? Array.Empty<byte>();
        }

        public CastMemberID Id { get; set; }

        public byte[] Colors => palette;

        public int Length => palette.Length / 3;

        private byte[] palette = Array.Empty<byte>();

        public PaletteV4() { }

        public PaletteV4(byte[] palette, CastMemberID id)
        {
            Palette = palette;
            Id = id;
        }

        public PaletteV4(PaletteV4 other)
        {
            Palette = (byte[])other.Palette.Clone();
            Id = other.Id;
        }
        public void LoadFromStream(SeekableReadStreamEndian stream, int id)
        {
            // Director palettes are usually 3 bytes per color (R, G, B)
            int length = stream.ReadUInt16(); // number of colors
            Palette = stream.ReadBytesRequired(length * 3);
            Id = new CastMemberID(id, 0); // CastLibID will be overwritten later
        }
        public string[] ToHexArray()
        {
            var hexColors = new string[Length];
            for (int i = 0; i < Length; i++)
            {
                int offset = i * 3;
                hexColors[i] = $"{palette[offset]:X2}{palette[offset + 1]:X2}{palette[offset + 2]:X2}";
            }
            return hexColors;
        }
    }


}

