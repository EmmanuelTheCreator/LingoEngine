using BlingoEngine.IO.Legacy.Cast.Data;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberAudioReader
    {
        internal BlCastMemberItem Read(byte[] specificData)
        {
            var member = new BlCastMemberAudio();
            return member;
        }
    }
}
