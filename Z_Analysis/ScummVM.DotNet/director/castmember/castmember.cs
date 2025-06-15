using Director.Graphics;
using System;
using System.Collections.Generic;
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
    public class CastMember
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
        public CastMember(Cast cast, int castId, CastType type)
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

        /// <summary>
        /// Returns the default registration offset relative to the widget's
        /// top-left corner.
        /// </summary>
        public virtual LingoPoint GetRegistrationOffset() => new(_regX, _regY);

        /// <summary>
        /// Returns the registration offset for a stretched width/height value.
        /// </summary>
        public virtual LingoPoint GetRegistrationOffset(int currentWidth, int currentHeight) =>
            GetRegistrationOffset();

        /// <summary>
        /// Calculates the bounding box of the cast member in stage coordinates.
        /// </summary>
        public virtual LingoRect GetBbox()
        {
            var off = GetRegistrationOffset();
            return new LingoRect(
                _initialRect.Left - off.X,
                _initialRect.Top - off.Y,
                _initialRect.Right - off.X,
                _initialRect.Bottom - off.Y);
        }

        /// <summary>
        /// Calculates the bounding box for a stretched size.
        /// </summary>
        public virtual LingoRect GetBbox(int currentWidth, int currentHeight)
        {
            var off = GetRegistrationOffset(currentWidth, currentHeight);
            return new LingoRect(
                -off.X,
                -off.Y,
                currentWidth - off.X,
                currentHeight - off.Y);
        }

        /// <summary>
        /// Simple hit test for a cast member.
        /// </summary>
        public virtual CollisionTest IsWithin(LingoRect bbox, LingoPoint pos, InkType ink) =>
            bbox.Contains(pos) ? CollisionTest.Yes : CollisionTest.No;

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
        public virtual bool HasField(int field)
        {
            CastField f = (CastField)field;
            return Enum.IsDefined(typeof(CastField), f);
        }

        public virtual Datum GetField(int field)
        {
            CastField f = (CastField)field;
            var info = _cast.GetCastMemberInfo(_castId);
            return f switch
            {
                CastField.MemberNum => new Datum(_castId),
                CastField.CastLibNum => new Datum(_cast.CastLibID),
                CastField.Name => new Datum(info?.Name ?? string.Empty),
                CastField.FileName => new Datum(info == null ? string.Empty : System.IO.Path.Combine(info.Directory, info.FileName)),
                _ => Datum.Void
            };
        }

        /// <summary>
        /// Sets the value of a specified field.
        /// </summary>
        public virtual bool SetField(int field, Datum value)
        {
            CastField f = (CastField)field;
            var info = _cast.GetCastMemberInfo(_castId);
            switch (f)
            {
                case CastField.Name:
                    if (info != null)
                    {
                        info.Name = value.AsString();
                        _cast.RebuildCastNameCache();
                        return true;
                    }
                    break;
                case CastField.FileName:
                    if (info != null)
                    {
                        info.FileName = value.AsString();
                        return true;
                    }
                    break;
            }
            return false;
        }

        public virtual bool HasProp(string propName)
        {
            return _propToField.ContainsKey(propName);
        }

        public virtual Datum GetProp(string propName)
        {
            return _propToField.TryGetValue(propName, out var f) ? GetField((int)f) : Datum.Void;
        }

        public virtual bool SetProp(string propName, Datum value, bool force = false)
        {
            return _propToField.TryGetValue(propName, out var f) && SetField((int)f, value);
        }

        private static readonly Dictionary<string, CastField> _propToField = new(StringComparer.OrdinalIgnoreCase)
        {
            ["backColor"] = CastField.BackColor,
            ["castLibNum"] = CastField.CastLibNum,
            ["castType"] = CastField.CastType,
            ["fileName"] = CastField.FileName,
            ["foreColor"] = CastField.ForeColor,
            ["height"] = CastField.Height,
            ["hilite"] = CastField.Hilite,
            ["loaded"] = CastField.Loaded,
            ["modified"] = CastField.Modified,
            ["memberNum"] = CastField.MemberNum,
            ["name"] = CastField.Name,
            ["number"] = CastField.Number,
            ["rect"] = CastField.Rect,
            ["purgePriority"] = CastField.PurgePriority,
            ["scriptText"] = CastField.ScriptText,
            ["size"] = CastField.Size,
            ["type"] = CastField.Type,
            ["width"] = CastField.Width
        };
    }

}
