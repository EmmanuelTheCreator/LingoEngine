using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberShapeReader
    {
        internal BlCastRawMemberItem Read(byte[] specificData, List<byte[]> blobs, List<int> prefixValues, bool isVecorShape)
        {
            var rawBytes = specificData.ToHexString();
            var member = new BlCastRawMemberShape();
            if (isVecorShape)
            {
                member.ShapeType = BlCastRawMemberShape.BlShapeType.PolyLine;
                ReadVectorShape(member, specificData);
            }
            else
            {
/*
Member 1: 00 01 00 00 00 00 00 39 00 47 00 01 FF 00 01 02 05    // MySquare                 77x57
Member 2: 00 02 00 00 00 00 00 36 00 41 00 01 FF 00 01 02 05    // MySquareRoundBorders     65x54
Member 3: 00 03 00 00 00 00 00 3B 00 42 00 01 FF 00 01 02 05    // MyOvalFilled             66x59
-> polyline
Member 4: 00 01 00 00 00 00 00 3B 00 48 00 01 FF 00 00 02 05    // MyRectangle
Member 5: 00 02 00 00 00 00 00 38 00 44 00 01 FF 00 00 02 05    // MySquareRoundBorders
Member 6: 00 03 00 00 00 00 00 3B 00 46 00 01 FF 00 00 02 05    // MyOval
Member 7: 00 04 00 00 00 00 00 27 00 4A 00 01 FF 00 01 02 05    // MyLine
                */
                var shapeType = specificData.ReadInt16(0);
                switch (shapeType)
                {
                    case 01: member.ShapeType = BlCastRawMemberShape.BlShapeType.Rectangle; break;
                    case 02: member.ShapeType = BlCastRawMemberShape.BlShapeType.RoundRectangle; break;
                    case 03: member.ShapeType = BlCastRawMemberShape.BlShapeType.Oval; break;
                    case 04: member.ShapeType = BlCastRawMemberShape.BlShapeType.Line; break;
                }
                member.Height = specificData.ReadInt16(6);
                member.Width = specificData.ReadInt16(8);
                var somethingA = specificData.ReadInt32(10); // 00 01
                var somethingB = specificData.ReadInt16(12); // FF 00
                member.Fill = specificData.ReadByteOrDefault(14) > 0; // 01 = fill , 00  not fill
            }
            return member;
        }

        private void ReadVectorShape(BlCastRawMemberShape member, byte[] payload)
        {
            var reader = new BlStreamReader(new MemoryStream(payload));
            var length = reader.ReadInt32();        // 0B
            var type = reader.ReadBytes(length);    // = vectorShape
            var value1 = reader.ReadInt32();        // 00 00 02 87      = 647
            var flash = reader.ReadBytes(4);        // 46 4C 53 48      = FLSH (flash? perhaps)
            var values = new List<int>();
            for (int i = 0; i < 14; i++) 
                values.Add(reader.ReadInt32());
            member.Width = values[8];
            member.Height = values[9];

            var values2 = new List<int>();
            for (int i = 0; i < 26; i++)
                values2.Add(reader.ReadInt32());

            // read colors
            var colorValues = new List<BlingoColorDTO>();
            for (int i = 0; i < 4; i++)
            {
                reader.ReadInt32(); // 12
                colorValues.Add(new BlingoColorDTO((byte)reader.ReadInt32(), (byte)reader.ReadInt32(), (byte)reader.ReadInt32()));
            }

            var values3 = new List<int>();
            for (int i = 0; i < 4; i++)
                values2.Add(reader.ReadInt32());
        }
    }
}
/*


// MyPolyLine -> VecorShape
// size = 283x193
// 6 vertex
// Colors:
//  bg purple   = #cc00ff
//  fill color  = #66ff66
//  line        = #000000

00 00 00 0B     76 65 63 74 6F 72 53 68 61 70 65    // = vectorShape
00 00 02 87                                         // ? = 647
46 4C 53 48                                         // = FLSH (flash? perhaps)
00 00 02 87   00 00 00 1A                           // ? = 647  ; ? = 26
00 00 00 00   00 00 00 01                           // 
00 00 00 60   00 00 00 8D                           // ? = 96   ; ? = 141
00 00 00 00   00 00 00 00 
00 00 00 C1   00 00 01 1B                           // Height + Width
00 00 00 01   00 00 00 00 
00 00 00 00   00 00 00 01 
42 C8 00 00   00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 00 
42 C8 00 00   00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 01   00 00 00 03 
00 00 00 01   00 00 00 01   00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 01 
3F 80 00 00   00 00 00 01   00 00 00 00 
42 C8 00 00   00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 01   
// Colors 
    00 00 00 12   00 00 00 00   00 00 00 00   00 00 00 00   // line color
    00 00 00 12   00 00 00 66   00 00 00 FF   00 00 00 66   // Fill color 
    00 00 00 12   00 00 00 CC   00 00 00 00   00 00 00 FF   // background color
    00 00 00 12   00 00 00 FF   00 00 00 00   00 00 00 00   // Some red color, think its the start of the shape

    00 00 00 07   00 00 00 06   00 00 00 0A   00 00 00 03 

    00 00 00 02   00 00 00 06   76 65 72 74 65 78       // = vertex
        00 00 00 08   FF FF FF C3   FF FF FF 7F 

    00 00 00 02   00 00 00 07   68 61 6E 64 6C 65 31    // = handle1
        00 00 00 08   FF FF FF F4   00 00 00 25   
    00 00 00 02   00 00 00 07   68 61 6E 64 6C 65 32    // = handle2
        00 00 00 08   00 00 00 0C   FF FF FF DB   00 00 00 0A   00 00 00 03 

    00 00 00 02   80 00 00 00   00 00 00 08   FF FF FF A7   00 00 00 25   
    00 00 00 02   80 00 00 01   00 00 00 08   00 00 00 14   00 00 00 19   
    00 00 00 02   80 00 00 02   00 00 00 08   FF FF FF EC   FF FF FF E7   00 00 00 0A   00 00 00 03   
    00 00 00 02   80 00 00 00   00 00 00 08   00 00 00 10   00 00 00 84   
    00 00 00 02   80 00 00 01   00 00 00 08   00 00 00 38   FF FF FF F5   
    00 00 00 02   80 00 00 02   00 00 00 08   FF FF FF C8   00 00 00 0B   00 00 00 0A   00 00 00 03   
    00 00 00 02   80 00 00 00   00 00 00 08   00 00 00 5E   FF FF FF ED   
    00 00 00 02   80 00 00 01   00 00 00 08   FF FF FF F8   FF FF FF CF   
    00 00 00 02   80 00 00 02   00 00 00 08   00 00 00 08   00 00 00 31   00 00 00 0A   00 00 00 03   
    00 00 00 02   80 00 00 00   00 00 00 08   00 00 00 27   FF FF FF A2   
    00 00 00 02   80 00 00 01   00 00 00 08   FF FF FF E2   FF FF FF CD   
    00 00 00 02   80 00 00 02   00 00 00 08   00 00 00 1E   00 00 00 33   00 00 00 0A   00 00 00 01   
    00 00 00 02   80 00 00 00   00 00 00 08   FF FF FF C5   FF FF FF 7F   
    00 00 00 00   
    00 00 00 02   00 00 00 0B   76   65 63 74 6F 72 53 68 61 70 65    // = vectorShape


*/
