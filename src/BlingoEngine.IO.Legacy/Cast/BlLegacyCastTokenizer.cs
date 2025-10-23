using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tools;
using System;
using System.IO;
using System.Diagnostics;
using System.Numerics;
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



        public BlCastTextMember ReadTextMember(byte[] castBytes)
        {
            ArgumentNullException.ThrowIfNull(castBytes);

            if (castBytes.Length < 12)
                throw new ArgumentException("CASt payload is too small to contain the header.", nameof(castBytes));

            using var memory = new MemoryStream(castBytes, writable: false);
            var reader = new BlStreamReader(memory)
            {
                Endianness = BlEndianness.BigEndian
            };

            reader.ReadUInt32();
            var infoLength = reader.ReadUInt32();
            var specificLength = reader.ReadUInt32();

            var infoBytes = infoLength > 0 ? reader.ReadBytes((int)infoLength) : Array.Empty<byte>();
            var specificBytes = specificLength > 0 ? reader.ReadBytes((int)specificLength) : Array.Empty<byte>();

            return ParseTextSpecific(infoBytes, specificBytes);
        }

        private static BlCastTextMember ParseTextSpecific(byte[] infoBytes, byte[] specificBytes)
        {
            using var specificStream = new MemoryStream(specificBytes, writable: false);
            var reader = new BlStreamReader(specificStream)
            {
                Endianness = BlEndianness.BigEndian
            };

            if (specificBytes.Length < 8)
                throw new InvalidDataException("Specific block does not contain the text header.");

            var typeLength = reader.ReadInt32();
            var type = reader.ReadAsciiString(typeLength);
            var specificDataLength = reader.ReadInt32();

            var isEditable = ReadBoolean(reader);
            var framing = (BlLegacyTextFraming)reader.ReadInt32();
            var tabsEnabled = ReadBoolean(reader);
            var dtdEnabled = ReadBoolean(reader);
            reader.Skip(4);

            var isAntialiasEnabled = ReadBoolean(reader);
            var antialiasMode = reader.ReadInt32();
            var antialiasLargerThan = reader.ReadInt32();
            reader.Skip(4);

            var kerningLargerThan = reader.ReadInt32();
            reader.Skip(4);

            var isKerningEnabled = ReadBoolean(reader);
            var kerningMode = reader.ReadInt32();
            var useHyperlinkStyles = ReadBoolean(reader);

            reader.Skip(4 * 3);

            var preRenderInk = (BlCastTextPreRenderInk)reader.ReadInt32();
            var savePreRenderBitmap = ReadBoolean(reader);

            var shaderTag = reader.ReadAsciiString(4);
            var shaderDataLength = reader.ReadInt32();
            var faceFlags = reader.ReadInt32();
            var tunnelDepth = ReadFixed1616(reader);
            var isBevelEnabled = ReadBoolean(reader);
            var bevelAmount = ReadFixed1616(reader);
            var bevelEdge = (BlCastTextBevelEdge)reader.ReadInt32();
            var smoothness = reader.ReadInt32();
            var lightSetting = (BlCastTextDirectionalLight)reader.ReadInt32();
            var shaderTexture = (BlCastTextShaderTexture)reader.ReadInt32();
            var diffuseIndex = reader.ReadInt32();
            var specularIndex = reader.ReadInt32();
            var reflectivity = reader.ReadInt32();

            var directionalColor = ReadColor(reader);
            var ambientColor = ReadColor(reader);
            var backgroundColor = ReadColor(reader);

            var cameraPosition = new Vector3(ReadSingle(reader), ReadSingle(reader), ReadSingle(reader));
            var cameraDistance = ReadFixed1616(reader);
            var cameraRotation = new Vector3(ReadSingle(reader), ReadSingle(reader), ReadSingle(reader));
            var cameraFocalLength = ReadFixed1616(reader);
            var textureName = reader.ReadCString();

            return new BlCastTextMember(
                type,
                specificDataLength,
                isEditable,
                framing,
                tabsEnabled,
                dtdEnabled,
                isAntialiasEnabled,
                antialiasMode,
                antialiasLargerThan,
                kerningLargerThan,
                isKerningEnabled,
                kerningMode,
                useHyperlinkStyles,
                preRenderInk,
                savePreRenderBitmap,
                shaderTag,
                shaderDataLength,
                faceFlags,
                tunnelDepth,
                isBevelEnabled,
                bevelAmount,
                bevelEdge,
                smoothness,
                lightSetting,
                shaderTexture,
                diffuseIndex,
                specularIndex,
                reflectivity,
                directionalColor,
                ambientColor,
                backgroundColor,
                cameraPosition,
                cameraDistance,
                cameraRotation,
                cameraFocalLength,
                textureName);
        }

        private static bool ReadBoolean(BlStreamReader reader) => reader.ReadInt32() != 0;

        private static double ReadFixed1616(BlStreamReader reader)
        {
            var raw = reader.ReadUInt32();
            return raw / 65536d;
        }

        private static BlLegacyColor ReadColor(BlStreamReader reader)
        {
            var raw = reader.ReadUInt32();
            var r = (byte)(raw >> 24);
            var g = (byte)(raw >> 16);
            var b = (byte)(raw >> 8);

            var rgb565 = PackRgb565(r, g, b);
            return UnpackRgb565(rgb565);
        }

        private static ushort PackRgb565(byte r, byte g, byte b)
        {
            return (ushort)(((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3));
        }

        private static BlLegacyColor UnpackRgb565(ushort value)
        {
            var red = (byte)((value >> 11) & 0x1F);
            var green = (byte)((value >> 5) & 0x3F);
            var blue = (byte)(value & 0x1F);

            red = (byte)((red * 255 + 15) / 31);
            green = (byte)((green * 255 + 31) / 63);
            blue = (byte)((blue * 255 + 15) / 31);

            return new BlLegacyColor(red, green, blue);
        }

        private static float ReadSingle(BlStreamReader reader)
        {
            var raw = reader.ReadUInt32();
            return BitConverter.Int32BitsToSingle(unchecked((int)raw));
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

