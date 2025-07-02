using ProjectorRays.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores
{
internal class RayKeyframeDeltaDecoder
    {
        public RayStreamAnnotatorDecorator Annotator { get; }
        public RayKeyframeDeltaDecoder(RayStreamAnnotatorDecorator annotator)
        {
            Annotator = annotator;
        }
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
            var s = new ReadStream(data, data.Length, Endianness.BigEndian, annotator: Annotator);

            // Skip header or unrelated bytes
            //s.Skip(4); // Unknown
            int foreColor = s.ReadUint8("foreColor");      // 0x32
            int backColor = s.ReadUint8("backColor");      // 0x33
            int frameOffset = s.ReadUint16("frameOffset");
            int someValue = s.ReadUint16("someValue");      //0x01 0x90 // 400 or 1 and 144

            int height = s.ReadUint16("height");        // 0x003E = 62
            int width = s.ReadUint16("width");         // 0x003C = 60
            int blendByte = s.ReadUint8("blendRaw");      // 0xCC
            int blend = (int)Math.Round(100f - blendByte / 255f * 100f);

            // Continue decoding based on field position
            int propOffset1 = s.ReadUint8("propOffset1");
            int propOffset2 = s.ReadUint8("propOffset2");
            int memberNum = s.ReadUint8("memberNum");
            int memberNum2 = s.ReadUint8("memberNum2");
            int castLib = s.ReadUint8("castLib");       // 0x0000
            int castLib2 = s.ReadUint8("castLib2");       // 0x0000

            int ink = s.ReadUint8("ink");            // next block
            int skew = s.ReadUint8("skewNum");           // skew numerator
            int rotation = s.ReadUint8("rotNum");       // rotation numerator
            int skewDenom = s.ReadUint8("skewDen");      // skew denominator
            int rotationDenom = s.ReadUint8("rotDen");  // rotation denominator

            int locH = s.ReadUint8("locH");          // x
            int locV = s.ReadUint8("locV");          // y
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
