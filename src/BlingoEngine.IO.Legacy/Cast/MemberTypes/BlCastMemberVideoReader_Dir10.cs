using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.Security.AccessControl;
using System.Text;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberVideoReader_Dir10
    {
        internal BlCastRawMemberItem Read(byte[] specificData, List<byte[]> blobs, List<int> prefixValues, string mediaType)
        {
            var member = new BlCastRawMemberVideo();
            if (mediaType == "windowsMedia")
            {
                var skipRead = specificData.ReadInt16(2);
                var startRead = 4 + skipRead;
                member.DurationSeconds = specificData.ReadInt32(startRead) / 10f;
                var value2 = specificData.ReadInt32(startRead + 4);
                member.PlayAudio = specificData.ReadInt32(startRead + 8) > 0;
                member.PlayVideo = specificData.ReadInt32(startRead + 12) > 0;
                member.StartValueMs = specificData.ReadInt32(startRead + 16);
                member.StartPause = member.StartValueMs > 0;
                var value6 = specificData.ReadInt32(startRead + 20);
                member.Height = specificData.ReadInt32(startRead + 24);
                member.Width = specificData.ReadInt32(startRead + 28);
                var value9 = specificData.ReadInt32(startRead + 32);
                var loopValue = specificData.ReadInt32(startRead + 36);
                member.EnableLoop = loopValue > 0;
                var value11 = specificData.ReadInt32(startRead + 40);
                var value12 = specificData.ReadInt32(startRead + 44);
                var value13 = specificData.ReadInt32(startRead + 48);
                var value14 = specificData.ReadInt32(startRead + 52);

            }
            if (mediaType == "avi")
            {

                var value11 = specificData.ReadInt16(0);
                var value12 = specificData.ReadInt16(2);
                var value13 = specificData.ReadInt16(4);
                var value14 = specificData.ReadInt16(6);
                var value15 = specificData.ReadInt16(8);
                var value16 = specificData.ReadInt16(10);
                member.VideoFps = specificData.ReadByteOrDefault(8);
                var value17 = specificData.ReadByteOrDefault(9);
                var playBackWay = specificData.ReadByteOrDefault(10);
                
                //  0 = Sync to sound
                //  1 = Paused on
                //  2 = Video off
                // 04 = Preload ON
                // 08 = PlayBack every frame
                // 28 = PlayBack fixed FPS
                // 18 = PlayBack Maximum
                var flags = specificData.ReadByteOrDefault(11);
                // 2A = Default
                // 0A = DTS off (DTS is on for all other values)
                // 22 = Audio off
                // 28 = Framing Crop
                // 29 = Framing Crop Center
                // 3A = Loop On

                var mode = (AviPlaybackWay)playBackWay;
                var opt = (AviFlags)flags;
                var v = member;
                v.PlayVideo = !mode.HasFlag(AviPlaybackWay.VideoOff);
                v.StartPause = mode.HasFlag(AviPlaybackWay.Paused);
                v.EnableLoop = opt == AviFlags.Loop;
                v.PlayAudio = !opt.HasFlag(AviFlags.AudioOff);

                if ((playBackWay & 0x38) == 0x08) v.VideoFps = (int)BlCastRawMemberVideo.BlRawVideoPlaybackRate.EveryFrame;
                else if ((playBackWay & 0x38) == 0x18) v.VideoFps = (int)BlCastRawMemberVideo.BlRawVideoPlaybackRate.Maximum;
                else if ((playBackWay & 0x38) == 0x28) v.VideoFps = (int)BlCastRawMemberVideo.BlRawVideoPlaybackRate.Fixed;
                else v.VideoFps = (int)BlCastRawMemberVideo.BlRawVideoPlaybackRate.Sync;

                if ((flags & 0x29) == 0x28) v.Framing = BlCastRawMemberVideo.BlRawVideoFraming.Crop;
                else if ((flags & 0x29) == 0x29) v.Framing = BlCastRawMemberVideo.BlRawVideoFraming.CropCenter;
                else v.Framing = BlCastRawMemberVideo.BlRawVideoFraming.Scale;
            }

            

            if (blobs.Count > 1)
            {
                if (blobs[0].Length > 0) member.LinkedFolder = blobs[0].Skip(1).ToArray().ReadCString(0);
                if (blobs[1].Length > 1) member.LinkedFileName = blobs[1].Skip(1).ToArray().ReadCString(0);
                if (blobs.Count > 2)
                {
                    // Windows Media
                    var todo = blobs[2];
                }
            }
            return member;
        }

        [Flags]
        private enum AviPlaybackWay : byte
        {
            Paused = 0x01,
            VideoOff = 0x02,
            Preload = 0x04,

            RateMask = 0x38,   // combobox
            Sync = 0x00,
            EveryFrame = 0x08,
            Maximum = 0x18,
            FixedFps = 0x28
        }

        [Flags]
        private enum AviFlags : byte
        {
            // checkboxes
            DtsOff = 0x0A,
            AudioOff = 0x22,
            Loop = 0x3A,

            // framing combobox
            FramingMask = 0x29,
            FramingScale = 0x00,
            FramingCrop = 0x28,
            FramingCenter = 0x01
        }

    }
}
