using Director.IO;
using Director.Primitives;

namespace Director.Members
{
    public class SoundCastMember : CastMember
    {
        public byte[] AudioData { get; private set; } = Array.Empty<byte>();

        public SoundCastMember(Cast cast, int castId)
            : base(cast, castId, CastType.Sound)
        {
        }

        public SoundCastMember(Cast cast, int castId, SeekableReadStreamEndian stream)
            : base(cast, castId, stream)
        {
        }

        public SoundCastMember(Cast cast, int castId, SoundCastMember source)
            : base(cast, castId, CastType.Sound)
        {
            source.Load();
            _loaded = true;
            if (cast == source._cast)
                _children = source._children;

            Looping = source.Looping;
            AudioData = (byte[])source.AudioData.Clone();
        }

        public int Looping { get; internal set; }

        public override CastMember Duplicate(Cast cast, int castId)
        {
            return new SoundCastMember(cast, castId, this);
        }

        public override string FormatInfo()
        {
            return $"bytes:{AudioData.Length}, looping:{Looping}";
        }

        public override void Load()
        {
            if (_loaded)
                return;

            int sndId = 0;
            uint tag = ResourceTags.SND;

            foreach (var child in _children)
            {
                if (child.Tag == ResourceTags.SND || child.Tag == ResourceTags.MKTAG('s','n','d',' '))
                {
                    sndId = child.Index;
                    tag = child.Tag;
                    break;
                }
            }

            var arch = _cast.GetArchive();
            if (arch.HasResource(tag, sndId))
            {
                using var snd = arch.GetResource(tag, sndId);
                AudioData = snd.ReadBytesRequired((int)snd.Length);
            }

            _loaded = true;
        }

        public override void Unload()
        {
            AudioData = Array.Empty<byte>();
            _loaded = false;
        }
    }
}
