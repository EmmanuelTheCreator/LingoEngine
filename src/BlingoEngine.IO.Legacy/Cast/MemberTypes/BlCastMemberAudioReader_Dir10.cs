using BlingoEngine.IO.Legacy.Cast.Data;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberAudioReader_Dir10
    {
        internal BlCastRawMemberItem Read(byte[] specificData)
        {
            var member = new BlCastRawMemberAudio();
            return member;
        }
    }
}
