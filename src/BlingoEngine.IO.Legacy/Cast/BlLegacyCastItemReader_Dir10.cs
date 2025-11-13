using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Cast.MemberTypes;
using BlingoEngine.IO.Legacy.Tools;
using System.Text;

namespace BlingoEngine.IO.Legacy.Cast
{
    internal class BlLegacyCastItemReader_Dir10
    {

        public (BlCastRawMemberItem? MemberItem, List<byte[]> Datas) ReadItem(string name, byte[] info)
        {
            var typeValue = info.ReadInt32(0);
            var infoLength = info.ReadInt32(4);
            var specificLength = info.ReadInt32(8);

            var infoBytesAvailable = info.Length - 12;
            if (infoBytesAvailable <= 0)
                return (null, new List<byte[]>());

            var infoSlice = new byte[infoLength];
            Buffer.BlockCopy(info, 12, infoSlice, 0, infoSlice.Length);

            var specificData = new byte[specificLength];
            Buffer.BlockCopy(info, (int)infoLength + 12, specificData, 0, specificData.Length);

            // Read prefix values. These store some values for some member types.
            var prefixValues = new List<int>
            {
                infoSlice.ReadInt32(0),
                infoSlice.ReadInt32(4),
                infoSlice.ReadInt32(8),
                infoSlice.ReadInt16(12),
                infoSlice.ReadByteOrDefault(14),
                infoSlice.ReadByteOrDefault(15),
                infoSlice.ReadInt16(16),
                infoSlice.ReadInt16(18)
            };

            // Now there is first a list of offsets where the data is stored
            // then read the data as clean byte arrays
            var datas = ReadDatasByOffsets(infoSlice);

            // These are the found data in fix indexes positions. (0-index table)
            // |            | index   |                             | 
            // |------------|---------|-----------------------------|
            // | ScriptText | 0       | for Scripts                 |
            // | Name       | 1       |                             |
            // | Video      | 2       | for Video                   |
            // | ScriptLink | 3       | for Scripts                 |
            // | ScriptA    | 5       | for Scripts                 |
            // |            | 9       | for GIF /Flash /Video       |
            // | MemberType | 10      | for text/Flash              |
            // | ScriptB    | 11      | for Scripts                 |
            // |            | 12      | for text/Flash              |
            // | BitmapType | 16      | for Bitmaps                 |
            // | created    | 17      |                             |
            // | modified   | 18      |                             |
            // | Username   | 19      |                             |
            // | Comment    | 20*     |                             |
            // |            | 21*     |                             |
            // * = only some types of members have 22 values other only 20, like custom painted bitmap

            var blops = new List<byte[]>();
            var nameIndex = 1;
            var memberFormat = "";
            var memberName = "";
            bool isScript = false;
            if (datas[nameIndex].Length > 0)
            {
                var memberNameLength = datas[nameIndex][0];
                memberName = Encoding.ASCII.GetString(datas[nameIndex].Skip(1).Take(memberNameLength).ToArray());
            }
            if (datas[0].Length > 0)
            {
                isScript = true;
                blops.Add(datas[0]);                                        // script text      : long
            }
            if (datas[2].Length > 0 || isScript) blops.Add(datas[2]);       // video            : long
            if (datas[3].Length > 0 || isScript) blops.Add(datas[3]);       // script link name : long
            if (datas[5].Length > 0 || isScript) blops.Add(datas[5]);       // script           : 20
            if (datas[9].Length > 0) blops.Add(datas[9]);                   // animated GIF     : 16
                                                                            // Flash            : 20
                                                                            // Text             : 16
                                                                            // Video            : 16
            // memberContentType:
            //      Text
            //      text
            //      Flash Component
            //      Windows Media
            string memberContentType = datas[10].ReadCString(0);            // all
            if (datas[11].Length > 0 || isScript) blops.Add(datas[11]);     // script           : 20
            if (datas[12].Length > 0) blops.Add(datas[12]);                 // text             : 20
                                                                            // Flash            : 20

            // Member format
            if (datas[16].Length > 0)
            {
                // Bitmap
                // Type = "Flash Component" 
                // Type = "Animated GIF..."
                // Type = "kMoaCfFormat_PNG" 
                // Type = "kMoaCfFormat_JPEG"
                // Sound
                // Type = "kMoaCfFormat_MPEG3"
                memberFormat = Encoding.ASCII.GetString(datas[16]);    // bitmapBitmapFormat : 16
                blops.Add(datas[16]);
            }

            // Common data
            var dateCreated = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt32(datas[17].Reverse().ToArray(), 0)).UtcDateTime;
            var dateModified = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt32(datas[18].Reverse().ToArray(), 0)).UtcDateTime;
            var userName = datas[19].ReadCString(0); // read "N/A" or username
            var comment = datas.Count > 20? datas[20].ReadCString(0) : ""; // Comment is not all member types

