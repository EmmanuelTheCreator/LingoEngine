using Director.IO;

namespace Director.Members
{
    public class SoundCastMember : CastMember
    {
        public SoundCastMember(Cast cast, int castId) : base(cast, castId, CastType.Sound)
        {
        }

        public SoundCastMember(Cast cast, int castId, SeekableReadStreamEndian stream) : base(cast, castId, stream)
        {
        }

        public int Looping { get; internal set; }
    }
}
