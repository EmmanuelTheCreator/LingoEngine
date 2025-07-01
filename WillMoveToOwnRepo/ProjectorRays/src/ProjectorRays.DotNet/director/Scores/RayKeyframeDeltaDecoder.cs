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
            RaysBehaviourRef Member);

        public DecodedKeyframe Decode(int frame, int channel, byte[] data)
        {
            var s = new ReadStream(data, data.Length, Endianness.BigEndian);
            //int foreColor = s.ReadInt16();        
            //int backColor2 = s.ReadInt16();       
            //int height2 = s.ReadInt8();      
            //int foreColor3 = s.ReadInt16();   
            //int foreColor2 = s.ReadInt16();  
            //int backColor = s.ReadInt16();   
            //int ink = s.ReadUint8();         
            //int blend = s.ReadUint8();       
            //int rotationRaw = s.ReadUint16();
            //float rotation = 0;
            //int locV = s.ReadUint16();    
            //float skew = 0;
            //int locH = s.ReadUint16();
            //int gg = s.ReadUint16();
            //int propOffset = s.ReadUint16();  
            //int castLib = s.ReadUint16();
            //int memberNum = s.ReadUint16();
            byte flags = s.ReadUint8();
            byte inkByte = s.ReadUint8();
            var ink = inkByte & 0x7F;
            var foreColor = s.ReadUint8();
            var backColor = s.ReadUint8();
            int castLib = s.ReadUint16();
            int memberNum = s.ReadUint16();
            s.ReadUint16();
            //Skip(2); // unknown
            var spritePropertiesOffset = s.ReadUint16(); //18,27,30,33,36
            var locV = s.ReadInt16();
            var locH = s.ReadInt16();
            var height = s.ReadInt16();
            var width = s.ReadInt16();
            byte colorcode = s.ReadUint8();
            var editable = (colorcode & 0x40) != 0;
            var scoreColor = colorcode & 0x0F;
            var blendA = s.ReadUint8();
            var blend = (int)Math.Round(100f - blendA / 255f * 100f);
            byte flag2 = s.ReadUint8();
            var flipV = (flag2 & 0x04) != 0;
            var flipH = (flag2 & 0x02) != 0;
            s.Skip(5);
            //var test = stream.ReadInt16();
            var rotation = s.ReadUint32() / 100f;
            var skew = s.ReadUint32() / 100f;
            var refMember = new RaysBehaviourRef { CastLib = castLib, CastMmb = memberNum };
            return new DecodedKeyframe(frame, channel, locH, locV, width, height, rotation, skew, foreColor, backColor, blend, refMember);
        }
    }
}
