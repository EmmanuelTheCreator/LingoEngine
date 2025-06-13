using Director.Primitives;

namespace Director.Graphics
{

    public class Picture
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitsPerPixel { get; set; }
        public byte[] Pixels { get; set; } = Array.Empty<byte>();
        public CastMemberID Clut { get; set; } = new(0, 0);

        public Picture()
        {
        }

        public Picture(int width, int height, int bpp, byte[] pixels, CastMemberID clut)
        {
            Width = width;
            Height = height;
            BitsPerPixel = bpp;
            Pixels = pixels;
            Clut = clut;
        }

        public void LoadFromStream(BinaryReader reader, int width, int height, int bitsPerPixel)
        {
            Width = width;
            Height = height;
            BitsPerPixel = bitsPerPixel;

            int byteCount = (width * height * bitsPerPixel) / 8;
            Pixels = reader.ReadBytes(byteCount);
        }

        public bool LoadDIB(BinaryReader reader)
        {
            uint headerSize = reader.ReadUInt32();
            if (headerSize != 40)
                return false;

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            if (height < 0)
            {
                // BUILDBOT: height < 0 for DIB
            }
            reader.ReadUInt16(); // planes
            BitsPerPixel = reader.ReadUInt16();
            uint compression = reader.ReadUInt32();
            reader.ReadUInt32(); // imageSize
            reader.ReadInt32(); // pixelsPerMeterX
            reader.ReadInt32(); // pixelsPerMeterY
            uint paletteColorCount = reader.ReadUInt32();
            reader.ReadUInt32(); // colorsImportant

            paletteColorCount = (paletteColorCount == 0) ? 255u : paletteColorCount;
            Width = width;
            Height = height;

            int dataSize = (Width * Height * BitsPerPixel) / 8;
            Pixels = reader.ReadBytes(dataSize);

            if (BitsPerPixel == 1)
            {
                for (int i = 0; i < Pixels.Length; i++)
                {
                    Pixels[i] = (Pixels[i] == 0x0f) ? (byte)0x00 : (byte)0xff;
                }
            }

            if (BitsPerPixel == 8)
            {
                for (int i = 0; i < Pixels.Length; i++)
                {
                    Pixels[i] = (byte)(255 - Pixels[i]);
                }
            }

            return true;
        }

        public static void CopyStretchImg(
Surface srcSurface,
Surface targetSurface,
LingoRect srcRect,
LingoRect targetRect,
byte[] palette)
        {
            if (srcSurface == null || targetSurface == null)
                return;

            if (srcSurface.Height <= 0 || srcSurface.Width <= 0)
                return;

            Surface temp1 = null;
            Surface temp2 = null;

            var targetFormat = DirectorApp.Instance._wm.PixelFormat;

            // Convert source surface to target color format if necessary
            if (srcSurface.Format.BytesPerPixel != targetFormat.BytesPerPixel)
            {
                temp1 = srcSurface.ConvertTo(
                    targetFormat,
                    DirectorApp.Instance._wm.GetPalette(),
                    DirectorApp.Instance._wm.GetPaletteSize(),
                    DirectorApp.Instance._wm.GetPalette(),
                    DirectorApp.Instance._wm.GetPaletteSize());
            }

            // Scale surface if source and target dimensions differ
            if (targetRect.Width != srcRect.Width || targetRect.Height != srcRect.Height)
            {
                temp2 = (temp1 ?? srcSurface).Scale((int)targetRect.Width, (int)targetRect.Height, false);
            }

            targetSurface.CopyFrom(temp2 ?? temp1 ?? srcSurface);

            temp1?.Free();
            temp1?.Dispose();

            temp2?.Free();
            temp2?.Dispose();
        }
    }

}


