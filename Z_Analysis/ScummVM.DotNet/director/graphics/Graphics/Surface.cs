namespace Director.Graphics
{

    public class Surface : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public PixelFormat Format { get; private set; }
        public byte[] Pixels { get; private set; } = Array.Empty<byte>();

        public void Create(int width, int height, PixelFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
            Pixels = new byte[width * height * format.BytesPerPixel];
        }

        public nint GetBasePtr(int x, int y)
        {
            int index = (y * Width + x) * Format.BytesPerPixel;
            unsafe
            {
                fixed (byte* ptr = &Pixels[0])
                {
                    return (nint)(ptr + index);
                }
            }
        }

        public void SetByte(int x, int y, byte value)
        {
            if (Format.BytesPerPixel != 1)
                throw new InvalidOperationException("SetByte only supports 8-bit surfaces");
            int index = (y * Width + x);
            Pixels[index] = value;
        }
        public void SetUInt16(int x, int y, ushort value)
        {
            if (Format.BytesPerPixel != 2)
                throw new InvalidOperationException("SetUInt16 only supports 16-bit surfaces");
            int index = (y * Width + x) * 2;
            Pixels[index] = (byte)(value & 0xFF);
            Pixels[index + 1] = (byte)((value >> 8) & 0xFF);
        }

        public void SetUInt32(int x, int y, uint value)
        {
            if (Format.BytesPerPixel != 4)
                throw new InvalidOperationException("SetUInt32 only supports 32-bit surfaces");
            int index = (y * Width + x) * 4;
            Pixels[index] = (byte)(value & 0xFF);
            Pixels[index + 1] = (byte)((value >> 8) & 0xFF);
            Pixels[index + 2] = (byte)((value >> 16) & 0xFF);
            Pixels[index + 3] = (byte)((value >> 24) & 0xFF);
        }


        public void CopyFrom(Surface src)
        {
            if (src.Width != Width || src.Height != Height || src.Format.BytesPerPixel != Format.BytesPerPixel)
                throw new InvalidOperationException("Surface formats or sizes do not match");
            Array.Copy(src.Pixels, Pixels, Pixels.Length);
        }

        public Surface Scale(int newWidth, int newHeight, bool smooth)
        {
            // Nearest-neighbor scaling
            Surface scaled = new Surface();
            scaled.Create(newWidth, newHeight, Format);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = x * Width / newWidth;
                    int srcY = y * Height / newHeight;

                    for (int b = 0; b < Format.BytesPerPixel; b++)
                    {
                        scaled.Pixels[(y * newWidth + x) * Format.BytesPerPixel + b] =
                            Pixels[(srcY * Width + srcX) * Format.BytesPerPixel + b];
                    }
                }
            }

            return scaled;
        }

        public Surface ConvertTo(PixelFormat newFormat, byte[] srcPalette, int srcPaletteSize, byte[] dstPalette, int dstPaletteSize)
        {
            // Simplified conversion assuming same pixel count and palette mapping
            if (newFormat.BytesPerPixel == Format.BytesPerPixel)
                return this;

            Surface converted = new Surface();
            converted.Create(Width, Height, newFormat);

            if (Format.BytesPerPixel == 1 && newFormat.BytesPerPixel == 1 && srcPalette != null && dstPalette != null)
            {
                // For example: map palette indices (simplified: 1:1 copy)
                Array.Copy(Pixels, converted.Pixels, Pixels.Length);
            }
            else
            {
                // Fallback: clear data or no-op
                Array.Clear(converted.Pixels, 0, converted.Pixels.Length);
            }

            return converted;
        }

        public void Free()
        {
            Pixels = Array.Empty<byte>();
        }

        public void Dispose()
        {
            Free();
        }
    }

}
