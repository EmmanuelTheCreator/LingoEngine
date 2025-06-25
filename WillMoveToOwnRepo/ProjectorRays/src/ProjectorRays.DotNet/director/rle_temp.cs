using System;
using System.Collections.Generic;

namespace ProjectorRays.Director;

/// <summary>
/// Temporary RLE decoder translated from the Python implementation at
/// https://github.com/Brian151/OpenShockwave/blob/master/tools/imports/shockabsorber/loader/rle.py
/// It converts the encoded sprite bitmap data used in Director files.
/// Many routines are currently unused but have been ported for completeness.
/// </summary>
public static class RleTemp
{
    /// <summary>Simple container for decompressed pixel data.</summary>
    public struct ImageData
    {
        public int Width;
        public int Height;
        public int FullWidth;
        public int Bpp;
        public byte[] Pixels;
    }

    /// <summary>Decode the given RLE data and convert it to an image.</summary>
    public static ImageData RleToImage(int width, int height, int fullwidth, int bpp, byte[] encoded)
    {
        return BytesToImage(RleDecode(width, height, fullwidth, bpp, encoded));
    }

    /// <summary>Convert decompressed bytes to an RGB(A) image representation.</summary>
    public static ImageData BytesToImage(ImageData image)
    {
        if (image.Pixels == null) return image;
        return image.Bpp switch
        {
            8  => Make8BitRgbImage(image.Width, image.Height, image.FullWidth, image.Pixels),
            16 => Make16BitRgbImage(image.Width, image.Height, image.FullWidth, image.Pixels),
            32 => Make32BitRgbImage(image.Width, image.Height, image.FullWidth, image.Pixels),
            _  => MakeGreyscaleImage(image.Width, image.Height, image.FullWidth, image.Pixels)
        };
    }

    /// <summary>Create an RGBA image from data and mask.</summary>
    public static ImageData BytesAndMaskToImage(ImageData image, ImageData mask)
    {
        if (image.Pixels == null) return image;
        if (image.Bpp == 16 && mask.Bpp == 8)
            return Make16BitRgbMaskedImage(image.Width, image.Height, image.FullWidth,
                mask.Width, mask.Height, mask.FullWidth, image.Pixels, mask.Pixels);
        if (image.Bpp == 32 && mask.Bpp == 8)
            return Make32BitRgbMaskedImage(image.Width, image.Height, image.FullWidth,
                mask.Width, mask.Height, mask.FullWidth, image.Pixels, mask.Pixels);
        return MakeGreyscaleImage(image.Width, image.Height, image.FullWidth, image.Pixels);
    }

    /// <summary>Decode RLE encoded byte data.</summary>
    public static ImageData RleDecode(int width, int height, int fullwidth, int bpp, byte[] encoded)
    {
        int bytesToOutput = fullwidth * height;
        byte[] res = new byte[bytesToOutput];
        int inPos = 0;
        int inLen = encoded.Length;
        int outPos = 0;
        while (inPos < inLen && outPos < bytesToOutput)
        {
            byte d = encoded[inPos++];
            if (d >= 128)
            {
                int runLength = 257 - d;
                byte v = encoded[inPos++];
                for (int i = 0; i < runLength && outPos < bytesToOutput; i++)
                    res[outPos + i] = v;
                outPos += runLength;
            }
            else
            {
                int litLength = 1 + d;
                Array.Copy(encoded, inPos, res, outPos, litLength);
                inPos += litLength;
                outPos += litLength;
            }
        }
        Console.WriteLine($"DB| rle end: out_pos: {outPos} vs. expected {bytesToOutput}; in_pos: {inPos} vs. {inLen}");
        return new ImageData { Width = width, Height = height, FullWidth = fullwidth, Bpp = bpp, Pixels = res };
    }

    private static ImageData MakeGreyscaleImage(int width, int height, int fullwidth, byte[] data)
    {
        return new ImageData { Width = width, Height = height, FullWidth = width, Bpp = 8, Pixels = data };
    }

    private static ImageData Make8BitRgbImage(int width, int height, int fullwidth, byte[] data)
    {
        var colorData = new byte[width * height * 3];
        int pos = 0;
        foreach (byte c in data)
        {
            int r = c >> 5;
            int g = (c >> 2) & 7;
            int b = c & 3;
            colorData[pos++] = (byte)(r * (255 / 7));
            colorData[pos++] = (byte)(g * (255 / 7));
            colorData[pos++] = (byte)(b * (255 / 3));
        }
        return new ImageData { Width = width, Height = height, FullWidth = width * 3, Bpp = 24, Pixels = colorData };
    }

