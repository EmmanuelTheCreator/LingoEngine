using Director.IO;

namespace Director.Members
{
    public class SoundCastMember : CastMember
    {
        public SoundCastMember(Cast cast, int castId, CastType type) : base(cast, castId, type)
        {
        }

        public SoundCastMember(Cast cast, int castId, SeekableReadStreamEndian stream) : base(cast, castId, stream)
        {
        }

        public int Looping { get; internal set; }
    }
}
