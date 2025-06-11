using Director.Graphics;
using Director.IO;
using Director.Primitives;
using Director.Scripts;

namespace Director.Members
{
    public enum CastType
    {
        Empty = 0,
        Bitmap = 1,
        FilmLoop = 2,
        Text = 3,
        Button = 4,
        Shape = 5,
        Movie = 6,
        DigitalVideo = 7,
        Script = 8,
        Paint = 9,
        Sound = 10,
        Palette = 11,
        RichText = 12,
        QuickDrawShape = 13,
        Flash = 14,
        EmbeddedVideo = 15,
        Xtra = 16,
        Any = 17
    }
    public class EditInfo
    {
        public LingoRect Rect { get; set; } = new LingoRect();
        public int SelStart { get; set; }
        public int SelEnd { get; set; }
        public byte Version { get; set; }
        public byte RulerFlag { get; set; }
    }
    public abstract class CastMember
    {
        private static int _refCount;
        protected Cast _cast;
        protected int _castId;
        protected CastType _type;
        protected int _flags1;
        protected uint _tag;
        protected Rect _initialRect;
        protected Rect _boundingRect;
        protected int _version;
        protected int _regX;
        protected int _regY;
        protected bool _loaded;
        protected bool _modified;
        protected List<ResourceChunk> _children = new();
        protected CastMember(Cast cast, int castId, CastType type)
        {
            _cast = cast;
            _castId = castId;
            _type = type;
            _flags1 = 0;
            _tag = 0;
            _initialRect = new Rect();
            _boundingRect = new Rect();
            _regX = 0;
            _regY = 0;
        }
        public int Id => _castId;
        public CastType Type => _type;
        public uint Tag => _tag;
        public int Flags1 => _flags1;
        public Rect InitialRect => _initialRect;
        public Rect BoundingRect => _boundingRect;
        public int RegX => _regX;
        public int RegY => _regY;
        public IReadOnlyList<ResourceChunk> Children => _children;

        protected CastMember(Cast cast, int castId, SeekableReadStreamEndian stream)
        {
            _cast = cast;
            _castId = castId;

            _tag = stream.ReadUInt32();
            _flags1 = stream.ReadInt16();
            _initialRect = Movie.ReadRect(stream);
            _boundingRect = Movie.ReadRect(stream);
            _regX = stream.ReadInt16();
            _regY = stream.ReadInt16();
            _type = (CastType)stream.ReadByte();

            _version = 0;
            _loaded = false;
            _modified = false;
        }

       
        public virtual void LoadFromStream(SeekableReadStreamEndian stream, Resource res)
        {
            // Default implementation: override in derived classes
        }

        
        

       

        public virtual string GetText() => string.Empty;
        public virtual int GetTextStyle() => 0;
        public virtual void SetTextStyle(int style) { }
        public virtual void SetColors(int foreColor, int backColor) { }
        public virtual void SetTextFont(int fontId) { }
        public virtual void SetTextSize(int size) { }
        public virtual int GetLineCount() => 0;

        public override string ToString() => $"CastMember[{_castId}] ({_type})";




        public virtual void Load()
        {
            // Optionally overridden in subclass
        }


        public virtual void LoadFromScriptStream(SeekableReadStreamEndian stream, int version)
        {
            // Optionally overridden in subclass
        }

        public virtual void ReleaseWidget()
        {
            // Optionally overridden in subclass
        }

        public void SetModified(bool modified)
        {
            _modified = modified;
        }

        public void IncRefCount()
        {
            _refCount++;
        }

        public void DecRefCount()
        {
            _refCount--;
            if (_refCount <= 0)
                Dispose();
        }

         public void AddChild(ResourceChunk chunk)
        {
            _children.Add(chunk);
        }
        public virtual void Unload() { }
        public virtual void Dispose()
        {
            // Cleanup logic
        }

        public virtual string FormatInfo() { return ""; }

        /// <summary>
        /// Creates a duplicate of this cast member.
        /// </summary>
        public virtual CastMember Duplicate(Cast cast, int castId)
        {
            throw new NotImplementedException();
        }
        public virtual bool HasField(int field) => false;

        public virtual Datum GetField(int field)=> new Datum();

        /// <summary>
        /// Sets the value of a specified field.
        /// </summary>
        public virtual bool SetField(int field, Datum value) => false;
    }

}
