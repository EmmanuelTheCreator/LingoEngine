namespace Director.Graphics
{
    public class PixelFormat
    {
        public int BytesPerPixel { get; }
        public int RBits { get; }
        public int GBits { get; }
        public int BBits { get; }
        public int ABits { get; }
        public int RShift { get; }
        public int GShift { get; }
        public int BShift { get; }
        public int AShift { get; }

        public PixelFormat(int bytesPerPixel, int rBits, int gBits, int bBits, int aBits, int rShift, int gShift, int bShift, int aShift)
        {
            BytesPerPixel = bytesPerPixel;
            RBits = rBits;
            GBits = gBits;
            BBits = bBits;
            ABits = aBits;
            RShift = rShift;
            GShift = gShift;
            BShift = bShift;
            AShift = aShift;
        }

        public static PixelFormat CreateFormatCLUT8() => new PixelFormat(1, 0, 0, 0, 0, 0, 0, 0, 0);
    }

   


}
