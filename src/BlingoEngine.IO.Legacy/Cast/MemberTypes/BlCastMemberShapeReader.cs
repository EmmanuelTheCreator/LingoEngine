using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            for (int i = 0; i < 13; i++) 
                values.Add(reader.ReadInt32());

            member.RegPoint = new BlingoPointDTO(values[5], values[4]);

            member.Height = values[8];
            member.Width = values[9];

            member.AntiAlias = reader.ReadInt32() > 0;  // AntiAlias On/Off

            member.Scale = BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
            var values2 = new List<int>();
            for (int i = 0; i < 10; i++)
                values2.Add(reader.ReadInt32());

            member.ScaleMode = (BlCastRawMemberShape.BlShapeScaleMode)reader.ReadInt32();

            var values3 = new List<int>();
            for (int i = 0; i < 6; i++)
                values3.Add(reader.ReadInt32());

            
            member.LineClosed = reader.ReadInt32() > 0;  // Line Closed On/Off
            member.StrokeWidth = BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
            var fillType = reader.ReadInt32();
            member.Fill = fillType > 0;             // Fill type shape: 0 = no fill, 1 = solid, 2 = gradient

            // Read gradient
            member.IsGradientFill = fillType == 2;
            member.GradientIsRadial = reader.ReadInt32() > 0;      // Gradiant radial
            member.GradientSpread = BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
            member.GradientAngle = BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
            member.GradientXOffset = reader.ReadInt32();
            member.GradientyOffset = reader.ReadInt32();
            member.GradientyCycles = reader.ReadInt32();

            // read colors
            var colorValues = new List<BlingoColorDTO>();
            for (int i = 0; i < 4; i++)
            {
                reader.ReadInt32(); // 12
                colorValues.Add(new BlingoColorDTO((byte)reader.ReadInt32(), (byte)reader.ReadInt32(), (byte)reader.ReadInt32()));
            }
            member.StrokeColor = colorValues[0];
            member.FillColor = colorValues[1];
            member.BackgroundColor = colorValues[2];
            member.GradientColor = colorValues[3];

            var tag = reader.ReadInt32();
            if (tag == 7)
            {
                var numberOfVertices = reader.ReadInt32();
                var curve = new BlCastRawMemberShape.BlShapeCurve();
                member.Curves = [curve];
                for (int i = 0; i < numberOfVertices; i++)
                {
                    tag = reader.ReadInt32();
                    if (tag == 0x0A)
                    {
                        // continue curve
                    }
                    else if (tag == 0x07)
                    {
                        // New curve
                        curve = new BlCastRawMemberShape.BlShapeCurve();
                        member.Curves.Add(curve);
                        var numberOfValues2 = reader.ReadInt32();       // 01
                        var tag2 = reader.ReadInt32();                  // 02
                        var nameLength = reader.ReadInt32();            // 08
                        var name = reader.ReadAsciiString(nameLength);  // newCurve
                        continue;
                    }
                    var vertex = new BlCastRawMemberShape.BlShapeVertex();
                    curve.Vertices.Add(vertex);
                    var numberOfValues = reader.ReadInt32();
                    for (int index = 0; index < numberOfValues; index++)
                    {
                        tag = reader.ReadInt32();                   // 02
                        if (tag == 2)
                        {
                            var tagByte = reader.ReadByte();
                            reader.ReadByte();                      // 00
                            if (tagByte == 0x80)
                            {
                                var typeTag = reader.ReadInt16();   // 00, 01 or 02
                                tag = reader.ReadInt32();           // 08

                                var x = reader.ReadInt32();
                                var y = reader.ReadInt32();
                                    
                                if (typeTag == 0) vertex.Position = new BlingoPointDTO(x, y);
                                else if (typeTag == 1) vertex.Handle1 = new BlingoPointDTO(x, y);
                                else if (typeTag == 2) vertex.Handle2 = new BlingoPointDTO(x, y);
                            }
                            else
                            {
                                // Vertex with name
                                var nameLength = reader.ReadInt16();
                                var name = reader.ReadAsciiString(nameLength);
                                tag = reader.ReadInt32(); // 08
                                var x = reader.ReadInt32();
                                var y = reader.ReadInt32();
                                if (name == "vertex") vertex.Position = new BlingoPointDTO(x, y);
                                else if (name == "handle1") vertex.Handle1 = new BlingoPointDTO(x, y);
                                else if (name == "handle2") vertex.Handle2 = new BlingoPointDTO(x, y);
                            }
                        }
                    }
                }
            }

            


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

