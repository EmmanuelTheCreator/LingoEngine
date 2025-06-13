using Director.IO;

namespace Director.Fonts
{
    public class FontStyle
    {
        public uint FormatStartOffset { get; set; }
        public ushort Height { get; set; }
        public ushort Ascent { get; set; }

        public ushort FontId { get; set; }
        public byte TextSlant { get; set; }
        public ushort FontSize { get; set; }

        public ushort R { get; set; }
        public ushort G { get; set; }
        public ushort B { get; set; }

        public FontStyle()
        {
            FormatStartOffset = 0;
            Height = 0;
            Ascent = 0;
            FontId = 0;
            TextSlant = 0;
            FontSize = 12;
            R = G = B = 0;
        }

        public void Read(SeekableReadStreamEndian reader, Cast cast)
        {
            FormatStartOffset = reader.ReadUInt32();
            ushort originalHeight = reader.ReadUInt16();
            Height = originalHeight;
            Ascent = reader.ReadUInt16();

            ushort originalFontId = reader.ReadUInt16();
            FontId = originalFontId;
            TextSlant = reader.ReadByte();
            reader.ReadByte(); // padding
            FontSize = reader.ReadUInt16();

            R = reader.ReadUInt16();
            G = reader.ReadUInt16();
            B = reader.ReadUInt16();

            if (cast.FontMap.TryGetValue(originalFontId, out var fontMapEntry))
            {
                FontId = fontMapEntry.ToFont;
                if (fontMapEntry.SizeMap.TryGetValue(originalHeight, out var newHeight))
                {
                    Height = newHeight;
                }
            }

            // Optional: Logging
            //Console.WriteLine($"FontStyle::Read(): FormatStartOffset: {FormatStartOffset}, Height: {originalHeight} -> {Height}, Ascent: {Ascent}, FontId: {originalFontId} -> {FontId}, TextSlant: {TextSlant}, FontSize: {FontSize}, R: {R:X}, G: {G:X}, B: {B:X}");
        }
    }

}
