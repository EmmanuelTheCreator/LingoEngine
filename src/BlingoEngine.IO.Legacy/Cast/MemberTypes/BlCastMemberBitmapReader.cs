using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberBitmapReader
    {
        public BlCastMemberItem Read(byte[] specificData, byte[] infoSlice, List<int> prefixValues)
        {
            var member = new BlCastMemberBitmap();
            var byte1 = specificData.ReadByteOrDefault(0);
            var byte2 = specificData.ReadByteOrDefault(1);
            member.Height = (int)specificData.ReadUInt16(6);
            member.Width = (int)specificData.ReadUInt16(8);
            var bit1 = specificData.ReadByteOrDefault(10);

            /*
             * The 4 bytes before the locH,locVn coordinates appear to store two signed Int16 values
             * Default they are 00 00 00 00 , once clicked they become this values.
            | Bytes         | Hex → Int16    | Decimal    |
            | ------------- | -------------- | ---------- |
            | FE DD FE 68 → | 0xFEDD, 0xFE68 | −291, −408 |
            | FF 49 FF 0B → | 0xFF49, 0xFF0B | −183, −245 |
            | FE 96 FE 5B → | 0xFE96, 0xFE5B | −362, −421 |
            */

            var someX = specificData.ReadInt16(14);
            var someY = specificData.ReadInt16(16);
            member.LocV = specificData.ReadInt16(18);
            member.LocH = specificData.ReadInt16(20);

            // some flags, unclear
            var byte14 = infoSlice.ReadUInt16(14);
            var byteFlags = specificData.ReadByteOrDefault(20);

            member.CompressionAmount = infoSlice.ReadByteOrDefault(infoSlice.Length-3);
            var compressionType = infoSlice.ReadByteOrDefault(infoSlice.Length-4); // FB, FD, FE
            switch (compressionType)
            {
                case 0xFB: member.CompressionType = BlCastMemberBitmap.BitmapCompressionType.MovieSetting;break;
                case 0xFE: member.CompressionType = BlCastMemberBitmap.BitmapCompressionType.Standard;break;
                case 0xFD: member.CompressionType = BlCastMemberBitmap.BitmapCompressionType.JPEG;break;
                default:
                    break;
            }
           

            member.PaletteId = infoSlice.ReadByteOrDefault(specificData.Length - 1);


//  is on another address:
//  Highlite On
//  Dither set
//00 80
//        // 0B 00: 
//        // 0B 01 : 
//        // 0B 01 : Trim Off
//        // 0B 80 : All On
//20 //

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

        public BlCastMemberItem ReadGif(byte[] specificData, List<byte[]> blobs, List<int> prefixValues)
        {
            var member = new BlCastMemberGif();

            var int1 = specificData.ReadUInt32(11); // 0x34
            var int2 = specificData.ReadUInt32(15); // 0x34
            var int3 = specificData.ReadUInt32(19); // 0x01
            var int4 = specificData.ReadUInt32(23); // 0x00
            var int5 = specificData.ReadUInt32(27); // 0x01
            var int6 = specificData.ReadUInt32(43); // 0x0F

            member.FrameCount = (int)specificData.ReadUInt16(57);
            member.Height = (int)specificData.ReadUInt32(59); 
            member.Width = (int)specificData.ReadUInt32(63);
            //Rate : Normal, Fixed, Lock-Step
            // FPS : value : 15
            // 1 frame
            
            // Media Linked on off
            // Playback : DTS /Direct to stage : on off
            // Import folder
            return member;
        }
    }
}