    private static readonly byte[] ScaleTable31To255 = InitScaleTable();
    private static byte[] InitScaleTable()
    {
        var t = new byte[32];
        for (int i = 0; i < 32; i++)
            t[i] = (byte)((i * 255) / 31);
        return t;
    }

    private static ImageData Make16BitRgbImage(int width, int height, int fullwidth, byte[] data)
    {
        var colorRes = new byte[width * height * 3];
        int pos = 0;
        for (int y = 0; y < height; y++)
        {
            int rowStart = y * fullwidth;
            for (int x = 0; x < width; x++)
            {
                int pixStart = rowStart + x;
                byte high = data[pixStart];
                byte low = data[pixStart + width];
                int bits = (high << 8) | low;
                int a = bits >> 15;
                int r = (bits >> 10) & 31;
                int g = (bits >> 5) & 31;
                int b = bits & 31;
                if (a > 0) { r = 31; g = 31; b = 0; }
                colorRes[pos++] = ScaleTable31To255[r];
                colorRes[pos++] = ScaleTable31To255[g];
                colorRes[pos++] = ScaleTable31To255[b];
            }
        }
        return new ImageData { Width = width, Height = height, FullWidth = width * 3, Bpp = 24, Pixels = colorRes };
    }

    private static ImageData Make16BitRgbMaskedImage(int width, int height, int fullwidth,
        int maskWidth, int maskHeight, int maskFullWidth, byte[] data, byte[] mask)
    {
        var colorRes = new byte[width * height * 4];
        int pos = 0;
        for (int y = 0; y < height; y++)
        {
            int rowStart = y * fullwidth;
            for (int x = 0; x < width; x++)
            {
                int pixStart = rowStart + x;
                byte high = data[pixStart];
                byte low = data[pixStart + width];
                int bits = (high << 8) | low;
                int a = bits >> 15;
                int r = (bits >> 10) & 31;
                int g = (bits >> 5) & 31;
                int b = bits & 31;
                if (a > 0) { r = 31; g = 31; b = 0; }
                colorRes[pos++] = ScaleTable31To255[r];
                colorRes[pos++] = ScaleTable31To255[g];
                colorRes[pos++] = ScaleTable31To255[b];
                pos++; // leave alpha for later
            }
        }
        for (int y = 0; y < Math.Min(height, maskHeight); y++)
        {
            int posA = y * width * 4;
            int rowStart = y * maskFullWidth;
            for (int x = 0; x < Math.Min(width, maskWidth); x++)
            {
                colorRes[posA + 3] = mask[rowStart + x];
                posA += 4;
            }
        }
        return new ImageData { Width = width, Height = height, FullWidth = width * 4, Bpp = 32, Pixels = colorRes };
    }

    private static ImageData Make32BitRgbImage(int width, int height, int fullwidth, byte[] data)
    {
        var colorRes = new byte[width * height * 4];
        int pos = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorRes[pos++] = data[y * fullwidth + x];
                colorRes[pos++] = data[y * fullwidth + x + width];
                colorRes[pos++] = data[y * fullwidth + x + 2 * width];
                colorRes[pos++] = data[y * fullwidth + x + 3 * width];
            }
        }
        return new ImageData { Width = width, Height = height, FullWidth = width * 4, Bpp = 32, Pixels = colorRes };
    }

    private static ImageData Make32BitRgbMaskedImage(int width, int height, int fullwidth,
        int maskWidth, int maskHeight, int maskFullWidth, byte[] data, byte[] mask)
    {
        var colorRes = new byte[width * height * 4];
        int pos = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorRes[pos++] = data[y * fullwidth + x];
                colorRes[pos++] = data[y * fullwidth + x + width];
                colorRes[pos++] = data[y * fullwidth + x + 2 * width];
                pos++; // leave alpha for later
            }
        }
        for (int y = 0; y < Math.Min(height, maskHeight); y++)
        {
            int posA = y * width * 4;
            int rowStart = y * maskFullWidth;
            for (int x = 0; x < Math.Min(width, maskWidth); x++)
            {
                colorRes[posA + 3] = mask[rowStart + x];
                posA += 4;
            }
        }
        return new ImageData { Width = width, Height = height, FullWidth = width * 4, Bpp = 32, Pixels = colorRes };
    }
}
