using Director.IO;
using Director.Primitives;

namespace Director.Members
{
    /// <summary>
    /// Vector shape cast member.
    /// </summary>
    public class ShapeCastMember : CastMember
    {
        public ShapeType ShapeType { get; private set; }
        public ushort Pattern { get; private set; }
        public byte FillType { get; private set; }
        public byte LineThickness { get; private set; }
        public byte LineDirection { get; private set; }
        public InkType Ink { get; private set; }
        public LingoColor ForeColor { get; private set; }
        public LingoColor BackColor { get; private set; }

        public ShapeCastMember(Cast cast, int castId)
            : base(cast, castId, CastType.Shape)
        {
        }

        public ShapeCastMember(Cast cast, int castId, SeekableReadStreamEndian stream, ushort version)
            : base(cast, castId, stream)
        {
            _type = CastType.Shape;
            Ink = InkType.Copy;

            if (version < 400)
            {
                stream.ReadByte(); // unk1
                ShapeType = (ShapeType)stream.ReadByte();
                _initialRect = Movie.ReadRect(stream);
                Pattern = stream.ReadUInt16BE();
                ForeColor = new LingoColor((byte)((stream.ReadByte() + 128) & 0xFF));
                BackColor = new LingoColor((byte)((stream.ReadByte() + 128) & 0xFF));
                FillType = stream.ReadByte();
                Ink = (InkType)(FillType & 0x3F);
                LineThickness = stream.ReadByte();
                LineDirection = stream.ReadByte();
            }
            else if (version >= 400 && version < 600)
            {
                stream.ReadByte(); // unk1
                ShapeType = (ShapeType)stream.ReadByte();
                _initialRect = Movie.ReadRect(stream);
                Pattern = stream.ReadUInt16BE();
                ForeColor = new LingoColor(stream.ReadByte());
                BackColor = new LingoColor(stream.ReadByte());
                FillType = stream.ReadByte();
                Ink = (InkType)(FillType & 0x3F);
                LineThickness = stream.ReadByte();
                LineDirection = stream.ReadByte();
            }
        }

        public ShapeCastMember(Cast cast, int castId, ShapeCastMember source)
            : base(cast, castId, CastType.Shape)
        {
            source.Load();
            _loaded = true;

            _initialRect = source._initialRect;
            _boundingRect = source._boundingRect;
            if (cast == source._cast)
                _children = source._children;

            ShapeType = source.ShapeType;
            Pattern = source.Pattern;
            FillType = source.FillType;
            LineThickness = source.LineThickness;
            LineDirection = source.LineDirection;
            Ink = source.Ink;
            ForeColor = source.ForeColor;
            BackColor = source.BackColor;
        }

        public override CastMember Duplicate(Cast cast, int castId)
            => new ShapeCastMember(cast, castId, this);

        public override string FormatInfo()
        {
            return $"initialRect:{_initialRect.Width}x{_initialRect.Height}@{_initialRect.Left},{_initialRect.Top}, " +
                   $"shapeType:{ShapeType}, pattern:{Pattern}, fill:{FillType}, line:{LineThickness}, ink:{Ink}";
        }

        public void SetForeColor(LingoColor color) => ForeColor = color;
        public void SetBackColor(LingoColor color) => BackColor = color;

        public override void Unload()
        {
            _loaded = false;
        }
    }
}
