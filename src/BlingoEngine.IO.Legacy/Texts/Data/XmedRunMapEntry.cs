namespace BlingoEngine.IO.Legacy.Texts.Data
{
    /// <summary>20-char token entry.</summary>
    public sealed class XmedRunMapEntry
    {
        public XmedRunMapEntry(ushort type, ushort f2, ushort length, ushort f4, ushort styleId, long position)
        {
            Type = type; F2 = f2; Length = length; F4 = f4; StyleId = styleId; Position = position;
        }
        public ushort Type { get; }
        public ushort F2 { get; }
        public ushort Length { get; }
        public ushort F4 { get; }
        public ushort StyleId { get; }
        public int End => (int)Position + Length;
        public long Position { get; }
    }
}
