using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Scores.Datas;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scores
{
 

    /// <summary>
    /// | Channel | Type         | Base   | Notes                                              |
    /// |---------|--------------|--------|----------------------------------------------------|
    /// | 0       | Behavior     | `0000` | Script / behavior controls                         |
    /// | 1       | Tempo        | `0030` | Global tempo or frame rate control                 |
    /// | 2       | Transition   | `0060` | Scene or movie transition effects                  |
    /// | 3       | Sound #1     | `0090` | First sound channel                                |
    /// | 4       | Sound #2     | `00C0` | Second sound channel                               |
    /// | 5       | Palette      | `00F0` | Global palette / color management                  |
    /// | 6       | Sprite start | `0120` | Beginning of regular sprite channels(user sprites) |
    /// 
    /// → From channel 6 upward, sprite tags follow a +0x30 offset pattern per channel.
    /// </summary>
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
        
    }
}
