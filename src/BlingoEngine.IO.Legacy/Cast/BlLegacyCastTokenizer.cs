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



        public List<CastToken> TokenizeInfo(byte[] info, int addonints = 0)
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
            

            var infoLength = reader.ReadUInt32();
            var specificLength = reader.ReadUInt32();

            var infoBytesAvailable = info.Length - (int)reader.Position;
            if (infoBytesAvailable <= 0)
                return returnData;

            if (infoLength > (uint)infoBytesAvailable)
                infoLength = (uint)infoBytesAvailable;

            // Add Header tokens
            returnData.Add(CastToken.NewEmptyBreak()); // "Header : "));
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
            var afterNameOffset = reader.ReadInt32(); // its the length of the name + 1
            for (int i = 0; i < 7; i++) 
                returnData.Add(CastToken.NewInt32(reader.Position, afterNameOffset));

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

            // 4 identical ints 0x15 or 0x17 unknown values
            var someOffset = reader.ReadInt32();
            returnData.Add(CastToken.NewInt32(reader.Position, someOffset));
            for (int i = 0; i < 3; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewEmptyBreak());

            // Mostly identical to previous
            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));

            for (int i = 0; i < 6; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewEmptyBreak());

            
            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));

            // 3 values :  0x19, 0x1D, 0x21 or 0x30, 0x34, 0x38
            // its 5 values when the typeValue == 1
            var valueCountB = typeValue == 1 ? 5 : 3;
            for (int i = 0; i < valueCountB; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewEmptyBreak());

            
            // Read the name
            var nameLength = reader.ReadByte();
            if (hasName) 
                returnData.Add(CastToken.NewText(reader.Position, reader.ReadAsciiString(nameLength)));
            else
                returnData.Add(CastToken.NewText(reader.Position, "[NO_NAME]"));
            returnData.Add(CastToken.NewEmptyBreak());

            var count = infoLength+( 3*4) - (int)reader.Position;
            for (int i = 0; i < count; i++)
            {
                returnData.Add(CastToken.NewByte(reader.Position, reader.ReadByte()));
            }
            //// Bitmap = 6
            //// Shape = 8 
            //// Text = 16
            //// Flash Component = 16
            //// Read X bytes
            //var bytesCounts = someOffset - (int)reader.Position;
            ////var bytesCounts = 16; // todo
            //for (int i = 0; i < bytesCounts; i++)
            //    returnData.Add(CastToken.NewByte(reader.Position, reader.ReadByte()));
            //returnData.Add(CastToken.NewEmptyBreak());

            //// member type
            //// Type = "Animated GIF"
            //// Type = "Format_PNG" 
            //// Type = "Format_JPEG"
            //// Type = "Flash Component" 
            //returnData.Add(CastToken.NewText(reader.Position, reader.ReadCString()));
            //returnData.Add(CastToken.NewEmptyBreak());

            //var bytesCounts2 = 0;
            //// Flash component has more bytes here : 00 FF FF FF F5 FF FF FF CE 00 00 00 0B 00 00 00 32
            //// Animated GIF : 2E 2E 2E 00
            //for (int i = 0; i < bytesCounts2; i++)
            //    returnData.Add(CastToken.NewByte(reader.Position, reader.ReadByte()));
            //returnData.Add(CastToken.NewEmptyBreak());

            //// byte in format of 2 sequences starting with 0x68, the 2 are very similar in numbers
            //// Example
            ////      68 F8 ED 98    68 F8 ED 98
            ////      68 F8 EC EE    68 F8 ED 0B
            ////      68 F8 ED 5D    68 F8 ED 5D
            ////      68 F8 D9 BC    68 F8 D9 BC
            //for (int i = 0; i < 4; i++) returnData.Add(CastToken.NewByte(reader.Position, reader.ReadByte()));
            //returnData.Add(CastToken.NewEmptyBreak());
            //for (int i = 0; i < 4; i++) returnData.Add(CastToken.NewByte(reader.Position, reader.ReadByte()));
            //returnData.Add(CastToken.NewEmptyBreak());

            //// read "N/A"
            //returnData.Add(CastToken.NewText(reader.Position, reader.ReadCString()));
            
            
            returnData.Add(CastToken.NewEmptyBreak());

            var text1 = TokenListToStringX(returnData);

            return returnData;
        }
        public string TokenListToStringX(List<CastToken> tokens)
        {
            var sb = new StringBuilder();
            var asciiString = new StringBuilder();
            var intString = new StringBuilder();
            int tokenWrite = 0;
            var addOffsetValues = false;
            var lastOffSetValue = 0;
            var lastOffSetDiffValue = 0;
            CastToken? lastIntToken = null;
            
            foreach (var token in tokens)
            {
                if (token.Type == CastTokenType.Padding && token.Length == 8)
                    addOffsetValues = true;
                if (token.Type == CastTokenType.EmptyBreak || tokenWrite == 8)
                {
                    var addon = (addOffsetValues ? " " + new string(' ', 32 - intString.Length) + $" | {lastOffSetValue,3} | {lastOffSetDiffValue,3} | ": " | ");
                    sb.AppendLine(asciiString.ToString() + new string(' ', 34 - asciiString.Length) + " | " +intString.ToString()+addon);
                    asciiString.Clear();
                    intString.Clear();
                    tokenWrite = 0;
                }
                else
                {
                    asciiString.Append(GetTokenString(token) + " ");
                    intString.Append(GetTokenStringToInt(token) + " ");
                    if (tokenWrite == 0 && addOffsetValues && token.Type == CastTokenType.Int32)
                    {
                        lastOffSetValue += token.IntValue!.Value;
                        lastOffSetDiffValue = lastIntToken != null? token.IntValue!.Value - lastIntToken.IntValue!.Value : token.IntValue!.Value;
                        lastIntToken = token;
                    }
                    tokenWrite++;
                    if (token.Type == CastTokenType.Text)
                    {
                        tokenWrite = 0;
                        addOffsetValues = false;
                    }
                    if (tokenWrite == 4)
                    {
                        asciiString.Append(" ");
                        intString.Append(" ");
                    }
                }
                
            }
            return sb.ToString();
        }

        public string TokenListToString(List<CastToken> tokens)
        {
            var sb = new StringBuilder();
            foreach (var token in tokens)
                sb.Append(GetTokenString(token));
            return sb.ToString();
        }
        private string GetTokenString(CastToken token)
        {
            string val = token.Type switch
            {
                CastTokenType.Text =>  $"\"{token.StringValue}\"({token.StringValue!.Length})",
                CastTokenType.Int32 =>  $"{token.IntValue:X2}",
                CastTokenType.Int16 =>  $"{token.IntValue:X2}",
                CastTokenType.Byte =>   $"{token.IntValue:X2}",

                CastTokenType.Padding => $"Pad({token.Length})",
                CastTokenType.Empty => $" ",
                CastTokenType.EmptyBreak => (token.Description != null? "\t"+token.Description:"")+ Environment.NewLine,
                _ => ""
            };
            return val;
        }
        private string GetTokenStringToInt(CastToken token)
        {
            string val = token.Type switch
            {
                CastTokenType.Text => $"\"{token.StringValue}\"({token.StringValue!.Length})",
                CastTokenType.Int32 => $"{token.IntValue,3}",
                CastTokenType.Int16 => $"{token.IntValue,3}",
                CastTokenType.Byte =>  $"{token.IntValue,3}",

                CastTokenType.Padding => $"Pad({token.Length})",
                CastTokenType.Empty => $" ",
                CastTokenType.EmptyBreak => (token.Description != null ? "\t" + token.Description : "") + Environment.NewLine,
                _ => ""
            };
            return val;
        }   
    }

}