            // special with 22 values
            if (datas.Count > 21 && datas[21].Length > 0)
                blops.Add(datas[21]);                                   // Bitmaps          : 4     : 251 , 80 , 0 , 0
            

            var member = CreateMember_Dir10(infoSlice, specificData, memberName, memberContentType, memberFormat, blops, dateCreated, dateModified, prefixValues, userName, comment);
           
            return (member, datas);

        }

        protected BlCastRawMemberItem CreateMember_Dir10(byte[] infoSlice, byte[] specificData, string memberName,string? memberContentType, string? memberFormat, List<byte[]> blobs, DateTime dateCreated, DateTime dateModified, List<int> prefixValues, string userName, string comment)
        {
            var memberType = GetMemberType(specificData, memberFormat);
            //if (string.IsNullOrWhiteSpace(memberType))
            {
                var contentDebugc = specificData.ToHexString(16, true, 12, true);
                contentDebugc += Environment.NewLine;
                contentDebugc += Environment.NewLine;
                contentDebugc += blobs.Count> 0? blobs[0].ToHexString(16, true, 0, true): "";
            }
            BlCastRawMemberItem? castMember = null;
            // todo : specificData
            switch (memberType)
            {
                case "text":
                    castMember = new BlCastMemberTextReader_Dir10().Read(specificData);
                    break;
                case "bitmap":
                case "bitmapPainted":
                    castMember = new BlCastMemberBitmapReader_Dir10().Read(specificData, infoSlice);
                    break;
                case "animGif":
                    castMember = new BlCastMemberBitmapReader_Dir10().ReadGif(specificData, blobs, prefixValues);
                    break;
                case "mp3":
                case "wav":
                case "aiff":
                    castMember = new BlCastMemberAudioReader_Dir10().Read(specificData);
                    break;
                case "shape":
                case "vectorShape":
                case "Shape":
                    if(memberType == "vectorShape")
                        castMember = new BlCastMemberShapeReader_Dir10().ReadVectorShape(specificData);
                    else
                        castMember = new BlCastMemberShapeReader_Dir10().ReadBasicShape(specificData);
                    break;
                case "script":
                    castMember = new BlCastMemberScriptReader_Dir10().Read(specificData, blobs, prefixValues);
                    break;
                case "video":
                case "windowsMedia":
                case "quickTimeMedia":
                case "avi":
                    castMember = new BlCastMemberVideoReader_Dir10().Read(specificData, blobs, prefixValues, memberType);
                    break;
                case "flashComponent":
                default:
                    break;
            }
            if (castMember == null)
                castMember = new BlCastRawMemberItem();
            castMember.Name = memberName;
            castMember.MediaContentType = memberContentType;
            castMember.MemberFormat = memberFormat;
            castMember.Blobs = blobs;
            castMember.Created = dateCreated;
            castMember.Modified = dateModified;
            castMember.MemberTypeString = memberType;
            castMember.ModifiedBy = userName;
            castMember.Comment = comment;
            return castMember;
        }

        protected string GetMemberType(byte[] specificData, string? memberContentType)
        {
            if (specificData.Length >= 6)
            {
                var length = specificData.ReadUInt32(0);
                if (length > 0 && length < 16)
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
                    case "kMoaCfFormat_TIFF":
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
            if (specificData.Length == 12)
                return "avi";
            if (specificData.Length == 2)
                return "script";
                if (specificData.Length < 20)
                return "Shape"; // Todo  another better way to detect
                return "";
        }

        /// <summary>
        /// Read first a list of offsets where the data is stored and then at all those offset, read the data as clean byte arrays
        /// </summary>
        protected static List<byte[]> ReadDatasByOffsets(byte[] infoSlice)
        {
            var offsets = new List<int>();
            var datas = new List<byte[]>();

            var offsetCountAddress = 20;
            var startOffsets = 24;

            var numberOfOffsets = infoSlice.ReadInt16(offsetCountAddress) + 1;// +1 to start from 0
            var readAddress = 0;
            for (int i = 0; i < numberOfOffsets; i++)
            {
                readAddress = startOffsets + (i * 4);
                var value = infoSlice.ReadInt16(readAddress);
                offsets.Add(value);
            }
            
            var startOffset = readAddress+  2;
            var texts = new List<string>();
            for (int i = 0; i < offsets.Count - 1; i++)
            {
                int addr = startOffset + offsets[i];
                int len = offsets[i + 1] - offsets[i];
                datas.Add(len <= 0 ? [] : infoSlice.Skip(addr).Take(len).ToArray());
            }

            return datas;
        }



    }

}

