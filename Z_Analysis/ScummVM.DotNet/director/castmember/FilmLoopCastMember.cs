using Director.IO;
using Director.Primitives;

namespace Director.Members
{
    /// <summary>
    /// Represents a film loop cast member consisting of multiple sprite frames.
    /// </summary>
    public class FilmLoopCastMember : CastMember
    {
        public bool EnableSound { get; private set; } = true;
        public bool Looping { get; private set; } = true;
        public bool Crop { get; private set; } = false;
        public bool Center { get; private set; } = false;
        public uint Flags { get; private set; }
        public FilmLoopCastMember(Cast cast, int castId)
            : base(cast, castId, CastType.FilmLoop)
        {
        }

        public FilmLoopCastMember(Cast cast, int castId, SeekableReadStreamEndian stream, ushort version)
            : base(cast, castId, stream)
        {
            _type = CastType.FilmLoop;

            _initialRect = Movie.ReadRect(stream);
            Flags = stream.ReadUInt32BE();
            stream.ReadUInt16BE(); // unk1

            if (version >= 400 && version < 500)
            {
                Looping = (Flags & 64) == 0;
                EnableSound = (Flags & 8) != 0;
                Crop = (Flags & 2) == 0;
                Center = (Flags & 1) != 0;
            }
            else if (version >= 500 && version < 600)
            {
                Looping = (Flags & 32) == 0;
                EnableSound = (Flags & 8) != 0;
                Crop = (Flags & 2) == 0;
                Center = (Flags & 1) != 0;
            }
        }

        public FilmLoopCastMember(Cast cast, int castId, FilmLoopCastMember source)
            : base(cast, castId, CastType.FilmLoop)
        {
            source.Load();
            _loaded = true;
            _initialRect = source._initialRect;
            _boundingRect = source._boundingRect;
            if (cast == source._cast)
                _children = source._children;

            EnableSound = source.EnableSound;
            Looping = source.Looping;
            Crop = source.Crop;
            Center = source.Center;
            Flags = source.Flags;
        }

        public override CastMember Duplicate(Cast cast, int castId)
            => new FilmLoopCastMember(cast, castId, this);

        public override string FormatInfo()
        {
            return $"initialRect:{_initialRect.Width}x{_initialRect.Height}@{_initialRect.Left},{_initialRect.Top}, " +
                   $"flags:{Flags:X}, looping:{Looping}, sound:{EnableSound}, crop:{Crop}, center:{Center}";
        }

        public override void Load()
        {
            _loaded = true;
        }

        public override void Unload()
        {
            _loaded = false;
        }
    }
}
