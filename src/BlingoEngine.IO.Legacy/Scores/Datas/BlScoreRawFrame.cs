using System.Diagnostics;

namespace BlingoEngine.IO.Legacy.Scores.Datas
{
    [DebuggerDisplay("Frame:{HexString}|Tokens={Tokens.Count}")]
    internal class BlScoreRawFrame
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
        public string HexString => $"{FrameNum:D2}) {Length:X4} " + string.Join(" ", RawBytes.Select(b => b.ToString("X2")));


    }
}
