using BlingoEngine.IO.Legacy.Tools;
using System.Diagnostics;
using System.Text;

namespace BlingoEngine.IO.Legacy.Cast
{
    public enum CastTokenType
    {
        Unknown, Int32, Padding, Text,EmptyBreak,Empty,
        Int16,
        Byte
    }
    internal class BlLegacyCastTokenizer
    {
        [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
        public class CastToken
        {
            public CastTokenType Type { get; set; }
            public long Offset { get; set; }
            public int Length { get; set; }
            public int? IntValue { get; set; }
            public string? StringValue { get; set; }
            public string? Description { get; private set; }

            private string GetDebuggerDisplay()
            {
                string val = Type switch
                {
                    CastTokenType.Text => $"Text=\"{StringValue}\"",
                    CastTokenType.Padding => $"Pad({Length})",
                    CastTokenType.Int32 => $"Int={IntValue}",
                    CastTokenType.Int16 => $"Int16={IntValue}",
                    CastTokenType.Byte => $"Byte={IntValue}",
                    _ => $"Unknown"
                };
                return $"[{Offset:X4}] {Type} {val}";
            }

            public static CastToken NewInt32(long offset, int value, string description = "")
            {
                return new CastToken
                {
                    Type = CastTokenType.Int32,
                    Offset = offset,
                    Length = 4,
                    IntValue = value,
                    Description = description
                };
            }
            public static CastToken NewInt16(long offset, int value, string description = "")
            {
                return new CastToken
                {
                    Type = CastTokenType.Int16,
                    Offset = offset,
                    Length = 4,
                    IntValue = value,
                    Description = description
                };
            }
            public static CastToken NewByte(long offset, int value, string description = "")
            {
                return new CastToken
                {
                    Type = CastTokenType.Byte,
                    Offset = offset,
                    Length = 4,
                    IntValue = value,
                    Description = description
                };
            }
            public static CastToken NewText(long offset, string value, string description = "")
            {
                return new CastToken
                {
                    Type = CastTokenType.Text,
                    Offset = offset,
                    Length = 4,
                    StringValue = value,
                    Description = description
                };
            }
            public static CastToken NewPadding(long offset, byte[] bytes, string description = "")
            {
                return new CastToken
                {
                    Type = CastTokenType.Padding,
                    Offset = offset,
                    Length = bytes.Length,
                    Description = description
                };
            }
            public static CastToken NewEmpty() => new CastToken
            {
                Type = CastTokenType.Empty,
            };
            public static CastToken NewEmptyBreak(string description = "") => new CastToken
            {
                Type = CastTokenType.EmptyBreak,
                Description = description
            };

        }



        public List<CastToken> TokenizeInfo(byte[] info)
        {
            var returnData = new List<CastToken>();
            using var memory = new MemoryStream(info, writable: false);
            var reader = new BlStreamReader(memory)
            {
                Endianness = BlEndianness.BigEndian
            };

            var test = reader.ReadBytesAsHexString(info.Length); // for debug
            memory.Seek(0, SeekOrigin.Begin);


            // Header = 12 bytes
            var typeValue = reader.ReadUInt32();
            var someOffset = typeValue;
            //var memberType = BlLegacyCastMemberTypeHelpers.MapMemberType(typeValue);

            var infoLength = reader.ReadUInt32();
            var specificLength = reader.ReadUInt32();

            var infoBytesAvailable = info.Length - (int)reader.Position;
            if (infoBytesAvailable <= 0)
                return returnData;

            if (infoLength > (uint)infoBytesAvailable)
                infoLength = (uint)infoBytesAvailable;

            // Add Header tokens
            returnData.Add(CastToken.NewEmptyBreak("Header : "));
            returnData.Add(CastToken.NewInt32(0,(int)typeValue));
            returnData.Add(CastToken.NewInt32(4,(int)infoLength));
            returnData.Add(CastToken.NewInt32(8,(int)specificLength));
            returnData.Add(CastToken.NewEmptyBreak());

            // Be specific to read known fields in order
            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32())); 
            returnData.Add(CastToken.NewPadding(reader.Position, reader.ReadBytes(14)));
            returnData.Add(CastToken.NewEmptyBreak());

            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32())); 
            returnData.Add(CastToken.NewPadding(reader.Position, reader.ReadBytes(8)));
            returnData.Add(CastToken.NewEmptyBreak());
            
            // 8 identical ints 0x00 or 0x14 unknown values, seems to be some kind of memory offset
            for (int i = 0; i < 8; i++) 
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));

            // The memory off set is name length + 1
            var hasName = returnData.Last().IntValue > 0;
            returnData.Add(CastToken.NewEmptyBreak());

            // 1 different value 0x10 or 0x27
            var address1 = reader.ReadInt32();
            returnData.Add(CastToken.NewInt32(reader.Position, address1));
            returnData.Add(CastToken.NewEmptyBreak());

            // 2 identical ints 0x15 or 0x17 unknown values
            for (int i = 0; i < 2; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewEmptyBreak());

            // 5 identical ints 0x15 or 0x17 unknown values
            var memberTypeTextOffset = reader.ReadInt32();
            returnData.Add(CastToken.NewInt32(reader.Position, memberTypeTextOffset));
            for (int i = 0; i < 4; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewEmptyBreak());

            // 3 values :  0x19, 0x1D, 0x21 or 0x30, 0x34, 0x38
            for (int i = 0; i < 3; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewEmptyBreak());


            // Read the name
            var nameLength = reader.ReadByte();
            if (hasName) 
                returnData.Add(CastToken.NewText(reader.Position, reader.ReadAsciiString(nameLength)));
            else
                returnData.Add(CastToken.NewText(reader.Position, "[NO_NAME]"));
            returnData.Add(CastToken.NewEmptyBreak());

            // Bitmap = 6
            // Shape = 8 
            // Text = 16
            // Flash Component = 16
            // Read X bytes
            var bytesCounts = 16; // todo
            for (int i = 0; i < bytesCounts; i++)
                returnData.Add(CastToken.NewByte(reader.Position, reader.ReadByte()));
            returnData.Add(CastToken.NewEmptyBreak());

            // member type
            returnData.Add(CastToken.NewText(reader.Position, reader.ReadCString()));
            returnData.Add(CastToken.NewEmptyBreak());

            var bytesCounts2 = 6; // todo
            for (int i = 0; i < bytesCounts2; i++)
                returnData.Add(CastToken.NewByte(reader.Position, reader.ReadByte()));
            returnData.Add(CastToken.NewEmptyBreak());

            // read "N/A"
            returnData.Add(CastToken.NewText(reader.Position, reader.ReadCString()));
            returnData.Add(CastToken.NewEmptyBreak());

            var test2 = TokenListToString(returnData); // for debug

            return returnData;
        }
        public string TokenListToString(List<CastToken> tokens)
        {
            var sb = new StringBuilder();
            foreach (var token in tokens)
            {
                sb.Append(GetTokenString(token)+" ");
            }
            return sb.ToString();
        }
        private string GetTokenString(CastToken token)
        {
            string val = token.Type switch
            {
                CastTokenType.Text =>  $"Text=\"{token.StringValue}\"({token.StringValue!.Length})",
                CastTokenType.Int32 =>  $"Int={token.IntValue:X2}({token.IntValue})",
                CastTokenType.Int16 =>  $"Int16={token.IntValue:X2}({token.IntValue})",
                CastTokenType.Byte =>   $"Byte={token.IntValue:X2}({token.IntValue})",

                CastTokenType.Padding => $"Pad({token.Length})",
                CastTokenType.Empty => $" ",
                CastTokenType.EmptyBreak => (token.Description != null? "\t"+token.Description:"")+ Environment.NewLine,
                _ => ""
            };
            return val;
        }
    }

}

