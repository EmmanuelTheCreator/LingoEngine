using Director.Graphics;

namespace Director.Tools
{

    public class BitdDecoder
    {
        private readonly int _pitch;
        private readonly ushort _version;
        private readonly ushort _bitsPerPixel;
        private Surface _surface;
        private Palette _palette;

        public BitdDecoder(int w, int h, ushort bitsPerPixel, ushort pitch, byte[] palette, ushort version)
        {
            _surface = new Surface();
            _pitch = pitch;
            _version = version;
            _bitsPerPixel = bitsPerPixel;

            int minPitch = (w * bitsPerPixel >> 3) + (w * bitsPerPixel % 8 > 0 ? 1 : 0);
            if (_pitch < minPitch)
            {
                _pitch = minPitch;
            }

            var format = bitsPerPixel switch
            {
                <= 8 => PixelFormat.CreateFormatCLUT8(),
                16 => new PixelFormat(2, 5, 5, 5, 0, 10, 5, 0, 0),
                32 => new PixelFormat(4, 8, 8, 8, 0, 16, 8, 0, 0),
                _ => throw new NotSupportedException($"Unsupported bpp {bitsPerPixel}")
            };

            _surface.Create(w, h, format);

            _palette = new Palette();
            _palette.Resize(255, false);
            _palette.Set(palette, 0, 255);
        }

        public bool LoadStream(Stream stream)
        {
            int x = 0, y = 0;
            var pixels = new List<int>();

            bool skipCompression = false;
            uint bytesNeed = (uint)(_pitch * _surface.Height);

            if (_bitsPerPixel != 1)
            {
                if (_version < 300)
                {
                    bytesNeed = (uint)(_surface.Width * _surface.Height * _bitsPerPixel / 8);
                    skipCompression = stream.Length >= bytesNeed;
                }
                else if (_version < 400)
                {
                    bytesNeed = (uint)(_surface.Width * _surface.Height * _bitsPerPixel / 8);
                    if ((_surface.Width & 1) != 0)
                        bytesNeed += (uint)(_surface.Height * _bitsPerPixel / 8);
                    skipCompression = stream.Length == bytesNeed;
                }
            }

            using var reader = new BinaryReader(stream);
            if (stream.Length == bytesNeed || skipCompression)
            {
                for (int i = 0; i < stream.Length; i++)
                    pixels.Add(reader.ReadByte());
            }
            else
            {
                while (stream.Position < stream.Length)
                {
                    if (_bitsPerPixel == 32 && _version < 400)
                    {
                        pixels.Add(reader.ReadByte());
                    }
                    else
                    {
                        int data = reader.ReadByte();
                        int len = data + 1;
                        if ((data & 0x80) != 0)
                        {
                            len = ((data ^ 0xFF) & 0xFF) + 2;
                            data = reader.ReadByte();
                            for (int p = 0; p < len; p++)
                                pixels.Add(data);
                        }
                        else
                        {
                            for (int p = 0; p < len; p++)
                                pixels.Add(reader.ReadByte());
                        }
                    }
                }
            }

            if (pixels.Count < bytesNeed)
            {
                int tail = (int)(bytesNeed - pixels.Count);
                for (int i = 0; i < tail; i++)
                    pixels.Add(0);
            }

            int offset = 0;
            if (_bitsPerPixel == 8 && _surface.Width < pixels.Count / _surface.Height)
                offset = _surface.Width % 2;

            for (y = 0; y < _surface.Height; y++)
            {
                for (x = 0; x < _surface.Width;)
                {
                    switch (_bitsPerPixel)
                    {
                        case 1:
                            for (int c = 0; c < 8 && x < _surface.Width; c++, x++)
                            {
                                var color = (pixels[y * _pitch + (x >> 3)] & 1 << 7 - c) != 0 ? 0xFF : 0x00;
                                _surface.SetByte(x, y, (byte)color);
                            }
                            break;
                        case 2:
                            for (int c = 0; c < 4 && x < _surface.Width; c++, x++)
                            {
                                var color = pixels[y * _pitch + (x >> 2)] >> 2 * (3 - c) & 0x03;
                                _surface.SetByte(x, y, (byte)color);
                            }
                            break;
                        case 4:
                            for (int c = 0; c < 2 && x < _surface.Width; c++, x++)
                            {
                                var color = pixels[y * _pitch + (x >> 1)] >> 4 * (1 - c) & 0x0F;
                                _surface.SetByte(x, y, (byte)color);
                            }
                            break;
                        case 8:
                            _surface.SetByte(x, y, (byte)pixels[y * _surface.Width + x + y * offset]);
                            x++;
                            break;
                        case 16:
                            uint color16;
                            if (_version < 400)
                            {
                                color16 = (uint)(pixels[y * _surface.Width * 2 + x * 2] << 8 |
                                                 pixels[y * _surface.Width * 2 + x * 2 + 1]);
                            }
                            else
                            {
                                color16 = (uint)(pixels[y * _surface.Width * 2 + x] << 8 |
                                                 pixels[y * _surface.Width * 2 + _surface.Width + x]);
                            }
                            _surface.SetUInt16(x, y, (ushort)color16);
                            x++;
                            break;
                        case 32:
                            uint color32;
                            if (_version < 400)
                            {
                                color32 = (uint)(pixels[y * _surface.Width * 4 + x * 4 + 1] << 16 |
                                                  pixels[y * _surface.Width * 4 + x * 4 + 2] << 8 |
                                                   pixels[y * _surface.Width * 4 + x * 4 + 3]);
                            }
                            else
                            {
                                color32 = (uint)(pixels[y * _surface.Width * 4 + _surface.Width + x] << 16 |
                                                  pixels[y * _surface.Width * 4 + 2 * _surface.Width + x] << 8 |
                                                   pixels[y * _surface.Width * 4 + 3 * _surface.Width + x]);
                            }
                            _surface.SetUInt32(x, y, color32);
                            x++;
                            break;
                        default:
                            x++;
                            break;
                    }
                }
            }

            return true;
        }

    }

}
