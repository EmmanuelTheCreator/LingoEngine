using ProjectorRays.Common;
using System.ComponentModel.DataAnnotations;
using System.IO;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores
{
    internal class RayKeyframeDeltaDecoder
    {
        public record DecodedKeyframe(
            int Frame,
            int SpriteChannel,
            int LocH, int LocV,
            int Width, int Height,
            float Rotation, float Skew,
            int ForeColor, int BackColor,
            int Blend,
            RaysBehaviourRef Member, int FrameOffset);

        public DecodedKeyframe Decode(int frame, int channel, byte[] data)
        {
            var s = new ReadStream(data, data.Length, Endianness.BigEndian);

            // Skip header or unrelated bytes
            //s.Skip(4); // Unknown
            int foreColor = s.ReadUint8();      // 0x32
            int backColor = s.ReadUint8();      // 0x33
            int frameOffset = s.ReadUint16();      
            int someValue = s.ReadUint16();      //0x01 0x90 // 400 or 1 and 144

            int height = s.ReadUint16();        // 0x003E = 62
            int width = s.ReadUint16();         // 0x003C = 60
            int blendByte = s.ReadUint8();      // 0xCC
            int blend = (int)Math.Round(100f - blendByte / 255f * 100f);

            // Continue decoding based on field position
            int propOffset1 = s.ReadUint8();  
            int propOffset2 = s.ReadUint8();  
            int memberNum = s.ReadUint8();    
            int memberNum2 = s.ReadUint8();    
            int castLib = s.ReadUint8();       // 0x0000
            int castLib2 = s.ReadUint8();       // 0x0000

            int ink = s.ReadUint8();            // next block
            int skew = s.ReadUint8();           // skew numerator
            int rotation = s.ReadUint8();       // rotation numerator
            int skewDenom = s.ReadUint8();      // skew denominator
            int rotationDenom = s.ReadUint8();  // rotation denominator

            int locH = s.ReadUint8();          // x
            int locV = s.ReadUint8();          // y
            //int locV2 = s.ReadUint8();          // y

            var refMember = new RaysBehaviourRef
            {
                CastLib = castLib,
                CastMmb = memberNum
            };

            return new DecodedKeyframe(frame, channel, locH, locV, width, height, rotation, skew, foreColor, backColor, blend, refMember, frameOffset);
        }
    }
}
