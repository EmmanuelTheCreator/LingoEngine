using Director.Graphics;
using Director.IO;
using Director.Primitives;
using System;
using System.IO;
using System.Text;

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
        private Picture _picture = new();
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

        public BitmapCastMember(Cast cast, int castId, SeekableReadStreamEndian stream, ushort version)
     : base(cast, castId, stream)
        {
            using var reader = new BinaryReader(stream.BaseStream, Encoding.Default, leaveOpen: true);

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

            if (version < 400)
            {
                _flags1 = Flags1;
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
            else if (version >= 400 && version < 600)
            {
                _flags1 = Flags1;
                _pitch = reader.ReadUInt16BE() & 0x0FFF;
                _initialRect = reader.ReadRect();
                _boundingRect = reader.ReadRect();
                _regY = reader.ReadUInt16BE();
                _regX = reader.ReadUInt16BE();
                _bitsPerPixel = 0;

                if (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    reader.ReadByte(); // unknown
                    _bitsPerPixel = reader.ReadByte();

                    int clutCastLib = -1;
                    if (version >= 500)
                        clutCastLib = reader.ReadInt16BE();

                    int clutId = reader.ReadInt16BE();
                    if (clutId <= 0)
                        _clut = new CastMemberID(clutId - 1, -1);
                    else
                    {
                        if (clutCastLib == -1)
                            clutCastLib = cast.CastLibId;
                        _clut = new CastMemberID(clutId, clutCastLib);
                    }

                    if (reader.BaseStream.Position + 12 <= reader.BaseStream.Length)
                    {
                        reader.ReadUInt16BE(); // unknown1
                        reader.ReadUInt16BE(); // unknown2
                        reader.ReadUInt16BE(); // unknown3
                        reader.ReadUInt32BE(); // unknown4
                        reader.ReadUInt32BE(); // unknown5
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
                throw new NotSupportedException($"Bitmaps not yet supported for version {version}");
            }

            _tag = Tag;
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

        /// <summary>
        /// Copy constructor used when duplicating bitmap members.
        /// </summary>
        public BitmapCastMember(Cast cast, int castId, BitmapCastMember source)
            : base(cast, castId, CastType.Bitmap)
        {
            source.Load();
            _loaded = true;

            _initialRect = source._initialRect;
            _boundingRect = source._boundingRect;
            if (cast == source._cast)
                _children = source._children;

            _picture = new Picture(source._picture.Width, source._picture.Height, source._picture.BitsPerPixel, source._picture.Pixels, source._picture.Clut);
            _ditheredImg = null;
            _matte = null;
            _pitch = source._pitch;
            _regX = source._regX;
            _regY = source._regY;
            _flags2 = source._flags2;
            _bytes = source._bytes;
            _clut = source._clut;
            _ditheredTargetClut = source._ditheredTargetClut;
            _bitsPerPixel = source._bitsPerPixel;
            _tag = source._tag;
            _noMatte = source._noMatte;
            _external = source._external;

            Width = source.Width;
            Height = source.Height;
            BitsPerPixel = source.BitsPerPixel;
            RowBytes = source.RowBytes;
            ImageDepth = source.ImageDepth;
            FormatFlags = source.FormatFlags;
            IsCompressed = source.IsCompressed;
            ImageSize = source.ImageSize;
            PixelData = (byte[])source.PixelData.Clone();
        }

        /// <summary>
        /// Creates a duplicate of this cast member.
        /// </summary>
        public override CastMember Duplicate(Cast cast, int castId)
        {
            return new BitmapCastMember(cast, castId, this);
        }

        public override string FormatInfo()
        {
            return $"initialRect:{_initialRect.Width}x{_initialRect.Height}@{_initialRect.Left},{_initialRect.Top}, " +
                   $"boundingRect:{_boundingRect.Width}x{_boundingRect.Height}@{_boundingRect.Left},{_boundingRect.Top}, " +
                   $"regX:{_regX}, regY:{_regY}, pitch:{_pitch}, bpp:{_bitsPerPixel}";
        }

        public LingoPoint GetRegistrationOffset()
            => new LingoPoint(_regX - _initialRect.Left, _regY - _initialRect.Top);

        public LingoPoint GetRegistrationOffset(int width, int height)
        {
            var offset = GetRegistrationOffset();
            return new LingoPoint(offset.X * width / Math.Max(1, _initialRect.Width),
                                  offset.Y * height / Math.Max(1, _initialRect.Height));
        }

        public bool IsModified() => _modified;

        public override void Unload()
        {
            PixelData = Array.Empty<byte>();
            _picture = new Picture();
            _loaded = false;
        }

    }

}
