namespace LingoEngine.Bitmaps
{
    public interface ILingoImage
    {
        public int Width { get; }
        public int Height { get; }
        /// <summary>
        /// bitDepth : allowed values : 1, 2, 4, 8, 16, or 32.
        /// </summary>
        public int BitDepths { get; }
    }
    public class LingoImage : ILingoImage
    {
        public static int[] AllowedBitDepthsValues => [1, 2, 4, 8, 16, 32];
        public int Width { get; private set; }

        public int Height { get; private set; }

        public int BitDepths { get; private set; }

        public LingoImage(int width, int height, int bitDepths)
        {
            if (!AllowedBitDepthsValues.Contains(bitDepths))
                throw new ArgumentException("bitDepths must be in the range of " + string.Join(',', AllowedBitDepthsValues), nameof(BitDepths));
            Width = width;
            Height = height;
            BitDepths = bitDepths;
        }
    }
}
