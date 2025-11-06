using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tools;
using System.Diagnostics;

namespace BlingoEngine.IO.Legacy.Scores
{
    internal class BlLegacyScoreTokenizer
    {
        private ReaderContext _context;

        public BlLegacyScoreTokenizer(ReaderContext context)
        {
            _context = context;
        }

        public string ToLog(List<BlScoreRawFrame> tokens)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var frame in tokens)
            {
                sb.AppendLine($"{frame.FrameNum:D2}) {frame.Length:X4}");
                foreach (var token in frame.Tokens)
                    sb.AppendLine("    " + token.HexString);
            }
            return sb.ToString();
        }

        internal List<BlScoreRawFrame> Tokenize(byte[] bytes)
        {
            var returnData = new List<BlScoreRawFrame>();
            //var data = bytes.ToHexString();
            var reader = new BlStreamReader(new MemoryStream(bytes));
            var frameNumber = 1;
            while (reader.Position < reader.Length)
            {
                var frameBytesLength = reader.ReadInt16();
                if (frameBytesLength == 0)
                {
                    // end normally
                    continue;
                    //returnData.Add(new BlScoreRawFrame([]));
                }
                else
                {
                    var frameBytes = reader.ReadBytes(frameBytesLength - 2);
                    var frameData = ReadFrame(frameBytes, frameBytesLength);
                    frameData.FrameNum = frameNumber;
                    returnData.Add(frameData);
                    frameNumber++;
                }
            }
            reader.BaseStream.Dispose();
            return returnData;
        }

        private BlScoreRawFrame ReadFrame(byte[] frameBytes, int frameBytesLength)
        {
            var frame = new BlScoreRawFrame(frameBytes, frameBytesLength);
            var reader = new BlStreamReader(new MemoryStream(frameBytes));
            while (reader.Position < reader.Length)
            {
                var payloadLength = reader.ReadInt16();
                var tag = reader.ReadInt16();
                var payload = reader.ReadBytes(payloadLength);
                var token = new BlScoreToken(tag, payload);
                frame.Tokens.Add(token);
            }
            reader.BaseStream.Dispose();
            return frame;
        }
        [DebuggerDisplay("Frame:{HexString}|Tokens={Tokens.Count}")]
        public class BlScoreRawFrame
        {
            public List<BlScoreToken> Tokens { get; set; } = new();
            public byte[] RawBytes { get; }
            public int Length { get; }

            public int FrameNum { get; internal set; }

            public BlScoreRawFrame(byte[] frameBytes, int frameBytesLength)
            {
                RawBytes = frameBytes;
                Length = frameBytesLength;
            }
            public string HexString => $"{FrameNum:D2}) {Length:X4} "+ string.Join(" ", RawBytes.Select(b => b.ToString("X2")));

            
        }
        [DebuggerDisplay("Token:{HexString}")]
        public class BlScoreToken
        {
            public BlScoreTag Tag { get; }
            public short TagNum { get; }
            public byte[] Payload { get; }
            public BlScoreToken(short tag, byte[] payload)
            {
                TagNum = tag;
                Payload = payload;
                if (Enum.TryParse<BlScoreTag>(TagNum.ToString("X4"), out var parsedTag))
                    Tag = parsedTag;
            }

            //public string HexString => $"{Tag.ToString()}|{TagNum:X4}={string.Join(" ", Payload.Select(b => b.ToString("X2")))}";
            public string HexString => $"{TagNum:X4}={string.Join(" ", Payload.Select(b => b.ToString("X2")))}";
        }
    }
    public enum BlScoreTag : ushort
    {
        /// <summary>EaseIn/EaseOut values (2 bytes)</summary>
        Ease = 0x0120,

        /// <summary>AdvanceFrame counter (confirmed)</summary>
        AdvanceFrame = 0x012E,

        /// <summary>Position pair (LocV, LocH) or composite pos+size (confirmed)</summary>
        PositionPair = 0x012C,

        /// <summary>Keyframe control: 01 = real KF, 81 = tween continuation (confirmed)</summary>
        KeyframeControl = 0x0136,

        /// <summary>Size pair (Width, Height) (confirmed)</summary>
        Size = 0x0130,

        /// <summary>Alternate position tag (legacy/variant)</summary>
        Position = 0x015C,

        /// <summary>File-specific channel family tag (unvalidated)</summary>
        PathPart = 0x0166, // unvalidated

        /// <summary>Ink / blend / composite flags (unvalidated)</summary>
        Ink = 0x0196, // unvalidated

        /// <summary>Composite block (confirmed)</summary>
        Composite = 0x0190,

        /// <summary>Rotation angle (confirmed)</summary>
        Rotation = 0x019E,

        /// <summary>Skew angle (confirmed)</summary>
        Skew = 0x01A2,

        /// <summary>Color pair (fore/back) (confirmed)</summary>
        Colors = 0x0212,

        /// <summary>Short color variant (confirmed)</summary>
        ColorsShort = 0x0182,

        /// <summary>Frame rectangle (unvalidated)</summary>
        FrameRect = 0x01EC, // unvalidated

        /// <summary>Curvature (tween parameter, confirmed)</summary>
        Curvature = 0x01F4,

        /// <summary>Tween flags bitmask (confirmed)</summary>
        TweenFlags = 0x01F6,

        /// <summary>Block control marker (unvalidated)</summary>
        BlockControl = 0x0180, // unvalidated

        /// <summary>Flags / control bits (unvalidated)</summary>
        Flags = 0x01FE, // unvalidated

        /// <summary>FlagsControl (unvalidated)</summary>
        FlagsControl = 0x01FC, // unvalidated

        /// <summary>Transition code / unknown purpose (unvalidated)</summary>
        TransitionCode = 0x0202, // unvalidated

        /// <summary>Unknown placeholder tags</summary>
        Unknown012A = 0x012A,
        Unknown013E = 0x013E,
        Unknown0142 = 0x0142,
        Unknown018A = 0x018A,
        Unknown01B0 = 0x01B0,
        Unknown01BA = 0x01BA,
        Unknown01C6 = 0x01C6,
        Unknown01CE = 0x01CE,
        Unknown01D2 = 0x01D2
    }

}
