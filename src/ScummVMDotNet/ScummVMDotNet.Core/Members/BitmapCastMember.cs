using Director.Graphics;
using Director.Primitives;
using System;
using System.IO;

namespace Director.Members
{

    public class BitmapCastMember : CastMember
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BitsPerPixel { get; private set; }
        public byte[] PixelData { get; private set; } = Array.Empty<byte>();
        public int RowBytes { get; private set; }
        public int FormatFlags { get; private set; }
        public int ImageDepth { get; private set; }
        public bool IsCompressed { get; private set; }
        public int ImageSize { get; private set; }

        // Internal state (from original C++)
        private Pictere _picture = new();
        private object? _ditheredImg = null;
        private object? _matte = null;
        private bool _noMatte = false;
        private int _bytes = 0;
        private int _pitch = 0;
        private int _flags2 = 0;
        private int _regX = 0;
        private int _regY = 0;
        private CastMemberID _clut = new(0, 0);
        private CastMemberID _ditheredTargetClut = new(0, 0);
        private int _bitsPerPixel = 0;
        private bool _external = false;

        public BitmapCastMember(Cast parent, int id)
            : base(parent, id, CastType.Bitmap)
        {
        }

        public override void LoadFromStream(Stream stream, Resource res)
        {
            using var reader = new BinaryReader(stream);

            _picture = new Picture();
            _ditheredImg = null;
            _matte = null;
            _noMatte = false;
            _bytes = 0;
            _pitch = 0;
            _flags2 = 0;
            _regX = _regY = 0;
            _clut = new CastMemberID(0, 0);
            _ditheredTargetClut = new CastMemberID(0, 0);
            _bitsPerPixel = 0;
            _external = false;
            // A little context about how bitmap bounding boxes are stored.
            // In the Director editor, images can be edited on a big scrolling canvas with
            // the image in the middle. _initialRect describes the location on that virtual
            // canvas, with the top-left being the start position of the image.
            // _regX and _regY is the registration offset, in canvas space.
            // This means if a bitmap cast member is placed at (64, 64) on the score, the
            // registration offset of the image is placed at (64, 64).
            // By default the registration offset is the dead centre of the image.
            // _boundingRect I think is used internally by the editor and not elsewhere.
            if (_version < 400)
            {
                _flags1 = res.Flags1;
                _bytes = reader.ReadUInt16BE();
                _initialRect = reader.ReadRect();
                _boundingRect = reader.ReadRect();
                _regY = reader.ReadInt16BE();
                _regX = reader.ReadInt16BE();

                if ((_bytes & 0x8000) != 0)
                {
                    _bitsPerPixel = reader.ReadUInt16BE();
                    int clutId = reader.ReadInt16BE();
                    _clut = clutId <= 0 ? new CastMemberID(clutId - 1, -1) : new CastMemberID(clutId, Cast.DEFAULT_CAST_LIB);
                }
                else
                {
                    _bitsPerPixel = 1;
                    _clut = new CastMemberID(Cast.KClutSystemMac, -1);
                }

                _pitch = _initialRect.Width;
                if (_pitch % 16 != 0)
                    _pitch += 16 - _initialRect.Width % 16;
                _pitch *= _bitsPerPixel;
                _pitch >>= 3;

            }
            else if (_version >= 400 && _version < 600)
            {
                _flags1 = res.Flags1;
                _pitch = reader.ReadUInt16BE() & 0x0FFF;
                _initialRect = reader.ReadRect();
                _boundingRect = reader.ReadRect();
                _regY = reader.ReadUInt16BE();
                _regX = reader.ReadUInt16BE();
                _bitsPerPixel = 0;

                if (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    reader.ReadByte();
                    _bitsPerPixel = reader.ReadByte();

                    int clutCastLib = -1;
                    if (_version >= 500)
                        clutCastLib = reader.ReadInt16BE();

                    int clutId = reader.ReadInt16BE();
                    if (clutId <= 0)
                        _clut = new CastMemberID(clutId - 1, -1);
                    else
                    {
                        if (clutCastLib == -1)
                            clutCastLib = _cast._castLibID;
                        _clut = new CastMemberID(clutId, clutCastLib);
                    }

                    if (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        reader.ReadUInt16BE();
                        reader.ReadUInt16BE();
                        reader.ReadUInt16BE();
                        reader.ReadUInt32BE();
                        reader.ReadUInt32BE();
                        _flags2 = reader.ReadUInt16BE();
                    }
                }

                if (_bitsPerPixel == 0)
                    _bitsPerPixel = 1;

                int tail = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                if (tail > 0)
                    reader.BaseStream.Seek(tail, SeekOrigin.Current);

            }
            else
            {
                throw new NotSupportedException($"Bitmaps not yet supported for version {_version}");
            }

            _tag = res.CastTag;
            Width = _boundingRect.Width;
            Height = _boundingRect.Height;
            BitsPerPixel = _bitsPerPixel;
            RowBytes = _pitch;
            ImageDepth = BitsPerPixel;
            FormatFlags = 0;
            IsCompressed = false;
            ImageSize = Width * Height * BitsPerPixel / 8;

            PixelData = reader.ReadBytes(ImageSize);
        }
    }

}
