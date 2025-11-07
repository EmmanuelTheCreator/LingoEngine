namespace BlingoEngine.IO.Legacy.Cast.Data
{
    internal class BlCastRawMemberBitmap : BlCastRawMemberItem
    {
        public int Height { get; internal set; }
        public int Width { get; internal set; }
        public bool Trim { get; internal set; } = true;

        public BlCastRawMemberBitmap()
        {
            MemberType = BlLegacyCastMemberType.Bitmap;
        }
        public BitmapCompressionType CompressionType { get; set; } = BitmapCompressionType.JPEG;
        public byte PaletteId { get; internal set; }
        public byte CompressionAmount { get; internal set; }
        public int LocH { get; internal set; }
        public int LocV { get; internal set; }

        public enum BitmapCompressionType
        {
            MovieSetting,
            Standard,
            JPEG,
        }
    }
    internal class BlCastMemberGif : BlCastRawMemberItem
    {
        public int Height { get; internal set; }
        public int Width { get; internal set; }
        public int FrameCount { get; internal set; }

        public BlCastMemberGif()
        {
            MemberType = BlLegacyCastMemberType.Bitmap;
        }
    }
}
