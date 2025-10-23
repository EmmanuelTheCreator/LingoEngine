using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Cast.MemberTypes;
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
    internal class BlLegacyCastItemReader
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



       

        public (List<CastToken> Tokens, BlCastMemberItem? MemberItem) ReadItem(string name, byte[] info)
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
                return (returnData,null);

            if (infoLength > (uint)infoBytesAvailable)
                infoLength = (uint)infoBytesAvailable;


            var infoSlice = new byte[infoLength];
            Buffer.BlockCopy(info, 12, infoSlice, 0, infoSlice.Length);

            var specificData = new byte[specificLength];
            Buffer.BlockCopy(info, (int)infoLength + 12, specificData, 0, specificData.Length);

            // Read the offsets
            (bool hasName, List<int> offsets) = ReadOffsets(returnData, reader, (int)typeValue, (int)infoLength, (int)specificLength);

            // Make a new array with the rest bytes
            var endingBytes = new byte[infoSlice.Length - reader.Position+12];
            Buffer.BlockCopy(infoSlice, (int)reader.Position-12, endingBytes, 0, endingBytes.Length);
            //var test111 = endingBytes.ToHexString(16,false,0,true); // to debug

            // Create slices
            var bytesValues = SliceOffsetRanges(endingBytes, offsets, 0);

            // Read the name
            var nameLength = reader.ReadByte();
            var memberName = "";
            if (hasName)
            {
                memberName = reader.ReadAsciiString(nameLength);
                returnData.Add(CastToken.NewText(reader.Position, memberName));
            }
            else
                returnData.Add(CastToken.NewText(reader.Position, ""));
            returnData.Add(CastToken.NewEmptyBreak());

            //// Type = "Animated GIF..."
            //// Type = "kMoaCfFormat_PNG" 
            //// Type = "kMoaCfFormat_JPEG"
            //// Type = "Flash Component" 
            string? memberContentType = null;
            byte[]? blob = null;

            // Count the number of arrays with 4 values there are at the end
            var count4 = bytesValues.Select(x => x.Length).Reverse().TakeWhile(x => x == 4).Count();
            var numberOfOtherValues = bytesValues.Count - count4;
            var readOffsetLast3 = numberOfOtherValues;
            if (numberOfOtherValues == 2)
            {
                // member type + Blob
                var blob2 = Encoding.ASCII.GetString(bytesValues[0]);
                blob = bytesValues[0];
                memberContentType = ReadCString(bytesValues[1], 0);
            }
            else if(numberOfOtherValues == 1)
            {
                // only blob
                //memberType = Encoding.ASCII.GetString(bytesValues[0]);
                memberContentType = ReadCString(bytesValues[0], 0);
                //blob = bytesValues[0];
            }

            // Read creation datetime
            var dateCreated = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt32(bytesValues[readOffsetLast3].Reverse().ToArray(), 0)).UtcDateTime;
            // Read modified datetime
            var dateModified = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt32(bytesValues[readOffsetLast3 + 1].Reverse().ToArray(), 0)).UtcDateTime;
            // read "N/A"
            var n_a = ReadCString(bytesValues[readOffsetLast3 + 2], 0);
            if (n_a !=  "N/A")
                throw new Exception("Expected N/A value not found in ."+ name);

            // add ending bytes a as tokens
            var count = endingBytes.Length - nameLength - 1;
            for (int i = 0; i < count; i++)
                returnData.Add(CastToken.NewByte(reader.Position, endingBytes[i+ nameLength +1]));
            returnData.Add(CastToken.NewEmptyBreak());

            //var text1 = TokenListToStringX(returnData);

            var memberType = GetMemberType(specificData, memberContentType);
            //if (string.IsNullOrWhiteSpace(memberType))
            {
                var contentDebugc = specificData.ToHexString(16, true, 12, true);
                contentDebugc += Environment.NewLine;
                contentDebugc += Environment.NewLine;
                contentDebugc += blob?.ToHexString(16, true, 0, true);
            }
            BlCastMemberItem? castMember = null;
            // todo : specificData
            switch (memberType)
            {
                case "text":
                    castMember = new BlCastMemberTextReader().Read(specificData);
                    break;
                case "bitmap":
                case "bitmapPainted":
                    castMember = new BlCastMemberBitmapReader().Read(specificData, infoSlice);
                    break;
                case "animGif":
                    castMember = new BlCastMemberBitmapReader().ReadGif(specificData);
                    break;
                case "mp3":
                case "wav":
                case "aiff":
                    castMember = new BlCastMemberAudioReader().Read(specificData);
                    break;
                case "flashComponent":
                default:
                    break;
            }
            if (castMember == null)
            castMember = new BlCastMemberItem();
            castMember.Name = memberName;
            castMember.MediaContentType = memberContentType;
            castMember.Blob = blob;
            castMember.Created = dateCreated;
            castMember.Modified = dateModified;
            castMember.MemberTypeString = memberType;
            return (returnData, castMember);
        }

        private string GetMemberType(byte[] specificData, string? memberContentType)
        {
            if (specificData.Length >= 6)
            {
                var length = specificData.ReadUInt32(0);
                if (length < 16)
                {
                    var memberType = Encoding.ASCII.GetString(specificData, 4, (int)length);
                    return memberType;
                }
            }
            if (!string.IsNullOrWhiteSpace(memberContentType))
            {
                switch (memberContentType)
                {
                    case "Animated GIF...":
                    case "kMoaCfFormat_PNG":
                    case "kMoaCfFormat_JPEG":
                        return "bitmap";
                    case "kMoaCfFormat_MPEG3": return "mp3";
                    case "kMoaCfFormat_WAV": return "wav";
                    case "kMoaCfFormat_AIFF": return "aiff";
                    //case "kMoaCfFormat_SWA": return "swa";
                    //case "kMoaCfFormat_IMA": return "iwa";
                    default:
                        throw new Exception("Not implemented yet: " + memberContentType);
                }
            }
            
            if (specificData.Length == 28)
                return "bitmapPainted";
            if (specificData.Length < 20)
                return "Shape"; // Todo  another better way to detect
                return "";
        }

        private static (bool HasName, List<int> Offsets) ReadOffsets(List<CastToken> returnData, BlStreamReader reader, int typeValue, int infoLength,int specificLength)
        {

            // Add Header tokens
            returnData.Add(CastToken.NewEmptyBreak()); // "Header : "));
            returnData.Add(CastToken.NewInt32(0, typeValue));
            returnData.Add(CastToken.NewInt32(4, infoLength));
            returnData.Add(CastToken.NewInt32(8, specificLength));
            returnData.Add(CastToken.NewEmptyBreak());

            // Be specific to read known fields in order
            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewPadding(reader.Position, reader.ReadBytes(14)));
            returnData.Add(CastToken.NewEmptyBreak());

            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            returnData.Add(CastToken.NewPadding(reader.Position, reader.ReadBytes(8)));
            returnData.Add(CastToken.NewEmptyBreak());



            var offsets = new List<int>();
            // 8 identical ints 0x00 or 0x14 unknown values, seems to be some kind of memory offset
            var afterNameOffset = reader.ReadInt32(); // its the length of the name + 1
            for (int i = 0; i < 7; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, afterNameOffset));
            offsets.Add(afterNameOffset);

            // The memory off set is name length + 1
            var hasName = returnData.Last().IntValue > 0;
            returnData.Add(CastToken.NewEmptyBreak());

            // 1 different value 0x10 or 0x27
            var address1 = reader.ReadInt32();
            offsets.Add(address1);
            returnData.Add(CastToken.NewInt32(reader.Position, address1));
            returnData.Add(CastToken.NewEmptyBreak());

            // 2 identical ints 0x15 or 0x17 unknown values
            for (int i = 0; i < 2; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            offsets.Add(returnData.Last().IntValue!.Value);
            returnData.Add(CastToken.NewEmptyBreak());

            // 4 identical ints 0x15 or 0x17 unknown values
            var someOffset = reader.ReadInt32();
            returnData.Add(CastToken.NewInt32(reader.Position, someOffset));
            for (int i = 0; i < 3; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            offsets.Add(returnData.Last().IntValue!.Value);
            returnData.Add(CastToken.NewEmptyBreak());

            // Mostly identical to previous
            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            offsets.Add(returnData.Last().IntValue!.Value);

            for (int i = 0; i < 6; i++)
                returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            offsets.Add(returnData.Last().IntValue!.Value);
            returnData.Add(CastToken.NewEmptyBreak());


            returnData.Add(CastToken.NewInt32(reader.Position, reader.ReadInt32()));
            offsets.Add(returnData.Last().IntValue!.Value);


            // 3 values :  0x19, 0x1D, 0x21 or 0x30, 0x34, 0x38
            // its 5 values when the typeValue == 1
            var valueCountB = typeValue == 1 ? 5 : 3;
            for (int i = 0; i < valueCountB; i++)
            {
                var val = reader.ReadInt32();
                returnData.Add(CastToken.NewInt32(reader.Position, val));
                offsets.Add(val);
            }
            returnData.Add(CastToken.NewEmptyBreak());
            return (hasName, offsets);
        }


        private static List<byte[]> SliceOffsetRanges(byte[] data, List<int> offsets, int startIndex = 5, int? endIndex = null)
        {
            var result = new List<byte[]>();
            if (offsets == null || offsets.Count < startIndex + 2)
                return result;

            int last = endIndex ?? offsets.Count - 1;
            var lastStart = -1;
            var lastLength = -1;
            for (int i = startIndex; i < last; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                if (end <= start || start >= data.Length)
                    continue;

                int length = Math.Min(end - start, data.Length - start);

                // Skip duplicates
                if (lastStart == start && lastLength == length)
                    continue; 
                lastStart = start;
                lastLength = length;

                var slice = data.Skip(start).Take(length).ToArray();
                result.Add(slice);
            }

            return result;
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






        // Build offset table (INFO slice only). Count depends on first header int.
        public static List<int> ReadInfoOffsets(byte[] info, int headerFirstInt, int infoLength)
        {
            var O = new List<int>();
            int pos = 0;
            int TargetCount = (headerFirstInt == 1) ? 21 : 19;

            int ReadBE32() => (pos + 4 <= info.Length)
                ? (info[pos++] << 24) | (info[pos++] << 16) | (info[pos++] << 8) | info[pos++]
                : 0;

            if (pos + 4 > info.Length) return O;
            O.Add(ReadBE32());                 // 0x14
            pos += 14;                         // Pad(14)
            if (pos + 4 > info.Length) return O;
            O.Add(ReadBE32());                 // 0x16
            pos += 8;                          // Pad(8)

            while (O.Count < TargetCount && pos + 4 <= info.Length)
                O.Add(ReadBE32());

            return O;
        }





        private static uint ReadU32BE(byte[] buf, int pos) =>
            (uint)((buf[pos] << 24) | (buf[pos + 1] << 16) | (buf[pos + 2] << 8) | buf[pos + 3]);

        private static string ReadCString(byte[] buf, int pos)
        {
            if (pos < 0 || pos >= buf.Length) return "";
            int end = Array.IndexOf(buf, (byte)0, pos);
            if (end < 0) end = buf.Length;
            return Encoding.ASCII.GetString(buf, pos, end - pos);
        }

    }

}

