using Director.IO;
using Director.Primitives;

namespace Director.Members
{
    /// <summary>
    /// Director movie cast member built from a film loop plus script flags.
    /// </summary>
    public class MovieCastMember : FilmLoopCastMember
    {
        public bool EnableScripts { get; private set; }

        public MovieCastMember(Cast cast, int castId)
            : base(cast, castId)
        {
            _type = CastType.Movie;
        }

        public MovieCastMember(Cast cast, int castId, SeekableReadStreamEndian stream, ushort version)
            : base(cast, castId, stream, version)
        {
            _type = CastType.Movie;
            EnableScripts = (Flags & 0x10) != 0;
        }

        public MovieCastMember(Cast cast, int castId, MovieCastMember source)
            : base(cast, castId, source)
        {
            EnableScripts = source.EnableScripts;
            _type = CastType.Movie;
        }

        public override CastMember Duplicate(Cast cast, int castId)
            => new MovieCastMember(cast, castId, this);

        public override string FormatInfo()
        {
            return base.FormatInfo() + $", scripts:{EnableScripts}";
        }
    }
}
