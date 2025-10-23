using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberBitmapReader
    {
        public BlCastMemberItem Read(byte[] specificData)
        {
            var member = new BlCastMemberBitmap();
            var byte1 = specificData.ReadByteOrDefault(0);
            var byte2 = specificData.ReadByteOrDefault(1);
            member.Height = (int)specificData.ReadUInt16(6);
            member.Width = (int)specificData.ReadUInt16(8);
            var bit1 = specificData.ReadByteOrDefault(10);

            var yy = specificData.ReadUInt16(18);
            var xx = specificData.ReadUInt16(20);
            // Palette

            // Highlight on/off
            // Dither on/off
            // UseAlpha on/off
            // Trim on/off

            // Compression : Movie settings, standard, JPEG
            // if jpeg ->
            //          Quality : int
            // Is use alpha ->
            //          Alpha threshold
            // Depth:-> not in castmember data
            return member;
        }

        public BlCastMemberItem ReadGif(byte[] specificData)
        {
            var member = new BlCastMemberBitmap();
            //Rate : Normal, Fixed, Lock-Step
            // FPS : value : 15
            // 1 frame
            // 640 x 480
            // Media Linked on off
            // Playback : DTS /Direct to stage : on off
            // Import folder
            return member;
        }
    }
}