00 00 00 0B     76 65 63 74 6F 72 53 68 61 70 65            // = vectorShape
00 00 02 87                                                 // ? = 647
46 4C 53 48                                                 // = FLSH (flash? perhaps)
00 00 02 87   00 00 00 1A                                   // ? = 647  ; ? = 26
00 00 00 00   00 00 00 01                                   // 
00 00 00 60   00 00 00 8D 							        // Regpoint Y,X
00 00 00 00   00 00 00 00 
00 00 00 C1   00 00 01 1B                                   // Height + Width
00 00 00 01   00 00 00 00 
00 00 00 00   
    00 00 00 01  							                // AntiAlias On/Off
    42 C8 00 00    								            // Scale 100.00 %  
00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 00 
42 C8 00 00   00 00 00 00   00 00 00 00   00 00 00 00   00 00 00 01   
    00 00 00 03                                             // Scale mode
00 00 00 01   00 00 00 01   00 00 00 00   00 00 00 00   00 00 00 00   
    00 00 00 01                                             // Line Closed On/Off
    3F 80 00 00                                             // Stroke Width
    00 00 00 01                                             // Fill type shape: 0 = no fill, 1 = solid, 2 = gradient
    00 00 00 00                                             // Gradiant is radial
    42 C8 00 00                                             // Gradient Spread float
    00 00 00 00                                             // Gradient Angle float
    00 00 00 00                                             // Gradient Y-offset   
    00 00 00 00                                             // Gradient X-Offset  
    00 00 00 01                                             // Gradient cycles
// Colors 
    00 00 00 12   00 00 00 00   00 00 00 00   00 00 00 00   // Stroke color
    00 00 00 12   00 00 00 66   00 00 00 FF   00 00 00 66   // Fill color 
    00 00 00 12   00 00 00 CC   00 00 00 00   00 00 00 FF   // Background color
    00 00 00 12   00 00 00 FF   00 00 00 00   00 00 00 00   // Gradient color
00 00 00 07   00 00 00 06                                   // number of verticies
    00 00 00 0A   00 00 00 03                               // 3 = vertex + 2 handles , 1 = only vertex3 
        00 00 00 02   00 00 00 06   76 65 72 74 65 78       // = vertex
            00 00 00 08   FF FF FF C3   FF FF FF 7F 
        00 00 00 02   00 00 00 07   68 61 6E 64 6C 65 31    // = handle1
            00 00 00 08   FF FF FF F4   00 00 00 25   
        00 00 00 02   00 00 00 07   68 61 6E 64 6C 65 32    // = handle2
            00 00 00 08   00 00 00 0C   FF FF FF DB   
    00 00 00 0A   00 00 00 03                               // 3 = vertex + 2 handles , 1 = only vertex3 
        00 00 00 02   80 00 00 00   00 00 00 08   FF FF FF A7   00 00 00 25   
        00 00 00 02   80 00 00 01   00 00 00 08   00 00 00 14   00 00 00 19   
        00 00 00 02   80 00 00 02   00 00 00 08   FF FF FF EC   FF FF FF E7   
    00 00 00 0A   00 00 00 03                                // 3 = vertex + 2 handles , 1 = only vertex3    
        00 00 00 02   80 00 00 00   00 00 00 08   00 00 00 10   00 00 00 84   
        00 00 00 02   80 00 00 01   00 00 00 08   00 00 00 38   FF FF FF F5   
        00 00 00 02   80 00 00 02   00 00 00 08   FF FF FF C8   00 00 00 0B   
    00 00 00 0A   00 00 00 03                                // 3 = vertex + 2 handles , 1 = only vertex3    
        00 00 00 02   80 00 00 00   00 00 00 08   00 00 00 5E   FF FF FF ED   
        00 00 00 02   80 00 00 01   00 00 00 08   FF FF FF F8   FF FF FF CF   
        00 00 00 02   80 00 00 02   00 00 00 08   00 00 00 08   00 00 00 31   
    00 00 00 0A   00 00 00 03                                // 3 = vertex + 2 handles , 1 = only vertex3    
        00 00 00 02   80 00 00 00   00 00 00 08   00 00 00 27   FF FF FF A2   
        00 00 00 02   80 00 00 01   00 00 00 08   FF FF FF E2   FF FF FF CD   
        00 00 00 02   80 00 00 02   00 00 00 08   00 00 00 1E   00 00 00 33   
    00 00 00 0A   00 00 00 01                                // 3 = vertex + 2 handles , 1 = only vertex3    
        00 00 00 02   80 00 00 00   00 00 00 08   FF FF FF C5   FF FF FF 7F   
00 00 00 00   
00 00 00 02   00 00 00 0B   76   65 63 74 6F 72 53 68 61 70 65    // = vectorShape


*/
